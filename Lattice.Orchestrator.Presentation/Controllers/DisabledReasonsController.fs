namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums
open Microsoft.OpenApi.Models
open System.Net

type DisabledReasonsController (env: IEnv) =
    [<Function "AddDisabledApplicationReason">]
    [<OpenApiOperation(operationId = "AddDisabledApplicationReason", Summary = "Adds the disabled reason to the application", Description = "Idempotently adds the given disabled reason to the application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiParameter("reasonId", In = ParameterLocation.Path, Required = true, Type = typeof<int>, Summary = "The disabled reason to add", Description = "The disabled reason to add to the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<DisabledApplicationReasonResponse>, Description = "Disabled reason added, returning total bitfield")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.AddDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        applicationId: int64,
        reasonId: int
    ) = task {
        let! res = AddDisabledApplicationReasonCommand.run env {
            ApplicationId = string applicationId
            DisabledReason = enum<DisabledApplicationReason> reasonId
        }
        
        match res with
        | Error AddDisabledApplicationReasonCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error AddDisabledApplicationReasonCommandError.AddFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok reasons ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (DisabledApplicationReasonResponse.fromDomain reasons)
            return res
    }

    [<Function "RemoveDisabledApplicationReason">]
    [<OpenApiOperation(operationId = "RemoveDisabledApplicationReason", Summary = "Removes the disabled reason to the application", Description = "Idempotently removes the given disabled reason to the application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiParameter("reasonId", In = ParameterLocation.Path, Required = true, Type = typeof<int>, Summary = "The disabled reason to remove", Description = "The disabled reason to remove from the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<DisabledApplicationReasonResponse>, Description = "Disabled reason removed, returning total bitfield")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.RemoveDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        applicationId: int64,
        reasonId: int
    ) = task {
        let! res = RemoveDisabledApplicationReasonCommand.run env {
            ApplicationId = string applicationId
            DisabledReason = enum<DisabledApplicationReason> reasonId
        }
        
        match res with
        | Error RemoveDisabledApplicationReasonCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error RemoveDisabledApplicationReasonCommandError.RemoveFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok reasons ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (DisabledApplicationReasonResponse.fromDomain reasons)
            return res
    }
