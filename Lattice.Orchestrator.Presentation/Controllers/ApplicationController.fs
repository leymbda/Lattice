namespace Lattice.Orchestrator.Presentation

open Lattice.Orchestrator.Application
open Lattice.Orchestrator.Contracts
open Lattice.Orchestrator.Domain
open Microsoft.Azure.Functions.Worker
open Microsoft.Azure.Functions.Worker.Http
open System.Net
open Thoth.Json.Net

type ApplicationController (env: IEnv) =
    [<Function "RegisterApp">]
    member _.RegisterApp (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "apps")>] req: HttpRequestData
    ) = task {
        let! json = req.ReadAsStringAsync()
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        match Decode.fromString RegisterAppPayload.decoder json with
        | Error message ->
            return!
                req.CreateResponse HttpStatusCode.BadRequest
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromSerializationError message)

        | Ok payload ->
            let props: RegisterApp.Props = {
                UserId = userId
                DiscordBotToken = payload.DiscordBotToken
            }

            match! RegisterApp.run env props with
            | Error RegisterApp.Failure.InvalidBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

            | Error RegisterApp.Failure.Forbidden ->
                return!
                    req.CreateResponse HttpStatusCode.Forbidden
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

            | Error RegisterApp.Failure.RegistrationFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok app ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse AppResponse.encoder (AppResponse.fromDomain app)
    }
    
    [<Function "GetApp">]
    member _.GetApp (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apps/{appId:long}")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        let props: GetApp.Props = {
            UserId = userId
            AppId = string appId
        }

        match! GetApp.run env props with
        | Error GetApp.Failure.Forbidden ->
            return!
                req.CreateResponse HttpStatusCode.Forbidden
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

        | Error GetApp.Failure.AppNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APP_NOT_FOUND)
                
        | Error GetApp.Failure.TeamNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

        | Ok app ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse AppResponse.encoder (AppResponse.fromDomain app)
    }
    
    [<Function "UpdateApp">]
    member _.UpdateApp (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "apps/{appId:long}")>] req: HttpRequestData,
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
            let props: UpdateApp.Props = {
                UserId = userId
                AppId = string appId
                DiscordBotToken = payload.DiscordBotToken
                Intents = payload.Intents
                ShardCount = payload.ShardCount
                Handler = payload.Handler |> Option.map (fun handler -> handler |> function
                    | Some (CreateHandlerPayload.WEBHOOK handler) -> Some (UpdateApp.HandlerProps.WEBHOOK (handler.Endpoint))
                    | Some (CreateHandlerPayload.SERVICE_BUS handler) -> Some (UpdateApp.HandlerProps.SERVICE_BUS (handler.QueueName, handler.ConnectionString))
                    | None -> None
                )
            }
        
            match! UpdateApp.run env props with
            | Error UpdateApp.Failure.InvalidBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

            | Error UpdateApp.Failure.Forbidden ->
                return!
                    req.CreateResponse HttpStatusCode.Forbidden
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

            | Error UpdateApp.Failure.AppNotFound ->
                return!
                    req.CreateResponse HttpStatusCode.NotFound
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APP_NOT_FOUND)
                    
            | Error UpdateApp.Failure.TeamNotFound ->
                return!
                    req.CreateResponse HttpStatusCode.NotFound
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

            | Error UpdateApp.Failure.DifferentBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)

            | Error UpdateApp.Failure.UpdateFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok app ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse AppResponse.encoder (AppResponse.fromDomain app)
    }
    
    [<Function "DeleteApp">]
    member _.DeleteApp (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "apps/{appId:long}")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        let props: DeleteApp.Props = {
            UserId = userId
            AppId = string appId
        }

        match! DeleteApp.run env props with
        | Error DeleteApp.Failure.Forbidden ->
            return!
                req.CreateResponse HttpStatusCode.Forbidden
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

        | Error DeleteApp.Failure.AppNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APP_NOT_FOUND)
                
        | Error DeleteApp.Failure.TeamNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

        | Ok _ ->
            return req.CreateResponse HttpStatusCode.NoContent
    }
    
    [<Function "SyncApplicationPrivilegedIntents">]
    member _.SyncApplicationPrivilegedIntents (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "apps/{appId:long}/sync-privileged-intents")>] req: HttpRequestData,
        appId: int64
    ) = task {
        let userId = "" // TODO: Get user ID from request header (TBD by swa auth stuff)

        let props: SyncApplicationPrivilegedIntents.Props = {
            UserId = userId
            AppId = string appId
        }

        match! SyncApplicationPrivilegedIntents.run env props with
        | Error SyncApplicationPrivilegedIntents.Failure.InvalidBotToken ->
            return!
                req.CreateResponse HttpStatusCode.UnprocessableContent
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

        | Error SyncApplicationPrivilegedIntents.Failure.Forbidden ->
            return!
                req.CreateResponse HttpStatusCode.Forbidden
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.FORBIDDEN)

        | Error SyncApplicationPrivilegedIntents.Failure.AppNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APP_NOT_FOUND)
                
        | Error SyncApplicationPrivilegedIntents.Failure.TeamNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.TEAM_NOT_FOUND)

        | Error SyncApplicationPrivilegedIntents.Failure.DifferentBotToken ->
            return!
                req.CreateResponse HttpStatusCode.UnprocessableContent
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)

        | Error SyncApplicationPrivilegedIntents.Failure.UpdateFailed ->
            return!
                req.CreateResponse HttpStatusCode.InternalServerError
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

        | Ok privilegedIntents ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse PrivilegedIntentsResponse.encoder (PrivilegedIntentsResponse.fromDomain privilegedIntents)
    }
    
    [<Function "AddDisabledAppReason">]
    member _.AddDisabledAppReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "apps/{appId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        appId: int64,
        reasonId: int
    ) = task {
        let props: AddDisabledAppReason.Props = {
            AppId = string appId
            DisabledReason = enum<DisabledAppReason> reasonId
        }

        match! AddDisabledAppReason.run env props with
        | Error AddDisabledAppReason.Failure.AppNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APP_NOT_FOUND)

        | Error AddDisabledAppReason.Failure.AddFailed ->
            return!
                req.CreateResponse HttpStatusCode.InternalServerError
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

        | Ok reasons ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse DisabledAppReasonResponse.encoder (DisabledAppReasonResponse.fromDomain reasons)
    }
    
    [<Function "RemoveDisabledAppReason">]
    member _.RemoveDisabledAppReason (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "apps/{appId:long}/disabled-reasons/{reasonId:int}")>] req: HttpRequestData,
        appId: int64,
        reasonId: int
    ) = task {
        let props: RemoveDisabledAppReason.Props = {
            AppId = string appId
            DisabledReason = enum<DisabledAppReason> reasonId
        }
        
        match! RemoveDisabledAppReason.run env props with
        | Error RemoveDisabledAppReason.Failure.AppNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APP_NOT_FOUND)

        | Error RemoveDisabledAppReason.Failure.RemoveFailed ->
            return!
                req.CreateResponse HttpStatusCode.InternalServerError
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

        | Ok reasons ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse DisabledAppReasonResponse.encoder (DisabledAppReasonResponse.fromDomain reasons)
    }
