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

            public GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.State HasInput;

            public GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.State HasHeat;

            public GameStateMachine<FusionReactor.States, FusionReactor.StatesInstance, FusionReactor, object>.State IsRunning;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = waiting;

                this.waiting
                    .EventTransition(GameHashes.OnStorageChange, this.HasInput, (FusionReactor.StatesInstance smi) => smi.master.HasInput() && smi.master.operational.IsOperational)
                    .EventTransition(GameHashes.OperationalChanged, this.waiting, (FusionReactor.StatesInstance smi) => smi.master.HasInput() && smi.master.operational.IsOperational);

                this.HasInput
                     .EventTransition(GameHashes.OnStorageChange, this.HasInput, (FusionReactor.StatesInstance smi) => smi.master.HasInput() && smi.master.operational.IsOperational)
                     .EventTransition(GameHashes.OperationalChanged, this.waiting, (FusionReactor.StatesInstance smi) => smi.master.HasInput() && smi.master.operational.IsOperational);
            }
        }

        private Operational operational;

        private Storage storage;

        public bool HasInput()
        {
            return this.storage.GetAmountAvailable(new Tag("Hydrogen")) >= 2f;
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            base.smi.StartSM();
        }
    }
}
