module VocationalSkill

open FallenLib.VocationGroup
open FallenLib.Vocation
open FallenLib.Dice
open FallenLib.SkillStat
open FallenLib.CoreSkillGroup

type Model = SkillStat

type Msg =
    | SetName of string
    | VocationalSkillStatMsg of VocationalSkillStat.Msg
    | CalculateDicePool

let init (governingAttribute: GoverningAttribute list) : Model =
    let lvl = Neg1To4Stat.init ()

    { name = ""
      lvl = lvl
      dicePool = vocationalSkillToString baseDicePool lvl governingAttribute }


let update
    (levelCap: ZeroToFourStat.Model)
    (governingAttributeList: GoverningAttribute list)
    (msg: Msg)
    (model: Model)
    : Model =
    match msg with
    | SetName newName -> { model with name = newName }
    | VocationalSkillStatMsg vocationalSkillStatMsg ->
        let lvl = VocationalSkillStat.update levelCap vocationalSkillStatMsg model.lvl

        { model with
            lvl = lvl
            dicePool = vocationalSkillToString baseDicePool lvl governingAttributeList }
    | CalculateDicePool ->
        { model with dicePool = vocationalSkillToString baseDicePool model.lvl governingAttributeList }

open Feliz
open Feliz.Bulma

let view (levelCap: ZeroToFourStat.Model) (model: Model) (dispatch: Msg -> unit) =
    Bulma.columns [
        Bulma.column [
            Bulma.dropdown [
                Bulma.dropdownTrigger [
                    Bulma.input.text [
                        prop.defaultValue model.name
                        prop.onTextChange (fun value -> dispatch (SetName value))
                    ]
                ]
                Bulma.dropdownMenu [
                    Bulma.dropdownContent [
                        Bulma.dropdownItem.a [
                            prop.children [
                                Html.span "Single Sequence"
                            ]
                        ]
                    ]
                ]
            ]
        ]
        Bulma.column [
            model.dicePool |> dicePoolToString |> prop.text
        ]
        Bulma.column [
            VocationalSkillStat.view levelCap model.lvl (VocationalSkillStatMsg >> dispatch)
        ]
    ]