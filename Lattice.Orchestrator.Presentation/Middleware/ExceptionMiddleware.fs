namespace Lattice.Orchestrator.Presentation

open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker.Middleware
open System.Net

type ExceptionMiddleware () =
    interface IFunctionsWorkerMiddleware with
        member _.Invoke (ctx, next) = task {
            try
                return! next.Invoke ctx

            with | ex ->
                // TODO: Handle exception here (logging, etc.)

                let! req = ctx.GetHttpRequestDataAsync()

                let res =
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

                ctx.GetInvocationResult().Value <- res
        }
