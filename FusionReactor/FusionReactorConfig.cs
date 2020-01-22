using System;
using TUNING;
using UnityEngine;

namespace FusionReactor
{
    public class FusionReactorConfig : IBuildingConfig
    {
            public const string ID = "FusionReactor";

            private const float IRON_CREATE_RATE = 0.01818f;

            private const float HYDROGEN_BURN_RATE = 1f;

            private const float STORAGE_SIZE = 50f;

            public const float CO2_OUTPUT_TEMPERATURE = 383.15f;

            public override BuildingDef CreateBuildingDef()
            {
                string id = "FusionReactor";
                int width = 3;
                int height = 3;
                string anim = "generatorphos_kanim";
                int hitpoints = 100;
                float construction_time = 120f;
                float[] tIER = BUILDINGS.CONSTRUCTION_MASS_KG.TIER5;
                string[] aLL_METALS = MATERIALS.ALL_METALS;
                float melting_point = 2400f;
                BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
                EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER5;
                BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, aLL_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER2, tIER2, 0.2f);
                buildingDef.GeneratorWattageRating = 2000f;
                buildingDef.GeneratorBaseCapacity = 2000f;
                buildingDef.SelfHeatKilowattsWhenActive = 300f;
                buildingDef.ViewMode = OverlayModes.Power.ID;
                buildingDef.InputConduitType = ConduitType.Gas;
                buildingDef.UtilityInputOffset = new CellOffset(-1, 0); 
                buildingDef.AudioCategory = "HollowMetal";
                buildingDef.AudioSize = "large";
                buildingDef.Overheatable = false;
                return buildingDef;
            }

            public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
            {
                go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
                EnergyGenerator energyGenerator = go.AddOrGet<EnergyGenerator>();

                energyGenerator.formula = EnergyGenerator.CreateSimpleFormula(
                    SimHashes.Hydrogen.CreateTag(), HYDROGEN_BURN_RATE, 10f,
                    SimHashes.Iron, IRON_CREATE_RATE, true, new CellOffset(0, 0), 473.15f);
            
                energyGenerator.meterOffset = Meter.Offset.Behind;
                energyGenerator.powerDistributionOrder = 9;

                energyGenerator.ignoreBatteryRefillPercent = true;
                Storage storage = go.AddOrGet<Storage>();
                storage.capacityKg = STORAGE_SIZE;

                ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
                conduitConsumer.conduitType = ConduitType.Gas;
                conduitConsumer.consumptionRate = 2f;
                conduitConsumer.capacityKG = 8f;
                conduitConsumer.forceAlwaysSatisfied = true;
                conduitConsumer.capacityTag = GameTagExtensions.Create(SimHashes.Hydrogen);
                conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Dump;

                ElementDropper elementDropper = go.AddOrGet<ElementDropper>();
                elementDropper.emitTag = new Tag("Iron");
                elementDropper.emitMass = 10f;
                elementDropper.emitOffset = new Vector3(0f, 0f, 0f);

                go.AddOrGet<LoopingSounds>();
                Prioritizable.AddRef(go);
                Tinkerable.MakePowerTinkerable(go);
                go.AddOrGet<MinimumOperatingTemperature>().minimumTemperature = 373.15f;
            }

            public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
            {
                GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_0);
            }

            public override void DoPostConfigureUnderConstruction(GameObject go)
            {
                GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_0);
            }

            public override void DoPostConfigureComplete(GameObject go)
            {
                GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_0);
                go.AddOrGet<LogicOperationalController>();
                go.AddOrGetDef<PoweredActiveController.Def>();
            }
    }
}
