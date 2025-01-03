namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open MediatR

type SetServiceBusApplicationHandlerCommandError =
    | NotImplemented

type SetServiceBusApplicationHandlerCommandResponse = Result<Handler, SetServiceBusApplicationHandlerCommandError>

type SetServiceBusApplicationHandlerCommand (
    applicationId: string,
    connectionString: string,
    queueName: string
) =
    interface IRequest<SetServiceBusApplicationHandlerCommandResponse>

    member val ApplicationId = applicationId with get, set
    member val ConnectionString = connectionString with get, set
    member val QueueName = queueName with get, set

type SetServiceBusApplicationHandlerCommandHandler () =
    interface IRequestHandler<SetServiceBusApplicationHandlerCommand, SetServiceBusApplicationHandlerCommandResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
