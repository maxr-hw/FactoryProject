using UnityEngine;
using System.Collections.Generic;
using Factory.Core;

namespace Factory.Factory
{
    public class Splitter : FactoryBuilding, IItemReceiver
    {
        private int outputIndex = 0;
        private List<ConveyorItem> itemsInTransit = new List<ConveyorItem>();
        private const float transferTime = 0.5f;

        private void Update()
        {
            if (itemsInTransit.Count > 0)
            {
                // Instantly pass for now, or use a timer 
                TryOutput();
            }
        }

        public bool CanReceive(ItemDefinition type)
        {
            return itemsInTransit.Count < 3; // buffer limit
        }

        public void ReceiveItem(ConveyorItem item)
        {
            // Visual pop to center
            item.transform.SetParent(transform);
            item.transform.position = transform.position + Vector3.up * 0.5f;
            itemsInTransit.Add(item);
        }

        private void TryOutput()
        {
            // Determine the 3 output directions (Forward, Right, Left)
            Vector3[] directions = new Vector3[]
            {
                transform.forward,
                transform.right,
                -transform.right
            };

            ConveyorItem toRoute = itemsInTransit[0];

            // Round Robin check up to 3 times
            for (int i = 0; i < 3; i++)
            {
                Vector3 tryDir = directions[outputIndex];
                IItemReceiver receiver = FindReceiver(tryDir);

                if (receiver != null && receiver.CanReceive(toRoute.Type))
                {
                    receiver.ReceiveItem(toRoute);
                    itemsInTransit.RemoveAt(0);
                    outputIndex = (outputIndex + 1) % 3;
                    return; // successful route
                }

                // Try next port
                outputIndex = (outputIndex + 1) % 3;
            }
        }

        private IItemReceiver FindReceiver(Vector3 dir)
        {
            Collider[] cols = Physics.OverlapSphere(transform.position + dir, 0.4f, LayerMask.GetMask("Interactable"));
            foreach (var col in cols)
            {
                if (col.gameObject == gameObject) continue;
                IItemReceiver receiver = col.GetComponent<IItemReceiver>();
                if (receiver != null) return receiver;
                receiver = col.GetComponentInParent<IItemReceiver>();
                if (receiver != null) return receiver;
            }
            return null;
        }
    }
}
