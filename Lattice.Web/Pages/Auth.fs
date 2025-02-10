module Lattice.Web.Pages.Auth

open Browser.Dom
open Feliz

let [<Literal>] STATE_KEY = "state"

[<ReactComponent>]
let Login () =
    React.useEffect((fun () ->
        let state = "" // TODO: Generate random secure state here

        window.sessionStorage.setItem(STATE_KEY, state)
        window.location.href <- "https://discord.com/oauth2/authorize?client_id=1169979466303418368&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A4280%2Fauth%2Flogin&scope=identify"

        // TODO: Make redirect URL a configuration value (also create & use builder function in FSharp.Discord.Utils?)
        // TODO: Create react context for state to abstract away session storage
    ), [||])

    Html.div [
        Html.text "Redirecting..."
    ]
    
[<ReactComponent>]
let Callback code state =
    let loading, setLoading = React.useState true
    let success, setSuccess = React.useState false

    React.useEffect((fun () ->
        let sessionState = window.sessionStorage.getItem STATE_KEY

        if state = sessionState then
            window.sessionStorage.removeItem STATE_KEY

            // TODO: Create api client to handle interacting with lattice orchestrator
            // TODO: Send code to server to exchange for token (then store it in local storage, abstracted away with react context)

            setSuccess true

        setLoading false
    ), [||])

    let message =
        match loading, success with
        | true, _ -> "Loading..."
        | false, false -> "Invalid request"
        | false, true -> "Success!"

    Html.div [
        Html.text message
    ]
