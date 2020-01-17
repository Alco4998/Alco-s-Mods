﻿using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using UnityEngine;
using TUNING;
using BUILDINGS = TUNING.BUILDINGS;
using Unity;

namespace StirlingGenerator
{

    public class SteamTurbine : Generator
    {
        public class States : GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine>
        {
            public class OperationalStates : GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine, object>.State
            {
                public GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine, object>.State idle;

                public GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine, object>.State active;

                public GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine, object>.State tooHot;
            }

            public GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine, object>.State inoperational;

            public SteamTurbine.States.OperationalStates operational;

            private static readonly HashedString[] ACTIVE_ANIMS = new HashedString[]
            {
            "working_pre",
            "working_loop"
            };

            private static readonly HashedString[] TOOHOT_ANIMS = new HashedString[]
            {
            "working_pre"
            };

            public override void InitializeStates(out StateMachine.BaseState default_state)
            {
                SteamTurbine.InitializeStatusItems();
                default_state = this.operational;
                this.root.Update("UpdateBlocked", delegate (SteamTurbine.Instance smi, float dt)
                {
                    smi.UpdateBlocked(dt);
                }, UpdateRate.SIM_200ms, false);
                this.inoperational.EventTransition(GameHashes.OperationalChanged, this.operational.active, (SteamTurbine.Instance smi) => smi.master.GetComponent<Operational>().IsOperational).QueueAnim("off", false, null);
                this.operational.DefaultState(this.operational.active).EventTransition(GameHashes.OperationalChanged, this.inoperational, (SteamTurbine.Instance smi) => !smi.master.GetComponent<Operational>().IsOperational).Update("UpdateOperational", delegate (SteamTurbine.Instance smi, float dt)
                {
                    smi.UpdateState(dt);
                }, UpdateRate.SIM_200ms, false).Exit(delegate (SteamTurbine.Instance smi)
                {
                    smi.DisableStatusItems();
                });
                this.operational.idle.QueueAnim("on", false, null);
                this.operational.active.Update("UpdateActive", delegate (SteamTurbine.Instance smi, float dt)
                {
                    smi.master.Pump(dt);
                }, UpdateRate.SIM_200ms, false).ToggleStatusItem((SteamTurbine.Instance smi) => SteamTurbine.activeStatusItem, (SteamTurbine.Instance smi) => smi.master).Enter(delegate (SteamTurbine.Instance smi)
                {
                    smi.GetComponent<KAnimControllerBase>().Play(SteamTurbine.States.ACTIVE_ANIMS, KAnim.PlayMode.Loop);
                    smi.GetComponent<Operational>().SetActive(true, false);
                }).Exit(delegate (SteamTurbine.Instance smi)
                {
                    smi.master.GetComponent<Generator>().ResetJoules();
                    smi.GetComponent<Operational>().SetActive(false, false);
                });
                this.operational.tooHot.Enter(delegate (SteamTurbine.Instance smi)
                {
                    smi.GetComponent<KAnimControllerBase>().Play(SteamTurbine.States.TOOHOT_ANIMS, KAnim.PlayMode.Loop);
                });
            }
        }

        public class Instance : GameStateMachine<SteamTurbine.States, SteamTurbine.Instance, SteamTurbine, object>.GameInstance
        {
            public bool insufficientMass;

            public bool insufficientTemperature;

            public bool buildingTooHot;

            private Guid inputBlockedHandle = Guid.Empty;

            private Guid inputPartiallyBlockedHandle = Guid.Empty;

            private Guid insufficientMassHandle = Guid.Empty;

            private Guid insufficientTemperatureHandle = Guid.Empty;

            private Guid buildingTooHotHandle = Guid.Empty;

            private Guid activeWattageHandle = Guid.Empty;

            public Instance(SteamTurbine master) : base(master)
            {
            }

            public void UpdateBlocked(float dt)
            {
                base.master.BlockedInputs = 0;
                for (int i = 0; i < base.master.TotalInputs; i++)
                {
                    int num = base.master.srcCells[i];
                    Element element = Grid.Element[num];
                    if (element.IsLiquid || element.IsSolid)
                    {
                        base.master.BlockedInputs++;
                    }
                }
                KSelectable component = base.GetComponent<KSelectable>();
                this.inputBlockedHandle = this.UpdateStatusItem(SteamTurbine.inputBlockedStatusItem, base.master.BlockedInputs == base.master.TotalInputs, this.inputBlockedHandle, component);
                this.inputPartiallyBlockedHandle = this.UpdateStatusItem(SteamTurbine.inputPartiallyBlockedStatusItem, base.master.BlockedInputs > 0 && base.master.BlockedInputs < base.master.TotalInputs, this.inputPartiallyBlockedHandle, component);
            }

            public void UpdateState(float dt)
            {
                bool flag = this.CanSteamFlow(ref this.insufficientMass, ref this.insufficientTemperature);
                bool flag2 = this.IsTooHot(ref this.buildingTooHot);
                this.UpdateStatusItems();
                StateMachine.BaseState currentState = base.smi.GetCurrentState();
                if (flag2)
                {
                    if (currentState != base.sm.operational.tooHot)
                    {
                        base.smi.GoTo(base.sm.operational.tooHot);
                    }
                }
                else if (flag)
                {
                    if (currentState != base.sm.operational.active)
                    {
                        base.smi.GoTo(base.sm.operational.active);
                    }
                }
                else if (currentState != base.sm.operational.idle)
                {
                    base.smi.GoTo(base.sm.operational.idle);
                }
            }

            private bool IsTooHot(ref bool building_too_hot)
            {
                building_too_hot = (base.gameObject.GetComponent<PrimaryElement>().Temperature > base.smi.master.maxBuildingTemperature);
                return building_too_hot;
            }

            private bool CanSteamFlow(ref bool insufficient_mass, ref bool insufficient_temperature)
            {
                float num = 0f;
                float num2 = 0f;
                for (int i = 0; i < base.master.srcCells.Length; i++)
                {
                    int num3 = base.master.srcCells[i];
                    float num4 = Grid.Mass[num3];
                    if (Grid.Element[num3].id == base.master.srcElem)
                    {
                        num = Mathf.Max(num, num4);
                        float num5 = Grid.Temperature[num3];
                        num2 = Mathf.Max(num2, num5);
                    }
                }
                insufficient_mass = (num < base.master.requiredMass);
                insufficient_temperature = (num2 < base.master.minActiveTemperature);
                return !insufficient_mass && !insufficient_temperature;
            }

            public void UpdateStatusItems()
            {
                KSelectable component = base.GetComponent<KSelectable>();
                this.insufficientMassHandle = this.UpdateStatusItem(SteamTurbine.insufficientMassStatusItem, this.insufficientMass, this.insufficientMassHandle, component);
                this.insufficientTemperatureHandle = this.UpdateStatusItem(SteamTurbine.insufficientTemperatureStatusItem, this.insufficientTemperature, this.insufficientTemperatureHandle, component);
                this.buildingTooHotHandle = this.UpdateStatusItem(SteamTurbine.buildingTooHotItem, this.buildingTooHot, this.buildingTooHotHandle, component);
                bool isActive = base.master.operational.IsActive;
                StatusItem status_item = (!isActive) ? Db.Get().BuildingStatusItems.GeneratorOffline : SteamTurbine.activeWattageStatusItem;
                this.activeWattageHandle = component.SetStatusItem(Db.Get().StatusItemCategories.Power, status_item, base.master);
            }

            private Guid UpdateStatusItem(StatusItem item, bool show, Guid current_handle, KSelectable ksel)
            {
                Guid result = current_handle;
                if (show != (current_handle != Guid.Empty))
                {
                    if (show)
                    {
                        result = ksel.AddStatusItem(item, base.master);
                    }
                    else
                    {
                        result = ksel.RemoveStatusItem(current_handle, false);
                    }
                }
                return result;
            }

            public void DisableStatusItems()
            {
                KSelectable component = base.GetComponent<KSelectable>();
                component.RemoveStatusItem(this.buildingTooHotHandle, false);
                component.RemoveStatusItem(this.insufficientMassHandle, false);
                component.RemoveStatusItem(this.insufficientTemperatureHandle, false);
                component.RemoveStatusItem(this.activeWattageHandle, false);
            }
        }

        private HandleVector<int>.Handle accumulator = HandleVector<int>.InvalidHandle;

        public SimHashes srcElem;

        public SimHashes destElem;

        public float requiredMass = 0.001f;

        public float minActiveTemperature = 398.15f;

        public float idealSourceElementTemperature = 473.15f;

        public float maxBuildingTemperature = 373.15f;

        public float outputElementTemperature = 368.15f;

        public float maxWattage = 850f;

        public float minConvertMass;

        public float pumpKGRate;

        public float maxSelfHeat;

        public float wasteHeatToTurbinePercent;

        private static readonly HashedString TINT_SYMBOL = new HashedString("meter_fill");

        [Serialize]
        private float storedMass;

        [Serialize]
        private float storedTemperature;

        [Serialize]
        private byte diseaseIdx = 255;

        [Serialize]
        private int diseaseCount;

        private static StatusItem inputBlockedStatusItem;

        private static StatusItem inputPartiallyBlockedStatusItem;

        private static StatusItem insufficientMassStatusItem;

        private static StatusItem insufficientTemperatureStatusItem;

        private static StatusItem activeWattageStatusItem;

        private static StatusItem buildingTooHotItem;

        private static StatusItem activeStatusItem;

        private const Sim.Cell.Properties floorCellProperties = (Sim.Cell.Properties)39;

        private MeterController meter;

        private HandleVector<Game.ComplexCallbackInfo<Sim.MassEmittedCallback>>.Handle simEmitCBHandle = HandleVector<Game.ComplexCallbackInfo<Sim.MassEmittedCallback>>.InvalidHandle;

        private SteamTurbine.Instance smi;

        private int[] srcCells;

        private Storage gasStorage;

        private Storage liquidStorage;

        private ElementConsumer consumer;

        private Guid statusHandle;

        private HandleVector<int>.Handle structureTemperature;

        private float lastSampleTime = -1f;

        public int BlockedInputs
        {
            get;
            private set;
        }

        public int TotalInputs
        {
            get
            {
                return this.srcCells.Length;
            }
        }

        public float CurrentWattage
        {
            get
            {
                return Game.Instance.accumulators.GetAverageRate(this.accumulator);
            }
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            this.accumulator = Game.Instance.accumulators.Add("Power", this);
            this.structureTemperature = GameComps.StructureTemperatures.GetHandle(base.get_gameObject());
            this.simEmitCBHandle = Game.Instance.massEmitCallbackManager.Add(new Action<Sim.MassEmittedCallback, object>(SteamTurbine.OnSimEmittedCallback), this, "SteamTurbineEmit");
            BuildingDef def = base.GetComponent<BuildingComplete>().Def;
            this.srcCells = new int[def.WidthInCells];
            int cell = Grid.PosToCell(this);
            for (int i = 0; i < def.WidthInCells; i++)
            {
                int x = i - (def.WidthInCells - 1) / 2;
                this.srcCells[i] = Grid.OffsetCell(cell, new CellOffset(x, -2));
            }
            this.smi = new SteamTurbine.Instance(this);
            this.smi.StartSM();
            this.CreateMeter();
        }

        private void CreateMeter()
        {
            this.meter = new MeterController(base.get_gameObject().GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, new string[]
            {
            "meter_OL",
            "meter_frame",
            "meter_fill"
            });
        }

        protected override void OnCleanUp()
        {
            if (this.smi != null)
            {
                this.smi.StopSM("cleanup");
            }
            Game.Instance.massEmitCallbackManager.Release(this.simEmitCBHandle, "SteamTurbine");
            this.simEmitCBHandle.Clear();
            base.OnCleanUp();
        }

        private void Pump(float dt)
        {
            float mass = this.pumpKGRate * dt / (float)this.srcCells.Length;
            int[] array = this.srcCells;
            for (int i = 0; i < array.Length; i++)
            {
                int gameCell = array[i];
                HandleVector<Game.ComplexCallbackInfo<Sim.MassConsumedCallback>>.Handle handle = Game.Instance.massConsumedCallbackManager.Add(new Action<Sim.MassConsumedCallback, object>(SteamTurbine.OnSimConsumeCallback), this, "SteamTurbineConsume");
                SimMessages.ConsumeMass(gameCell, this.srcElem, mass, 1, handle.index);
            }
        }

        private static void OnSimConsumeCallback(Sim.MassConsumedCallback mass_cb_info, object data)
        {
            ((SteamTurbine)data).OnSimConsume(mass_cb_info);
        }

        private void OnSimConsume(Sim.MassConsumedCallback mass_cb_info)
        {
            if (mass_cb_info.mass > 0f)
            {
                this.storedTemperature = SimUtil.CalculateFinalTemperature(this.storedMass, this.storedTemperature, mass_cb_info.mass, mass_cb_info.temperature);
                this.storedMass += mass_cb_info.mass;
                SimUtil.DiseaseInfo diseaseInfo = SimUtil.CalculateFinalDiseaseInfo(this.diseaseIdx, this.diseaseCount, mass_cb_info.diseaseIdx, mass_cb_info.diseaseCount);
                this.diseaseIdx = diseaseInfo.idx;
                this.diseaseCount = diseaseInfo.count;
                if (this.storedMass > this.minConvertMass && this.simEmitCBHandle.IsValid())
                {
                    Game.Instance.massEmitCallbackManager.GetItem(this.simEmitCBHandle);
                    this.gasStorage.AddGasChunk(this.srcElem, this.storedMass, this.storedTemperature, this.diseaseIdx, this.diseaseCount, true, true);
                    this.storedMass = 0f;
                    this.storedTemperature = 0f;
                    this.diseaseIdx = 255;
                    this.diseaseCount = 0;
                }
            }
        }

        private static void OnSimEmittedCallback(Sim.MassEmittedCallback info, object data)
        {
            ((SteamTurbine)data).OnSimEmitted(info);
        }

        private void OnSimEmitted(Sim.MassEmittedCallback info)
        {
            if (info.suceeded != 1)
            {
                this.storedTemperature = SimUtil.CalculateFinalTemperature(this.storedMass, this.storedTemperature, info.mass, info.temperature);
                this.storedMass += info.mass;
                if (info.diseaseIdx != 255)
                {
                    SimUtil.DiseaseInfo a = new SimUtil.DiseaseInfo
                    {
                        idx = this.diseaseIdx,
                        count = this.diseaseCount
                    };
                    SimUtil.DiseaseInfo b = new SimUtil.DiseaseInfo
                    {
                        idx = info.diseaseIdx,
                        count = info.diseaseCount
                    };
                    SimUtil.DiseaseInfo diseaseInfo = SimUtil.CalculateFinalDiseaseInfo(a, b);
                    this.diseaseIdx = diseaseInfo.idx;
                    this.diseaseCount = diseaseInfo.count;
                }
            }
        }

        public static void InitializeStatusItems()
        {
            SteamTurbine.activeStatusItem = new StatusItem("TURBINE_ACTIVE", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Good, false, OverlayModes.None.ID, true, 129022);
            SteamTurbine.inputBlockedStatusItem = new StatusItem("TURBINE_BLOCKED_INPUT", "BUILDING", "status_item_vent_disabled", StatusItem.IconType.Custom, NotificationType.BadMinor, false, OverlayModes.None.ID, true, 129022);
            SteamTurbine.inputPartiallyBlockedStatusItem = new StatusItem("TURBINE_PARTIALLY_BLOCKED_INPUT", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, OverlayModes.None.ID, true, 129022);
            SteamTurbine.inputPartiallyBlockedStatusItem.resolveStringCallback = new Func<string, object, string>(SteamTurbine.ResolvePartialBlockedStatus);
            SteamTurbine.insufficientMassStatusItem = new StatusItem("TURBINE_INSUFFICIENT_MASS", "BUILDING", "status_item_resource_unavailable", StatusItem.IconType.Custom, NotificationType.BadMinor, false, OverlayModes.Power.ID, true, 129022);
            SteamTurbine.insufficientMassStatusItem.resolveStringCallback = new Func<string, object, string>(SteamTurbine.ResolveStrings);
            SteamTurbine.buildingTooHotItem = new StatusItem("TURBINE_TOO_HOT", "BUILDING", "status_item_plant_temperature", StatusItem.IconType.Custom, NotificationType.BadMinor, false, OverlayModes.None.ID, true, 129022);
            SteamTurbine.buildingTooHotItem.resolveTooltipCallback = new Func<string, object, string>(SteamTurbine.ResolveStrings);
            SteamTurbine.insufficientTemperatureStatusItem = new StatusItem("TURBINE_INSUFFICIENT_TEMPERATURE", "BUILDING", "status_item_plant_temperature", StatusItem.IconType.Custom, NotificationType.BadMinor, false, OverlayModes.Power.ID, true, 129022);
            SteamTurbine.insufficientTemperatureStatusItem.resolveStringCallback = new Func<string, object, string>(SteamTurbine.ResolveStrings);
            SteamTurbine.insufficientTemperatureStatusItem.resolveTooltipCallback = new Func<string, object, string>(SteamTurbine.ResolveStrings);
            SteamTurbine.activeWattageStatusItem = new StatusItem("TURBINE_ACTIVE_WATTAGE", "BUILDING", string.Empty, StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.Power.ID, true, 129022);
            SteamTurbine.activeWattageStatusItem.resolveStringCallback = new Func<string, object, string>(SteamTurbine.ResolveWattageStatus);
        }

        private static string ResolveWattageStatus(string str, object data)
        {
            SteamTurbine steamTurbine = (SteamTurbine)data;
            float num = Game.Instance.accumulators.GetAverageRate(steamTurbine.accumulator) / steamTurbine.maxWattage;
            return str.Replace("{Wattage}", GameUtil.GetFormattedWattage(steamTurbine.CurrentWattage, GameUtil.WattageFormatterUnit.Automatic)).Replace("{Max_Wattage}", GameUtil.GetFormattedWattage(steamTurbine.maxWattage, GameUtil.WattageFormatterUnit.Automatic)).Replace("{Efficiency}", GameUtil.GetFormattedPercent(num * 100f, GameUtil.TimeSlice.None)).Replace("{Src_Element}", ElementLoader.FindElementByHash(steamTurbine.srcElem).name);
        }

        private static string ResolvePartialBlockedStatus(string str, object data)
        {
            SteamTurbine steamTurbine = (SteamTurbine)data;
            return str.Replace("{Blocked}", steamTurbine.BlockedInputs.ToString()).Replace("{Total}", steamTurbine.TotalInputs.ToString());
        }

        private static string ResolveStrings(string str, object data)
        {
            SteamTurbine steamTurbine = (SteamTurbine)data;
            str = str.Replace("{Src_Element}", ElementLoader.FindElementByHash(steamTurbine.srcElem).name);
            str = str.Replace("{Dest_Element}", ElementLoader.FindElementByHash(steamTurbine.destElem).name);
            str = str.Replace("{Overheat_Temperature}", GameUtil.GetFormattedTemperature(steamTurbine.maxBuildingTemperature, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true, false));
            str = str.Replace("{Active_Temperature}", GameUtil.GetFormattedTemperature(steamTurbine.minActiveTemperature, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true, false));
            str = str.Replace("{Min_Mass}", GameUtil.GetFormattedMass(steamTurbine.requiredMass, GameUtil.TimeSlice.None, GameUtil.MetricMassFormat.UseThreshold, true, "{0:0.#}"));
            return str;
        }

        public void SetStorage(Storage steamStorage, Storage waterStorage)
        {
            this.gasStorage = steamStorage;
            this.liquidStorage = waterStorage;
        }

        public override void EnergySim200ms(float dt)
        {
            base.EnergySim200ms(dt);
            ushort circuitID = base.CircuitID;
            this.operational.SetFlag(Generator.wireConnectedFlag, circuitID != 65535);
            if (!this.operational.IsOperational)
            {
                return;
            }
            float num = 0f;
            if (this.gasStorage != null && this.gasStorage.items.Count > 0)
            {
                GameObject gameObject = this.gasStorage.FindFirst(ElementLoader.FindElementByHash(this.srcElem).tag);
                if (gameObject != null)
                {
                    PrimaryElement component = gameObject.GetComponent<PrimaryElement>();
                    float num2 = 0.1f;
                    if (component.Mass > num2)
                    {
                        num2 = Mathf.Min(component.Mass, this.pumpKGRate * dt);
                        float num3 = this.JoulesToGenerate(component);
                        num = Mathf.Min(num3 * (num2 / this.pumpKGRate), this.maxWattage * dt);
                        float num4 = this.HeatFromCoolingSteam(component);
                        float num5 = num4 * (num2 / component.Mass);
                        float num6 = num2 / component.Mass;
                        int num7 = Mathf.RoundToInt((float)component.DiseaseCount * num6);
                        component.Mass -= num2;
                        component.ModifyDiseaseCount(-num7, "SteamTurbine.EnergySim200ms");
                        float display_dt = (this.lastSampleTime <= 0f) ? 1f : (Time.get_time() - this.lastSampleTime);
                        this.lastSampleTime = Time.get_time();
                        GameComps.StructureTemperatures.ProduceEnergy(this.structureTemperature, num5 * this.wasteHeatToTurbinePercent, BUILDINGS.PREFABS.STEAMTURBINE2.HEAT_SOURCE, display_dt);
                        this.liquidStorage.AddLiquid(this.destElem, num2, this.outputElementTemperature, component.DiseaseIdx, num7, false, true);
                    }
                }
            }
            num = Mathf.Clamp(num, 0f, this.maxWattage);
            Game.Instance.accumulators.Accumulate(this.accumulator, num);
            if (num > 0f)
            {
                base.GenerateJoules(num, false);
            }
            this.meter.SetPositionPercent(Game.Instance.accumulators.GetAverageRate(this.accumulator) / this.maxWattage);
            this.meter.SetSymbolTint(SteamTurbine.TINT_SYMBOL, Color.Lerp(Color.get_red(), Color.get_green(), Game.Instance.accumulators.GetAverageRate(this.accumulator) / this.maxWattage));
        }

        public float HeatFromCoolingSteam(PrimaryElement steam)
        {
            float temperature = steam.Temperature;
            return -GameUtil.CalculateEnergyDeltaForElement(steam, temperature, this.outputElementTemperature);
        }

        public float JoulesToGenerate(PrimaryElement steam)
        {
            float temperature = steam.Temperature;
            float num = (temperature - this.outputElementTemperature) / (this.idealSourceElementTemperature - this.outputElementTemperature);
            return this.maxWattage * (float)Math.Pow((double)num, 1.0);
        }
    }
}