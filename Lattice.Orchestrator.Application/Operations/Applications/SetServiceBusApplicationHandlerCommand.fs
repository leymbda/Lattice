namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain

type SetServiceBusApplicationHandlerCommandProps = {
    ApplicationId: string
    ConnectionString: string
    QueueName: string
}

type SetServiceBusApplicationHandlerCommandError =
    | ApplicationNotFound
    | UpdateFailed

module SetServiceBusApplicationHandlerCommand =
    let run (env: #IPersistence) (props: SetServiceBusApplicationHandlerCommandProps) = task {
        // Get current application from db
        match! env.GetApplicationById props.ApplicationId with
        | Error _ -> return Error SetServiceBusApplicationHandlerCommandError.ApplicationNotFound
        | Ok app ->

        // Add handler to application
        let handler = Handler.SERVICE_BUS (ServiceBusHandler.create props.ConnectionString props.QueueName)
        let updatedApp = app |> Application.setHandler handler

        match! env.UpsertApplication updatedApp with
        | Ok { Handler = Some (Handler.SERVICE_BUS handler) } -> return Ok handler
        | _ -> return Error SetServiceBusApplicationHandlerCommandError.UpdateFailed
    }
