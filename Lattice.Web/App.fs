module Lattice.Web

open Browser.Dom
open Feliz

[<ReactComponent>]
let App () = React.fragment [
    Html.div [
        Html.text "Hello world"
    ]
]

ReactDOM
    .createRoot(document.getElementById "root")
    .render(App())