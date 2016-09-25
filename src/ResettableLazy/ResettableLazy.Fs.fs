namespace ResettableLazy.Fs

type private Query<'a> =
    | Reset
    | IsCreated of AsyncReplyChannel<bool>
    | Get of AsyncReplyChannel<'a>

type ResettableLazy<'a> = private ResettableLazy of MailboxProcessor<Query<'a>>

module ResettableLazy =

    let create valueFactory =
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
                         | None -> valueFactory()
                do rc.Reply(v')
                Some v'
        let agent = MailboxProcessor<Query<'a>>.Start(fun inbox ->
            let rec messageLoop oldValue = async {
                let! q = inbox.Receive()
                return! applyQuery oldValue q |> messageLoop
            }
            messageLoop None)
        ResettableLazy agent

    let isValueCreated (ResettableLazy agent) =
        agent.PostAndAsyncReply IsCreated

    let value (ResettableLazy agent) =
        agent.PostAndAsyncReply Get

    let reset (ResettableLazy agent) =
        agent.Post Reset
