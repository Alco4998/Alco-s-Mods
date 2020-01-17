using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using KSerialization;
using STRINGS;

namespace CoalSynth
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{CoalSynthConfig.ID.ToUpper()}.NAME", UI.FormatAsLink("Coal Synthisizer", CoalSynthConfig.ID));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{CoalSynthConfig.ID.ToUpper()}.EFFECT", string.Concat("Converts ", UI.FormatAsLink("Carbon Dioxide", "CARBONDIOXIDE")," to ", UI.FormatAsLink("Coal", "CARBON"), " and ", UI.FormatAsLink("Oxygen", "OXYGEN")));
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{CoalSynthConfig.ID.ToUpper()}.DESC", "Able to Compress Carbon from CO2 Back into Coal with some Bonus Oxgyen");


            ModUtil.AddBuildingToPlanScreen("Oxygen", CoalSynthConfig.ID);
        }
    }

    [HarmonyPatch(typeof(Db))]
    [HarmonyPatch("Initialize")]
    public static class Db_Initialize_Patch
    {
        private static void Prefix()
        {
            var techList = new List<string>(Database.Techs.TECH_GROUPING["ImprovedOxygen"]) { CoalSynthConfig.ID };
            Database.Techs.TECH_GROUPING["ImprovedOxygen"] = techList.ToArray();
        }
    }

    [SerializationConfig(MemberSerialization.OptIn)]
    public class CoalSynth : StateMachineComponent<CoalSynth.StatesInstance>, IEffectDescriptor
    {
        public class StatesInstance : GameStateMachine<CoalSynth.States, CoalSynth.StatesInstance, CoalSynth, object>.GameInstance
        {
            public StatesInstance(CoalSynth smi) : base(smi)
            {
            }
        }

        public class States : GameStateMachine<CoalSynth.States, CoalSynth.StatesInstance, CoalSynth>
        {
            public GameStateMachine<CoalSynth.States, CoalSynth.StatesInstance, CoalSynth, object>.State waiting;

            public GameStateMachine<CoalSynth.States, CoalSynth.StatesInstance, CoalSynth, object>.State hasfilter;

            public GameStateMachine<CoalSynth.States, CoalSynth.StatesInstance, CoalSynth, object>.State converting;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.converting;
                this.waiting
                    .EventTransition(GameHashes.OnStorageChange, this.hasfilter, (CoalSynth.StatesInstance smi) => smi.master.HasFilter() && smi.master.operational.IsOperational)
                    .EventTransition(GameHashes.OperationalChanged, this.hasfilter, (CoalSynth.StatesInstance smi) => smi.master.HasFilter() && smi.master.operational.IsOperational);

                this.hasfilter
                    .EventTransition(GameHashes.OnStorageChange, this.converting, (CoalSynth.StatesInstance smi) => smi.master.IsConvertable())
                    .EventTransition(GameHashes.OperationalChanged, this.waiting, (CoalSynth.StatesInstance smi) => !smi.master.operational.IsOperational).Enter("EnableConsumption", delegate (CoalSynth.StatesInstance smi)
                {
                    smi.master.elementConsumer.EnableConsumption(true);
                }).Exit("DisableConsumption", delegate (CoalSynth.StatesInstance smi)
                {
                    smi.master.elementConsumer.EnableConsumption(false);
                });
                this.converting.Enter("SetActive(true)", delegate (CoalSynth.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Exit("SetActive(false)", delegate (CoalSynth.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                }).Enter("EnableConsumption", delegate (CoalSynth.StatesInstance smi)
                {
                    smi.master.elementConsumer.EnableConsumption(true);
                }).Exit("DisableConsumption", delegate (CoalSynth.StatesInstance smi)
                {
                    smi.master.elementConsumer.EnableConsumption(false);
                }).EventTransition(GameHashes.OnStorageChange, this.waiting, (CoalSynth.StatesInstance smi) => !smi.master.IsConvertable()).EventTransition(GameHashes.OperationalChanged, this.waiting, (CoalSynth.StatesInstance smi) => !smi.master.operational.IsOperational);
            }
        }

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private ElementConverter elementConverter;

        [MyCmpGet]
        private ElementConsumer elementConsumer;

        public Tag filterTag;

        public bool HasFilter()
        {
            //this is still really bad
            //this.elementConverter.HasEnoughMass(this.filterTag);
            return true;
            //please redo when i understand
        }

        public bool IsConvertable()
        {
            return this.elementConverter.HasEnoughMassToStartConverting();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            base.smi.StartSM();
        }

        public List<Descriptor> GetDescriptors(BuildingDef def)
        {
            return null;
        }
    }
}
