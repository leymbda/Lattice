namespace Lattice.Orchestrator.Application

// TODO: Should these be converted into an injectible service with abstraction? Maybe JWT factory as well?

module Aes =
    let encrypt (key: string) (value: string) =
        ""

    let decrypt (key: string) (value: string) =
        ""

    // TODO: Implement following https://propertyguru.tech/doing-aes-encryption-correct-in-your-net-application-5d66168b5b44

module Ed25519 =
    let generate () =
        {| PublicKey = ""; PrivateKey = "" |}

    // TODO: Implement using https://github.com/XeroXP/TweetNaclSharp
