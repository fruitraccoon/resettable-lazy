module ResettableLazySpec

open Xunit
open FsUnit
open FsUnit.Xunit
open ResettableLazy.Fs

let testResult = 42
let simpleFactory = fun () -> testResult

module ``The value function`` =

    module ``Should return the factory result`` =

        [<Fact>]
        let ``When called once`` () =
            let rl = ResettableLazy.create simpleFactory
            let result = ResettableLazy.value rl |> Async.RunSynchronously
            result |> should equal testResult

        [<Fact>]
        let ``When called twice`` () =
            let rl = ResettableLazy.create simpleFactory
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            let result = ResettableLazy.value rl |> Async.RunSynchronously
            result |> should equal testResult

        [<Fact>]
        let ``When called after the reset function`` () =
            let rl = ResettableLazy.create simpleFactory
            do ResettableLazy.reset rl
            let result = ResettableLazy.value rl |> Async.RunSynchronously
            result |> should equal testResult


    module ``Should cause only a single call of the factory`` =

        [<Fact>]
        let ``When called twice`` () =
            let mutable count = 0
            let countingFactory = fun () -> count <- count + 1; testResult
            let rl = ResettableLazy.create countingFactory
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            count |> should equal 1

        [<Fact>]
        let ``When called by multiple threads`` () =
            let mutable count = 0
            let countingFactory = fun () -> async { count <- count + 1; return () }
            let rl = ResettableLazy.createAsync countingFactory
            Seq.unfold (fun f -> Some (f(),f)) (fun () -> ResettableLazy.value rl)
            |> Seq.take 20
            |> Async.Parallel
            |> Async.RunSynchronously
            |> ignore
            count |> should equal 1

    module ``Should cause two calls of the factory`` =

        [<Fact>]
        let ``When called twice with a call of the reset function in between`` () =
            let mutable count = 0
            let countingFactory = fun () -> count <- count + 1; testResult
            let rl = ResettableLazy.create countingFactory
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            do ResettableLazy.reset rl
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            count |> should equal 2


module ``The isValueCreated function`` =

    module ``Should return false`` =

        [<Fact>]
        let ``When called before the value function`` () =
            let rl = ResettableLazy.create simpleFactory
            let result = ResettableLazy.isValueCreated rl |> Async.RunSynchronously
            result |> should equal false

        [<Fact>]
        let ``When called after the reset function`` () =
            let rl = ResettableLazy.create simpleFactory
            do ResettableLazy.reset rl
            let result = ResettableLazy.isValueCreated rl |> Async.RunSynchronously
            result |> should equal false

        [<Fact>]
        let ``When called after the value and then the reset functions`` () =
            let rl = ResettableLazy.create simpleFactory
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            do ResettableLazy.reset rl
            let result = ResettableLazy.isValueCreated rl |> Async.RunSynchronously
            result |> should equal false


    module ``Should return true`` =

        [<Fact>]
        let ``When called after the value function`` () =
            let rl = ResettableLazy.create simpleFactory
            ResettableLazy.value rl |> Async.RunSynchronously |> ignore
            let result = ResettableLazy.isValueCreated rl |> Async.RunSynchronously
            result |> should equal true
