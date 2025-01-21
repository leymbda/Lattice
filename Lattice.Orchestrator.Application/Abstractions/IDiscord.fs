namespace Lattice.Orchestrator.Application

open System.Threading.Tasks

type IDiscordApplication =
    abstract Id: string
    abstract HasPresenceIntent: bool
    abstract HasPresenceLimitedIntent: bool
    abstract HasGuildMembersIntent: bool
    abstract HasGuildMembersLimitedIntent: bool
    abstract HasMessageContentIntent: bool
    abstract HasMessageContentLimitedIntent: bool

type IDiscord =
    abstract GetApplicationInformation: token: string -> Task<IDiscordApplication option>
