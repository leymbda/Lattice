namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net

type AuthController (env: IEnv) =
    [<Function "Login">]
    member _.Login (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")>] req: HttpRequestData,
        [<FromBody>] payload: LoginPayload
    ) = task {
        let! res = LoginCommand.run env {
            Code = payload.Code |> String.defaultValue ""
            RedirectUri = payload.RedirectUri |> String.defaultValue ""
        }

        match res with
        | Error LoginCommandError.CodeExchangeFailed ->
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INVALID_OAUTH_CODE)
            return res

        | Error LoginCommandError.LoginFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok token ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (TokenResponse(token))
            return res
    }
