import { ofSeq, tail, exists, cons, length, getSlice, head, ofArray } from "./.fable/fable-library.3.0.4/List.js";
import { Record, Union } from "./.fable/fable-library.3.0.4/Types.js";
import { record_type, option_type, list_type, tuple_type, int32_type, union_type } from "./.fable/fable-library.3.0.4/Reflection.js";
import { randomNext, equals, equalArrays } from "./.fable/fable-library.3.0.4/Util.js";
import { FSharpChoice$2 } from "./.fable/fable-library.3.0.4/Choice.js";
import { Cmd_none, Cmd_ofSub } from "./.fable/Fable.Elmish.3.1.0/cmd.fs.js";
import { createElement } from "react";
import { reactApi } from "./.fable/Feliz.1.29.0/Interop.fs.js";
import { empty, collect, singleton, append, delay } from "./.fable/fable-library.3.0.4/Seq.js";
import { interpolate, toText } from "./.fable/fable-library.3.0.4/String.js";
import { ProgramModule_mkProgram, ProgramModule_withConsoleTrace, ProgramModule_withSubscription, ProgramModule_run } from "./.fable/Fable.Elmish.3.1.0/program.fs.js";
import { Program_withReactSynchronous } from "./.fable/Fable.Elmish.React.3.0.1/react.fs.js";

export const rnd = {};

export const Const_width = 30;

export const Const_height = 30;

export const Const_bricksize = 30;

export const Const_initSnake = ofArray([[7, 5], [6, 5], [5, 5]]);

export const Const_startInterval = 500;

export class Direction extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Up", "Down", "Left", "Right"];
    }
}

export function Direction$reflection() {
    return union_type("App.Direction", [], Direction, () => [[], [], [], []]);
}

export class GameState extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Start", "Running", "Lost"];
    }
}

export function GameState$reflection() {
    return union_type("App.GameState", [], GameState, () => [[], [], []]);
}

export class Model extends Record {
    constructor(Score, Snake, Apple, CurrentDirection, GameState, Timer) {
        super();
        this.Score = (Score | 0);
        this.Snake = Snake;
        this.Apple = Apple;
        this.CurrentDirection = CurrentDirection;
        this.GameState = GameState;
        this.Timer = Timer;
    }
}

export function Model$reflection() {
    return record_type("App.Model", [], Model, () => [["Score", int32_type], ["Snake", list_type(tuple_type(int32_type, int32_type))], ["Apple", tuple_type(int32_type, int32_type)], ["CurrentDirection", Direction$reflection()], ["GameState", GameState$reflection()], ["Timer", option_type(int32_type)]]);
}

export class Msg extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["MoveNext", "ChangeDirection", "SetTimer", "StartGame"];
    }
}

export function Msg$reflection() {
    return union_type("App.Msg", [], Msg, () => [[], [["Item", Direction$reflection()]], [["Item", option_type(int32_type)]], []]);
}

export function DirectionModule_toVector(direction) {
    switch (direction.tag) {
        case 1: {
            return [0, 1];
        }
        case 2: {
            return [-1, 0];
        }
        case 3: {
            return [1, 0];
        }
        default: {
            return [0, -1];
        }
    }
}

export function Snake_calcMove(direction, snake) {
    const patternInput = DirectionModule_toVector(direction);
    const vy = patternInput[1] | 0;
    const vx = patternInput[0] | 0;
    const patternInput_1 = head(snake);
    const hy = patternInput_1[1] | 0;
    const hx = patternInput_1[0] | 0;
    const patternInput_2 = [hx + vx, hy + vy];
    const ny = patternInput_2[1] | 0;
    const nx = patternInput_2[0] | 0;
    return cons([nx, ny], getSlice(0, length(snake) - 2, snake));
}

function Snake_isCollided(snake) {
    let y_1, x_1;
    const patternInput = head(snake);
    const y = patternInput[1] | 0;
    const x = patternInput[0] | 0;
    const matchValue = [x, y];
    if (matchValue[0] === -1) {
        return true;
    }
    else if (matchValue[1] === -1) {
        return true;
    }
    else if (y_1 = (matchValue[1] | 0), (x_1 = (matchValue[0] | 0), (x_1 === Const_width) ? true : (y_1 === Const_height))) {
        const y_2 = matchValue[1] | 0;
        const x_2 = matchValue[0] | 0;
        return true;
    }
    else {
        return exists((tupledArg) => {
            const tx = tupledArg[0] | 0;
            const ty = tupledArg[1] | 0;
            return equalArrays([tx, ty], [x, y]);
        }, tail(snake));
    }
}

export function Snake_calcCatch(direction, snake) {
    const patternInput = DirectionModule_toVector(direction);
    const vy = patternInput[1] | 0;
    const vx = patternInput[0] | 0;
    const patternInput_1 = head(snake);
    const hy = patternInput_1[1] | 0;
    const hx = patternInput_1[0] | 0;
    const patternInput_2 = [hx + vx, hy + vy];
    const ny = patternInput_2[1] | 0;
    const nx = patternInput_2[0] | 0;
    return cons([nx, ny], snake);
}

function Snake_gotApple(applePos, snake) {
    const snakePos = head(snake);
    return equals(applePos, snakePos);
}

export function Snake_$007CIsCollided$007CGotApple$007CAllGood$007C(applePos, snake) {
    if (Snake_isCollided(snake)) {
        return new FSharpChoice$2(0, void 0);
    }
    else if (Snake_gotApple(applePos, snake)) {
        return new FSharpChoice$2(1, void 0);
    }
    else {
        return new FSharpChoice$2(2, void 0);
    }
}

export function Commands_startTimer(interval, state) {
    return Cmd_ofSub((dispatch) => {
        if (state.Timer != null) {
        }
        else {
            const timer = setInterval(() => {
                dispatch(new Msg(0));
            }, interval) | 0;
            dispatch(new Msg(2, timer));
        }
    });
}

export function Commands_updateTimer(interval, state) {
    return Cmd_ofSub((dispatch) => {
        const matchValue = state.Timer;
        if (matchValue != null) {
            const timer = matchValue | 0;
            clearInterval(timer);
            dispatch(new Msg(2, void 0));
            const timer_1 = setInterval(() => {
                dispatch(new Msg(0));
            }, interval) | 0;
            dispatch(new Msg(2, timer_1));
        }
    });
}

export function Commands_stopTimer(state) {
    return Cmd_ofSub((dispatch) => {
        const matchValue = state.Timer;
        if (matchValue != null) {
            const timer = matchValue | 0;
            clearInterval(timer);
            dispatch(new Msg(2, void 0));
        }
    });
}

export function init() {
    const newApple = [randomNext(0, Const_width), randomNext(0, Const_height)];
    const state = new Model(0, Const_initSnake, newApple, new Direction(3), new GameState(0), void 0);
    return [state, Cmd_none()];
}

export function update(msg, state) {
    switch (msg.tag) {
        case 1: {
            const dir = msg.fields[0];
            return [new Model(state.Score, state.Snake, state.Apple, dir, state.GameState, state.Timer), Cmd_none()];
        }
        case 0: {
            const matchValue = state.GameState;
            switch (matchValue.tag) {
                case 2: {
                    return [state, Commands_stopTimer(state)];
                }
                case 0: {
                    return [state, Cmd_none()];
                }
                default: {
                    const newSnake = Snake_calcMove(state.CurrentDirection, state.Snake);
                    const matchValue_1 = [state.Apple, newSnake];
                    const activePatternResult833618 = Snake_$007CIsCollided$007CGotApple$007CAllGood$007C(matchValue_1[0], matchValue_1[1]);
                    if (activePatternResult833618.tag === 1) {
                        const newSnake_1 = Snake_calcCatch(state.CurrentDirection, state.Snake);
                        const newScore = (state.Score + 1) | 0;
                        const newTimerInverall = (~(~(((100 - newScore) * Const_startInterval) / 100))) | 0;
                        const newApple = [randomNext(0, Const_width), randomNext(0, Const_height)];
                        return [new Model(state.Score + 1, newSnake_1, newApple, state.CurrentDirection, state.GameState, state.Timer), Commands_updateTimer(newTimerInverall, state)];
                    }
                    else if (activePatternResult833618.tag === 2) {
                        return [new Model(state.Score, newSnake, state.Apple, state.CurrentDirection, state.GameState, state.Timer), Cmd_none()];
                    }
                    else {
                        return [new Model(state.Score, state.Snake, state.Apple, state.CurrentDirection, new GameState(2), state.Timer), Cmd_none()];
                    }
                }
            }
        }
        case 2: {
            const timer = msg.fields[0];
            return [new Model(state.Score, state.Snake, state.Apple, state.CurrentDirection, state.GameState, timer), Cmd_none()];
        }
        default: {
            return [new Model(state.Score, state.Snake, state.Apple, state.CurrentDirection, new GameState(1), state.Timer), Commands_startTimer(Const_startInterval, state)];
        }
    }
}

export function drawPlayground(width, heigth) {
    const children = ofArray([createElement("div", {
        style: {
            position: "absolute",
            left: 0,
            top: 0,
            backgroundColor: "#00c000",
            width: Const_bricksize * (width + 2),
            height: Const_bricksize,
        },
    }), createElement("div", {
        style: {
            position: "absolute",
            left: 0,
            top: 0,
            backgroundColor: "#00c000",
            width: Const_bricksize,
            height: Const_bricksize * (Const_height + 2),
        },
    }), createElement("div", {
        style: {
            position: "absolute",
            left: Const_bricksize * (width + 1),
            top: 0,
            backgroundColor: "#00c000",
            width: Const_bricksize,
            height: Const_bricksize * (Const_height + 2),
        },
    }), createElement("div", {
        style: {
            position: "absolute",
            left: 0,
            top: 30 * (Const_height + 1),
            backgroundColor: "#00c000",
            width: Const_bricksize * (width + 2),
            height: Const_bricksize,
        },
    })]);
    return createElement("div", {
        children: reactApi.Children.toArray(Array.from(children)),
    });
}

export function view(state, dispatch) {
    const children = ofSeq(delay(() => append(singleton(drawPlayground(Const_width, Const_height)), delay(() => {
        let value_6;
        return append(singleton(createElement("div", {
            style: {
                position: "absolute",
                left: Const_bricksize,
                top: Const_bricksize,
            },
            children: reactApi.Children.toArray([(value_6 = toText(interpolate("Score: %P()", [state.Score])), createElement("h1", {
                children: [value_6],
            }))]),
        })), delay(() => {
            const patternInput = state.Apple;
            const ay = patternInput[1] | 0;
            const ax = patternInput[0] | 0;
            return append(singleton(createElement("div", {
                style: {
                    position: "absolute",
                    left: Const_bricksize + (ax * Const_bricksize),
                    top: Const_bricksize + (ay * Const_bricksize),
                    backgroundColor: "#c00000",
                    width: Const_bricksize,
                    height: Const_bricksize,
                },
            })), delay(() => append(collect((matchValue) => {
                const y = matchValue[1] | 0;
                const x = matchValue[0] | 0;
                return singleton(createElement("div", {
                    style: {
                        position: "absolute",
                        left: Const_bricksize + (x * Const_bricksize),
                        top: Const_bricksize + (y * Const_bricksize),
                        backgroundColor: "#00c0c0",
                        width: Const_bricksize,
                        height: Const_bricksize,
                    },
                }));
            }, state.Snake), delay(() => {
                const matchValue_1 = state.GameState;
                switch (matchValue_1.tag) {
                    case 2: {
                        return singleton(createElement("div", {
                            style: {
                                position: "absolute",
                                left: Const_bricksize + (11 * Const_bricksize),
                                top: Const_bricksize + (12 * Const_bricksize),
                            },
                            children: reactApi.Children.toArray([createElement("h1", {
                                children: ["You Loose!"],
                            }), createElement("button", {
                                onClick: (_arg1) => {
                                    dispatch(new Msg(3));
                                },
                                onClick: (_arg2) => {
                                    dispatch(new Msg(3));
                                },
                                children: "Again!",
                                style: {
                                    fontSize: 70 + "px",
                                },
                            })]),
                        }));
                    }
                    case 0: {
                        return singleton(createElement("div", {
                            style: {
                                position: "absolute",
                                left: Const_bricksize + (11 * Const_bricksize),
                                top: Const_bricksize + (12 * Const_bricksize),
                            },
                            children: reactApi.Children.toArray([createElement("h1", {
                                children: ["Ready?"],
                            }), createElement("button", {
                                onClick: (_arg3) => {
                                    dispatch(new Msg(3));
                                },
                                children: "Start Game",
                                style: {
                                    fontSize: 26 + "px",
                                },
                            })]),
                        }));
                    }
                    default: {
                        return empty();
                    }
                }
            }))));
        }));
    }))));
    return createElement("div", {
        children: reactApi.Children.toArray(Array.from(children)),
    });
}

export function subscription(state) {
    return Cmd_ofSub((dispatch) => {
        const value = document.addEventListener("keydown", (e) => {
            const keyev = e;
            const matchValue = keyev.key;
            switch (matchValue) {
                case "w": {
                    dispatch(new Msg(1, new Direction(0)));
                    break;
                }
                case "d": {
                    dispatch(new Msg(1, new Direction(3)));
                    break;
                }
                case "a": {
                    dispatch(new Msg(1, new Direction(2)));
                    break;
                }
                case "s": {
                    dispatch(new Msg(1, new Direction(1)));
                    break;
                }
                default: {
                }
            }
        });
        void undefined;
    });
}

ProgramModule_run(Program_withReactSynchronous("elmish-app", ProgramModule_withSubscription(subscription, ProgramModule_withConsoleTrace(ProgramModule_mkProgram(init, update, view)))));

