module Attribute

open FallenLib.Attribute

type Msg = Neg1To4Msg of Neg1To4.Msg

let init () = { name = ""; level = Neg1To4.init () }

let update (msg: Msg) (model: Attribute) : Attribute =
    match msg with
    | Neg1To4Msg neg1To4 -> { model with level = Neg1To4.update neg1To4 model.level }

open Feliz
open Feliz.Bulma

let view (model: Attribute) (dispatch: Msg -> unit) =
    Bulma.columns [
        Bulma.column [ prop.text model.name ]
        Bulma.column [
            Neg1To4.view model.level (Neg1To4Msg >> dispatch)
        ]
    ]