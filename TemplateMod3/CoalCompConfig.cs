using STRINGS;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace CoalComp
{
    public class CoalCompConfig : IBuildingConfig
    {

        public const string ID = "CoalComp";

        public override BuildingDef CreateBuildingDef()
        {
            string id = "CoalComp";
            int width = 3;
            int height = 1;
            string anim = "filter_gas_kanim";
            int hitpoints = 100;
            float construction_time = 60f;
            float[] tIER = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
            string[] aLL_METALS = MATERIALS.ALL_METALS;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER5;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, aLL_METALS, 2400f, BuildLocationRule.Anywhere, BUILDINGS.DECOR.PENALTY.TIER1, tIER2, 0.2f);
            buildingDef.Overheatable = false;
            buildingDef.RequiresPowerInput = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 0);
            buildingDef.EnergyConsumptionWhenActive = 120f;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.InputConduitType = ConduitType.Gas;
            buildingDef.OutputConduitType   = ConduitType.Gas;
            buildingDef.UtilityOutputOffset = new CellOffset(1, 0);
            buildingDef.UtilityInputOffset = new CellOffset(-1, 0);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            Tag tag = SimHashes.CarbonDioxide.CreateTag();
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            CoalComp oxyliteRefinery = go.AddOrGet<CoalComp>();
            oxyliteRefinery.emitTag = SimHashes.Carbon.CreateTag();
            oxyliteRefinery.emitMass = 10f;
            oxyliteRefinery.dropOffset = new Vector3(0f, 1f);
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Gas;
            conduitConsumer.consumptionRate = 1.2f;
            conduitConsumer.capacityTag = tag;
            conduitConsumer.capacityKG = 6f;
            conduitConsumer.forceAlwaysSatisfied = true;
            conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Gas;
            conduitDispenser.elementFilter = new SimHashes[]
            {
                SimHashes.Oxygen
            };
            Storage storage = go.AddOrGet<Storage>();
            storage.capacityKg = 23.2f;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            storage.showInUI = true;
            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[]
            {
                new ElementConverter.ConsumedElement(tag, 1f)
            };
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(.25f, SimHashes.Carbon, 0f, false, true, 0f, 0.5f, 0.25f, 255, 0),
                new ElementConverter.OutputElement(.75f, SimHashes.Oxygen, 0f, false, true, 0f, 0f, 0.75f, 255, 0)
            };
            

            Prioritizable.AddRef(go);
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
