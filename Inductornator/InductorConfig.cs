using STRINGS;
using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace Inductornator
{
    public class InductorConfig : IBuildingConfig
    {
        public const string ID = "Inductor";
        public const string NAME = "Inductor";
        public const string EFFECT = "GeyserTamer";
        public const string DESCRIPTION = "FEEESH";
        public const string RECIPE_DESCRIPTION = "Melts {0} into {1}";

        private const float INPUT_KG = 100f;

        public static readonly CellOffset outPipeOffset = new CellOffset(1, 3);

        private static readonly List<Storage.StoredItemModifier> RefineryStoredItemModifiers = new List<Storage.StoredItemModifier>
        {
            Storage.StoredItemModifier.Hide,
            Storage.StoredItemModifier.Preserve
        };

        public override BuildingDef CreateBuildingDef()
        {
            string id = "Inductor";
            int width = 5;
            int height = 4;
            string anim = "glassrefinery_kanim";
            int hitpoints = 30;
            float construction_time = 60f;
            float[] tIER = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER5;
            string[] aLL_MINERALS = MATERIALS.ALL_MINERALS;
            float melting_point = 2400f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tIER2 = NOISE_POLLUTION.NOISY.TIER6;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tIER, aLL_MINERALS, melting_point, build_location_rule, TUNING.BUILDINGS.DECOR.PENALTY.TIER2, tIER2, 0.2f);
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 1200f;
            buildingDef.SelfHeatKilowattsWhenActive = 16f;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.UtilityOutputOffset = InductorConfig.outPipeOffset;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.AudioSize = "large";
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<DropAllWorkable>();
            go.AddOrGet<BuildingComplete>().isManuallyOperated = true;
            Inductor Inductor = go.AddOrGet<Inductor>();
            Inductor.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid;
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>();
            Inductor.duplicantOperated = true;
            BuildingTemplates.CreateComplexFabricatorStorage(go, Inductor);
            Inductor.outStorage.capacityKg = 2000f;
            Inductor.storeProduced = true;
            Inductor.inStorage.SetDefaultStoredItemModifiers(InductorConfig.RefineryStoredItemModifiers);
            Inductor.buildStorage.SetDefaultStoredItemModifiers(InductorConfig.RefineryStoredItemModifiers);
            Inductor.outStorage.SetDefaultStoredItemModifiers(InductorConfig.RefineryStoredItemModifiers);
            Inductor.outputOffset = new Vector3(1f, 0.5f);
            complexFabricatorWorkable.overrideAnims = new KAnimFile[]
            {
            Assets.GetAnim("anim_interacts_metalrefinery_kanim")
            };
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.storage = Inductor.outStorage;
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.elementFilter = null;
            conduitDispenser.alwaysDispense = true;

            List<Element> list = ElementLoader.elements.FindAll((Element e) => e.IsSolid && e.HasTag(GameTags.Metal));
            ComplexRecipe complexRecipe;
            foreach (Element current in list)
            {
                if (current.tag != SimHashes.TempConductorSolid.CreateTag())
                Element highTempTransition = current.highTempTransition;
                Element lowTempTransition = highTempTransition.lowTempTransition;
                if (lowTempTransition != current)
                {
                    ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[]
                    {
                    new ComplexRecipe.RecipeElement(current.tag, 100f)
                    };
                    ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[]
                    {
                    new ComplexRecipe.RecipeElement(highTempTransition.tag, 100f)
                    };
                    string obsolete_id = ComplexRecipeManager.MakeObsoleteRecipeID("Inductor", current.tag);
                    string text = ComplexRecipeManager.MakeRecipeID("Inductor", array, array2);
                    complexRecipe = new ComplexRecipe(text, array, array2);
                    complexRecipe.time = 100f;
                    complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.METALREFINERY.RECIPE_DESCRIPTION, highTempTransition.name, current.name);
                    complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
                    complexRecipe.fabricators = new List<Tag>
                {
                    TagManager.Create("Inductor")
                };
                    ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id, text);
                }
            }
            /*ComplexRecipe.RecipeElement[] Input = new ComplexRecipe.RecipeElement[] //Copper
                {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.Copper).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Input2 = new ComplexRecipe.RecipeElement[] //Iron
                {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.IronOre).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Input3 = new ComplexRecipe.RecipeElement[] //Gold
                {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.GoldAmalgam).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Input4 = new ComplexRecipe.RecipeElement[] //Tungstan
                {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.Wolframite).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Input5 = new ComplexRecipe.RecipeElement[] //Aluminium
                {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.AluminumOre).tag, 100f)
                };

            ComplexRecipe.RecipeElement[] Output = new ComplexRecipe.RecipeElement[] //Copper
            {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.MoltenCopper).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Output2 = new ComplexRecipe.RecipeElement[] //Iron
            {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.MoltenIron).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Output3 = new ComplexRecipe.RecipeElement[] //Gold
            {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.MoltenGold).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Output4 = new ComplexRecipe.RecipeElement[] //Tungstan
            {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.MoltenTungsten).tag, 100f)
                };
            ComplexRecipe.RecipeElement[] Output5 = new ComplexRecipe.RecipeElement[] //Aluminium
            {
                    new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.MoltenAluminum).tag, 100f)
                };


            string recipeID  = ComplexRecipeManager.MakeRecipeID("Inductor", Input, Output);
            string recipeID2 = ComplexRecipeManager.MakeRecipeID("Inductor", Input2, Output2);
            string recipeID3 = ComplexRecipeManager.MakeRecipeID("Inductor", Input3, Output3);
            string recipeID4 = ComplexRecipeManager.MakeRecipeID("Inductor", Input4, Output4);
            string recipeID5 = ComplexRecipeManager.MakeRecipeID("Inductor", Input5, Output5);

            new ComplexRecipe(recipeID, Input, Output)
            {
                time = 100f,
                description = string.Format(InductorConfig.RECIPE_DESCRIPTION, ElementLoader.GetElement(Input[0].material).name, ElementLoader.GetElement(Output[0].material).name),
                fabricators = new List<Tag>()
                    {
                        TagManager.Create("inductor")
                    },
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
            };

            new ComplexRecipe(recipeID2, Input2, Output2)
            {
                time = 100f,
                description = string.Format(InductorConfig.RECIPE_DESCRIPTION, ElementLoader.GetElement(Input2[0].material).name, ElementLoader.GetElement(Output2[0].material).name),
                fabricators = new List<Tag>()
                    {
                        TagManager.Create("inductor")
                    },
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
            };

            new ComplexRecipe(recipeID3, Input3, Output3)
            {
                time = 100f,
                description = string.Format(InductorConfig.RECIPE_DESCRIPTION, ElementLoader.GetElement(Input3[0].material).name, ElementLoader.GetElement(Output3[0].material).name),
                fabricators = new List<Tag>()
                    {
                        TagManager.Create("inductor")
                    },
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
            };

            new ComplexRecipe(recipeID4, Input4, Output4)
            {
                time = 100f,
                description = string.Format(InductorConfig.RECIPE_DESCRIPTION, ElementLoader.GetElement(Input4[0].material).name, ElementLoader.GetElement(Output4[0].material).name),
                fabricators = new List<Tag>()
                    {
                        TagManager.Create("inductor")
                    },
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
            };

            new ComplexRecipe(recipeID5, Input5, Output5)
            {
                time = 100f,
                description = string.Format(InductorConfig.RECIPE_DESCRIPTION, ElementLoader.GetElement(Input5[0].material).name, ElementLoader.GetElement(Output5[0].material).name),
                fabricators = new List<Tag>()
                    {
                        TagManager.Create("inductor")
                    },
                nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
            };
            ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.IronOre).tag, 100f)
            };
            ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement(ElementLoader.FindElementByHash(SimHashes.MoltenIron).tag, 100f)
            };

            string obsolete_id = ComplexRecipeManager.MakeObsoleteRecipeID("Inductor", array[0].material);
            string text = ComplexRecipeManager.MakeRecipeID("Inductor", array, array2);
            ComplexRecipe complexRecipe = new ComplexRecipe(text, array, array2);
            complexRecipe.time = 40f;
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.Output;
            complexRecipe.description = string.Format(InductorConfig.RECIPE_DESCRIPTION, ElementLoader.GetElement(array2[0].material).name, ElementLoader.GetElement(array[0].material).name);
            complexRecipe.fabricators = new List<Tag>
            {
                TagManager.Create("Inductor")
            };
            ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id, text);*/






            Prioritizable.AddRef(go);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            SymbolOverrideControllerUtil.AddToPrefab(go);
            go.AddOrGetDef<PoweredActiveStoppableController.Def>();
            go.GetComponent<KPrefabID>().prefabSpawnFn += delegate (GameObject game_object)
            {
                ComplexFabricatorWorkable component = game_object.GetComponent<ComplexFabricatorWorkable>();
                component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Processing;
                component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
                component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
                component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
                component.SkillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
            };
        }
    }

}
