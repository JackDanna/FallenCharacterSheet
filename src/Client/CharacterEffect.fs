module CharacterEffect

open FallenLib.EffectForDisplay
open FallenLib.SkillDiceModificationEffect
open FallenLib.CarryWeightEffect
open FallenLib.CoreSkillGroup

open FallenLib.CharacterEffect

let getCharacterEffectName characterEffect =
    match characterEffect with
    | EffectForDisplay effectForDisplay -> effectForDisplay.name
    | SkillDiceModificationEffectForDisplay (sdmew, _) -> sdmew.name
    | CalculatedCarryWeightEffectForDisplay ccwefd -> ccwefd.carryWeightCalculation.name

type Msg =
    | EffectForDisplayMsg of EffectForDisplay.Msg
    | SkillDiceModificationEffectForDisplayMsg of SkillDiceModificationEffectForDisplay.Msg
    | CalculatedCarryWeightEffectForDisplayMsg of CarryWeightEffectForDisplay.Msg

let update
    (coreSkillGroupList: CoreSkillGroup list)
    (inventoryWeight: float)
    (carryWeightCalculationMap: Map<string, CarryWeightCalculation>)
    (weightClassList: WeightClass list)
    (msg: Msg)
    (model: CharacterEffect)
    : CharacterEffect =
    match msg, model with
    | EffectForDisplayMsg msg, EffectForDisplay effectForDisplay ->
        EffectForDisplay.update msg effectForDisplay
        |> EffectForDisplay
    | SkillDiceModificationEffectForDisplayMsg msg, SkillDiceModificationEffectForDisplay (sdmew, das) ->
        SkillDiceModificationEffectForDisplay.update msg (sdmew, das)
        |> SkillDiceModificationEffectForDisplay
    | CalculatedCarryWeightEffectForDisplayMsg msg, CalculatedCarryWeightEffectForDisplay ccwefd ->
        CarryWeightEffectForDisplay.update
            (coreSkillGroupList: CoreSkillGroup list)
            (inventoryWeight: float)
            (weightClassList: WeightClass list)
            msg
            ccwefd
        |> CalculatedCarryWeightEffectForDisplay
    | _ -> model


open Feliz
open Feliz.Bulma

let characterEffectTableData (model: CharacterEffect) (dispatch: Msg -> unit) =

    match model with
    | EffectForDisplay effectForDisplay ->
        EffectForDisplay.effectForDisplayTableData effectForDisplay (EffectForDisplayMsg >> dispatch)
    | SkillDiceModificationEffectForDisplay sdmefd ->
        SkillDiceModificationEffectForDisplay.skillDiceModificationForDisplayTableData
            sdmefd
            (SkillDiceModificationEffectForDisplayMsg
             >> dispatch)
    | CalculatedCarryWeightEffectForDisplay ccwefd -> CarryWeightEffectForDisplay.carryWeightEffectForDisplay ccwefd

let view (model: CharacterEffect) (dispatch: Msg -> unit) =

    characterEffectTableData model dispatch |> Html.tr