using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using STRINGS;

namespace Inductornator
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{InductorConfig.ID.ToUpper()}.NAME", UI.FormatAsLink(InductorConfig.NAME, InductorConfig.ID));

            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{InductorConfig.ID.ToUpper()}.EFFECT",
            
                string.Concat(
                    "Produces ",
                    UI.FormatAsLink("Molten Refined Metal", "REFINEDMETAL"),
                    " from raw ",
                    UI.FormatAsLink("Metal Ore", "RAWMETAL"),
                    "\nbut ",
                    UI.FormatAsLink("Heats", "HEAT"),
                    " itself up significantly"
                )); 

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
