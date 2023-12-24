module OldCoreSkill

open FallenLib.CoreSkillGroup
open FallenLib.Dice
open FallenLib.SkillStat
open FallenLib.SkillDiceModEffect
open FallenLib.Attribute
open FallenLib.AttributeDeterminedDiceModEffect

type Msg =
    | Neg1To4Msg of Neg1To4.Msg
    | CalculateDicePool

let init
    (skillDiceModEffectList: SkillDiceModEffect list)
    (attributeDeterminedDiceModEffectList: AttributeDeterminedDiceModEffect list)
    (attributeStat: Attribute)
    =
    let lvl = Neg1To4.init ()

    let name = ""

    { name = name
      lvl = lvl
      dicePool =
        coreSkillToDicePool
            baseDicePool
            lvl
            attributeStat
            (collectSkillAdjustmentDiceMods name skillDiceModEffectList)
            attributeDeterminedDiceModEffectList }

let update
    (skillDiceModEffectList: SkillDiceModEffect list)
    (attributeDeterminedDiceModEffectList: AttributeDeterminedDiceModEffect list)
    (attributeStat: Attribute)
    (msg: Msg)
    (model: SkillStat)
    : SkillStat =

    match msg with
    | Neg1To4Msg neg1To4StatMsg ->
        let newLvl = Neg1To4.update neg1To4StatMsg model.lvl

        { model with
            lvl = newLvl
            dicePool =
                coreSkillToDicePool
                    baseDicePool
                    newLvl
                    attributeStat
                    (collectSkillAdjustmentDiceMods model.name skillDiceModEffectList)
                    attributeDeterminedDiceModEffectList }
    | CalculateDicePool ->
        { model with
            dicePool =
                coreSkillToDicePool
                    baseDicePool
                    model.lvl
                    attributeStat
                    (collectSkillAdjustmentDiceMods model.name skillDiceModEffectList)
                    attributeDeterminedDiceModEffectList }

open Feliz
open Feliz.Bulma

let view (model: SkillStat) (dispatch: Msg -> unit) =
    Bulma.columns [
        Bulma.column [ prop.text model.name ]
        Bulma.column [
            model.dicePool |> dicePoolToString |> prop.text
        ]
        Bulma.column [
            Neg1To4.view model.lvl (Neg1To4Msg >> dispatch)
        ]
    ]