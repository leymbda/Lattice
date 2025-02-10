module Lattice.Web.Pages.NotFound

open Feliz

[<ReactComponent>]
let NotFound () =
    Html.div [
        Html.text "Page not found"
    ]
