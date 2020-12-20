module App

open Feliz
open Elmish


let rnd = System.Random()

module Const =

    let width = 30
    let height = 30
    
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


    let private isCollided snake =
        let (x,y) = snake |> List.head
        match x,y with
        | -1,_  -> true
        | _, -1 -> true
        | x, y when x = Const.width || y = Const.height ->
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


    let (|IsCollided|GotApple|AllGood|) (applePos,snake) =
        if isCollided snake then IsCollided
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



let init () =
    let newApple = (rnd.Next(0, Const.width), rnd.Next (0, Const.height))
    let state =
        {
            Score = 0
            Snake = Const.initSnake
            Apple = newApple
            CurrentDirection = Right
            GameState = Start
            Timer = None
        }
    state, Cmd.none


let update msg state =
    match msg with
    | StartGame ->
        match state.GameState with
        | Start ->
            { state with GameState = Running } , state |> Commands.startTimer Const.startInterval
        | Running ->
            state, Cmd.none
        | Lost ->
            init () |> fst, Cmd.none
        
    | ChangeDirection dir ->
        { state with CurrentDirection = dir }, Cmd.none
    | MoveNext ->
        match state.GameState with
        | Running ->
            let newSnake = Snake.calcMove state.CurrentDirection state.Snake
            match (state.Apple,newSnake) with
            | Snake.IsCollided ->
                { state with GameState = Lost }, Cmd.none
            | Snake.GotApple ->
                let newSnake = Snake.calcCatch state.CurrentDirection state.Snake
                let newScore = state.Score + 1
                let newTimerInverall = (100 - newScore) * Const.startInterval / 100
                let newApple = (rnd.Next(0, Const.width), rnd.Next (0, Const.height))
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


let drawPlayground width heigth =
    Html.div [
        Html.div [
            prop.style [
                style.position.absolute
                style.left 0
                style.top 0
                style.backgroundColor "#00c000"
                style.width (Const.bricksize * (width + 2))
                style.height Const.bricksize
            ]
        ]
        Html.div [
            prop.style [
                style.position.absolute
                style.left 0
                style.top 0
                style.backgroundColor "#00c000"
                style.width Const.bricksize
                style.height (Const.bricksize * (Const.height + 2))
            ]
        ]
        Html.div [
            prop.style [
                style.position.absolute
                style.left (Const.bricksize * (width + 1))
                style.top 0
                style.backgroundColor "#00c000"
                style.width Const.bricksize
                style.height (Const.bricksize * (Const.height + 2))
            ]
        ]
        Html.div [
            prop.style [
                style.position.absolute
                style.left 0
                style.top (30 * (Const.height + 1))
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
 

let drawPad dispatch =
    Html.div [
        // Up
        brick 11 32 5 5 "#606060" [
            Html.h1 [
                prop.style [
                    style.fontSize 90
                ]
                prop.text "🔼"
                prop.onClick (fun _ -> dispatch (ChangeDirection Up))
            ]
        ]


        // Left
        brick 5 38 5 5 "#606060" [
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
        brick 11 38 5 5 "#606060" [
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
        brick 17 38 5 5 "#606060" [
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
        drawPlayground Const.width Const.height
        Html.div [
            prop.style [
                style.position.absolute
                style.left Const.bricksize
                style.top Const.bricksize
            ]
            prop.children [
                //Html.p $"{state.CurrentDirection}"
                //Html.p $"{state.GameState}"
                Html.h1 $"Score: {state.Score}"
                //Html.p $"X: {state.Snake |> List.head |> fst} Y:{state.Snake |> List.head |> snd}"
            ]
        ]
        
        

        let ax, ay = state.Apple
        Html.div [
            prop.style [
                style.position.absolute
                style.left (Const.bricksize + ax * Const.bricksize)
                style.top (Const.bricksize + ay * Const.bricksize)
                style.backgroundColor "#c00000"
                style.width Const.bricksize
                style.height Const.bricksize
            ]
        ]


        for (x,y) in state.Snake do
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (Const.bricksize + x * Const.bricksize)
                    style.top (Const.bricksize + y * Const.bricksize)
                    style.backgroundColor "#00c0c0"
                    style.width Const.bricksize
                    style.height Const.bricksize
                ]
            ]

        drawPad dispatch

        match state.GameState with
        | Running ->
            drawPad dispatch
        | Lost ->
            Html.div [
                prop.style [
                    style.position.absolute
                    style.left (Const.bricksize + 11 * Const.bricksize)
                    style.top (Const.bricksize + 12 * Const.bricksize)
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
                    style.left (Const.bricksize + 11 * Const.bricksize)
                    style.top (Const.bricksize + 12 * Const.bricksize)
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


open Browser.Types

let subscription (state:Model) =
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


open Elmish.React


Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withSubscription subscription
|> Program.withReactSynchronous "elmish-app"
|> Program.run