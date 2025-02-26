namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System
open System.Net
open Thoth.Json.Net

type AuthController (env: IEnv) =
    [<Function "Login">]
    member _.Login (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")>] req: HttpRequestData
    ) = task {
        let! json = req.ReadAsStringAsync()

        match Decode.fromString LoginPayload.decoder json with
        | Error message ->
            return!
                req.CreateResponse HttpStatusCode.BadRequest
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromSerializationError message)

        | Ok payload ->
            let props: LoginCommandProps = {
                Code = payload.Code
                RedirectUri = payload.RedirectUri
            }

            match! LoginCommand.run env props with
            | Error LoginCommandError.CodeExchangeFailed ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_OAUTH_CODE)

            | Error LoginCommandError.LoginFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok (user, token) ->
                let domain = req.Url.GetLeftPart UriPartial.Authority
                let cookie = HttpCookie("token", token, Domain = domain, HttpOnly = true, Secure = true)

                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withCookie cookie
                    |> HttpResponseData.withResponse UserResponse.encoder (UserResponse.fromDomain user)
    }

    [<Function "Logout">]
    member _.Logout (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/logout")>] req: HttpRequestData
    ) = task {
        // TODO: Setup auth middleware which then handles this check already. So `Seq.find` could be used instead
        // TODO: Create env function for getting current time to remove `DateTime.UtcNow` side effect

        match req.Cookies |> Seq.tryFind (fun c -> c.Name = "token") with
        | None -> return req.CreateResponse HttpStatusCode.Unauthorized
        | Some cookie ->
            let removalCookie = HttpCookie(cookie.Name, String.Empty, Expires = DateTime.UtcNow.AddYears -1)

            return
                req.CreateResponse HttpStatusCode.NoContent
                |> HttpResponseData.withCookie removalCookie
    }
