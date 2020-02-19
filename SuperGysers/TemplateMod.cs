using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;

namespace SuperGeysers
{
    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    internal class TemplateMod_Game_OnPrefabInit
    {
        private static void Postfix(Game __instance)
        {
            Debug.Log(" === TemplateMod_Game_OnPrefabInit Postfix === ");
        }
    }

    [HarmonyPatch(typeof(MethaneGeneratorConfig), "CreatePrefab")]
    public class MethaneGeneratorConfig_CreatePrefab
    {
        private static void Postfix(GameObject gameObject)
        {
            var geyserConfigurator = gameObject.AddOrGet<GeyserConfigurator>();
            geyserConfigurator.presetMax = 12f;
        }
    }
}
