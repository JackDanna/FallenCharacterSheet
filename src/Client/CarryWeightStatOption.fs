module CarryWeightStatOption

open FallenLib.CarryWeightStat
open FallenLib.CarryWeightCalculation
open FallenLib.CoreSkillGroup
open FallenLib.WeightClass
open FallenLib.AttributeDeterminedDiceModEffect

type Msg =
    | CarryWeightStatMsg of CarryWeightStat.Msg
    | SetString of string

let update
    (inventoryWeight: float)
    (coreSkillGroupList: CoreSkillGroup list)
    (carryWeightCalculationMap: Map<string, CarryWeightCalculation>)
    (weightClassList: WeightClass list)
    (msg: Msg)
    (model: CarryWeightStat option)
    : CarryWeightStat option =

    match msg, model with
    | SetString newString, None ->
        if
            Seq.exists
                (fun carryWeightCalculationName -> carryWeightCalculationName = newString)
                carryWeightCalculationMap.Keys then

            let carryWeightCalculation = carryWeightCalculationMap.Item newString
            let maxCarryWeight = calculateCarryWeight carryWeightCalculation coreSkillGroupList

            { currentWeight = inventoryWeight
              carryWeightCalculation = carryWeightCalculation
              maxWeight = maxCarryWeight
              weightClass = determineWeightClass maxCarryWeight inventoryWeight weightClassList }
            |> Some
        else
            None

    | SetString newString, Some carryWeightStat ->
        if
            Seq.exists
                (fun carryWeightCalculationName -> carryWeightCalculationName = newString)
                carryWeightCalculationMap.Keys then
            CarryWeightStat.update
                (CarryWeightStat.Msg.SetCarryWeightCalculation
                    { newCarryWeightCalculationName = newString
                      coreSkillGroupList = coreSkillGroupList
                      carryWeightCalculationMap = carryWeightCalculationMap
                      weightClassList = weightClassList })
                carryWeightStat
            |> Some
        else
            None

    | CarryWeightStatMsg msg, Some carryWeightStat -> CarryWeightStat.update msg carryWeightStat |> Some

    | _ -> model

open Feliz
open Feliz.Bulma

let view (carryWeightCalculationNameList: string list) (model: CarryWeightStat option) (dispatch: Msg -> unit) =

    Bulma.container [
        Bulma.label "Carry Weight Stat:"
        Bulma.table [
            table.isBordered
            prop.children [
                Html.thead [
                    List.map
                        (fun (thString: string) -> Html.th thString)
                        [ "Carry Weight Calculation"
                          "Weight Class"
                          "Effect"
                          "Current Weight"
                          "Max Weight" ]
                    |> Html.tr
                ]
                Html.tableBody [
                    match model with
                    | Some carryWeightStat ->
                        [ carryWeightStat.carryWeightCalculation.name
                          carryWeightStat.weightClass.name
                          (attributeDeterminedDiceModEffectToEffectString
                              carryWeightStat.weightClass.attributeDeterminedDiceModEffect)
                          carryWeightStat.currentWeight |> string
                          carryWeightStat.maxWeight |> string ]
                        |> List.map Html.td
                    | None -> []
                    |> Html.tr
                ]

                Html.tfoot [
                    Html.div [
                        Bulma.input.text [
                            prop.list "carryWeightEffectNameList"
                            prop.onTextChange (fun input -> dispatch (SetString input))
                        ]
                        Html.datalist [
                            prop.id "carryWeightEffectNameList"
                            prop.children (
                                List.map
                                    (fun (carryWeightEffectName: string) ->
                                        Html.option [
                                            prop.value carryWeightEffectName
                                        ])
                                    (List.append [ "None" ] carryWeightCalculationNameList)
                            )
                        ]
                    ]
                ]
            ]
        ]
    ]