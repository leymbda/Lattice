namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open MediatR

type GetApplicationsQueryError =
    | NotImplemented

type GetApplicationsQueryResponse = Result<Application, GetApplicationsQueryError>

type GetApplicationsQuery (
    before: string option,
    after: string option,
    limit: int option
) =
    interface IRequest<GetApplicationsQueryResponse>

    member val Before = before with get, set
    member val After = after with get, set
    member val Limit = Option.defaultValue 100 limit with get, set

type GetApplicationsQueryHandler () =
    interface IRequestHandler<GetApplicationsQuery, GetApplicationsQueryResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
