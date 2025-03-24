namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net
open Thoth.Json.Net

type ApplicationController (env: IEnv) =
    [<Function "RegisterApplication">]
    member _.RegisterApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications")>] req: HttpRequestData
    ) = task {
        let! json = req.ReadAsStringAsync()
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        match Decode.fromString RegisterAppPayload.decoder json with
        | Error message ->
            return!
                req.CreateResponse HttpStatusCode.BadRequest
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromSerializationError message)

        | Ok payload ->
            let props: RegisterApplicationCommandProps = {
                UserId = userId
                DiscordBotToken = payload.DiscordBotToken
            }

            match! RegisterApplicationCommand.run env props with
            | Error RegisterApplicationCommandError.InvalidBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

            | Error RegisterApplicationCommandError.Forbidden ->
                return!
                    req.CreateResponse HttpStatusCode.Forbidden
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

            | Error RegisterApplicationCommandError.RegistrationFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok app ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse AppResponse.encoder (AppResponse.fromDomain app)
    }
    
    [<Function "GetApplication">]
    member _.GetApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{appId:long}")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        let props: GetApplicationQueryProps = {
            UserId = userId
            AppId = string appId
        }

        match! GetApplicationQuery.run env props with
        | Error GetApplicationQueryError.Forbidden ->
            return!
                req.CreateResponse HttpStatusCode.Forbidden
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

        | Error GetApplicationQueryError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
                
        | Error GetApplicationQueryError.TeamNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

        | Ok app ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse AppResponse.encoder (AppResponse.fromDomain app)
    }
    
    [<Function "UpdateApplication">]
    member _.UpdateApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "applications/{appId:long}")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let! json = req.ReadAsStringAsync()
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        match Decode.fromString UpdateAppPayload.decoder json with
        | Error message ->
            return!
                req.CreateResponse HttpStatusCode.BadRequest
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromSerializationError message)

        | Ok payload ->
            let props: UpdateApplicationCommandProps = {
                UserId = userId
                AppId = string appId
                DiscordBotToken = payload.DiscordBotToken
                Intents = payload.Intents
                ShardCount = payload.ShardCount
                Handler = payload.Handler |> Option.map (fun handler -> handler |> function
                    | Some (CreateHandlerPayload.WEBHOOK handler) -> Some (UpdateApplicationCommandHandlerProps.WEBHOOK (handler.Endpoint))
                    | Some (CreateHandlerPayload.SERVICE_BUS handler) -> Some (UpdateApplicationCommandHandlerProps.SERVICE_BUS (handler.QueueName, handler.ConnectionString))
                    | None -> None
                )
            }
        
            match! UpdateApplicationCommand.run env props with
            | Error UpdateApplicationCommandError.InvalidBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

            | Error UpdateApplicationCommandError.Forbidden ->
                return!
                    req.CreateResponse HttpStatusCode.Forbidden
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

            | Error UpdateApplicationCommandError.ApplicationNotFound ->
                return!
                    req.CreateResponse HttpStatusCode.NotFound
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
                    
            | Error UpdateApplicationCommandError.TeamNotFound ->
                return!
                    req.CreateResponse HttpStatusCode.NotFound
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

            | Error UpdateApplicationCommandError.DifferentBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)

            | Error UpdateApplicationCommandError.UpdateFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok app ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse AppResponse.encoder (AppResponse.fromDomain app)
    }
    
    [<Function "DeleteApplication">]
    member _.DeleteApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{appId:long}")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        let props: DeleteApplicationCommandProps = {
            UserId = userId
            AppId = string appId
        }

        match! DeleteApplicationCommand.run env props with
        | Error DeleteApplicationCommandError.Forbidden ->
            return!
                req.CreateResponse HttpStatusCode.Forbidden
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

        | Error DeleteApplicationCommandError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
                
        | Error DeleteApplicationCommandError.TeamNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

        | Ok _ ->
            return req.CreateResponse HttpStatusCode.NoContent
    }
    
    [<Function "SyncApplicationPrivilegedIntents">]
    member _.SyncApplicationPrivilegedIntents (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{appId:long}/sync-privileged-intents")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        let props: SyncApplicationPrivilegedIntentsCommandProps = {
            UserId = userId
            AppId = string appId
        }

        match! SyncApplicationPrivilegedIntentsCommand.run env props with
        | Error SyncApplicationPrivilegedIntentsCommandError.InvalidBotToken ->
            return!
                req.CreateResponse HttpStatusCode.UnprocessableContent
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

        | Error SyncApplicationPrivilegedIntentsCommandError.Forbidden ->
            return!
                req.CreateResponse HttpStatusCode.Forbidden
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

        | Error SyncApplicationPrivilegedIntentsCommandError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)
                
        | Error SyncApplicationPrivilegedIntentsCommandError.TeamNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

        | Error SyncApplicationPrivilegedIntentsCommandError.DifferentBotToken ->
            return!
                req.CreateResponse HttpStatusCode.UnprocessableContent
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)

        | Error SyncApplicationPrivilegedIntentsCommandError.UpdateFailed ->
            return!
                req.CreateResponse HttpStatusCode.InternalServerError
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

        | Ok privilegedIntents ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse PrivilegedIntentsResponse.encoder (PrivilegedIntentsResponse.fromDomain privilegedIntents)
    }
    
    [<Function "AddDisabledApplicationReason">]
    member _.AddDisabledApplicationReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "applications/{appId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        appId: int64,
        reasonId: int
    ) = task {
        let props: AddDisabledApplicationReasonCommandProps = {
            AppId = string appId
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
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{appId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        appId: int64,
        reasonId: int
    ) = task {
        let props: RemoveDisabledApplicationReasonCommandProps = {
            AppId = string appId
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
