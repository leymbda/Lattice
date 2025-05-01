namespace Lattice.Orchestrator.Application

open FSharp.Discord.Gateway
open Lattice.Orchestrator.Domain
open System
open System.Threading.Tasks

type IEvents =
    abstract ShardInstanceScheduleStart: nodeId: Guid * shardId: ShardId * token: string * intents: int * startAt: DateTime -> Task
    abstract ShardInstanceScheduleClose: nodeId: Guid * shardId: ShardId * closeAt: DateTime -> Task
    abstract ShardInstanceGatewayEvent: nodeId: Guid * shardId: ShardId * event: GatewaySendEvent -> Task
