module Lattice.Orchestrator.Presentation.NameValueCollection

open System.Collections.Specialized

let tryGetValue (key: string) (collection: NameValueCollection) =
    let nullableList = collection.GetValues key

    match isNull nullableList with
    | false -> Seq.tryHead nullableList
    | true -> None
