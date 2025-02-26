namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker.Middleware
open Microsoft.Extensions.DependencyInjection
open System
open System.Net

type AuthorizeAttribute () =
    inherit Attribute ()

type AuthorizeMiddleware () =
    interface IFunctionsWorkerMiddleware with
        member _.Invoke (ctx, next) = task {
            let env = ctx.InstanceServices.GetRequiredService<IEnv>()
            let! req = ctx.GetHttpRequestDataAsync()

            match FunctionContext.getCustomAttribute<AuthorizeAttribute> ctx with
            | None ->
                return! next.Invoke ctx

            | Some attr ->
                let token =
                    req.Cookies
                    |> Seq.tryFind (fun c -> c.Name = Constants.TOKEN_COOKIE_NAME)
                    |> Option.map _.Value
                    |> Option.map (Jwt.decode<TokenClaims> env.JwtHashingKey)
                    |> Option.flatten

                match token with
                | None ->
                    ctx.GetInvocationResult().Value <- req.CreateResponse HttpStatusCode.Unauthorized

                | Some claims ->
                    // TODO: Add permissions (roles) to the attribute and claims, then compare here

                    return! next.Invoke ctx
        }

// TODO: Create input binding to retrieve the claims in the function itself (?) Need to check if user has permission to
//       access resources so most endpoints will need the subject claim. Operations can return an error for incorrect
//       user accessing, which the function can map to 403.
