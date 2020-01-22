using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using STRINGS;

namespace GeyserTamer
{
    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    internal class TemplateMod_Game_OnPrefabInit
    {
        private static void Postfix(Game __instance)
        {
            Debug.Log(" === GeyserTame V1 === ");



        }
    }

    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{GeyserTamerConfig.ID.ToUpper()}.NAME", UI.FormatAsLink(GeyserTamerConfig.NAME, GeyserTamerConfig.ID));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{GeyserTamerConfig.ID.ToUpper()}.EFFECT", GeyserTamerConfig.EFFECT);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{GeyserTamerConfig.ID.ToUpper()}.DESC", GeyserTamerConfig.DESCRIPTION);

            ModUtil.AddBuildingToPlanScreen("Oxygen", GeyserTamerConfig.ID);
        }
    }

    [HarmonyPatch(typeof(Db))]
    [HarmonyPatch("Initialize")]
    public static class Db_Initialize_Patch
    {
        private static void Prefix()
        {
            var techList = new List<string>(Database.Techs.TECH_GROUPING["ImprovedOxygen"]) { GeyserTamerConfig.ID };
            Database.Techs.TECH_GROUPING["ImprovedOxygen"] = techList.ToArray();
        }
    }
}
