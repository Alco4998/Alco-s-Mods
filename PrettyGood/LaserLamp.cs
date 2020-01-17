using Database;
using Harmony;
using System;
using STRINGS;
using System.Collections.Generic;

namespace LaserLamp
{
    public class LaserLampmod
    {

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        internal class GeneratedBuildings_LoadGeneratedBuildings
        {

            //public static LocString DESC = LocString("This Building Looks Pretty Good", "STRING.BUILDING.PREFAB." + LaserLampConfig.ID.ToUpper() + ".DESC");

            //public static LocString EFFECT = LocString("Pretty Good", "STRING.BUILDING.PREFAB." + LaserLampConfig.ID.ToUpper() + ".EFFECT");

            private static void Prefix()
            {
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{LaserLampConfig.ID.ToUpper()}.NAME", UI.FormatAsLink("Laser Lamp", LaserLampConfig.ID));
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{LaserLampConfig.ID.ToUpper()}.EFFECT", "A very powerful lamp");
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{LaserLampConfig.ID.ToUpper()}.DESC", "A Light that is really powerfull but hot. Keep your dupes under it too long and they will Get Sun burn");


                ModUtil.AddBuildingToPlanScreen("Power", LaserLampConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            private static void Prefix()
            {
                var techList = new List<string>(Database.Techs.TECH_GROUPING["InteriorDecor"]) { LaserLampConfig.ID };
                Database.Techs.TECH_GROUPING["InteriorDecor"] = techList.ToArray();
            }
        }
    }
}
