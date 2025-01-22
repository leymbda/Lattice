namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums
open Microsoft.OpenApi.Models
open System.Net

type HandlerController (env: IEnv) =
    [<Function "SetWebhookApplicationHandler">]
    [<OpenApiOperation(operationId = "SetWebhookApplicationHandler", Summary = "Sets the handler for an application", Description = "Replaces any existing handler with the given handler", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiRequestBody("application/json", typeof<SetWebhookApplicationHandlerPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<WebhookHandlerResponse>, Description = "Application handler set")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.SetWebookApplicationHandler (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId:long}/handler/webhook")>] req: HttpRequestData,
        [<FromBody>] payload: SetWebhookApplicationHandlerPayload,
        applicationId: int64
    ) = task {
        let! res = SetWebhookApplicationHandlerCommand.run env {
            ApplicationId = string applicationId
            Endpoint = payload.Endpoint |> String.defaultValue ""
        }

        match res with
        | Error SetWebhookApplicationHandlerCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error SetWebhookApplicationHandlerCommandError.UpdateFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok handler ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (WebhookHandlerResponse.fromDomain handler)
            return res
    }
    
    [<Function "SetServiceBusApplicationHandler">]
    [<OpenApiOperation(operationId = "SetServiceBusApplicationHandler", Summary = "Sets the handler for an application", Description = "Replaces any existing handler with the given handler", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiRequestBody("application/json", typeof<SetServiceBusApplicationHandlerPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<ServiceBusHandlerResponse>, Description = "Application handler set")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.SetServiceBusApplicationHandler (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId:long}/handler/service-bus")>] req: HttpRequestData,
        [<FromBody>] payload: SetServiceBusApplicationHandlerPayload,
        applicationId: int64
    ) = task {
        let! res = SetServiceBusApplicationHandlerCommand.run env {
            ApplicationId = string applicationId
            ConnectionString = payload.ConnectionString |> String.defaultValue ""
            QueueName = payload.QueueName |> String.defaultValue ""
        }

        match res with
        | Error SetServiceBusApplicationHandlerCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error SetServiceBusApplicationHandlerCommandError.UpdateFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok handler ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (ServiceBusHandlerResponse.fromDomain handler)
            return res
    }

    [<Function "RemoveApplicationHandler">]
    [<OpenApiOperation(operationId = "RemoveApplicationHandler", Summary = "Removes the handler for the application", Description = "Removes the handler for the application, which also deactivates the bot", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Application handler removed")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.RemoveApplicationHandler (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId:long}/handler")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let! res = RemoveApplicationHandlerCommand.run env {
            ApplicationId = string applicationId
        }

        match res with
        | Error RemoveApplicationHandlerCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error RemoveApplicationHandlerCommandError.RemovalFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok () ->
            return req.CreateResponse HttpStatusCode.NoContent
    }
