using Database;
using Harmony;
using System;
using STRINGS;
using System.Collections.Generic;

namespace thermalReducer
{
    public class thermalReducermod
    {

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        internal class GeneratedBuildings_LoadGeneratedBuildings
        {

            private static void Prefix()
            {
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{thermalReducerConfig.ID.ToUpper()}.NAME", UI.FormatAsLink("Laser Lamp", thermalReducerConfig.ID));
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{thermalReducerConfig.ID.ToUpper()}.EFFECT", "A very powerful lamp");
                Strings.Add($"STRINGS.BUILDINGS.PREFABS.{thermalReducerConfig.ID.ToUpper()}.DESC", "A Light that is really powerfull but hot. Keep your dupes under it too long and they will Get Sun burn");


                ModUtil.AddBuildingToPlanScreen("Power", thermalReducerConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            private static void Prefix()
            {
                var techList = new List<string>(Database.Techs.TECH_GROUPING["InteriorDecor"]) { thermalReducerConfig.ID };
                Database.Techs.TECH_GROUPING["InteriorDecor"] = techList.ToArray();
            }
        }
    }
}
