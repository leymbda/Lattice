module Lattice.Web.App

open Browser.Dom
open Feliz
open Feliz.Router
open Lattice.Web.Pages

[<ReactComponent>]
let Router () =
    let (currentUrl, updateUrl) = React.useState (Router.currentPath())

    React.router [
        router.pathMode
        router.onUrlChanged updateUrl
        router.children [
            match currentUrl with
            // Home page
            | [] -> Index.Index()

            // Authentication
            | ["auth"; "login"; Route.Query ["code", code; "state", state]] -> Auth.Callback code state
            | ["auth"; "login"]
            | ["auth"; "login"; _]-> Auth.Login()

            // Fallback
            | _ -> NotFound.NotFound()
        ]
    ]


ReactDOM
    .createRoot(document.getElementById "root")
    .render(Router())
