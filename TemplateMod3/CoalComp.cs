using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using KSerialization;
using UnityEngine;
using STRINGS;
using CoalSynth;

namespace CoalComp
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{CoalCompConfig.ID.ToUpper()}.NAME", UI.FormatAsLink("Coal Compactinator", CoalCompConfig.ID));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{CoalCompConfig.ID.ToUpper()}.EFFECT", string.Concat("Uses piped input to Convert ", UI.FormatAsLink("Carbon Dioxide", "CARBONDIOXIDE")," to ", UI.FormatAsLink("Coal", "CARBON"), " with ", UI.FormatAsLink("Oxygen", "OXYGEN"), " Piped out"));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{CoalCompConfig.ID.ToUpper()}.DESC", string.Concat("A piped version of the ", UI.FormatAsLink("Coal Synthisizer", CoalSynthConfig.ID), " able to Compress Carbon from CO2 Back into Coal with some Bonus Oxgyen"));

            ModUtil.AddBuildingToPlanScreen("Oxygen", CoalCompConfig.ID);
        }
    }

    [HarmonyPatch(typeof(Db))]
    [HarmonyPatch("Initialize")]
    public static class Db_Initialize_Patch
    {
        private static void Prefix()
        {
            var techList = new List<string>(Database.Techs.TECH_GROUPING["ImprovedOxygen"]) { CoalCompConfig.ID };
            Database.Techs.TECH_GROUPING["ImprovedOxygen"] = techList.ToArray();
        }
    }

    [SerializationConfig(MemberSerialization.OptIn)]
    public class CoalComp : StateMachineComponent<CoalComp.StatesInstance>
    {
        public class StatesInstance : GameStateMachine<CoalComp.States, CoalComp.StatesInstance, CoalComp, object>.GameInstance
        {
            public StatesInstance(CoalComp smi) : base(smi)
            {
            }

            public void TryEmit()
            {
                Storage storage = base.smi.master.storage;
                GameObject gameObject = storage.FindFirst(base.smi.master.emitTag);
                if (gameObject != null)
                {
                    PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                    if (component.Mass >= base.master.emitMass)
                    {
                        Vector3 position = base.transform.GetPosition() + base.master.dropOffset;
                        position.z = Grid.GetLayerZ(Grid.SceneLayer.Ore);
                        storage.Drop(gameObject, true);
                    }
                }
            }
        }

        public class States : GameStateMachine<CoalComp.States, CoalComp.StatesInstance, CoalComp>
        {
            public GameStateMachine<CoalComp.States, CoalComp.StatesInstance, CoalComp, object>.State disabled;

            public GameStateMachine<CoalComp.States, CoalComp.StatesInstance, CoalComp, object>.State waiting;

            public GameStateMachine<CoalComp.States, CoalComp.StatesInstance, CoalComp, object>.State converting;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.disabled;
                this.root.EventTransition(GameHashes.OperationalChanged, this.disabled, (CoalComp.StatesInstance smi) => !smi.master.operational.IsOperational);
                this.disabled.EventTransition(GameHashes.OperationalChanged, this.waiting, (CoalComp.StatesInstance smi) => smi.master.operational.IsOperational);
                this.waiting.EventTransition(GameHashes.OnStorageChange, this.converting, (CoalComp.StatesInstance smi) => smi.master.GetComponent<ElementConverter>().HasEnoughMassToStartConverting());
                this.converting.Enter(delegate (CoalComp.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Exit(delegate (CoalComp.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                }).Transition(this.waiting, (CoalComp.StatesInstance smi) => !smi.master.GetComponent<ElementConverter>().CanConvertAtAll(), UpdateRate.SIM_200ms).EventHandler(GameHashes.OnStorageChange, delegate (CoalComp.StatesInstance smi)
                {
                    smi.TryEmit();
                });
            }
        }

        [MyCmpAdd]
        private Storage storage;

        [MyCmpReq]
        private Operational operational;

        public Tag emitTag;

        public float emitMass;

        public Vector3 dropOffset;

        protected override void OnSpawn()
        {
            base.smi.StartSM();
        }
    }
}
