using UnityEngine;
using System.Collections.Generic;

namespace Factory.Factory
{
    // A single 1x1 segment that pushes items forward at 2m/s.
    public class ConveyorSegment : FactoryBuilding, IItemReceiver, IItemProvider
    {
        public float speed = 2f;
        public float length = 1f;
        
        private List<ConveyorItem> itemsOnBelt = new List<ConveyorItem>();
        private const float itemSpacing = 0.5f;
        private IItemReceiver nextReceiver;

        private void Update()
        {
            MoveItems();
            TryPassToNext();
        }

        private void MoveItems()
        {
            if (itemsOnBelt.Count == 0) return;

            // Move first item
            float currentDist = GetLocalDistance(itemsOnBelt[0].transform.position);
            float targetDist = currentDist + (speed * Time.deltaTime);

            if (targetDist > length) targetDist = length;
            itemsOnBelt[0].transform.position = transform.position + transform.forward * targetDist + Vector3.up * 0.5f;

            // Move trailing items, maintaining physical spacing constraint
            for (int i = 1; i < itemsOnBelt.Count; i++)
            {
                float prevDist = GetLocalDistance(itemsOnBelt[i-1].transform.position);
                float curDist = GetLocalDistance(itemsOnBelt[i].transform.position);
                float maxDist = prevDist - itemSpacing;
                
                float newDist = curDist + (speed * Time.deltaTime);
                if (newDist > maxDist) newDist = maxDist;
                if (newDist > length) newDist = length;

                itemsOnBelt[i].transform.position = transform.position + transform.forward * newDist + Vector3.up * 0.5f;
            }
        }

        private void TryPassToNext()
        {
            if (itemsOnBelt.Count == 0) return;
            
            float dist = GetLocalDistance(itemsOnBelt[0].transform.position);
            if (dist >= length - 0.05f)
            {
                if (nextReceiver == null) nextReceiver = FindNextReceiver();
                
                if (nextReceiver != null && nextReceiver.CanReceive(itemsOnBelt[0].Type))
                {
                    nextReceiver.ReceiveItem(itemsOnBelt[0]);
                    itemsOnBelt.RemoveAt(0);
                    nextReceiver = null; // Re-check next time since target could be destroyed
                }
            }
        }

        private float GetLocalDistance(Vector3 worldPos)
        {
            // Simple projected distance along forward vector, assuming flat terrain
            return Vector3.Distance(transform.position, new Vector3(worldPos.x, transform.position.y, worldPos.z));
        }

        private IItemReceiver FindNextReceiver()
        {
            // Cast slightly ahead to grab the next interactable building
            Collider[] cols = Physics.OverlapSphere(transform.position + transform.forward * length, 0.4f, LayerMask.GetMask("Interactable"));
            foreach (var col in cols)
            {
                if (col.gameObject == gameObject) continue;
                IItemReceiver receiver = col.GetComponent<IItemReceiver>();
                if (receiver != null) return receiver;
                
                // Backup for parents
                receiver = col.GetComponentInParent<IItemReceiver>();
                if (receiver != null) return receiver;
            }
            return null;
        }

        // --- IItemReceiver ---
        public bool CanReceive(Core.ItemDefinition type)
        {
            if (itemsOnBelt.Count == 0) return true;
            float dist = GetLocalDistance(itemsOnBelt[itemsOnBelt.Count - 1].transform.position);
            return dist > itemSpacing;
        }

        public void ReceiveItem(ConveyorItem item)
        {
            item.transform.SetParent(transform);
            item.transform.position = transform.position + Vector3.up * 0.5f; // start of belt
            itemsOnBelt.Add(item);
        }

        // --- IItemProvider ---
        public bool HasItem() => itemsOnBelt.Count > 0 && GetLocalDistance(itemsOnBelt[0].transform.position) >= length - 0.05f;
        public Core.ItemDefinition PeekItem() => itemsOnBelt.Count > 0 ? itemsOnBelt[0].Type : null;
        public Core.ItemDefinition ExtractItem()
        {
            if (itemsOnBelt.Count == 0) return null;
            var type = itemsOnBelt[0].Type;
            // Physical item destruction happens locally when machine consumes it (in machine logic)
            // Or we pass the ConveyorItem object. Wait, ExtractItem implies we take but don't take physical.
            // Actually the standard is Machine just receives if it's IItemReceiver. 
            // So we'll pass standard logic directly via MoveItems() -> TryPassToNext().
            return type;
        }
    }
}
