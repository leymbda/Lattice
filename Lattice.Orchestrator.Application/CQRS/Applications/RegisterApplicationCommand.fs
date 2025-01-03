namespace Lattice.Orchestrator.Application

open Lattice.Orchestrator.Domain
open MediatR

type RegisterApplicationCommandError =
    | NotImplemented

type RegisterApplicationCommandResponse = Result<Application, RegisterApplicationCommandError>

type RegisterApplicationCommand (
    discordBotToken: string
) =
    interface IRequest<RegisterApplicationCommandResponse>

    member val DiscordBotToken = discordBotToken with get, set

type RegisterApplicationCommandHandler () =
    interface IRequestHandler<RegisterApplicationCommand, RegisterApplicationCommandResponse> with
        member _.Handle (req, ct) = task {
            return Error NotImplemented
        }
