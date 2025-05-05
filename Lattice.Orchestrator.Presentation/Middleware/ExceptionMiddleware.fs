namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Contracts
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker.Middleware
open Microsoft.Extensions.Logging
open System.Net

type ExceptionMiddleware () =
    interface IFunctionsWorkerMiddleware with
        member _.Invoke (ctx, next) = task {
            try
                return! next.Invoke ctx

            with | ex ->
                ctx.GetLogger().LogError (ex, "Unexpected error caught by exception middleware")

                let binding =
                    ctx.GetOutputBindings<HttpResponseData>()
                    |> Seq.tryFind (fun b -> b.BindingType = "http" && b.Name <> "$return")

                let! req = ctx.GetHttpRequestDataAsync()
                    
                let! res =
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

                match binding with
                | Some http -> http.Value <- res
                | None -> ctx.GetInvocationResult().Value <- res
        }
