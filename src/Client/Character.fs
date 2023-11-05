module Character

open Elmish
open Fable.Remoting.Client
open Shared

open FallenLib.Vocation
open FallenLib.Dice
open FallenLib.CoreSkillGroup
open FallenLib.Attribute
open FallenLib.Item
open FallenLib.CombatRoll
open FallenLib.MagicSkill
open FallenLib.MagicCombat
open FallenLib.Range

type Model =
    { name: string
      coreSkillTables: CoreSkillGroups.Model
      vocationTables: VocationTables.Model
      equipmentRowList: EquipmentRowList.Model
      combatRolls: CombatRoll list
      AllItemList: Item list
      magicSkillMap: Map<string, MagicSkill>
      magicCombatMap: Map<string, MagicCombat>
      rangeMap: Map<string, Range> }

let defaultCoreSkillTables: CoreSkillGroups.Model =
    let attribtueStat = AttributeStat.init ()
    let lvl = Neg1To4Stat.init ()
    let dicePool = coreSkillToDicePool baseDicePool lvl attribtueStat.lvl

    [ { attributeStat = { attribtueStat with attribute = "STR" }
        coreSkillList =
          [ { name = "Athletics"
              lvl = lvl
              dicePool = dicePool }
            { name = "Climb"
              lvl = lvl
              dicePool = dicePool }
            { name = "Endurance"
              lvl = lvl
              dicePool = dicePool }
            { name = "Lift"
              lvl = lvl
              dicePool = dicePool } ] }
      { attributeStat = { AttributeStat.init () with attribute = "RFX" }
        coreSkillList =
          [ { name = "Athletics"
              lvl = lvl
              dicePool = dicePool }
            { name = "Climb"
              lvl = lvl
              dicePool = dicePool }
            { name = "Endurance"
              lvl = lvl
              dicePool = dicePool }
            { name = "Lift"
              lvl = lvl
              dicePool = dicePool } ] }
      { attributeStat = { AttributeStat.init () with attribute = "INT" }
        coreSkillList =
          [ { name = "Athletics"
              lvl = lvl
              dicePool = dicePool }
            { name = "Climb"
              lvl = lvl
              dicePool = dicePool }
            { name = "Endurance"
              lvl = lvl
              dicePool = dicePool }
            { name = "Lift"
              lvl = lvl
              dicePool = dicePool } ] } ]

let coreSkillGroupToAttributes (coreSkillTables: CoreSkillGroups.Model) =
    List.map (fun (table: CoreSkillGroup.Model) -> table.attributeStat) coreSkillTables


type Msg =
    | CoreSkillTablesMsg of CoreSkillGroups.Msg
    | VocationTableMsg of VocationTables.Msg
    | SetName of string
    | GotDamageTypes of Item list
    | EquipmentRowListMsg of EquipmentRowList.Msg
    | CombatRollTableMsg of CombatRollTable.Msg

let fallenDataApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IFallenDataApi>

let init () : Model * Cmd<Msg> =
    let attributeStatListTemp = coreSkillGroupToAttributes defaultCoreSkillTables

    { name = "Javk Wick"
      coreSkillTables = defaultCoreSkillTables
      vocationTables = VocationTables.init attributeStatListTemp
      AllItemList = []
      equipmentRowList = EquipmentRowList.init ()
      combatRolls = []
      magicSkillMap = Map.empty
      magicCombatMap = Map.empty
      rangeMap = Map.empty },
    Cmd.OfAsync.perform fallenDataApi.getItems () GotDamageTypes

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | CoreSkillTablesMsg coreSkillTableMsg ->
        let newCoreSkillTables =
            CoreSkillGroups.update coreSkillTableMsg model.coreSkillTables

        { model with
            coreSkillTables = newCoreSkillTables
            vocationTables =
                VocationTables.update
                    (coreSkillGroupToAttributes newCoreSkillTables)
                    VocationTables.Msg.SetAttributeStatsAndCalculateDicePools
                    model.vocationTables },
        Cmd.none

    | VocationTableMsg vocationTableMsg ->
        { model with
            vocationTables =
                VocationTables.update
                    (coreSkillGroupToAttributes model.coreSkillTables)
                    vocationTableMsg
                    model.vocationTables },
        Cmd.none

    | SetName name -> { model with name = name }, Cmd.none
    | GotDamageTypes damageTypes -> { model with AllItemList = damageTypes }, Cmd.none
    | EquipmentRowListMsg equipmentRowListMsg ->
        let newEquipmentRowList =
            EquipmentRowList.update model.AllItemList equipmentRowListMsg model.equipmentRowList

        { model with
            equipmentRowList = newEquipmentRowList
            combatRolls =
                CombatRollTable.update
                    newEquipmentRowList
                    (coreSkillGroupToAttributes model.coreSkillTables)
                    model.vocationTables
                    []
                    [ "STR"; "RFX"; "INT" ]
                    model.magicSkillMap
                    model.magicCombatMap
                    model.rangeMap
                    (CombatRollTable.Msg.RecalculateCombatRolls)
                    model.combatRolls },
        Cmd.none


open Feliz
open Feliz.Bulma

let view (model: Model) (dispatch: Msg -> unit) =

    Bulma.hero [
        hero.isFullHeight
        color.isDanger

        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl
                "https://www.onlygfx.com/wp-content/uploads/2015/12/simple-old-paper-1-transparent.jpg"
            style.backgroundPosition "no-repeat center center fixed"
        ]

        prop.children [

            Bulma.heroHead [
                Bulma.navbar [
                    color.isPrimary
                    prop.children [
                        Bulma.navbarItem.div [
                            Bulma.title.h3 [
                                prop.text "Fallen"
                                prop.style [
                                    style.fontFamily "PT Serif Caption"
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            Bulma.heroBody [

                Bulma.container [
                    Bulma.content [
                        Bulma.input.text [
                            prop.value model.name
                            prop.placeholder "Character Name"
                            prop.onChange (fun newName -> SetName newName |> dispatch)
                            prop.classes [
                                "is-large"
                                "has-text-centered"
                            ]
                        ]
                    ]
                    Bulma.container [
                        CoreSkillGroups.view model.coreSkillTables (CoreSkillTablesMsg >> dispatch)
                    ]
                    Bulma.container [
                        VocationTables.view model.vocationTables (VocationTableMsg >> dispatch)
                    ]
                    Bulma.container [
                        EquipmentRowList.view
                            (List.map (fun (item: Item) -> item.name) model.AllItemList)
                            model.equipmentRowList
                            (EquipmentRowListMsg >> dispatch)
                        |> Bulma.content
                    ]
                    Bulma.container [
                        CombatRollTable.view model.combatRolls (CombatRollTableMsg >> dispatch)
                    ]
                ]
            ]
        ]
    ]