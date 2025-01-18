namespace Lattice.Orchestrator.Application

type SetServiceBusApplicationHandlerCommandProps = {
    ApplicationId: string
    ConnectionString: string
    QueueName: string
}

type SetServiceBusApplicationHandlerCommandError =
    | InvalidToken

module SetServiceBusApplicationHandlerCommand =
    let run (env) (props: SetServiceBusApplicationHandlerCommandProps) = task {
        return Error SetServiceBusApplicationHandlerCommandError.InvalidToken
    }
