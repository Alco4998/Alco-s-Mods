using STRINGS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace thermalReducer
{
    public class ThermalReducer : StateMachineComponent<ThermalReducer.StatesInstance>
    {
        public class StatesInstance : GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.GameInstance
        {
            public StatesInstance(ThermalReducer master) : base(master)
            {
            }
        }

        public class States : GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer>
        {
            public class OnlineStates : GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.State
            {
                public GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.State heating;

                public GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.State overtemp;

                public GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.State undermassliquid;

                public GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.State undermassgas;
            }

            public GameStateMachine<ThermalReducer.States, ThermalReducer.StatesInstance, ThermalReducer, object>.State offline;

            public ThermalReducer.States.OnlineStates online;

            private StatusItem statusItemUnderMassLiquid;

            private StatusItem statusItemUnderMassGas;

            private StatusItem statusItemOverTemp;

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                default_state = this.offline;
                base.serializable = false;
                this.statusItemUnderMassLiquid = new StatusItem("statusItemUnderMassLiquid", BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_LIQUID.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_LIQUID.TOOLTIP, string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, OverlayModes.None.ID, 129022);
                this.statusItemUnderMassGas = new StatusItem("statusItemUnderMassGas", BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_GAS.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDLOWMASS_GAS.TOOLTIP, string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, OverlayModes.None.ID, 129022);
                this.statusItemOverTemp = new StatusItem("statusItemOverTemp", BUILDING.STATUSITEMS.HEATINGSTALLEDHOTENV.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDHOTENV.TOOLTIP, string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, OverlayModes.None.ID, 129022);
                this.statusItemOverTemp.resolveStringCallback = delegate (string str, object obj)
                {
                    ThermalReducer.StatesInstance statesInstance = (ThermalReducer.StatesInstance)obj;
                    return string.Format(str, GameUtil.GetFormattedTemperature(statesInstance.master.TargetTemperature, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true, false));
                };
                this.offline.EventTransition(GameHashes.OperationalChanged, this.online, (ThermalReducer.StatesInstance smi) => smi.master.operational.IsOperational);
                this.online.EventTransition(GameHashes.OperationalChanged, this.offline, (ThermalReducer.StatesInstance smi) => !smi.master.operational.IsOperational).DefaultState(this.online.heating).Update("spaceheater_online", delegate (ThermalReducer.StatesInstance smi, float dt)
                {
                    switch (smi.master.MonitorHeating(dt))
                    {
                        case ThermalReducer.MonitorState.ReadyToHeat:
                            smi.GoTo(this.online.heating);
                            break;
                        case ThermalReducer.MonitorState.TooHot:
                            smi.GoTo(this.online.overtemp);
                            break;
                        case ThermalReducer.MonitorState.NotEnoughLiquid:
                            smi.GoTo(this.online.undermassliquid);
                            break;
                        case ThermalReducer.MonitorState.NotEnoughGas:
                            smi.GoTo(this.online.undermassgas);
                            break;
                    }
                }, UpdateRate.SIM_4000ms, false);
                this.online.heating.Enter(delegate (ThermalReducer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(true, false);
                }).Exit(delegate (ThermalReducer.StatesInstance smi)
                {
                    smi.master.operational.SetActive(false, false);
                });
                this.online.undermassliquid.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, this.statusItemUnderMassLiquid, null);
                this.online.undermassgas.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, this.statusItemUnderMassGas, null);
                this.online.overtemp.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, this.statusItemOverTemp, null);
            }
        }

        private enum MonitorState
        {
            ReadyToHeat,
            TooHot,
            NotEnoughLiquid,
            NotEnoughGas
        }

        private HandleVector<int>.Handle structureTemperature;

        public float targetTemperature = 10f;

        public float minimumCellMass;

        public int radius = 2;

        [SerializeField]
        private bool heatLiquid;

        [MyCmpReq]
        private Operational operational;

        private List<int> monitorCells = new List<int>();

        public float TargetTemperature
        {
            get
            {
                return this.targetTemperature;
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            base.smi.StartSM();
            //Mathf.Clamp(this., 0, 10000);
        }

        public void SetLiquidHeater()
        {
            this.heatLiquid = true;
        }

        private ThermalReducer.MonitorState MonitorHeating(float dt)
        {
            this.monitorCells.Clear();
            int cell = Grid.PosToCell(base.transform.GetPosition());    
            GameUtil.GetNonSolidCells(cell, this.radius, this.monitorCells);
            int num = 0;
            float num2 = 0f;

            for (int i = 0; i < this.monitorCells.Count; i++)
            {
                if (Grid.Mass[this.monitorCells[i]] > this.minimumCellMass && ((Grid.Element[this.monitorCells[i]].IsGas && !this.heatLiquid) || (Grid.Element[this.monitorCells[i]].IsLiquid && this.heatLiquid)))
                {

                    num++;
                    num2 += Grid.Temperature[this.monitorCells[i]];

                }
            }

            if (num == 0)
            {
                return (!this.heatLiquid) ? ThermalReducer.MonitorState.NotEnoughGas : ThermalReducer.MonitorState.NotEnoughLiquid;
            }

            bool flag = !(num2 / (float)num >= this.targetTemperature);

            if (flag)
            {
                return ThermalReducer.MonitorState.TooHot;
            }
            return ThermalReducer.MonitorState.ReadyToHeat;
        }
    }

}
