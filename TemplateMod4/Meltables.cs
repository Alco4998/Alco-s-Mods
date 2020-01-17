using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STRINGS;
using Harmony;

namespace TemplateMod4
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
                new ComplexRecipe.RecipeElement(SimHashes.MoltenNiobium.CreateTag(), 100f)
            };

            string id = ComplexRecipeManager.MakeRecipeID(GlassForgeConfig.ID, In, Out);
            ComplexRecipe complex = new ComplexRecipe(id, In, Out);
            complex.time = 1f;
            complex.description = "Turn " + UI.FormatAsLink("Abyssalite", "KATAIRITE") + " Into " + UI.FormatAsLink("Thermium", "TEMPCONDUCTORSOLID") + " With the Power of Science";
            complex.nameDisplay = ComplexRecipe.RecipeNameDisplay.Result;
            complex.sortOrder = 10;
            complex.fabricators = new List<Tag> { TagManager.Create(GlassForgeConfig.ID) };
            Prioritizable.AddRef(go);
        }   
    }
}
