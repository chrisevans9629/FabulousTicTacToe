namespace Tests

open NUnit.Framework
open FabApp
open FabApp.App
open Fabulous.Core
open Fabulous.DynamicViews
open System.Linq
[<TestClass>]
type TestClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.Test1 () =
        Assert.Pass()

    [<Test>]
    member this.TopWin () =
        
        let addX = 
            let f = App.updateBoard App.initModel.Board 0 0 X
            let s = App.updateBoard f 0 1 X
            let t = App.updateBoard s 0 2 X
            t
        let win = App.checkForWinnder addX

        Assert.AreEqual(win, X)
    [<Test>]
    member this.BottomWin () =
           
        let addX = 
            let f = App.updateBoard App.initModel.Board 2 0 X
            let s = App.updateBoard f 2 1 X
            let t = App.updateBoard s 2 2 X
            t
        let win = App.checkForWinnder addX

        Assert.AreEqual(win, X)
    [<Test>]
    member this.LeftWin () =
           
        let addX = 
            let f = App.updateBoard App.initModel.Board 0 0 X
            let s = App.updateBoard f 1 0 X
            let t = App.updateBoard s 2 0 X
            t
        let win = App.checkForWinnder addX

        Assert.AreEqual(win, X)
    [<Test>]
    member this.RightWin () =
           
        let addX = 
            let f = App.updateBoard App.initModel.Board 0 2 X
            let s = App.updateBoard f 1 2 X
            let t = App.updateBoard s 2 2 X
            t
        let win = App.checkForWinnder addX

        Assert.AreEqual(win, X)
    [<Test>]
    member this.DiagnalWinX () =
        let player = X
        let addX = 
            let f = App.updateBoard App.initModel.Board 0 0 player
            let s = App.updateBoard f 1 1 player
            let t = App.updateBoard s 2 2 player
            t
        let win = App.checkForWinnder addX

        Assert.AreEqual(win, player)
    [<Test>]
       member this.DiagnalWinO () =
           let player = O
           let addX = 
               let f = App.updateBoard App.initModel.Board 0 0 player
               let s = App.updateBoard f 1 1 player
               let t = App.updateBoard s 2 2 player
               t
           let win = App.checkForWinnder addX

           Assert.AreEqual(win, player)
    [<Test>]
    member this.RightNotWin () =
           
        let addX = 
            let f = App.updateBoard App.initModel.Board 0 2 X
            let s = App.updateBoard f 1 2 O
            let t = App.updateBoard s 2 2 X
            t
        let win = App.checkForWinnder addX

        Assert.AreEqual(win, None)
    [<Test>]
    member this.CoinNotNull()=
        let model = App.initModel
        Assert.NotNull(model.Coin)

    [<Test>]
    member this.``Coin is not null when updated``()=
        let model,_ = App.initModel |> App.update FlipCoin

        Assert.NotNull(model.Coin)
    
    [<Test>]
    member this.``Box View gesture no exception``()=
        let view = ContentPageViewer(App.coinFlipView App.initModel (fun e-> ()))

        let box = BoxViewViewer(view.Content)

        let gesture = TapGestureRecognizerViewer( box.GestureRecognizers.First())

        gesture.Command.Execute()
