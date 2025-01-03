namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open MediatR

type UpdateApplicationCommandError =
    | NotImplemented

type UpdateApplicationCommandResponse = Result<Application, UpdateApplicationCommandError>

type UpdateApplicationCommand (
    applicationId: string,
    discordBotToken: string option,
    intents: int option,
    shardCount: int option,
    disabledReasons: int option
) =
    interface IRequest<UpdateApplicationCommandResponse>

    member val ApplicationId = applicationId with get, set
    member val DiscordBotToken = discordBotToken with get, set
    member val Intents = intents with get, set
    member val ShardCount = shardCount with get, set
    member val DisabledReasons = disabledReasons with get, set

type UpdateApplicationCommandHandler () =
    interface IRequestHandler<UpdateApplicationCommand, UpdateApplicationCommandResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
