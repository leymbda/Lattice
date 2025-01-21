namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type SetWebhookApplicationHandlerCommandProps = {
    ApplicationId: string
    Endpoint: string
}

type SetWebhookApplicationHandlerCommandError =
    | ApplicationNotFound
    | UpdateFailed

module SetWebhookApplicationHandlerCommand =
    let run (env: #IPersistence) (props: SetWebhookApplicationHandlerCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error SetWebhookApplicationHandlerCommandError.ApplicationNotFound
        | Ok app ->

        // Create ed25519 key pair
        let publicKey = ""
        let privateKey = ""

        // TODO: Implement actual ed25519 key pair generation
        
        // Add handler to application
        let handler = Handler.WEBHOOK (WebhookHandler.create props.Endpoint publicKey privateKey)
        let updatedApp = app |> Application.setHandler handler

        match! env.UpsertApplication updatedApp with
        | Ok { Handler = Some (Handler.WEBHOOK handler) } -> return Ok handler
        | _ -> return Error SetWebhookApplicationHandlerCommandError.UpdateFailed
    }
