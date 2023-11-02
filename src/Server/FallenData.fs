namespace FallenData

module Data =
    open FSharp.Data

    open FallenLib.Damage
    open FallenLib.Penetration
    open FallenLib.EngageableOpponents
    open FallenLib.Range
    open FallenLib.ResourceClass
    open FallenLib.Attribute
    open FallenLib.MagicSkill
    open FallenLib.MagicCombat
    open FallenLib.Dice
    open FallenLib.WeaponClass
    open FallenLib.ConduitClass
    open FallenLib.WeaponResourceClass    
    open FallenLib.ItemTier
    open FallenLib.Item
    open FallenLib.Equipment
    open FallenLib.Neg1To4
    open FallenLib.SkillStat
    open FallenLib.Vocation
    open FallenLib.Character
    open FallenLib.TypeUtils
    open FallenLib.MovementSpeedCalculation
    open FallenLib.Effects
    open FallenLib.AreaOfEffect
    open FallenLib.DefenseClass
    open FallenLib.CarryWeightCalculation

    let makeFallenDataPath fileName =
        __SOURCE_DIRECTORY__ + "/FallenData/" + fileName

    let makeFallenData fileName mappingFunc =
        CsvFile.Load(makeFallenDataPath fileName, hasHeaders = true).Rows
        |> Seq.map (mappingFunc) 
        |> List.ofSeq

    let Bool boolString =
        match boolString with
        | "TRUE" -> true
        | "FALSE" -> false
        | _-> failwith("Error: returns " + boolString)

    // DamageTypeData
    let damageTypeData = 
        makeFallenData
            "DamageTypeData.csv"
            (fun row -> (DamageType row.["desc"]))
    let stringToDamageTypeArray =
        damageTypeData
        |> stringListToTypeMap 
        |> stringAndMapToDamageTypeArray 

    // EngageableOpponents
    let engageableOpponentsCalculationData = 
        makeFallenData 
            "EngageableOpponentsCalculationData.csv"
            (fun row -> {
                name = string row.["desc"]
                combatRollDivisor = uint row.["combatRollDivisor"]
                maxEO = mapMaxEO row.["maxEO"]
            })

    let engageableOpponentsMap = 
        eoMap (eoCalculationListToMap engageableOpponentsCalculationData)

    // Range
    let calculatedRangeData = 
        makeFallenData
            "CalculatedRangeData.csv"
            (fun row -> {
                name = string row.["desc"]
                effectiveRange = uint row.["effectiveRange"]
                maxRange =  uint row.["maxRange"]
            })

    let rangeCalculationData = 
        makeFallenData
            "RangeCalculationData.csv"
            (fun row -> {
                name = string row.["desc"]
                numDicePerEffectiveRangeUnit = uint row.["numDicePerEffectiveRangeUnit"]
                ftPerEffectiveRangeUnit = uint row.["ftPerEffectiveRangeUnit"]
                roundEffectiveRangeUp = Bool row.["roundEffectiveRangeUp"]
                maxRange = uint row.["maxRange"]
            })

    let rangeMap = createRangeMap calculatedRangeData rangeCalculationData

    let rangeOptionMap string =
            match string with
            | "None" -> None
            | _      -> rangeMap.Item string |> Some
    
    // ResourceClass
    let resourceClassData =
        makeFallenData
            "ResourceClassData.csv"
            (fun row -> (ResourceClass row.["desc"]))

    let resourceClassMap = stringListToTypeMap resourceClassData

    let weaponResourceClassOptionMap string =
        match string with
        | "None" -> None
        | _      -> Some <| resourceClassMap.Item string

    // Attribute
    let attributeData =
        makeFallenData
            "AttributeData.csv"
            (fun row -> Attribute row.["desc"])
        
    let attributeMap = stringListToTypeMap attributeData

    let mapAndStringToAttributes (attributeMap:Map<string,Attribute>) (input) = 
            String.filter ((<>)' ') input
            |> (fun s -> s.Split(',', System.StringSplitOptions.RemoveEmptyEntries))
            |> List.ofArray
            |> List.map (fun attributeString -> attributeMap.Item attributeString)

    let stringToAttributes = mapAndStringToAttributes attributeMap

    // MagicSkill
    let magicSkillData = 
        makeFallenData
            "MagicSkillData.csv"
            (fun row -> {
                name = string row.["desc"]
                damageTypes = stringToDamageTypeArray (string row.["damageTypes"])
                rangeAdjustment = int row.["rangeAdjustment"]
                isMeleeCapable = Bool row.["meleeCapable"]
                magicResourceClass = string     row.["magicResourceClass"]
            })
    
    let magicSkillMap =
        List.map (fun (magicSkill:MagicSkill) -> magicSkill.name, magicSkill ) magicSkillData
        |> Map.ofList

    // MagicCombat
    let magicCombatData = 
        makeFallenData
            "MagicCombatData.csv"
            (fun row -> {
                name                  = string row.["Description"]
                lvlRequirment         = int row.["Lvl Requirment"] |> intToNeg1To4
                diceModification      = string row.["Dice Modification"] |> stringToDicePoolModification
                penetration           = uint row.["Penetration"]
                range                 = rangeMap.Item (string row.["Range"])
                engageableOpponents   = engageableOpponentsMap row.["Engageable Opponents"]
                minResourceRequirment = uint row.["Resource Requirment"]
                areaOfEffect          = AreaOfEffectOptionMap.Item row.["Area Of Effect"]
            })

    // WeaponClass
    let weaponClassData =
        makeFallenData
            "WeaponClassData.csv"
            (fun row -> {
                name                = string row.["desc"]
                oneHandedWeaponDice = stringToDicePoolModificationOption row.["oneHandedWeaponDice"]
                twoHandedWeaponDice = stringToDicePoolModification row.["twoHandedWeaponDice"]
                penetration         = uint row.["penetration"]
                range               = rangeMap.Item row.["range"]
                damageTypes         = stringToDamageTypeArray row.["damageTypes"]
                engageableOpponents = engageableOpponentsMap row.["engageableOpponents"]
                dualWieldableBonus  = stringToDicePoolModificationOption row.["dualWieldableBonus"]
                areaOfEffect        = AreaOfEffectOptionMap.Item row.["areaOfEffect"]
                resourceClass       = weaponResourceClassOptionMap row.["resourceClass"]
            })
    
    let weaponClassMap =
        List.map (fun (weaponClass:WeaponClass) -> weaponClass.name, weaponClass ) weaponClassData
        |> Map.ofList
        
    // ConduitClass
    let conduitClassData =
        makeFallenData
            "ConduitClassData.csv"
            (fun row -> {
                name = string row.["desc"]
                oneHandedDice = stringToDicePoolModificationOption row.["oneHandedDice"]
                twoHandedDice = stringToDicePoolModification row.["twoHandedDice"]
                penetration = uint row.["penetration"]
                rangeAdjustment = int row.["rangeAdjustment"]
                damageTypes = stringToDamageTypeArray row.["damageTypes"]
                engageableOpponents = match row.["engageableOpponents"] with | "None" -> None | _ -> Some <| engageableOpponentsMap row.["engageableOpponents"]
                dualWieldableBonus = stringToDicePoolModificationOption row.["dualWieldableBonus"]
                areaOfEffect = AreaOfEffectOptionMap.Item row.["areaOfEffect"]
                resourceClass = weaponResourceClassOptionMap row.["resourceClass"]
                governingAttributes = stringToAttributes row.["governingAttributes"]
                effectedMagicSkills =
                    row.["effectedMagicSkills"].Split ", "
                    |> List.ofArray
                    |> List.map ( fun magicSkillStr -> magicSkillMap.Item magicSkillStr )
            })

    let conduitClassMap =
        List.map (fun (conduitClass:ConduitClass) -> conduitClass.name, conduitClass ) conduitClassData
        |> Map.ofList

    // DefenseClass
    let defenseClassData : DefenseClass list = 
        makeFallenData
            "DefenseClassData.csv"
            (fun row -> {
                name = string row.["desc"]
                physicalDefense = float row.["physicalDefense"]
                mentalDefense = float row.["mentalDefense"]
                spiritualDefense = float row.["spiritualDefense"]
            })

    let defenseClassMap =
        List.map (fun (defenseClass:DefenseClass) -> defenseClass.name, defenseClass ) defenseClassData
        |> Map.ofList
    
    // WeaponResourceClass
    let weaponResourceClassData = 
        makeFallenData
            "WeaponResourceClassData.csv"
            (fun row -> {
                name = string row.["desc"]
                resourceClass = resourceClassMap.Item row.["resourceClass"]
                resourceDice = stringToDicePoolModification row.["resourceDice"]
                penetration = uint row.["penetration"]
                range = rangeOptionMap row.["range"]
                damageTypes = stringToDamageTypeArray row.["damageTypes"]
                areaOfEffect = AreaOfEffectOptionMap.Item row.["areaOfEffect"]
            })
    
    let weaponResourceClassMap =
        List.map (fun (weaponResourceClass:WeaponResourceClass) -> weaponResourceClass.name, weaponResourceClass) weaponResourceClassData
        |> Map.ofList

    // ItemTier
    let itemTierData = 
        makeFallenData
            "ItemTierData.csv"
            (fun row -> {
                name = string row.["desc"]
                level = int row.["level"]
                runeSlots = uint row.["runeSlots"]
                baseDice = stringToDicePool row.["baseDice"]
                durabilityMax = uint row.["durabilityMax"]
            })
    
    let itemTierMap =
        List.map (fun (itemTier:ItemTier) -> itemTier.name, itemTier) itemTierData
        |> Map.ofList

    // Item
    let stringToItemClassList (weaponClassMap:Map<string,WeaponClass>) (conduitClassMap:Map<string,ConduitClass>) (weaponResourceClassMap:Map<string,WeaponResourceClass>) (input:string)  =
            input.Split ", "
            |> List.ofArray
            |> List.collect ( fun className ->
                match className with
                | weaponClassName  when weaponClassMap.Keys.Contains weaponClassName  -> weaponClassMap.Item weaponClassName |> WeaponClass |> List.singleton
                | conduitClassName when conduitClassMap.Keys.Contains conduitClassName -> conduitClassMap.Item conduitClassName |> ConduitClass |> List.singleton
                | weaponResourceClassName when weaponResourceClassMap.Keys.Contains weaponResourceClassName -> weaponResourceClassMap.Item weaponResourceClassName |> WeaponResourceClass |> List.singleton
                | defenseClassName when defenseClassMap.Keys.Contains defenseClassName -> defenseClassMap.Item defenseClassName |> DefenseClass |> List.singleton
                | _ -> []
            )

    let itemData = 
        makeFallenData
            "ItemData.csv"
            (fun row -> {
                name = string row.["desc"]
                itemClasses =
                    stringToItemClassList
                        weaponClassMap 
                        conduitClassMap 
                        weaponResourceClassMap 
                        row.["itemClasses"]
                itemTier = itemTierMap.Item row.["itemTier"]
                value = string row.["value"]
                weight = float  row.["weight"]
            })
    
    // MovementSpeedCalculation
    let movementSpeedCalculationData =
        makeFallenData
            "MovementSpeedCalculationData.csv"
            (fun row -> {
                name = string row.["desc"]
                baseMovementSpeed = uint row.["baseMovementSpeed"]
                governingAttributes = stringToAttributes row.["governingAttributes"]
                feetPerAttributeLvl = uint row.["feetPerAttributeLvl"]
                governingSkill = string row.["governingSkill"]
                feetPerSkillLvl = uint row.["feetPerSkillLvl"]
            })

    let carryWeightCalculationData = 
        makeFallenData
            "CarryWeightCalculationData.csv"
            (fun row -> (
                string row.["desc"],
                uint   row.["baseWeight"],
                string row.["governingAttribute"],
                uint   row.["weightIncreasePerAttribute"],
                string row.["governingSkill"],
                uint   row.["weightIncreasePerSkill"]
            ))

    let weightClassData =
        makeFallenData
            "WeightClassData.csv"
            (fun row -> (
                string row.["name"],
                float  row.["bottomPercent"],
                float  row.["topPercent"],
                float  row.["percentOfMovementSpeed"]
               
            ))

    let attributeDeterminedDiceModData =
        makeFallenData
            "AttributeDeterminedDiceModData.csv"
            (fun row -> (
                string row.["name"],
                string row.["attributesToEffect"],
                string row.["dicePoolModification"]
            ))

    let skillStatData = [|
        ("Endurance",          1, "STR")
        ("Athletics",          3, "STR")
        ("Climb",              1, "STR")
        ("Swim",               2, "STR")
        ("Lift",              -1, "STR")
        ("Perception",         2, "RFX")
        ("Acrobatics",         2, "RFX")
        ("Ride/Pilot",        -1, "RFX")
        ("Sleight of Hand",   -1, "RFX")
        ("Stealth",            0, "RFX")
        ("General Knowledge",  4, "INT")
        ("Willpower",          1, "INT")
        ("Communication",      1, "INT")
        ("Spiritual",          1, "INT")
        ("Survival",           1, "INT")
    |]
