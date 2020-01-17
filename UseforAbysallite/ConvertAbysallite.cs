using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using STRINGS;

namespace UseforAbysallite
{
    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    internal class TemplateMod_Game_OnPrefabInit
    {
        private static void Postfix()
        {
        ComplexRecipe.RecipeElement[] In = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Katairite.CreateTag(), 50f)
            };

        ComplexRecipe.RecipeElement[] Out = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.TempConductorSolid.CreateTag(), 100f)
            };

            string id = ComplexRecipeManager.MakeRecipeID(SupermaterialRefineryConfig.ID, In, Out);
            ComplexRecipe complex = new ComplexRecipe(id, In, Out);
            complex.time = 1f;
            complex.description = "Turn " + UI.FormatAsLink("Abyssalite", "KATAIRITE") + " Into " + UI.FormatAsLink("Thermium", "TEMPCONDUCTORSOLID") + " With the Power of Science" ;
            complex.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
            complex.sortOrder = 10;
            complex.fabricators = new List<Tag> { SupermaterialRefineryConfig.ID };

            //─────────────────────────────────Thermium/\───────SuperInsulator\/───────────────────────

            ComplexRecipe.RecipeElement[] In2 = new ComplexRecipe.RecipeElement[]
            {
                new ComplexRecipe.RecipeElement(SimHashes.Katairite.CreateTag(), 50f)
            };

            ComplexRecipe.RecipeElement[] Out2 = new ComplexRecipe.RecipeElement[]
                {
                new ComplexRecipe.RecipeElement(SimHashes.SuperInsulator.CreateTag(), 100f)
                };

            string id2 = ComplexRecipeManager.MakeRecipeID(SupermaterialRefineryConfig.ID, In2, Out2);
            ComplexRecipe complex2 = new ComplexRecipe(id2, In2, Out2);
            complex2.time = 1f;
            complex2.description = "Turn " + UI.FormatAsLink("Abyssalite", "KATAIRITE") + " Into " + UI.FormatAsLink("Insulation", "SUPERINSULATOR") + " With the Power of Science";
            complex2.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
            complex2.sortOrder = 12;
            complex2.fabricators = new List<Tag> { SupermaterialRefineryConfig.ID };
        }
    }
}
