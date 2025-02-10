module Lattice.Web.App

open Browser.Dom
open Feliz
open Feliz.Router
open Lattice.Web.Pages

[<ReactComponent>]
let Router () =
    let currentUrl, updateUrl = React.useState (Router.currentPath())

    React.router [
        router.pathMode
        router.onUrlChanged updateUrl
        router.children [
            match currentUrl with
            // Main content pages
            | [] -> Index.Index()

            // Auth pages
            | "auth" :: "login" :: rest ->
                match rest with
                | [ Route.Query ["code", code; "state", state] ] -> Auth.Callback code state
                | _ -> Auth.Login ()

            // Fallback pages
            | _ -> Fallback.NotFound()
        ]
    ]

[<ReactComponent>]
let App () =
    // TODO: Initialise contexts here

    Router()

ReactDOM
    .createRoot(document.getElementById "root")
    .render(App())
