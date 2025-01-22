namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes
open Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums
open Microsoft.OpenApi.Models
open System.Net

type ApplicationController (env: IEnv) =
    [<Function "RegisterApplication">]
    [<OpenApiOperation(operationId = "RegisterApplication", tags = [| "application" |], Summary = "Register an application", Description = "Uses the Discord bot token to setup and handle the Discord gateway connection", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiRequestBody("application/json", typeof<RegisterApplicationPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof<ApplicationResponse>, Description = "Application successfully registered")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof<ErrorResponse>, Description = "Application already registered")>]
    member _.RegisterApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications")>] req: HttpRequestData,
        [<FromBody>] payload: RegisterApplicationPayload
    ) = task {
        let! res = RegisterApplicationCommand.run env {
            DiscordBotToken = payload.DiscordBotToken
        }

        match res with
        | Error RegisterApplicationCommandError.InvalidToken ->
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)
            return res

        | Error RegisterApplicationCommandError.RegistrationFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok application ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (ApplicationResponse.fromDomain application)
            return res
    }

    [<Function "GetApplication">]
    [<OpenApiOperation(operationId = "GetApplication", tags = [| "application" |], Summary = "Fetch an application by its ID", Description = "Checks if a bot is setup, and if so returns information about it", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<ApplicationResponse>, Description = "Application found")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.GetApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId}")>] req: HttpRequestData,
        applicationId: string
    ) = task {
        let! res = GetApplicationQuery.run env {
            ApplicationId = applicationId
        }

        match res with
        | Error GetApplicationQueryError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Ok application ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (ApplicationResponse.fromDomain application)
            return res
    }

    [<Function "UpdateApplication">]
    [<OpenApiOperation(operationId = "UpdateApplication", tags = [| "application" |], Summary = "Update properties on an application", Description = "Used to update various configurations and details for an application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiRequestBody("application/json", typeof<UpdateApplicationPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<ApplicationResponse>, Description = "Application updated")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.UpdateApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "applications/{applicationId}")>] req: HttpRequestData,
        [<FromBody>] payload: UpdateApplicationPayload,
        applicationId: string
    ) = task {
        let! res = UpdateApplicationCommand.run env {
            ApplicationId = applicationId
            DiscordBotToken = payload.DiscordBotToken
            Intents = payload.Intents
            ShardCount = payload.ShardCount
            DisabledReasons = payload.DisabledReasons |> Option.map DisabledApplicationReason.fromBitfield
        }
        
        match res with
        | Error UpdateApplicationCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error UpdateApplicationCommandError.InvalidToken -> 
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)
            return res

        | Error UpdateApplicationCommandError.DifferentBotToken -> 
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)
            return res

        | Error UpdateApplicationCommandError.UpdateFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok application ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (ApplicationResponse.fromDomain application)
            return res            
    }

    [<Function "DeleteApplication">]
    [<OpenApiOperation(operationId = "DeleteApplication", tags = [| "application" |], Summary = "Remove an application", Description = "Removes any data associated with the given application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Application deleted")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.DeleteApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId}")>] req: HttpRequestData,
        applicationId: string
    ) = task {
        let! res = DeleteApplicationCommand.run env {
            ApplicationId = applicationId
        }

        match res with
        | Error DeleteApplicationCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | _ ->
            return req.CreateResponse HttpStatusCode.NoContent
    }
    
    [<Function "SyncApplicationPrivilegedIntents">]
    [<OpenApiOperation(operationId = "SyncApplicationPrivilegedIntents", tags = [| "application" |], Summary = "Syncs the application's privileged intents with Discord", Description = "Fetches the application's privileged intents from Discord and updates internally", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<PrivilegedIntentsResponse>, Description = "Application updated")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.SyncApplicationPrivilegedIntents (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId}/sync-privileged-intents")>] req: HttpRequestData,
        applicationId: string
    ) = task {
        let! res = SyncApplicationPrivilegedIntentsCommand.run env {
            ApplicationId = applicationId
        }

        match res with
        | Error SyncApplicationPrivilegedIntentsCommandError.ApplicationNotFound ->
            let res = req.CreateResponse HttpStatusCode.NotFound
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
            return res

        | Error SyncApplicationPrivilegedIntentsCommandError.InvalidToken -> 
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)
            return res

        | Error SyncApplicationPrivilegedIntentsCommandError.DifferentBotToken -> 
            let res = req.CreateResponse HttpStatusCode.BadRequest
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)
            return res

        | Error SyncApplicationPrivilegedIntentsCommandError.UpdateFailed ->
            let res = req.CreateResponse HttpStatusCode.InternalServerError
            do! res.WriteAsJsonAsync (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)
            return res

        | Ok privilegedIntents ->
            let res = req.CreateResponse HttpStatusCode.OK
            do! res.WriteAsJsonAsync (PrivilegedIntentsResponse.fromDomain privilegedIntents)
            return res 
    }

    [<Function "SetApplicationHandler">]
    [<OpenApiOperation(operationId = "SetApplicationHandler", tags = [| "application"; "handler" |], Summary = "Sets the handler for an application", Description = "Replaces any existing handler with the given handler", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiRequestBody("application/json", typeof<SetApplicationHandlerPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<HandlerResponse>, Description = "Application handler set")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.SetApplicationHandler (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId}/handler")>] req: HttpRequestData,
        [<FromBody>] payload: SetApplicationHandlerPayload,
        applicationId: string
    ) = task {
        match payload with
        | SetApplicationHandlerPayload.WEBHOOK payload ->
            let! res = SetWebhookApplicationHandlerCommand.run env {
                ApplicationId = applicationId
                Endpoint = payload.Endpoint
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
                do! res.WriteAsJsonAsync (HandlerResponse.fromDomain (Handler.WEBHOOK handler))
                return res

        | SetApplicationHandlerPayload.SERVICE_BUS payload ->
            let! res = SetServiceBusApplicationHandlerCommand.run env {
                ApplicationId = applicationId
                ConnectionString = payload.ConnectionString
                QueueName = payload.QueueName
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
                do! res.WriteAsJsonAsync (HandlerResponse.fromDomain (Handler.SERVICE_BUS handler))
                return res
    }

    [<Function "RemoveApplicationHandler">]
    [<OpenApiOperation(operationId = "RemoveApplicationHandler", tags = [| "application"; "handler" |], Summary = "Removes the handler for the application", Description = "Removes the handler for the application, which also deactivates the bot", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Application handler removed")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.RemoveApplicationHandler (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId}/handler")>] req: HttpRequestData,
        applicationId: string
    ) = task {
        let! res = RemoveApplicationHandlerCommand.run env {
            ApplicationId = applicationId
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

    [<Function "AddDisabledApplicationReason">]
    [<OpenApiOperation(operationId = "AddDisabledApplicationReason", tags = [| "application"; "disabled-reasons" |], Summary = "Adds the disabled reason to the application", Description = "Idempotently adds the given disabled reason to the application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiParameter("disabledReason", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The disabled reason to add", Description = "The disabled reason to add to the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<DisabledApplicationReasonResponse>, Description = "Disabled reason added, returning total bitfield")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.AddDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{applicationId}/disabledReasons/{disabledReason}")>] req: HttpRequestData,
        applicationId: string,
        disabledReason: int
    ) = task {
        let! res = AddDisabledApplicationReasonCommand.run env {
            ApplicationId = applicationId
            DisabledReason = enum<DisabledApplicationReason> disabledReason
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
    [<OpenApiOperation(operationId = "RemoveDisabledApplicationReason", tags = [| "application"; "disabledReasons" |], Summary = "Removes the disabled reason to the application", Description = "Idempotently removes the given disabled reason to the application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiParameter("disabledReason", In = ParameterLocation.Path, Required = true, Type = typeof<string>, Summary = "The disabled reason to remove", Description = "The disabled reason to remove from the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<DisabledApplicationReasonResponse>, Description = "Disabled reason removed, returning total bitfield")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.RemoveDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId}/disabled-reasons/{disabledReason}")>] req: HttpRequestData,
        applicationId: string,
        disabledReason: int
    ) = task {
        let! res = RemoveDisabledApplicationReasonCommand.run env {
            ApplicationId = applicationId
            DisabledReason = enum<DisabledApplicationReason> disabledReason
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
