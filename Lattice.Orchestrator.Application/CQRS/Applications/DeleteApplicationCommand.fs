namespace Lattice.Orchestrator.Application

open MediatR

type DeleteApplicationCommandError =
    | NotImplemented

type DeleteApplicationCommandResponse = Result<unit, DeleteApplicationCommandError>

type DeleteApplicationCommand (
    applicationId: string
) =
    interface IRequest<DeleteApplicationCommandResponse>

    member val ApplicationId = applicationId with get, set

type DeleteApplicationCommandHandler () =
    interface IRequestHandler<DeleteApplicationCommand, DeleteApplicationCommandResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
