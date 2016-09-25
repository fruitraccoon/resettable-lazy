namespace ResettableLazy.Cs

open System
open System.Threading.Tasks
open ResettableLazy

type ResettableLazy<'T> private (valueFactory : unit -> Task<'T>, ignore) =

    let rl = Fs.ResettableLazy.createAsync (valueFactory >> Async.AwaitTask)

    new (valueFactory : Func<'T>) =
        let factory = fun () -> valueFactory.Invoke() |> Task.FromResult
        ResettableLazy(factory, ())

    new (valueFactory : Func<Task<'T>>) =
        let factory = fun () -> valueFactory.Invoke()
        ResettableLazy(factory, ())

    member x.IsValueCreated =
        Fs.ResettableLazy.isValueCreated rl |> Async.StartAsTask

    member x.Value =
        Fs.ResettableLazy.value rl |> Async.StartAsTask

    member x.Reset () =
        Fs.ResettableLazy.reset rl


[<AbstractClass; Sealed>]
type ResettableLazy private () =

    static member Create<'T>(valueFactory : Func<'T>) =
       new ResettableLazy<'T>(valueFactory)

    static member Create<'T>(valueFactory : Func<Task<'T>>) =
       new ResettableLazy<'T>(valueFactory)
