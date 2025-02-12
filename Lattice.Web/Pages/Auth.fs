module Lattice.Web.Pages.Auth

open Browser.Dom
open Feliz
open FSharp.Discord.Types
open FSharp.Discord.Utils
open Lattice.Web
open System.Net.Http
open System.Security.Cryptography

let [<Literal>] CLIENT_ID = "1169979466303418368" // TODO: Set from configuration
let [<Literal>] REDIRECT_URI = "http://localhost:4280/auth/login" // TODO: Set from configuration

[<ReactComponent>]
let Login () =
    let _, dispatch = React.useContext StateContext.context

    React.useEffect((fun () ->
        // Create state for CSRF protection
        let state = RandomNumberGenerator.GetHexString 16
        StateContext.Msg.Set state |> dispatch

        // Redirect to oauth authorization page
        window.location.href <- OAuth.authorizationUrl CLIENT_ID REDIRECT_URI [OAuth2Scope.IDENTIFY] (Some state) OAuthConsent.None None
    ), [||])

    Html.div [
        Html.text "Redirecting..."
    ]
    
[<ReactComponent>]
let Callback code state =
    let savedState, dispatch = React.useContext StateContext.context

    let loading, setLoading = React.useState true
    let error, setError = React.useState Option<int>.None

    React.useEffect((fun () -> task {
        match state, savedState.State with
        | state, Some saved when state = saved ->
            StateContext.Msg.Clear |> dispatch

            use client = new HttpClient()

            match! Api.login code REDIRECT_URI client with
            | Error status ->
                setError (Some (int status))

            | Ok res ->
                res.Token |> ignore // TODO: Add to local storage as user context (should the api also return info about the user?)
        | _ -> ()

        setLoading false
    }), [||])

    let message =
        match loading, error with
        | true, _ -> "Loading..."
        | false, Some error -> $"Invalid request: Error {error}"
        | false, None -> "Success!"

    Html.div [
        Html.text message
    ]
