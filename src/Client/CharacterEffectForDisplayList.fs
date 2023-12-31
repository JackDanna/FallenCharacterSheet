module CharacterEffectForDisplayList

open FallenLib.EffectForDisplay
open FallenLib.WeightClass
open FallenLib.CoreSkill
open FallenLib.Attribute
open FallenLib.MovementSpeedEffect
open FallenLib.MovementSpeedEffectForDisplay

type Msg =
    | ModifyCharacterEffect of int * CharacterEffectForDisplay.Msg
    | Insert of string
    | Remove of int
    | RecalculateMovementSpeed

let init () : EffectForDisplay list = []

let update
    (coreSkillAndAttributeData: CoreSkillAndAttributeData)
    (characterEffectMap: Map<string, EffectForDisplay>)
    (movementSpeedCalculationMap: Map<string, MovementSpeedCalculation>)
    (msg: Msg)
    (model: EffectForDisplay list)
    : EffectForDisplay list =
    match msg with
    | ModifyCharacterEffect (position, msg) ->
        model
        |> List.mapi (fun index characterEffect ->
            if position = index then
                CharacterEffectForDisplay.update msg characterEffect
            else
                characterEffect)
    | Insert characterEffectName ->
        if characterEffectMap.ContainsKey characterEffectName then
            (characterEffectMap.Item characterEffectName)
            |> List.singleton
            |> List.append model
        elif movementSpeedCalculationMap.ContainsKey characterEffectName then
            (movementSpeedCalculationMap.Item characterEffectName)
            |> determineMovementSpeedEffectForDisplay coreSkillAndAttributeData
            |> MovementSpeedEffectForDisplay
            |> List.singleton
            |> List.append model
        else
            model
    | Remove position -> List.removeAt position model
    | RecalculateMovementSpeed ->
        model
        |> List.map (fun effectForDisplay ->
            CharacterEffectForDisplay.update
                (CharacterEffectForDisplay.Msg.CalculationEffectForDisplayMsg(
                    MovementSpeedEffectForDisplay.Msg.RecalculateMovementSpeed(coreSkillAndAttributeData)
                ))
                effectForDisplay)


open Feliz
open Feliz.Bulma

let view (characterEffectNameList: string list) (model: EffectForDisplay list) (dispatch: Msg -> unit) =
    Bulma.container [
        Bulma.label "Character Effects:"
        Bulma.table [
            table.isBordered
            prop.children [
                Html.thead [
                    List.map
                        (fun (thString: string) -> Html.th thString)
                        [ "Name"
                          "Effect"
                          "Duration"
                          "Source" ]
                    |> Html.tr
                ]
                Html.tableBody (
                    List.mapi
                        (fun position equipmentRow ->
                            let characterEffect =
                                (CharacterEffectForDisplay.view equipmentRow (fun msg ->
                                    dispatch (ModifyCharacterEffect(position, msg))))

                            let deleteEquipmentRowButton =
                                Html.td [
                                    Html.button [
                                        prop.onClick (fun _ -> dispatch (Remove(position)))
                                        prop.text "-"
                                    ]
                                ]
                                |> List.singleton

                            Html.tr (List.append characterEffect deleteEquipmentRowButton))
                        model
                )
                Html.tfoot [
                    Html.div [
                        Bulma.input.text [
                            prop.list "re"
                            prop.onTextChange (fun input -> dispatch (Insert input))
                        ]
                        Html.datalist [
                            prop.id "re"
                            prop.children (
                                List.map
                                    (fun (characterEffect: string) ->
                                        Html.option [
                                            prop.value characterEffect
                                        ])
                                    characterEffectNameList
                            )
                        ]
                    ]
                ]
            ]
        ]
    ]