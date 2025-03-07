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

        match Decode.fromString RegisterApplicationPayload.decoder json with
        | Error message ->
            return!
                req.CreateResponse HttpStatusCode.BadRequest
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromSerializationError message)

        | Ok payload ->
            let props: RegisterApplicationCommandProps = {
                DiscordBotToken = payload.DiscordBotToken
            }

            match! RegisterApplicationCommand.run env props with
            | Error RegisterApplicationCommandError.InvalidToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

            | Error RegisterApplicationCommandError.RegistrationFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok application ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse ApplicationResponse.encoder (ApplicationResponse.fromDomain application)
    }
    
    [<Function "GetApplication">]
    member _.GetApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "applications/{applicationId:long}")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let props: GetApplicationQueryProps = {
            ApplicationId = string applicationId
        }

        match! GetApplicationQuery.run env props with
        | Error GetApplicationQueryError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)

        | Ok application ->
            return!
                req.CreateResponse HttpStatusCode.OK
                |> HttpResponseData.withResponse ApplicationResponse.encoder (ApplicationResponse.fromDomain application)
    }
    
    [<Function "UpdateApplication">]
    member _.UpdateApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "applications/{applicationId:long}")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let! json = req.ReadAsStringAsync()

        match Decode.fromString UpdateApplicationPayload.decoder json with
        | Error message ->
            return!
                req.CreateResponse HttpStatusCode.BadRequest
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromSerializationError message)

        | Ok payload ->
            let props: UpdateApplicationCommandProps = {
                ApplicationId = string applicationId
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
            | Error UpdateApplicationCommandError.ApplicationNotFound ->
                return!
                    req.CreateResponse HttpStatusCode.NotFound
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)

            | Error UpdateApplicationCommandError.InvalidToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

            | Error UpdateApplicationCommandError.DifferentBotToken ->
                return!
                    req.CreateResponse HttpStatusCode.UnprocessableContent
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.DIFFERENT_BOT_TOKEN)

            | Error UpdateApplicationCommandError.UpdateFailed ->
                return!
                    req.CreateResponse HttpStatusCode.InternalServerError
                    |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INTERNAL_SERVER_ERROR)

            | Ok application ->
                return!
                    req.CreateResponse HttpStatusCode.OK
                    |> HttpResponseData.withResponse ApplicationResponse.encoder (ApplicationResponse.fromDomain application)
    }
    
    [<Function "DeleteApplication">]
    member _.DeleteApplication (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "applications/{applicationId:long}")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let props: DeleteApplicationCommandProps = {
            ApplicationId = string applicationId
        }

        match! DeleteApplicationCommand.run env props with
        | Error DeleteApplicationCommandError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)

        | _ ->
            return req.CreateResponse HttpStatusCode.NoContent
    }
    
    [<Function "SyncApplicationPrivilegedIntents">]
    member _.SyncApplicationPrivilegedIntents (
        [<HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "applications/{applicationId:long}/sync-privileged-intents")>] req: HttpRequestData,
        applicationId: int64
    ) = task {
        let props: SyncApplicationPrivilegedIntentsCommandProps = {
            ApplicationId = string applicationId
        }

        match! SyncApplicationPrivilegedIntentsCommand.run env props with
        | Error SyncApplicationPrivilegedIntentsCommandError.ApplicationNotFound ->
            return!
                req.CreateResponse HttpStatusCode.NotFound
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.APPLICATION_NOT_FOUND)

        | Error SyncApplicationPrivilegedIntentsCommandError.InvalidToken ->
            return!
                req.CreateResponse HttpStatusCode.UnprocessableContent
                |> HttpResponseData.withErrorResponse (ErrorResponse.fromCode ErrorCode.INVALID_TOKEN)

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
