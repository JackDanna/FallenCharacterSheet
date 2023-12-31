namespace Shared

open FallenLib.Item
open FallenLib.MagicSkill
open FallenLib.MagicCombat
open FallenLib.Range
open FallenLib.EffectForDisplay
open FallenLib.CarryWeightCalculation
open FallenLib.MovementSpeedEffect
open FallenLib.WeightClass
open FallenLib.Attribute
open FallenLib.CoreSkill
open FallenLib.ItemStack

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type FallenData =
    { defaultAttributeList: Attribute List
      defaultCoreSkillList: CoreSkill List
      allItemStackList: ItemStack list
      magicSkillMap: Map<string, MagicSkill>
      magicCombatMap: Map<string, MagicCombat>
      rangeMap: Map<string, Range>
      combatVocationalSkill: string list
      effectForDisplayMap: Map<string, EffectForDisplay>
      carryWeightCalculationMap: Map<string, CarryWeightCalculation>
      weightClassList: WeightClass List
      movementSpeedCalculationMap: Map<string, MovementSpeedCalculation> }

type IFallenDataApi =
    { getInitData: unit -> Async<FallenData> }