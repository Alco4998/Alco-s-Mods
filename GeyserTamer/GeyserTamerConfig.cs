using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using TUNING;

namespace GeyserTamer
{
    class GeyserTamerConfig : IBuildingConfig
    {
        public const string ID = "GeyserTamer";
        public const string NAME = "GeyserTamer";
        public const string EFFECT = "GeyserTamer";
        public const string DESCRIPTION = "FEEESH";

        public override BuildingDef CreateBuildingDef()
        {
            //Building info
            int width = 5;
            int height = 5;
            string anim = "spaceheater_kanim";
            int hitpoints = 50;
            float Contime = 100f;
            float[] ConstructMass = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] Materials = MATERIALS.REFINED_METALS;
            EffectorValues loudness = NOISE_POLLUTION.NOISY.TIER5;
            EffectorValues deco = BUILDINGS.DECOR.PENALTY.TIER1;
            BuildLocationRule BuildRule = BuildLocationRule.Anywhere;

            //Building
            BuildingDef building = BuildingTemplates.CreateBuildingDef
                (
                    ID, width, height, anim, hitpoints, Contime, ConstructMass,
                    Materials, 1000f, BuildRule, loudness, deco, -10f
                );

            //Power
            building.PowerInputOffset = new CellOffset(0, 0);
            building.RequiresPowerInput = true;
            building.EnergyConsumptionWhenActive = 1000f;

            //Functional
            building.ExhaustKilowattsWhenActive = -4000f;
            building.ThermalConductivity = 800f;
            building.Floodable = false;

            //Other
            building.AudioCategory = "HollowMetal";
            building.AudioSize = "large";

            return building;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<MinimumOperatingTemperature>().minimumTemperature = 10f;
            go.AddOrGet<LoopingSounds>();
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_1);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_1);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
            SpaceHeater spaceHeater = go.AddOrGet<SpaceHeater>();
            spaceHeater.targetTemperature = 5000f;
        }
    }
}
