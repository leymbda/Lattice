namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open MediatR

type GetApplicationQueryError =
    | NotImplemented

type GetApplicationQueryResponse = Result<Application, GetApplicationQueryError>

type GetApplicationQuery (
    applicationId: string
) =
    interface IRequest<GetApplicationQueryResponse>

    member val ApplicationId = applicationId with get, set

type GetApplicationQueryHandler () =
    interface IRequestHandler<GetApplicationQuery, GetApplicationQueryResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
