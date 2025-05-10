module Lattice.WorkerNode.Sub

open System
open System.Threading
open System.Threading.Tasks

let delay (timespan: TimeSpan) (callback: unit -> unit) =
    use cts = new CancellationTokenSource()

    Task.Delay(timespan, cts.Token)
    |> _.ContinueWith(fun _ -> callback())
    |> ignore

    { new IDisposable with
        member _.Dispose () = cts.Cancel() }
