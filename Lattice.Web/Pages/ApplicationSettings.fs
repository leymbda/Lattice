module Lattice.Web.Pages.ApplicationSettings

open Feliz

[<ReactComponent>]
let Page (applicationId: string) =
    Html.div [
        Html.text "Application settings"
        Html.br []
        Html.input [ // This will be changed to select specific intents or types of dispatch events to receive
            prop.type' "number"
            prop.placeholder "Intents"
        ]
        Html.br []
        Html.input [
            prop.type' "number"
            prop.placeholder "Max shards"
        ]
        Html.br []
        // Need to also add way to configure handler (webhook/service-bus/etc)
        Html.input [
            prop.type' "button"
            prop.value "Sync privileged intents"
        ]
        Html.br []
        Html.input [
            prop.type' "button"
            prop.value "Delete application"
        ]
        Html.br []
        // Should admin actions (e.g. setting disabled reasons) go on this page or a separate admin panel?
        Html.a [
            prop.href $"/applications/{applicationId}/overview"
            prop.children [
                Html.text "Return to overview"
            ]
        ]
    ]
