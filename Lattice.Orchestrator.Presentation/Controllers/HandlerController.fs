namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net

// TODO: Refactor to handle in just update application

//type HandlerController (env: IEnv) =
//    [<Function "SetWebhookApplicationHandler">]
//    member _.SetWebookApplicationHandler (
//        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId:long}/handler/webhook")>] req: HttpRequestData,
//        [<FromBody>] payload: SetWebhookApplicationHandlerPayload,
//        applicationId: int64
//    ) = task {
//        let! res = SetWebhookApplicationHandlerCommand.run env {
//            ApplicationId = string applicationId
//            Endpoint = payload.Endpoint |> String.defaultValue ""
//        }

//        match res with
//        | Error SetWebhookApplicationHandlerCommandError.ApplicationNotFound ->
//            let res = req.CreateResponse HttpStatusCode.NotFound
//            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
//            return res

//        | Error SetWebhookApplicationHandlerCommandError.UpdateFailed ->
//            let res = req.CreateResponse HttpStatusCode.InternalServerError
//            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
//            return res

//        | Ok handler ->
//            let res = req.CreateResponse HttpStatusCode.OK
//            do! res.WriteAsJsonAsync (WebhookHandlerResponse.fromDomain handler)
//            return res
//    }
    
//    [<Function "SetServiceBusApplicationHandler">]
//    member _.SetServiceBusApplicationHandler (
//        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId:long}/handler/service-bus")>] req: HttpRequestData,
//        [<FromBody>] payload: SetServiceBusApplicationHandlerPayload,
//        applicationId: int64
//    ) = task {
//        let! res = SetServiceBusApplicationHandlerCommand.run env {
//            ApplicationId = string applicationId
//            ConnectionString = payload.ConnectionString |> String.defaultValue ""
//            QueueName = payload.QueueName |> String.defaultValue ""
//        }

//        match res with
//        | Error SetServiceBusApplicationHandlerCommandError.ApplicationNotFound ->
//            let res = req.CreateResponse HttpStatusCode.NotFound
//            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
//            return res

//        | Error SetServiceBusApplicationHandlerCommandError.UpdateFailed ->
//            let res = req.CreateResponse HttpStatusCode.InternalServerError
//            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
//            return res

//        | Ok handler ->
//            let res = req.CreateResponse HttpStatusCode.OK
//            do! res.WriteAsJsonAsync (ServiceBusHandlerResponse.fromDomain handler)
//            return res
//    }

//    [<Function "RemoveApplicationHandler">]
//    member _.RemoveApplicationHandler (
//        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId:long}/handler")>] req: HttpRequestData,
//        applicationId: int64
//    ) = task {
//        let! res = RemoveApplicationHandlerCommand.run env {
//            ApplicationId = string applicationId
//        }

//        match res with
//        | Error RemoveApplicationHandlerCommandError.ApplicationNotFound ->
//            let res = req.CreateResponse HttpStatusCode.NotFound
//            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
//            return res

//        | Error RemoveApplicationHandlerCommandError.RemovalFailed ->
//            let res = req.CreateResponse HttpStatusCode.InternalServerError
//            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
//            return res

//        | Ok () ->
//            return req.CreateResponse HttpStatusCode.NoContent
//    }
