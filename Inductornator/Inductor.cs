using System;
using UnityEngine;

namespace Inductornator
{

    public class Inductor : ComplexFabricator
    {
        private Guid statusHandle;

        private static readonly EventSystem.IntraObjectHandler<Inductor> CheckPipesDelegate = new EventSystem.IntraObjectHandler<Inductor>(delegate (Inductor component, object data)
        {
            component.CheckPipes(data);
        });

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
        }

        private void CheckPipes(object data)
        {
            KSelectable component = base.GetComponent<KSelectable>();
            int cell = Grid.OffsetCell(Grid.PosToCell(this), InductorConfig.outPipeOffset);
            GameObject gameObject = Grid.Objects[cell, 16];
            if (gameObject != null)
            {
                PrimaryElement component2 = gameObject.GetComponent<PrimaryElement>();
                if (component2.Element.highTemp > ElementLoader.FindElementByHash(SimHashes.MoltenTungsten).lowTemp)
                {
                    component.RemoveStatusItem(this.statusHandle, false);
                }
                else
                {
                    this.statusHandle = component.AddStatusItem(Db.Get().BuildingStatusItems.PipeMayMelt, null);
                }
            }
            else
            {
                component.RemoveStatusItem(this.statusHandle, false);
            }
        }
    }
}
