module Lattice.Web

open Browser.Dom
open Feliz
open Feliz.Router

[<ReactComponent>]
let Router () =
    let (currentUrl, updateUrl) = React.useState (Router.currentPath())

    React.router [
        router.pathMode
        router.onUrlChanged updateUrl
        router.children [
            match currentUrl with
            | [] -> Html.text "Hello world"
            | _ -> Html.text "Not Found"
        ]
    ]


ReactDOM
    .createRoot(document.getElementById "root")
    .render(Router())
