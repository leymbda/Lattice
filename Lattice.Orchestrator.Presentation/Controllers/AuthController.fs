namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
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
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromSerializationError message)
            return res

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

            | Ok token ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse TokenResponse.encoder (TokenResponse.fromDomain token)
    }
