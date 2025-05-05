module Lattice.Web.Pages.Index

open Feliz

[<ReactComponent>]
let Page () =
    Html.div [
        Html.text "Index"
        Html.br []
        Html.a [
            prop.href "/applications"
            prop.children [
                Html.text "View your applications"
            ]
        ]
        Html.br []
        Html.a [
            prop.href "/applications/register"
            prop.children [
                Html.text "Register application"
            ]
        ]
    ]
