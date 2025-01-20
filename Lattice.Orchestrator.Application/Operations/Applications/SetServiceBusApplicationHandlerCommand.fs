namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type SetServiceBusApplicationHandlerCommandProps = {
    ApplicationId: string
    ConnectionString: string
    QueueName: string
}

type SetServiceBusApplicationHandlerCommandError =
    | ApplicationNotFound
    | ApplicationNotActivated
    | UpdateFailed

module SetServiceBusApplicationHandlerCommand =
    let run (env: #IPersistence) (props: SetServiceBusApplicationHandlerCommandProps) = task {
        // Get current application from db and ensure it is activated
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error SetServiceBusApplicationHandlerCommandError.ApplicationNotFound
        | Ok (Application.REGISTERED _) -> return Error SetServiceBusApplicationHandlerCommandError.ApplicationNotActivated
        | Ok (Application.ACTIVATED app) ->

        // Add handler to application
        let updatedApp =
            app
            |> ActivatedApplication.setHandler (Handler.SERVICE_BUS (ServiceBusHandler.create props.ConnectionString props.QueueName))
            |> Application.ACTIVATED

        match! env.UpsertApplication updatedApp with
        | Ok (Application.ACTIVATED { Handler = Some (Handler.SERVICE_BUS handler) }) -> return Ok handler
        | _ -> return Error SetServiceBusApplicationHandlerCommandError.UpdateFailed
    }
