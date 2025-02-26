namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net

type DisabledReasonsController (env: IEnv) =
    [<Function "AddDisabledApplicationReason">]
    member _.AddDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        applicationId: int64,
        reasonId: int
    ) = task {
        let props: AddDisabledApplicationReasonCommandProps = {
            ApplicationId = string applicationId
            DisabledReason = enum<DisabledApplicationReason> reasonId
        }

        match! AddDisabledApplicationReasonCommand.run env props with
        | Error AddDisabledApplicationReasonCommandError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)

        | Error AddDisabledApplicationReasonCommandError.AddFailed ->
            return!
                req.CreateResponse HttpStatusCode.InternalServerError
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

        | Ok reasons ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse DisabledApplicationReasonResponse.encoder (DisabledApplicationReasonResponse.fromDomain reasons)
    }

    [<Function "RemoveDisabledApplicationReason">]
    member _.RemoveDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        applicationId: int64,
        reasonId: int
    ) = task {
        let props: RemoveDisabledApplicationReasonCommandProps = {
            ApplicationId = string applicationId
            DisabledReason = enum<DisabledApplicationReason> reasonId
        }
        
        match! RemoveDisabledApplicationReasonCommand.run env props with
        | Error RemoveDisabledApplicationReasonCommandError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)

        | Error RemoveDisabledApplicationReasonCommandError.RemoveFailed ->
            return!
                req.CreateResponse HttpStatusCode.InternalServerError
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

        | Ok reasons ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse DisabledApplicationReasonResponse.encoder (DisabledApplicationReasonResponse.fromDomain reasons)
    }
