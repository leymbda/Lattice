namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type SetWebhookApplicationHandlerCommandProps = {
    ApplicationId: string
    Endpoint: string
}

type SetWebhookApplicationHandlerCommandError =
    | ApplicationNotFound
    | ApplicationNotActivated
    | UpdateFailed

module SetWebhookApplicationHandlerCommand =
    let run (env: #IPersistence) (props: SetWebhookApplicationHandlerCommandProps) = task {
        // Get current application from db and ensure it is activated
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error SetWebhookApplicationHandlerCommandError.ApplicationNotFound
        | Ok (Application.REGISTERED _) -> return Error SetWebhookApplicationHandlerCommandError.ApplicationNotActivated
        | Ok (Application.ACTIVATED app) ->

        // Create ed25519 key pair
        let publicKey = ""
        let privateKey = ""

        // TODO: Implement actual ed25519 key pair generation
        
        // Add handler to application
        let updatedApp =
            app
            |> ActivatedApplication.setHandler (Handler.WEBHOOK (WebhookHandler.create props.Endpoint publicKey privateKey))
            |> Application.ACTIVATED

        match! env.UpsertApplication updatedApp with
        | Ok (Application.ACTIVATED { Handler = Some (Handler.WEBHOOK handler) }) -> return Ok handler
        | _ -> return Error SetWebhookApplicationHandlerCommandError.UpdateFailed
    }
