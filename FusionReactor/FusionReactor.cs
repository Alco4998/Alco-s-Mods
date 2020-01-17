﻿using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using Harmony;
using UnityEngine;

namespace FusionReactor
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{FusionReactorConfig.ID.ToUpper()}.NAME", string.Concat("Fusion Reactor", UI.FormatAsLink("Stellarator", FusionReactorConfig.ID)));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{FusionReactorConfig.ID.ToUpper()}.EFFECT", string.Concat("Smashing Atoms together"));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{FusionReactorConfig.ID.ToUpper()}.DESC", string.Concat("This Fusion Reactor is a Highly Powerful Generator that Gets really Hot.\nit's based on the Stellarator fusion reactor and uses Stellar nucleosynthesis to create Metal from Hydrogen"));

            ModUtil.AddBuildingToPlanScreen("Power", FusionReactorConfig.ID);
        }
    }

    [HarmonyPatch(typeof(Db))]
    [HarmonyPatch("Initialize")]
    public static class Db_Initialize_Patch
    {
        private static void Prefix()
        {
            var techList = new List<string>(Database.Techs.TECH_GROUPING["RenewableEnergy"]) { FusionReactorConfig.ID };
            Database.Techs.TECH_GROUPING["RenewableEnergy"] = techList.ToArray();
        }
    }

    /*[HarmonyPatch(typeof(SingleSliderSideScreen), "isvalidfortarget")]
    internal class SingleSliderSideScreen_IsValidForTarget
    {
        public bool prefix()
        {
            
        }
    }*/
}
