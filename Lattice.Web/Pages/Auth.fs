module Lattice.Web.Pages.Auth

open Browser.Dom
open Feliz
open Lattice.Web
open System.Security.Cryptography
open System.Web

[<ReactComponent>]
let Login () =
    let _, dispatch = React.useContext StateContext.context

    React.useEffect((fun () ->
        // Create state for CSRF protection
        let state = RandomNumberGenerator.GetHexString 16
        StateContext.Msg.Set state |> dispatch

        // Redirect to oauth authorization page
        let authorizeUrl (clientId: string) (redirectUri: string) (scopes: string list) =
            $"""https://discord.com/oauth2/authorize?client_id={clientId}&response_type=code&redirect_uri={HttpUtility.UrlEncode redirectUri}&scope={scopes |> String.concat "+"}"""
            // TODO: Implement this kind of function in FSharp.Discord then use here
            // TODO: Figure out how to use environment variables to set client ID and redirect URI

        window.location.href <- authorizeUrl "1169979466303418368" "http://localhost:4280/auth/login" ["identify"]
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
