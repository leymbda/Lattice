namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open MediatR

type SetWebhookApplicationHandlerCommandError =
    | NotImplemented

type SetWebhookApplicationHandlerCommandResponse = Result<Handler, SetWebhookApplicationHandlerCommandError>

type SetWebhookApplicationHandlerCommand (
    applicationId: string,
    endpoint: string
) =
    interface IRequest<SetWebhookApplicationHandlerCommandResponse>

    member val ApplicationId = applicationId with get, set
    member val Endpoint = endpoint with get, set

type SetWebhookApplicationHandlerCommandHandler () =
    interface IRequestHandler<SetWebhookApplicationHandlerCommand, SetWebhookApplicationHandlerCommandResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
