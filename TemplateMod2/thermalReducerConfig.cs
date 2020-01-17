using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using TUNING;
using UnityEngine;

namespace thermalReducer
{
    class thermalReducerConfig : IBuildingConfig
    {

        public const string ID = "thermalReducer";

        private static readonly List<Storage.StoredItemModifier> StoredItemModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Insulate,
            Storage.StoredItemModifier.Seal
        };

        public override BuildingDef CreateBuildingDef()
        {
            string id = "thermalReducer";
            int width = 2;
            int height = 2;
            string anim = "spaceheater_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tIER = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] aLL_METALS = MATERIALS.ALL_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER2;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, aLL_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.BONUS.TIER1, tIER2, 0.2f);
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 120f;
            buildingDef.ExhaustKilowattsWhenActive = -200f;
            buildingDef.SelfHeatKilowattsWhenActive = -200f;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.ThermalConductivity = 1000f;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            ThermalReducer spaceHeater = go.AddOrGet<ThermalReducer>();
            spaceHeater.targetTemperature = 10f;
            go.AddOrGet<MinimumOperatingTemperature>().minimumTemperature = 20f;
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
