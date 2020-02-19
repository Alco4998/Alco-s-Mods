using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace StirlingGenerator
{
    public class StirlingConst
    {
        public static float MAX_WATTAGE = 850f;

        public static readonly List<Storage.StoredItemModifier> StoredItemModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Insulate,
            Storage.StoredItemModifier.Seal
        };
    }

    public class StirlingGeneratorConfig : IBuildingConfig
    {
        public const string ID = "StirlingGenerator";

        private static readonly List<Storage.StoredItemModifier> StoredItemModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Insulate,
            Storage.StoredItemModifier.Seal
        };

        public override BuildingDef CreateBuildingDef()
        {
            string id = "StirlingGenerator";
            int width = 2;
            int height = 2;
            string anim = "liquidconditioner_kanim";
            int hitpoints = 100;
            float construction_time = 120f;
            float[] tIER = BUILDINGS.CONSTRUCTION_MASS_KG.TIER6;
            string[] aLL_METALS = MATERIALS.ALL_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER2;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, aLL_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, tIER2, 0.2f);
            buildingDef.GeneratorWattageRating = 1000f;
            buildingDef.GeneratorBaseCapacity = 1000f;
            buildingDef.SelfHeatKilowattsWhenActive = -585.06f;
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.PermittedRotations = PermittedRotations.FlipH;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.OverheatTemperature = 398.15f;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            StirlingGenerator generator = go.AddOrGet<StirlingGenerator>();


            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.consumptionRate = 10f;

            Storage storage = BuildingTemplates.CreateDefaultStorage(go, false);
            storage.showInUI = true;
            storage.capacityKg = 2f * conduitConsumer.consumptionRate;
            storage.SetDefaultStoredItemModifiers(StirlingGeneratorConfig.StoredItemModifiers);

            SolarPanel stirling = go.AddOrGet<SolarPanel>();
            stirling.powerDistributionOrder = 8;

            //EnergyGenerator energyGenerator = go.AddOrGet<EnergyGenerator>();
            //energyGenerator.powerDistributionOrder = 8;
            //energyGenerator.ignoreBatteryRefillPercent = true;
            //energyGenerator.meterOffset = Meter.Offset.Behind;

            go.AddOrGet<MinimumOperatingTemperature>().minimumTemperature = 273.15f;


            Tinkerable.MakePowerTinkerable(go);
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}