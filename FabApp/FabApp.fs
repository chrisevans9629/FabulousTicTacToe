// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace FabApp

open System.Diagnostics
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open System.Linq
open System

module App = 
    type Player =
        X | O | None
    type Model = 
      { 
        IsX : bool
        Board : Player list list
        Winner : Player
      }

    type Msg = 
        | PlacePiece of int * int
        | ResetGame
        

    let initModel = { IsX=true; Board = [for r in 0..2 -> [for c in 0..2 -> None]]; Winner = None}

    let init () = initModel, Cmd.none

    //let timerCmd = 
    //    async { do! Async.Sleep 200
    //            return TimedTick }
    //    |> Cmd.ofAsyncMsg
    let checkForWinnder (model: Player list list) =
        let check (list: Player list) = 
            let sum = list.Select(fun t -> match t with | X -> 1 | O -> -1 | _ -> 0).Sum()
            if sum = 3 then X else if sum = -3 then O else None
        
        let top = model.[0]
        let mid = model.[1]
        let bottom = model.[2]

        let first = [model.[0].[0];model.[1].[0];model.[2].[0]]
        let second = [model.[0].[1];model.[1].[1];model.[1].[1]]
        let third = [model.[0].[2];model.[1].[2];model.[2].[2]]

        let diag = [model.[0].[0];model.[1].[1];model.[2].[2]]
        let otherDiag = [model.[0].[2];model.[1].[1];model.[2].[0]]

        let checks = [top;mid;bottom;first;second;third;diag;otherDiag]
        if checks.Any(fun r -> check r = X) then X
        else if checks.Any(fun r -> check r = O) then O
        else None
    let updateBoard (model: Player list list) r c player =
        let board = [for row in 0..2 -> [for col in 0..2 -> if r = row && c = col then player else model.[row].[col]]]
        board
    let update msg model =
        match msg with
        | ResetGame ->
            initModel, Cmd.none
        | PlacePiece (r,c) ->
            if model.IsX then
                let board = updateBoard model.Board r c X
                { model with IsX = false; Board=board; Winner = checkForWinnder board}, Cmd.none
            else
                let board = updateBoard model.Board r c O
                { model with IsX = true; Board=board; Winner = checkForWinnder board}, Cmd.none
    let ticTacToe model dispatch =
        View.ContentPage(title="Tic Tac Toe",
            content =
                View.Grid(
                    rowdefs = [box "1*";box "2*"],
                    children = [
                        View.StackLayout(children = [
                                View.Label(text = sprintf "Player %s's Turn" (if model.Winner <> None then "Game Over!" else if model.IsX then "X" else "O"))
                                View.Button(text= "Reset Game", command = (fun () -> dispatch ResetGame))
                            ]).GridRow(0)
                        
                        View.Grid(
                                rowdefs = [for i in 1..3 -> box "*"],
                                coldefs = [for i in 1..3 -> box "*"],
                                children = [
                                    for r in 0..2 do
                                        for c in 0..2 ->
                                            View.Button(
                                                text = (if model.Board.[r].[c] <> None then model.Board.[r].[c].ToString() else ""), 
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


