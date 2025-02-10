module Lattice.Web.Pages.Auth

open Browser.Dom
open Feliz
open Lattice.Web

[<ReactComponent>]
let Login () =
    let _, dispatch = React.useContext StateContext.context

    React.useEffect((fun () ->
        let state = "" // TODO: Generate random secure state here
        StateContext.Msg.Set state |> dispatch

        window.location.href <- "https://discord.com/oauth2/authorize?client_id=1169979466303418368&response_type=code&redirect_uri=http%3A%2F%2Flocalhost%3A4280%2Fauth%2Flogin&scope=identify"

        // TODO: Make redirect URL a configuration value (also create & use builder function in FSharp.Discord.Utils?)
    ), [||])

    Html.div [
        Html.text "Redirecting..."
    ]
    
[<ReactComponent>]
let Callback code state =
    let savedState, dispatch = React.useContext StateContext.context

    let loading, setLoading = React.useState true
    let success, setSuccess = React.useState false

    React.useEffect((fun () ->
        match state, savedState.State with
        | state, Some saved when state = saved ->
            StateContext.Msg.Clear |> dispatch

            // TODO: Create api client to handle interacting with lattice orchestrator
            // TODO: Send code to server to exchange for token (then store it in local storage, abstracted away with react context)

            setSuccess true
        | _ -> ()

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
