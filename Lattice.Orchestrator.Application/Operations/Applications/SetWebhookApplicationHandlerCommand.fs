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

        // Add handler to application
        let ed25519 = Ed25519.generate()

        let handler = Handler.WEBHOOK (WebhookHandler.create props.Endpoint ed25519.PublicKey ed25519.PrivateKey)
        let updatedApp = app |> Application.setHandler handler

        match! env.UpsertApplication updatedApp with
        | Ok { Handler = Some (Handler.WEBHOOK handler) } -> return Ok handler
        | _ -> return Error SetWebhookApplicationHandlerCommandError.UpdateFailed
    }
