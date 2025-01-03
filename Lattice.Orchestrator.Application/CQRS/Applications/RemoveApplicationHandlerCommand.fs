namespace Lattice.Orchestrator.Application

open MediatR

type RemoveApplicationHandlerCommandError =
    | NotImplemented

type RemoveApplicationHandlerCommandResponse = Result<unit, RemoveApplicationHandlerCommandError>

type RemoveApplicationHandlerCommand (
    applicationId: string
) =
    interface IRequest<RemoveApplicationHandlerCommandResponse>

    member val ApplicationId = applicationId with get, set

type RemoveApplicationHandlerCommandHandler () =
    interface IRequestHandler<RemoveApplicationHandlerCommand, RemoveApplicationHandlerCommandResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
