module App

open Feliz
open Elmish


let rnd = System.Random()

module Const =

    //let width = 30
    //let height = 30
    
    let bricksize = 30
    
    let initSnake = [ (7,5); (6,5); (5,5);  ]
    
    let startInterval = 500


type Direction =
    | Up
    | Down
    | Left
    | Right


type GameState =
    | Start
    | Running
    | Lost


type Model = {
    X:int
    Y:int
    Width: int
    Height: int
    StartLevel: int
    Score: int
    Snake: (int * int) list
    Apple: (int * int)
    CurrentDirection: Direction
    GameState: GameState
    Timer: int option
}


type Msg =
    | MoveNext
    | ChangeDirection of Direction
    | SetTimer of int option
    | StartGame

module Helpers =

    let parseIntWithDefault defa (str:string) =
        match System.Int32.TryParse str with
        | true, x -> x
        | false, _ -> defa

module Snake =

    module private Direction =
    
        let toVector direction =
            match direction with
            | Up    -> (0,-1)
            | Down  -> (0,1)
            | Left  -> (-1,0)
            | Right -> (1,0)

    let calcMove direction snake =
        let vx,vy = direction |> Direction.toVector
        let hx,hy = snake |> List.head
        let nx,ny = hx + vx, hy + vy
        (nx,ny) :: snake.[0..snake.Length-2]


    let private isCollided width height snake =
        let (x,y) = snake |> List.head
        match x,y with
        | -1,_  -> true
        | _, -1 -> true
        | x, y when x = width || y = height ->
            true
        | _, _ -> 
            // check collision with its own
            snake
            |> List.tail
            |> List.exists (fun (tx,ty) -> (tx,ty) = (x,y))


    let calcCatch direction snake =
        let vx,vy = direction |> Direction.toVector
        let hx,hy = snake |> List.head
        let nx,ny = hx + vx, hy + vy
        (nx,ny) :: snake


    let private gotApple applePos snake =
        let snakePos = snake |> List.head
        applePos = snakePos


    let (|IsCollided|GotApple|AllGood|) (width,height,applePos,snake) =
        if isCollided width height snake then IsCollided
        elif gotApple applePos snake then GotApple
        else AllGood
    
    
module Commands =
    
    let startTimer interval state =
        fun dispatch ->
            match state.Timer with
            | None ->
                let timer = Fable.Core.JS.setInterval (fun () -> dispatch MoveNext) interval
                dispatch (SetTimer (Some timer))
            | Some _ ->
                ()
        |> Cmd.ofSub


    let updateTimer interval state =
        fun dispatch ->
            match state.Timer with
            | None ->
                ()
            | Some timer ->
                Fable.Core.JS.clearInterval timer
                dispatch (SetTimer None)
                let timer = Fable.Core.JS.setInterval (fun () -> dispatch MoveNext) interval
                dispatch (SetTimer (Some timer))
        |> Cmd.ofSub


    let stopTimer state =
        fun dispatch ->
            match state.Timer with
            | None ->
                ()
            | Some timer ->
                Fable.Core.JS.clearInterval timer
                dispatch (SetTimer None)
        |> Cmd.ofSub


    open Browser.Types

    let subscribeKeys =
        fun dispatch ->
            Browser.Dom.document.addEventListener ("keydown", 
            (fun e ->
                let keyev = e :?> KeyboardEvent
                match keyev.key with
                | "w" -> dispatch (ChangeDirection Up)
                | "d" -> dispatch (ChangeDirection Right)
                | "a" -> dispatch (ChangeDirection Left)
                | "s" -> dispatch (ChangeDirection Down)
                | _ ->
                    ()
            )
            ) |> ignore
        |> Cmd.ofSub

open System

let init x y width height startLevel =
    let startLevel = startLevel |> Helpers.parseIntWithDefault 0
    let x = x |> Helpers.parseIntWithDefault 0
    let y = y |> Helpers.parseIntWithDefault 0
    let width = width |> Helpers.parseIntWithDefault 30
    let height = height |> Helpers.parseIntWithDefault 30
        
    let newApple = (rnd.Next(0, width), rnd.Next (0, height))
    let state =
        {
            X = x
            Y = y
            Width = width
            Height = height
            StartLevel = startLevel
            Score = startLevel
            Snake = Const.initSnake
            Apple = newApple
            CurrentDirection = Right
            GameState = Start
            Timer = None
        }
    state, Commands.subscribeKeys


let update msg state =
    match msg with
    | StartGame ->
        match state.GameState with
        | Start ->
            { state with GameState = Running } , state |> Commands.startTimer Const.startInterval
        | Running ->
            state, Cmd.none
        | Lost ->
            init (state.X |> string) (state.Y |> string) (state.Width |> string) (state.Height |> string) (state.StartLevel |> string) |> fst, Cmd.none
        
    | ChangeDirection dir ->
        { state with CurrentDirection = dir }, Cmd.none
    | MoveNext ->
        match state.GameState with
        | Running ->
            let newSnake = Snake.calcMove state.CurrentDirection state.Snake
            match (state.Width,state.Height, state.Apple, newSnake) with
            | Snake.IsCollided ->
                { state with GameState = Lost }, Cmd.none
            | Snake.GotApple ->
                let newSnake = Snake.calcCatch state.CurrentDirection state.Snake
                let newScore = state.Score + 1
                let newTimerInverall = (100 - newScore) * Const.startInterval / 100
                let newApple = (rnd.Next(0, state.Width), rnd.Next (0, state.Height))
                { state with 
                    Snake = newSnake
                    Score = state.Score + 1 
                    Apple = newApple
                }, state |> Commands.updateTimer newTimerInverall
            | Snake.AllGood ->
                { state with Snake = newSnake }, Cmd.none
        | Lost ->
            state, Commands.stopTimer state
        | Start ->
            state, Cmd.none

    | SetTimer timer ->
        { state with Timer = timer }, Cmd.none


let drawPlayground x y width height =
    Html.div [
        Html.div [
            prop.style [
                style.position.absolute
                style.left (x * 30)
                style.top (y * 30)
                style.backgroundColor "#00c000"
                style.width (Const.bricksize * (width + 2))
                style.height Const.bricksize
            ]
        ]
        Html.div [
            prop.style [
                style.position.absolute
                style.left (x * 30)
                style.top (y * 30)
                style.backgroundColor "#00c000"
                style.width Const.bricksize
                style.height (Const.bricksize * (height + 2))
            ]
        ]
        Html.div [
            prop.style [
                style.position.absolute
                style.left (Const.bricksize * (x + width + 1))
                style.top (y * 30)
                style.backgroundColor "#00c000"
                style.width Const.bricksize
                style.height (Const.bricksize * (height + 2))
            ]
        ]
        Html.div [
            prop.style [
                style.position.absolute
                style.left (x * 30)
                style.top (30 * (y + height + 1))
                style.backgroundColor "#00c000"
                style.width (Const.bricksize * (width + 2))
                style.height Const.bricksize
            ]
        ]
    ]
  

let brick x y w h color (content:ReactElement list) =
    Html.div [
        prop.style [
            style.position.absolute
            style.left (Const.bricksize + x * Const.bricksize)
            style.top (Const.bricksize + y * Const.bricksize)
            style.width (Const.bricksize * w)
            style.height (Const.bricksize * h)
            style.backgroundColor color
            style.display.flex
            style.alignItems.center
            style.justifyContent.center
        ]
        prop.children content
    ]
 

let drawPad width height dispatch =
    Html.div [
        // Up
        brick 11 (height + 5) 5 5 "#606060" [
            Html.h1 [
                prop.style [
                    style.fontSize 90
                ]
                prop.text "🔼"
                prop.onClick (fun _ -> dispatch (ChangeDirection Up))
            ]
        ]


        // Left
        brick 5 (height + 11) 5 5 "#606060" [
            Html.h1 [
                prop.style [
                    style.transform.rotate 270
                    style.fontSize 90
                ]
                prop.text "🔼"
                prop.onClick (fun _ -> dispatch (ChangeDirection Left))
            ]
        ]

        // Down
        brick 11 (height + 11) 5 5 "#606060" [
            Html.h1 [
                prop.style [
                    style.transform.rotate 180
                    style.fontSize 90
                ]
                prop.text "🔼"
                prop.onClick (fun _ -> dispatch (ChangeDirection Down))
            ]
        ]

        // Right
        brick 17 (height + 11) 5 5 "#606060" [
            Html.h1 [
                prop.style [
                    style.transform.rotate 90
                    style.fontSize 90
                ]
                prop.text "🔼"
                prop.onClick (fun _ -> dispatch (ChangeDirection Right))
            ]
        ]

    ]
    

let view state dispatch =
    Html.div [
        drawPlayground state.X state.Y state.Width state.Height
        Html.div [
            prop.style [
                style.position.absolute
                style.left (Const.bricksize * (state.X + 1))
                style.top (Const.bricksize * (state.Y))
            ]
            prop.children [
                Html.h1 $"Score: {state.Score}"
            ]
        ]
        
        

        let ax, ay = state.Apple
        Html.div [
            prop.style [
                style.position.absolute
                style.left (Const.bricksize + (ax + state.X) * Const.bricksize)
                style.top (Const.bricksize + (ay + state.Y) * Const.bricksize)
                style.backgroundColor "#c00000"
                style.width Const.bricksize
                style.height Const.bricksize
            ]
        ]


        for (x,y) in state.Snake do
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (Const.bricksize + (x + state.X) * Const.bricksize)
                    style.top (Const.bricksize + (y + state.Y) * Const.bricksize)
                    style.backgroundColor "#00c0c0"
                    style.width Const.bricksize
                    style.height Const.bricksize
                ]
            ]

        match state.GameState with
        | Running ->
            drawPad state.Width state.Height dispatch
        | Lost ->
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (Const.bricksize + (state.Width / 2 + state.X - 2) * Const.bricksize)
                    style.top (Const.bricksize + (state.Height / 2 + state.Y - 2) * Const.bricksize)
                ]
                prop.children [
                    Html.h1 "You Loose!"
                    Html.button [
                        prop.onClick (fun _ -> dispatch StartGame)
                        prop.onClick (fun _ -> dispatch StartGame)
                        prop.text "Again!"
                        prop.style [
                            style.fontSize 70
                        ]
                    ]
                ]
            ]
        | Start ->
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (Const.bricksize + (state.Width / 2 + state.X - 2) * Const.bricksize)
                    style.top (Const.bricksize + (state.Height / 2 + state.Y - 2) * Const.bricksize)
                ]
                prop.children [
                    Html.h1 "Ready?"
                    Html.button [
                        prop.onClick (fun _ -> dispatch StartGame)
                        prop.text "Start Game"
                        prop.style [
                            style.fontSize 26
                        ]
                    ]
                ]
            ]

        
    ]


