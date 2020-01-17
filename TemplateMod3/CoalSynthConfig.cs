using STRINGS;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace CoalSynth
{
    public class CoalSynthConfig : IBuildingConfig
    {
        public const string ID = "CoalSynth";
        public const float CO2CONS = 0.1f;
        private const float REFILL_RATE = 2400f;
        private const float SAND_STORAGE_AMOUNT = 320.000031f;
        private const float COAL_PER_LOAD = 10f;

        public override BuildingDef CreateBuildingDef()
        {
            string id = "CoalSynth";
            int width = 1;
            int height = 1;
            string anim = "co2filter_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tIER = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
            string[] rAW_MINERALS = MATERIALS.RAW_MINERALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFoundationRotatable;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER0;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, rAW_MINERALS, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, tIER2, 0.2f);
            buildingDef.Overheatable = false;
            buildingDef.ViewMode = OverlayModes.Oxygen.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            Prioritizable.AddRef(go);
            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showInUI = true;
            storage.capacityKg = 50f;
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            ElementConsumer elementConsumer = go.AddOrGet<ElementConsumer>();   
            elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
            elementConsumer.consumptionRate = 0.3f;
            elementConsumer.consumptionRadius = 3;
            elementConsumer.showInStatusPanel = true;
            elementConsumer.sampleCellOffset = new Vector3(0f, 0f, 0f);
            elementConsumer.isRequired = false;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showDescriptor = false;
            ElementDropper elementDropper = go.AddComponent<ElementDropper>();
            elementDropper.emitMass = 10f;
            elementDropper.emitTag = new Tag("Carbon");
            elementDropper.emitOffset = new Vector3(0f, 0f, 0f);
            ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[]
            {
            //new ElementConverter.ConsumedElement(new Tag("Filter"), 0.13333334f),
            new ElementConverter.ConsumedElement(new Tag("CarbonDioxide"), 0.25f)
            };
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(0.0833333f, SimHashes.Carbon, 0f, false, true, 0f, 0.5f, 0.25f, 255, 0),
                new ElementConverter.OutputElement(0.1666667f, SimHashes.Oxygen, 0f, false, false, 0f, 0f, 0.75f, 255, 0)
            };
            CoalSynth airFilter = go.AddOrGet<CoalSynth>();
            airFilter.filterTag = new Tag("CarbonDioxide");
            go.AddOrGet<KBatchedAnimController>().randomiseLoopedOffset = true;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<ActiveController.Def>();
        }
    }
}
