// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace FabApp

open System.Diagnostics
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms

module App = 
    type Player =
        X | O | None
    type Model = 
      { Count : int
        Step : int
        TimerOn: bool
        IsX : bool
        Board : Player list list
        Winner : Player}

    type Msg = 
        | Increment 
        | Decrement 
        | Reset
        | SetStep of int
        | TimerToggled of bool
        | TimedTick
        | PlacePiece of int * int
        

    let initModel = { Count = 0; Step = 1; TimerOn=false; IsX=true; Board = [for r in 0..2 -> [for c in 0..2 -> None]]; Winner = None}

    let init () = initModel, Cmd.none

    let timerCmd = 
        async { do! Async.Sleep 200
                return TimedTick }
        |> Cmd.ofAsyncMsg
    let checkForWinnder model r c =
        let check = [for i in model.Board do
                        for j in i -> if j = X then 1 else if j = O then -1 else 0]

        None
    let update msg model =
        match msg with
        | Increment -> { model with Count = model.Count + model.Step }, Cmd.none
        | Decrement -> { model with Count = model.Count - model.Step }, Cmd.none
        | Reset -> init ()
        | SetStep n -> { model with Step = n }, Cmd.none
        | TimerToggled on -> { model with TimerOn = on }, (if on then timerCmd else Cmd.none)
        | TimedTick -> 
            if model.TimerOn then 
                { model with Count = model.Count + model.Step }, timerCmd
            else 
                model, Cmd.none
        | PlacePiece (r,c) ->
            if model.IsX then
                { model with IsX = false; Board=[for row in 0..2 -> [for col in 0..2 -> if r = row && c = col then X else model.Board.[row].[col]]]; Winner = checkForWinnder model r c}, Cmd.none
            else
                { model with IsX = true; Board=[for row in 0..2 -> [for col in 0..2 -> if r = row && c = col then O else model.Board.[row].[col]]]; Winner = checkForWinnder model r c}, Cmd.none
    //let mainPage model dispatch =
    //    View.ContentPage(title="Counter",
    //        content = View.StackLayout(padding = 20.0, verticalOptions = LayoutOptions.Center,
    //          children = [ 
    //              View.Label(text = sprintf "%d" model.Count, horizontalOptions = LayoutOptions.Center, widthRequest=200.0, horizontalTextAlignment=TextAlignment.Center)
    //              View.Button(text = "Increment", command = (fun () -> dispatch Increment), horizontalOptions = LayoutOptions.Center)
    //              View.Button(text = "Decrement", command = (fun () -> dispatch Decrement), horizontalOptions = LayoutOptions.Center)
    //              View.Label(text = "Timer", horizontalOptions = LayoutOptions.Center)
    //              View.Switch(isToggled = model.TimerOn, toggled = (fun on -> dispatch (TimerToggled on.Value)), horizontalOptions = LayoutOptions.Center)
    //              View.Slider(minimumMaximum = (0.0, 10.0), value = double model.Step, valueChanged = (fun args -> dispatch (SetStep (int (args.NewValue + 0.5)))), horizontalOptions = LayoutOptions.FillAndExpand)
    //              View.Label(text = sprintf "Step size: %d" model.Step, horizontalOptions = LayoutOptions.Center) 
    //              View.Button(text = "Reset", horizontalOptions = LayoutOptions.Center, command = (fun () -> dispatch Reset), canExecute = (model <> initModel))
    //          ]))
    let ticTacToe model dispatch =
        View.ContentPage(title="Tic Tac Toe",
            content =
                View.Grid(
                    rowdefs = [box "1*";box "2*"],
                    children = [
                        View.Label(text = sprintf "Player %s's Turn" (if model.Winner <> None then "Game Over!" else if model.IsX then "X" else "O")).GridRow(0)
                        View.Grid(
                                rowdefs = [for i in 1..3 -> box "*"],
                                coldefs = [for i in 1..3 -> box "*"],
                                children = [
                                    for r in 0..2 do
                                        for c in 0..2 ->
                                            View.Button(
                                                text = model.Board.[r].[c].ToString(), 
                                                fontSize = 48,  
                                                command= (fun () -> if model.Board.[r].[c] = None && model.Winner = None then dispatch (PlacePiece (r,c)) else ())
                                                ).GridRow(r).GridColumn(c)
                                    ]).GridRow(1)
                    ])
              )
    let view (model: Model) dispatch =
        View.TabbedPage(children= 
            [
                //mainPage model dispatch
                ticTacToe model dispatch
            ])
   


    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/tools.html for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/models.html for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


