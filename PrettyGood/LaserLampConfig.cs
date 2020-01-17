using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TUNING;
using UnityEngine;

namespace LaserLamp
{
    public class MyConst
    {
        public static int Light = 360000;

        public static float Radius = 7f;

        public static float Range = 7f;
    }


    class LaserLampConfig : IBuildingConfig
    {
	    public const string ID = "LaserLamp";

        public override BuildingDef CreateBuildingDef()
        {
            string id = "LaserLamp";

            int width = 1; int height = 1;

            string anim = "ceilinglight_kanim";

            int hitpoints = 10;

            float construction_time = 10f;

            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER1;

            string[] ALL_METALS = MATERIALS.ALL_METALS;

            float melting_point = 800f;

            BuildLocationRule build_location_rule = BuildLocationRule.OnCeiling;

            EffectorValues none = NOISE_POLLUTION.NONE;

            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time,
                tier, ALL_METALS, melting_point, build_location_rule, BUILDINGS.DECOR.BONUS.TIER5, none, 0.2f);

            buildingDef.RequiresPowerInput = true;

            buildingDef.EnergyConsumptionWhenActive = 780f;

            buildingDef.SelfHeatKilowattsWhenActive = .1f;

            buildingDef.ViewMode = OverlayModes.Light.ID;

            buildingDef.AudioCategory = "Metal";

            return buildingDef;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            LightShapePreview lightShapePreview = go.AddComponent<LightShapePreview>();

            lightShapePreview.lux = MyConst.Light;

            lightShapePreview.radius = MyConst.Radius;

            lightShapePreview.shape = LightShape.Circle;

        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.LightSource, false);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LoopingSounds>();

            Light2D light2D = go.AddOrGet<Light2D>();

            light2D.overlayColour = LIGHT2D.CEILINGLIGHT_OVERLAYCOLOR;

            light2D.Color = LIGHT2D.CEILINGLIGHT_COLOR;

            light2D.Range = MyConst.Range;

            light2D.Angle = MyConst.Radius;

            light2D.Direction = LIGHT2D.CEILINGLIGHT_DIRECTION;

            light2D.Offset = LIGHT2D.CEILINGLIGHT_OFFSET;

            light2D.shape = LightShape.Circle;

            light2D.drawOverlay = true;

            light2D.Lux = MyConst.Light;

            go.AddOrGetDef<LightController.Def>();
        }
        
    }
}
