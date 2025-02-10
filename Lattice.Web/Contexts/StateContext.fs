[<RequireQualifiedAccess>]
module Lattice.Web.StateContext

open Browser.Dom
open Elmish
open Feliz
open Feliz.UseElmish

type Msg =
    | Set of string
    | Clear

type Model = { State: string option }

let private init () =
    { State = None }, Cmd.none

let private update msg model =
    match msg with
    | Msg.Set v -> { model with State = Some v }, Cmd.none
    | Msg.Clear -> { model with State = None }, Cmd.none

let context = React.createContext()

[<ReactComponent>]
let Provider (children: ReactElement seq) =
    let state, dispatch = React.useElmish(init, update, [||])

    React.useEffect ((fun () ->
        let key = "state"

        match state with
        | { State = Some s } -> window.sessionStorage.setItem(key, s)
        | { State = None } -> window.sessionStorage.removeItem key
    ), [| box state |])

    React.contextProvider(context, (state, dispatch), children)
