namespace Lattice.Orchestrator.Application

open System.Threading.Tasks

type IDiscordUser =
    abstract Id: string
    abstract Username: string

type IDiscordApplication =
    abstract Id: string
    abstract HasPresenceIntent: bool
    abstract HasPresenceLimitedIntent: bool
    abstract HasGuildMembersIntent: bool
    abstract HasGuildMembersLimitedIntent: bool
    abstract HasMessageContentIntent: bool
    abstract HasMessageContentLimitedIntent: bool

type IDiscord =
    abstract GetUserInformation: accessToken: string -> Task<IDiscordUser option>
    abstract GetApplicationInformation: botToken: string -> Task<IDiscordApplication option>
