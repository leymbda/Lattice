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
            | [] -> Index.Page()

            // Application pages
            | ["applications"] -> ApplicationList.Page()
            | ["applications"; "register"] -> ApplicationRegistration.Page()
            | ["applications"; Route.Int64 applicationId; "overview"] -> ApplicationOverview.Page (string applicationId)
            | ["applications"; Route.Int64 applicationId; "settings"] -> ApplicationSettings.Page (string applicationId)

            // Fallback pages
            | _ -> Fallback.NotFound()
        ]
    ]

[<ReactComponent>]
let App () =
    // Providers listed in order of wrapping (outermost to innermost)
    let providers = []

    // Fold providers into router to render
    providers
    |> List.rev
    |> List.fold (fun acc provider -> provider [ acc ]) (Router ())

ReactDOM
    .createRoot(document.getElementById "root")
    .render(App())
