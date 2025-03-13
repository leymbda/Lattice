module Lattice.Web.Pages.ApplicationList

open Feliz

[<ReactComponent>]
let Page () =
    Html.div [
        Html.text "Index"
        // All applications associated with the user will be listed here (need to implement api side of saving apps to a user)
        Html.br []
        Html.a [
            prop.href "/applications/register"
            prop.children [
                Html.text "Register application"
            ]
        ]
    ]
