using UnityEngine;
using Factory.Core;
using Factory.Contracts;

namespace Factory.Factory
{
    public class DeliveryMachine : FactoryBuilding, IItemReceiver
    {
        // 2x2 machine that acts as a sink, routing items out to fulfilling contracts.
        
        private void Awake()
        {
            // Ensure the machine is on the Interactable layer for selection
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            foreach (Transform t in transform) t.gameObject.layer = gameObject.layer;
        }

        public bool CanReceive(ItemDefinition type)
        {
            return true; // The vacuum sink accepts EVERYTHING.
        }

        public void ReceiveItem(ConveyorItem item)
        {
            ItemDefinition delivered = item.Type;
            Destroy(item.gameObject); // Consumed visually

            ContractManager.Instance?.HandleItemDelivered(delivered);
        }
    }
}
