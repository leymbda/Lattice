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
    [<OpenApiOperation(operationId = "RegisterApplication", Summary = "Register an application", Description = "Uses the Discord bot token to setup and handle the Discord gateway connection", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiRequestBody("application/json", typeof<RegisterApplicationPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof<ApplicationResponse>, Description = "Application successfully registered")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.Conflict, "application/json", typeof<ErrorResponse>, Description = "Application already registered")>]
    member _.RegisterApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications")>] req: HttpRequestData,
        [<FromBody>] payload: RegisterApplicationPayload
    ) = task {
        let! res = RegisterApplicationCommand.run env {
            DiscordBotToken = payload.DiscordBotToken |> String.defaultValue ""
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
    [<OpenApiOperation(operationId = "GetApplication", Summary = "Fetch an application by its ID", Description = "Checks if a bot is setup, and if so returns information about it", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<ApplicationResponse>, Description = "Application found")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.GetApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId:long}")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let! res = GetApplicationQuery.run env {
            ApplicationId = string applicationId
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
    [<OpenApiOperation(operationId = "UpdateApplication", Summary = "Update properties on an application", Description = "Used to update various configurations and details for an application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiRequestBody("application/json", typeof<UpdateApplicationPayload>, Required = true, Description = "The required payload for this endpoint")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<ApplicationResponse>, Description = "Application updated")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.UpdateApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "applications/{applicationId:long}")>] req: HttpRequestData,
        [<FromBody>] payload: UpdateApplicationPayload,
        applicationId: int64
    ) = task {
        let! res = UpdateApplicationCommand.run env {
            ApplicationId = string applicationId
            DiscordBotToken = Option.ofString payload.DiscordBotToken
            Intents = Option.ofNullable payload.Intents
            ShardCount = Option.ofNullable payload.ShardCount
            DisabledReasons = Option.ofNullable payload.DisabledReasons |> Option.map DisabledApplicationReason.fromBitfield
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
    [<OpenApiOperation(operationId = "DeleteApplication", Summary = "Remove an application", Description = "Removes any data associated with the given application", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Application deleted")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.DeleteApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId:long}")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let! res = DeleteApplicationCommand.run env {
            ApplicationId = string applicationId
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
    [<OpenApiOperation(operationId = "SyncApplicationPrivilegedIntents", Summary = "Syncs the application's privileged intents with Discord", Description = "Fetches the application's privileged intents from Discord and updates internally", Visibility = OpenApiVisibilityType.Advanced)>]
    [<OpenApiParameter("applicationId", In = ParameterLocation.Path, Required = true, Type = typeof<int64>, Summary = "The ID of the application", Description = "The ID of the application")>]
    [<OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof<PrivilegedIntentsResponse>, Description = "Application updated")>]
    [<OpenApiResponseWithBody(HttpStatusCode.BadRequest, "application/json", typeof<ErrorResponse>, Description = "Invalid values provided in payload")>]
    [<OpenApiResponseWithBody(HttpStatusCode.NotFound, "application/json", typeof<ErrorResponse>, Description = "Application not found")>]
    member _.SyncApplicationPrivilegedIntents (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId:long}/sync-privileged-intents")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let! res = SyncApplicationPrivilegedIntentsCommand.run env {
            ApplicationId = string applicationId
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
