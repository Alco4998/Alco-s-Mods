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

        public const string EFFECT = "test";

        public const string DESCRIPTION = "Melt metals with the great power of magnets to produce Loads of Metals without dupes";

        public const string RECIPE_DESCRIPTION = "Melts {1} into Molten {0}";

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
            Inductor.duplicantOperated = false;
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

            Inductor.resultState = ComplexFabricator.ResultState.Melted;
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.storage = Inductor.outStorage;
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.elementFilter = null;
            conduitDispenser.alwaysDispense = true;

            this.ConfigureRecipes();
            Prioritizable.AddRef(go);
        }

        private void ConfigureRecipes()
        {
            List<Element> list = ElementLoader.elements.FindAll((Element e) => e.IsSolid && e.HasTag(GameTags.Metal));
            ComplexRecipe complexRecipe;
            foreach (Element current in list)
            {
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
                    complexRecipe = new ComplexRecipe(text, array, array2)
                    {
                        time = (current.highTemp / 10),

                        description = string.Format(RECIPE_DESCRIPTION, highTempTransition.name, current.name),

                        nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult,

                        fabricators = new List<Tag>
                        {
                            TagManager.Create("Inductor")
                        }
                    };
                ComplexRecipeManager.Get().AddObsoleteIDMapping(obsolete_id, text);
                }
            }
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
