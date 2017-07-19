// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.Net

open Nancy
open Nancy.Hosting

let (?) (parameters:obj) param =
    (parameters :?> Nancy.DynamicDictionary).[param]

let readBodyAsString (requestStream:IO.RequestStream) = 
    use reader = new IO.StreamReader(requestStream)
    reader.ReadToEnd()

let composeGetResponse requestId =
    sprintf "Get recived on %O with id: %O" (DateTime.Now.ToString()) requestId

let composePostResponse requestId body =
     sprintf "Id: \"%O\", Body: \"%O\" " requestId body

let logGet requestId= 
    printfn "[%A] GET -> /Get/%s\n" (DateTime.Now.ToString()) (requestId.ToString())

let logPost requestId headers body =
    printfn "[%A] POST -> /Post/%s {\n\tBody: {%s}\n\tHeaders: {%A}\n\t}\n" (DateTime.Now.ToString()) (requestId.ToString()) body headers

type App() as this =
    inherit NancyModule()
    do
        this.Get.["/"] <- fun _ -> this.Response.AsJson(["Ok"]) :> obj

        this.Get.["/Get/{id}"] <- fun parameters -> 
            let requestId = (parameters :?> Nancy.DynamicDictionary).["id"]
            let response = this.Response.AsJson([composeGetResponse requestId])

            logGet requestId

            response.ContentType <- "application/json"
            response :> obj

        this.Post.["/Post/{id}"] <- fun parameters -> 
            let requestId = (parameters :?> Nancy.DynamicDictionary).["id"]
            let headers = this.Request.Headers.Values
            let body = readBodyAsString(this.Request.Body)
            let response = this.Response.AsJson([composePostResponse requestId body])

            logPost requestId headers body

            response :> obj

[<EntryPoint>]
let main argv = 
    let nancy = new Nancy.Hosting.Self.NancyHost(new Uri("http://localhost:" + "8100"))
    nancy.Start()
    while true do Console.ReadLine() |> ignore
    0
