module Lattice.Web.Pages.ApplicationOverview

open Feliz

[<ReactComponent>]
let Page (applicationId: string) =
    Html.div [
        Html.text "Application overview"
        Html.br []
        Html.a [
            prop.href $"/applications"
            prop.children [
                Html.text "Return to application list"
            ]
        ]
        Html.br []
        Html.a [
            prop.href $"/applications/{applicationId}/settings"
            prop.children [
                Html.text "Change settings"
            ]
        ]
    ]
