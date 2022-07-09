module App

open System
open System.Threading
open Fable
open Fable.Core
open Feliz

type RemoteData<'t, 'e> =
  | NotAsked
  | Loading
  | Success of 't
  | Failure of 'e

module Api =

  let tryFetchData (fetchID : Guid) : Async<Result<Guid, Exception>> =
    async {
      for i = 1 to 5 do
        printfn $"[{fetchID}]... {i}"

        do! Async.Sleep (TimeSpan.FromSeconds 1)

      let data = Guid.NewGuid()

      printfn $"[{fetchID}]... Done"

      return Ok data
    }

[<ReactComponent>]
let App() =
  let fetchID, setFetchID = React.useState(Guid.NewGuid())
  let status, setStatus = React.useState(NotAsked)

  let fetchData =
    async {
      setStatus(Loading)

      let! response = Api.tryFetchData fetchID

      match response with
      | Ok data ->
        setStatus (Success data)
      | Error exn ->
        setStatus (Failure exn)
    }

  React.useEffect(
    (fun () ->
      let cts = new CancellationTokenSource()

      Async.StartAsPromise(fetchData, cts.Token)
      |> ignore

      cts :> IDisposable),
    [| box fetchID |]
  )

  Html.div [
    Html.p $"Status: {status}"

    Html.button [
      prop.onClick (fun _ -> setFetchID (Guid.NewGuid()))
      prop.text "Refresh"
    ]

    Html.h1 $"Fetch ID: {fetchID}"
  ]

open Browser.Dom

ReactDOM.render(App(), document.getElementById "root")
