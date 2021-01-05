module ReactWebComponent

open Feliz.UseElmish
open Feliz
open Fable.React.WebComponent


[<ReactWebComponent>]
let SnakeComponent (parms: {| x:string; y:string; width:string; height:string; startlevel:string |}) =
    let state, dispatch =
        React.useElmish (
            App.init parms.x parms.y parms.width parms.height parms.startlevel,
            App.update,
            [| parms.x; parms.y; parms.width; parms.height; parms.startlevel |] |> Array.map (fun x -> x :> obj)
        )
    App.view state dispatch



[<CreateReactWebComponent("snake-game")>]
let snakeWebComponent = SnakeComponent        
        


