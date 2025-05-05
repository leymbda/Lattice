module Lattice.Web.Pages.Fallback

open Feliz

[<ReactComponent>]
let NotFound () =
    Html.div [
        Html.text "Page not found"
    ]
