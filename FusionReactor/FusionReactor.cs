using System;
using System.Collections.Generic;
using System.Linq;
using STRINGS;
using Harmony;
using UnityEngine;
using KSerialization;

namespace FusionReactor
{
    [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
    internal class GeneratedBuildings_LoadGeneratedBuildings
    {
        private static void Prefix()
        {
            Strings.Add($"STRINGS.BUILDINGS.PREFABS.{FusionReactorConfig.ID.ToUpper()}.NAME", string.Concat("Fusion ", UI.FormatAsLink("\"Stellarator\"", FusionReactorConfig.ID), " Reactor"));
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

    [HarmonyPatch(typeof(SingleSliderSideScreen), "IsValidForTarget")]
    public class SingleSliderSideScreen_IsValidForTarget
     {
        public static void Postfix(ref bool __result, GameObject target)
        {
            if (target.GetComponent<KPrefabID>() != null) {

                KPrefabID component = target.GetComponent<KPrefabID>();
                    
                if (component.HasTag("FusionReactor".ToTag()))
                {
                    __result = false;
                }
            }
        }
    }

    [SerializationConfig(MemberSerialization.OptIn)]
    public class FusionReactor : StateMachineComponent<FusionReactor.StatesInstance>, IEffectDescriptor
    {
        public class StatesInstance : GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.GameInstance
        {
            public StatesInstance(FusionReactor smi) : base(smi)
            {
            }
        }

        public class States : GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor>
        {
            public GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.State waiting;

            public GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.State hasfilter;

            public GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.State converting;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.converting;
                this.waiting
                    .EventTransition(GameHashes.OnStorageChange, this.hasfilter, (FusionReactor.StatesInstance smi) => smi.master.HasFilter() && smi.master.operational.IsOperational)
                    .EventTransition(GameHashes.OperationalChanged, this.hasfilter, (FusionReactor.StatesInstance smi) => smi.master.HasFilter() && smi.master.operational.IsOperational);

                this.hasfilter
                    .EventTransition(GameHashes.OnStorageChange, this.converting, (FusionReactor.StatesInstance smi) => smi.master.IsConvertable())
                    .EventTransition(GameHashes.OperationalChanged, this.waiting, (FusionReactor.StatesInstance smi) => !smi.master.operational.IsOperational).Enter("EnableConsumption", delegate (FusionReactor.StatesInstance smi)
                    {
                        smi.master.conduitConsumer.EnableConsumption(true);
                    ).Exit("DisableConsumption", delegate (FusionReactor.StatesInstance smi)
                    {
                        smi.master.conduitConsumer.EnableConsumption(false);
                    });
                this.converting.Enter("SetActive(true)", delegate (FusionReactor.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Exit("SetActive(false)", delegate (FusionReactor.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                }).Enter("EnableConsumption", delegate (FusionReactor.StatesInstance smi)
                }
        }

        [MyCmpGet]
        private Operational operational;

        [MyCmpGet]
        private Storage storage;

        [MyCmpGet]
        private ElementConverter elementConverter;

        [MyCmpGet]
        private ConduitConsumer conduitConsumer;

        [MyCmpGet]
        private conduitConsumer conduitConsumer;

        public Tag filterTag;

        public bool HasFilter()
        {
            //this is still really bad
            //this.elementConverter.HasEnoughMass(this.filterTag);
            return this.storage.Has(SimHashes.Hydrogen.CreateTag()) && this.storage.GetAmountAvailable(SimHashes.Hydrogen.CreateTag()) >= 2f;
        }

        public bool IsConvertable()
        {
            return true;
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
