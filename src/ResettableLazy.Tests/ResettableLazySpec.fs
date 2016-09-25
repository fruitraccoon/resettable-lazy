module ResettableLazySpec

open Xunit
open FsUnit
open FsUnit.Xunit
open ResettableLazy.Fs

module ``The value function`` =

    let testResult = 42
    let factory = fun () -> testResult

    [<Fact>]
    let ``Should return the factory result`` () =
        let rl = ResettableLazy.create factory
        let result = ResettableLazy.value rl |> Async.RunSynchronously
        result |> should equal testResult
