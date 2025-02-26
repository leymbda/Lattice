module Lattice.Orchestrator.Presentation.FunctionContext

open Microsoft.Azure.Functions.Worker
open System

let getCustomAttribute<'a> (ctx: FunctionContext) =
    let entrypoint = ctx.FunctionDefinition.EntryPoint
    let lastIndex = entrypoint.LastIndexOf '.'
    let typeName = entrypoint[..lastIndex]
    let methodName = entrypoint.[lastIndex + 1..]

    try
        Type
            .GetType(typeName)
            .GetMethod(methodName)
            .GetCustomAttributes(typeof<'a>, false)
        |> Seq.cast<'a>
        |> Seq.tryHead

    with | _ ->
        None
