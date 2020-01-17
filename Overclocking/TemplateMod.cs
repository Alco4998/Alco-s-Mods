using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;

namespace Overclocking
{
    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    internal class TemplateMod_Game_OnPrefabInit
    {
        private static void Postfix()
        {
            Debug.Log(" === TemplateMod_Game_OnPrefabInit Postfix === ");
        }
    }
}
