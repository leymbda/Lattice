module Lattice.Orchestrator.Infrastructure.Pool.WebPubSubAction

open Microsoft.Azure.Functions.Worker
open System

let sendToUser (userId: string) (message: string) =
    let action = SendToUserAction(DataType = WebPubSubDataType.Text, UserId = userId)
    action.Data <- message |> BinaryData
    action :> WebPubSubAction
