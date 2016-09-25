namespace ResettableLazy.Fs

type private Query<'a> =
    | Reset
    | IsCreated of AsyncReplyChannel<bool>
    | Get of AsyncReplyChannel<Async<'a>>

type ResettableLazy<'a> = private ResettableLazy of MailboxProcessor<Query<'a>>

module ResettableLazy =

    /// Creates an asynchronous workflow that runs the asynchronous workflow given as an argument at most once.
    /// When the returned workflow is started for the second time, it reuses the result of the previous execution.
    /// From: https://github.com/fsprojects/FSharpx.Async/blob/b3a8c41813f04862a9ea1935a051c5ed65fa20d4/src/FSharpx.Async/Async.fs#L67
    let private asyncCache (input:Async<'T>) =
        let agent = MailboxProcessor<AsyncReplyChannel<_>>.Start(fun agent -> async {
            let! repl = agent.Receive()
            let! res = input
            repl.Reply(res)
            while true do
                let! repl = agent.Receive()
                repl.Reply(res) })
        agent.PostAndAsyncReply(id)

    let createAsync valueFactory =
        let applyQuery value query =
            match query with
            | Reset ->
                None
            | IsCreated rc ->
                do rc.Reply(Option.isSome value)
                value
            | Get rc ->
                let v' = match value with
                         | Some v -> v
                         | None -> valueFactory() |> asyncCache
                do rc.Reply(v')
                Some v'
        let agent = MailboxProcessor<Query<'a>>.Start(fun inbox ->
            let rec messageLoop oldValue = async {
                let! q = inbox.Receive()
                return! applyQuery oldValue q |> messageLoop
            }
            messageLoop None)
        ResettableLazy agent

    let create valueFactory =
        createAsync (valueFactory >> async.Return)

    let isValueCreated (ResettableLazy agent) =
        agent.PostAndAsyncReply IsCreated

    let value (ResettableLazy agent) =
        async.Bind(agent.PostAndAsyncReply Get, id)

    let reset (ResettableLazy agent) =
        agent.Post Reset
