namespace Lattice.Orchestrator.Contracts

open Thoth.Json.Net

type NegotiateResponse = {
    BaseUrl: string
    AccessToken: string
    Url: string
}

module NegotiateResponse =
    module Property =
        let [<Literal>] BaseUrl = "baseUrl"
        let [<Literal>] AccessToken = "AccessToken"
        let [<Literal>] Url = "url"

    let decoder: Decoder<NegotiateResponse> =
        Decode.object (fun get -> {
            BaseUrl = get.Required.Field Property.BaseUrl Decode.string
            AccessToken = get.Required.Field Property.AccessToken Decode.string
            Url = get.Required.Field Property.Url Decode.string
        })

    let encoder (v: NegotiateResponse) =
        Encode.object [
            Property.BaseUrl, Encode.string v.BaseUrl
            Property.AccessToken, Encode.string v.AccessToken
            Property.Url, Encode.string v.Url
        ]
