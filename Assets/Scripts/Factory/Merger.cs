using UnityEngine;
using System.Collections.Generic;
using Factory.Core;

namespace Factory.Factory
{
    public class Merger : FactoryBuilding, IItemReceiver
    {
        private Queue<ConveyorItem> itemsInTransit = new Queue<ConveyorItem>();

        private void Update()
        {
            if (itemsInTransit.Count > 0)
            {
                TryOutput();
            }
        }

        public bool CanReceive(ItemDefinition type)
        {
            // Arbitrary buffer size for mergers
            return itemsInTransit.Count < 3;
        }

        public void ReceiveItem(ConveyorItem item)
        {
            item.transform.SetParent(transform);
            item.transform.position = transform.position + Vector3.up * 0.5f;
            itemsInTransit.Enqueue(item);
        }

        private void TryOutput()
        {
            ConveyorItem toRoute = itemsInTransit.Peek();
            IItemReceiver receiver = FindReceiver(transform.forward);

            if (receiver != null && receiver.CanReceive(toRoute.Type))
            {
                receiver.ReceiveItem(toRoute);
                itemsInTransit.Dequeue();
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
