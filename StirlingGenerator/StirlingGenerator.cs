using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using UnityEngine;
using STRINGS;

namespace StirlingGenerator
{
    class StirlingGenerator : KMonoBehaviour, ISim200ms
    {
        [MyCmpReq]

        private readonly float surface_area = 8f;

        public StatusItem _no_space_status;

        public StatusItem _radiating_status;

        private HandleVector<int>.Handle accumulator = HandleVector<int>.InvalidHandle;
        private int inputCell;
        [MyCmpReq] protected Operational operational;
        private int outputCell;
        private CellOffset[] radiatorOffsets; // the tiles that must be checked for vacuum
        [MyCmpReq] private KSelectable selectable; // does tooltip-related stuff
        private Guid statusHandle; // essentially a reference to a statusitem in particular

        private HandleVector<int>.Handle structureTemperature;
        public ConduitType type = ConduitType.Liquid;

        public float CurrentCooling { get; private set; }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();

            //smi.StartSM();
        }

        /*public class SMInstance : GameStateMachine<States, SMInstance, StirlingGenerator, object>.GameInstance
        {
            private readonly Operational _operational;

            public readonly ConduitConsumer _consumer;

            public SMInstance(StirlingGenerator master) : base(master)
            {
                _operational = master.GetComponent<Operational>();
                _consumer = master.GetComponent<ConduitConsumer>();
            }

            public bool IsOperational => _operational.IsOperational;

            public bool Consumerfull()
            {
                return _consumer.IsSatisfied;
            }
        }

        public class States : GameStateMachine<States, SMInstance, StirlingGenerator>
        {
            public State Running;

            public State NotRunning;

            public State Notforfilled;

            public override void InitializeStates(out BaseState defaultStates)
            {
                defaultState = NotRunning;

                root
                    .EventTransition(GameHashes.OperationalChanged, NotRunning, SMI => !SMI.IsOperational);

                NotRunning
                        .QueueAnim("off")
                        .EventTransition(GameHashes.OperationalChanged, Notforfilled, smi => smi.IsOperational);

                Notforfilled
                    .QueueAnim("off")
                    .Enter(smi => smi.master.operational.SetActive(false))
                    .Update("NoLight", (smi, dt) => { if (smi.Consumerfull()) smi.GoTo(Running); }, UpdateRate.SIM_1000ms);
            }
        }
        */

        protected override void OnCleanUp()
        {
            base.OnCleanUp();

        }

        private bool IsConnected {  }

        public void Sim200ms(float dt)
        {
            var temp = gameObject.GetComponent<PrimaryElement>().Temperature;
            if (temp < 5) return;

            if (CheckInSpace())
            {
                var cooling = radiative_heat(temp) * 1;
                if (cooling > 1f)
                {
                    CurrentCooling = (float)cooling;
                    GameComps.StructureTemperatures.ProduceEnergy(structureTemperature, (float)-cooling / 1000,
                        BUILDING.STATUSITEMS.OPERATINGENERGY.PIPECONTENTS_TRANSFER, (float)-cooling / 1000);
                }

                UpdateStatusItem();
            }
            else
            {
                UpdateStatusItem(true);
            }
        }
    }
}

