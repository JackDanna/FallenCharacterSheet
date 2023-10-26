module VocationalSkillRow

open FallenLib.SkillUtils
open FallenLib.Dice

type Model = Skill

type Msg =
    | SetName of string
    | VocationalSkillStatMsg of VocationalSkillStat.Msg

let init() : Model = skillInit()


let update (vocationLevel: Neg1To4Stat.Model) (msg: Msg) (model: Model) : Model =
    match msg with
    | VocationalSkillStatMsg vocationalSkillStatMsg ->
        { model with level = VocationalSkillStat.update vocationLevel vocationalSkillStatMsg model.level }
    | SetName name -> { model with name = name }
        
open Feliz
open Feliz.Bulma

let view (governingAttributes:GoverningAttribute list) (vocationLevel: Neg1To4Stat.Model) (model: Model) (dispatch: Msg -> unit) =
    Bulma.columns [
        Bulma.column [
            Bulma.dropdown [
                Bulma.dropdownTrigger [
                    Bulma.input.text [ 
                        prop.defaultValue model.name
                        prop.onTextChange (fun value -> dispatch (SetName value) )
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
            vocationToDicePoolString
                baseDicePoolCalculation
                governingAttributes
                model.level
            |> prop.text
        ]
        Bulma.column [
            VocationalSkillStat.view
                vocationLevel
                model.level
                (VocationalSkillStatMsg >> dispatch)
        ]
    ]