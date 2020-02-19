using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace Coolerizer
{
    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    internal class TemplateMod_Game_OnPrefabInit
    {
        private static void Postfix(Game __instance)
        {
            Debug.Log(" === Inductornator XL Ultra Don't @ me === ");
        }
    }

    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{InductorConfig.ID.ToUpper()}.NAME", UI.FormatAsLink(InductorConfig.NAME, InductorConfig.ID));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{InductorConfig.ID.ToUpper()}.EFFECT", InductorConfig.EFFECT);
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{InductorConfig.ID.ToUpper()}.DESC", InductorConfig.DESCRIPTION);

            ModUtil.AddBuildingToPlanScreen("Refining", InductorConfig.ID);
        }
    }

    [HarmonyPatch(typeof(Db))]
    [HarmonyPatch("Initialize")]
    public static class Db_Initialize_Patch
    {
        private static void Prefix()
        {
            var techList = new List<string>(Database.Techs.TECH_GROUPING["HighTempForging"]) { InductorConfig.ID };
            Database.Techs.TECH_GROUPING["HighTempForging"] = techList.ToArray();
        }
    }
}
