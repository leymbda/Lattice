namespace Lattice.Orchestrator.Application

type SetWebhookApplicationHandlerCommandProps = {
    ApplicationId: string
    Endpoint: string
}

type SetWebhookApplicationHandlerCommandError =
    | InvalidToken

module SetWebhookApplicationHandlerCommand =
    let run (env) (props: SetWebhookApplicationHandlerCommandProps) = task {
        return Error SetWebhookApplicationHandlerCommandError.InvalidToken
    }
