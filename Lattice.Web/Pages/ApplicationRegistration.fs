module Lattice.Web.Pages.ApplicationRegistration

open Feliz

[<ReactComponent>]
let Page () =
    Html.div [
        Html.text "Register application"
        Html.br []
        Html.input [
            prop.type' "text"
            prop.placeholder "Bot token"
        ]
        Html.br []
        Html.a [ // This will actually be a form submit button
            prop.href "/applications/1234567890/overview"
            prop.children [
                Html.text "Register"
            ]
        ]
    ]
