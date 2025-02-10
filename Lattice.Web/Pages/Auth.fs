module Lattice.Web.Pages.Auth

open Browser.Dom
open Feliz

let [<Literal>] STATE_KEY = "state"

[<ReactComponent>]
let Login () =
    React.useEffect((fun () ->
        let state = "" // TODO: Generate random secure state here

        window.sessionStorage.setItem(STATE_KEY, state)
        // window.location.href <- "https://google.com" // TODO: Redirect to discord application

        // TODO: Create react context for state to abstract away session storage
    ), [||])

    Html.div [
        Html.text "Redirecting..."
    ]
    
[<ReactComponent>]
let Callback (code: string) (state: string) =
    let (loading, setLoading) = React.useState true
    let (success, setSuccess) = React.useState false

    React.useEffect((fun () ->
        let sessionState = window.sessionStorage.getItem STATE_KEY

        match state with
        | state when state = sessionState && code <> "" ->
            window.sessionStorage.removeItem STATE_KEY

            // TODO: Send code to server to exchange for token

        | _ ->
            setSuccess false

        setLoading false
    ), [||])

    let text =
        if loading then
            "Loading..."
        elif not success then
            "Invalid request"
        else
            "Success!"

    Html.div [
        Html.text text
    ]
