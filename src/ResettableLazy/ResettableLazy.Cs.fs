namespace ResettableLazy.Cs

open ResettableLazy

type ResettableLazy<'T>(valueFactory) =

    let rl = Fs.ResettableLazy.create valueFactory

    member x.IsValueCreated =
        Fs.ResettableLazy.isValueCreated rl |> Async.StartAsTask

    member x.Value =
        Fs.ResettableLazy.value rl |> Async.StartAsTask

    member x.Reset () =
        Fs.ResettableLazy.reset rl
