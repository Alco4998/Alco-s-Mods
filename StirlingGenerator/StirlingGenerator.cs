using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
public class StirlingGenerator : KMonoBehaviour, ISaveLoadable, IEffectDescriptor, ISim200ms
{
    [MyCmpReq]
    private KSelectable selectable;

    [MyCmpReq]
    protected Storage storage;

    [MyCmpReq]
    protected Operational operational;

    [MyCmpReq]
    private ConduitConsumer consumer;

    [MyCmpReq]
    private BuildingComplete building;

    [MyCmpGet]
    private OccupyArea occupyArea;

    private HandleVector<int>.Handle structureTemperature;

    public float temperatureDelta = -14f;

    public float maxEnvironmentDelta = -50f;

    private float lowTempLag;

    private bool showingLowTemp;

    public bool isLiquidConditioner;

    private bool showingHotEnv;

    private Guid statusHandle;

    [Serialize]
    private float targetTemperature;

    private int cooledAirOutputCell = -1;

    private static readonly EventSystem.IntraObjectHandler<AirConditioner> OnOperationalChangedDelegate = new EventSystem.IntraObjectHandler<AirConditioner>(delegate (AirConditioner component, object data)
    {
        component.OnOperationalChanged(data);
    });

    private static readonly EventSystem.IntraObjectHandler<AirConditioner> OnActiveChangedDelegate = new EventSystem.IntraObjectHandler<AirConditioner>(delegate (AirConditioner component, object data)
    {
        component.OnActiveChanged(data);
    });

    private float lastSampleTime = -1f;

    private float envTemp;

    private int cellCount;

    public float lastEnvTemp
    {
        get;
        private set;
    }

    public float lastGasTemp
    {
        get;
        private set;
    }

    public float TargetTemperature
    {
        get
        {
            return this.targetTemperature;
        }
    }

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        base.Subscribe<AirConditioner>(-592767678, AirConditioner.OnOperationalChangedDelegate);
        base.Subscribe<AirConditioner>(824508782, AirConditioner.OnActiveChangedDelegate);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        this.structureTemperature = GameComps.StructureTemperatures.GetHandle(base.get_gameObject());
        this.cooledAirOutputCell = this.building.GetUtilityOutputCell();
    }

    public void Sim200ms(float dt)
    {
        if (this.operational != null && !this.operational.IsOperational)
        {
            this.operational.SetActive(false, false);
            return;
        }
        this.UpdateState(dt);
    }

    private static bool UpdateStateCb(int cell, object data)
    {
        AirConditioner airConditioner = data as AirConditioner;
        airConditioner.cellCount++;
        airConditioner.envTemp += Grid.Temperature[cell];
        return true;
    }

    private void UpdateState(float dt)
    {
        bool value = this.consumer.IsSatisfied;
        this.envTemp = 0f;
        this.cellCount = 0;
        if (this.occupyArea != null && base.get_gameObject() != null)
        {
            this.occupyArea.TestArea(Grid.PosToCell(base.get_gameObject()), this, new Func<int, object, bool>(AirConditioner.UpdateStateCb));
            this.envTemp /= (float)this.cellCount;
        }
        this.lastEnvTemp = this.envTemp;
        List<GameObject> items = this.storage.items;
        for (int i = 0; i < items.Count; i++)
        {
            PrimaryElement component = items[i].GetComponent<PrimaryElement>();
            if (component.Mass > 0f)
            {
                if (!this.isLiquidConditioner || !component.Element.IsGas)
                {
                    if (this.isLiquidConditioner || !component.Element.IsLiquid)
                    {
                        value = true;
                        this.lastGasTemp = component.Temperature;
                        float num = component.Temperature + this.temperatureDelta;
                        if (num < 1f)
                        {
                            num = 1f;
                            this.lowTempLag = Mathf.Min(this.lowTempLag + dt / 5f, 1f);
                        }
                        else
                        {
                            this.lowTempLag = Mathf.Min(this.lowTempLag - dt / 5f, 0f);
                        }
                        ConduitFlow conduitFlow = Game.Instance.liquidConduitFlow;
                        float num2 = conduitFlow.AddElement(this.cooledAirOutputCell, component.ElementID, component.Mass, num, component.DiseaseIdx, component.DiseaseCount);
                        component.KeepZeroMassObject = true;
                        float num3 = num2 / component.Mass;
                        int num4 = (int)((float)component.DiseaseCount * num3);
                        component.Mass -= num2;
                        component.ModifyDiseaseCount(-num4, "AirConditioner.UpdateState");
                        float num5 = num - component.Temperature;
                        float num6 = num5 * component.Element.specificHeatCapacity * num2;
                        float display_dt = (this.lastSampleTime <= 0f) ? 1f : (Time.get_time() - this.lastSampleTime);
                        this.lastSampleTime = Time.get_time();
                        GameComps.StructureTemperatures.ProduceEnergy(this.structureTemperature, -num6, BUILDING.STATUSITEMS.OPERATINGENERGY.PIPECONTENTS_TRANSFER, display_dt);
                        break;
                    }
                }
            }
        }
        if (Time.get_time() - this.lastSampleTime > 2f)
        {
            GameComps.StructureTemperatures.ProduceEnergy(this.structureTemperature, 0f, BUILDING.STATUSITEMS.OPERATINGENERGY.PIPECONTENTS_TRANSFER, Time.get_time() - this.lastSampleTime);
            this.lastSampleTime = Time.get_time();
        }
        this.operational.SetActive(value, false);
        this.UpdateStatus();
    }

    private void OnOperationalChanged(object data)
    {
        if (this.operational.IsOperational)
        {
            this.UpdateState(0f);
        }
    }

    private void OnActiveChanged(object data)
    {
        this.UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (this.operational.IsActive)
        {
            if (this.lowTempLag >= 1f && !this.showingLowTemp)
            {
                this.statusHandle = ((!this.isLiquidConditioner) ? this.selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.CoolingStalledColdGas, this) : this.selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.CoolingStalledColdLiquid, this));
                this.showingLowTemp = true;
                this.showingHotEnv = false;
            }
            else if (this.lowTempLag <= 0f && (this.showingHotEnv || this.showingLowTemp))
            {
                this.statusHandle = this.selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.Cooling, null);
                this.showingLowTemp = false;
                this.showingHotEnv = false;
            }
            else if (this.statusHandle == Guid.Empty)
            {
                this.statusHandle = this.selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, Db.Get().BuildingStatusItems.Cooling, null);
                this.showingLowTemp = false;
                this.showingHotEnv = false;
            }
        }
        else
        {
            this.statusHandle = this.selectable.SetStatusItem(Db.Get().StatusItemCategories.Main, null, null);
        }
    }

    public List<Descriptor> GetDescriptors(BuildingDef def)
    {
        List<Descriptor> list = new List<Descriptor>();
        string formattedTemperature = GameUtil.GetFormattedTemperature(this.temperatureDelta, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Relative, true, false);
        Element element = ElementLoader.FindElementByName((!this.isLiquidConditioner) ? "Oxygen" : "Water");
        float num = Mathf.Abs(this.temperatureDelta * element.specificHeatCapacity);
        float dtu_s = num * 1f;
        Descriptor item = default(Descriptor);
        string txt = string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.HEATGENERATED_AIRCONDITIONER : UI.BUILDINGEFFECTS.HEATGENERATED_LIQUIDCONDITIONER, GameUtil.GetFormattedHeatEnergyRate(dtu_s, GameUtil.HeatEnergyFormatterUnit.Automatic));
        string tooltip = string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED_AIRCONDITIONER : UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED_LIQUIDCONDITIONER, GameUtil.GetFormattedHeatEnergy(num, GameUtil.HeatEnergyFormatterUnit.Automatic));
        item.SetupDescriptor(txt, tooltip, Descriptor.DescriptorType.Effect);
        list.Add(item);
        Descriptor item2 = default(Descriptor);
        item2.SetupDescriptor(string.Format(UI.BUILDINGEFFECTS.LIQUIDCOOLING, formattedTemperature), string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.TOOLTIPS.GASCOOLING : UI.BUILDINGEFFECTS.TOOLTIPS.LIQUIDCOOLING, formattedTemperature), Descriptor.DescriptorType.Effect);
        list.Add(item2);
        return list;
    }
}
