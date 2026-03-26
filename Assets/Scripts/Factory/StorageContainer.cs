using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Factory.Core;

namespace Factory.Factory
{
    public class StorageContainer : FactoryBuilding, IItemReceiver
    {
        public List<ItemStack> storedItems = new List<ItemStack>();
        private const int MaxCapacity = 200;

        private float outputTimer = 0f;
        private const float outputRate = 0.5f;

        private void Update()
        {
            outputTimer += Time.deltaTime;
            if (outputTimer >= outputRate)
            {
                TryOutput();
                outputTimer = 0f;
            }
        }

        public int GetTotalItems()
        {
            int total = 0;
            foreach (var stack in storedItems) total += stack.amount;
            return total;
        }

        public bool CanReceive(ItemDefinition type)
        {
            return GetTotalItems() < MaxCapacity;
        }

        public void ReceiveItem(ConveyorItem item)
        {
            ItemDefinition type = item.Type;
            Destroy(item.gameObject); // Consumed visually

            var stack = storedItems.FirstOrDefault(s => s.item == type);
            if (stack != null)
            {
                stack.amount++;
            }
            else
            {
                storedItems.Add(new ItemStack(type, 1));
            }
        }

        private void TryOutput()
        {
            if (storedItems.Count == 0) return;

            IItemReceiver receiver = FindReceiverFromPoint(transform.position + transform.forward * 2f); // Size is 2x2, so forward by 2 from center? Wait, if 2x2, center logic needs testing. Let's cast from edge.
            
            // Re-align cast to front edge
            Vector3 frontEdge = transform.position + transform.forward * 1.5f;
            
            if (receiver == null) receiver = FindReceiverFromPoint(frontEdge);

            if (receiver != null && storedItems[0].amount > 0)
            {
                ItemDefinition outType = storedItems[0].item;
                if (receiver.CanReceive(outType))
                {
                    // Deduct
                    storedItems[0].amount--;
                    if (storedItems[0].amount <= 0) storedItems.RemoveAt(0);

                    // Spawn visual
                    GameObject visual = new GameObject("ConveyorItem");
                    ConveyorItem cItem = visual.AddComponent<ConveyorItem>();
                    SpriteRenderer sr = visual.AddComponent<SpriteRenderer>(); // Mock visual
                    cItem.Initialize(outType);

                    receiver.ReceiveItem(cItem);
                }
            }
        }

        private IItemReceiver FindReceiverFromPoint(Vector3 pt)
        {
            Collider[] cols = Physics.OverlapSphere(pt, 0.4f, LayerMask.GetMask("Interactable"));
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
