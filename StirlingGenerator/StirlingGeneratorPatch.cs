using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using STRINGS;
using UnityEngine;

namespace StirlingGenerator
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{StirlingGeneratorConfig.ID.ToUpper()}.NAME", UI.FormatAsLink("Stirling Generator", StirlingGeneratorConfig.ID));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{StirlingGeneratorConfig.ID.ToUpper()}.EFFECT", "Generates Power from piped liquids");
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{StirlingGeneratorConfig.ID.ToUpper()}.DESC", string.Concat(new string[]
                        {
                            "Cools the ",
                            UI.FormatAsLink("Liquid", "ELEMENTS_LIQUID"),
                            " piped through it and Deletes ",
                            UI.FormatAsLink("Heat", "HEAT")
                        }));


            ModUtil.AddBuildingToPlanScreen("Power", StirlingGeneratorConfig.ID);

        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            private static void Prefix()
            {
                var techList = new List<string>(Database.Techs.TECH_GROUPING["RenewableEnergy"]) { StirlingGeneratorConfig.ID };
                Database.Techs.TECH_GROUPING["RenewableEnergy"] = techList.ToArray();
            }
        }

        [HarmonyPatch(typeof(SingleSliderSideScreen), "IsValidForTarget")]
        public class SingleSliderSideScreen_IsValidForTarget
        {
            public static void Postfix(ref bool __result, GameObject target)
            {
                if (target.GetComponent<KPrefabID>() != null)
                {

                    KPrefabID component = target.GetComponent<KPrefabID>();

                    if (component.HasTag("StirlingGenerator".ToTag()))
                    {
                        __result = false;
                    }
                }
            }
        }
    }
}

