using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Factory.Core;

namespace Factory.Factory
{
    public abstract class Machine : FactoryBuilding, IItemReceiver
    {
        public Recipe CurrentRecipe;
        protected float processingProgress;
        protected bool isProcessing;

        // Uses a flat inventory for inputs/outputs
        public List<ItemStack> InputInventory = new List<ItemStack>();
        public List<ItemStack> OutputInventory = new List<ItemStack>();

        protected virtual void Update()
        {
            if (CurrentRecipe != null)
            {
                if (!isProcessing && CanProcess())
                {
                    StartProcessing();
                }

                if (isProcessing)
                {
                    processingProgress += Time.deltaTime;
                    if (processingProgress >= CurrentRecipe.processingTime)
                    {
                        FinishProcessing();
                    }
                }
            }

            TryEjectOutput();
        }

        protected virtual bool CanProcess()
        {
            if (CurrentRecipe == null) return false;

            // Verify all inputs are present in required amounts
            foreach (var req in CurrentRecipe.inputs)
            {
                var stock = InputInventory.FirstOrDefault(s => s.item == req.item);
                if (stock == null || stock.amount < req.amount) return false;
            }

            // Output buffer limit check (100 items per output type)
            foreach (var outReq in CurrentRecipe.outputs)
            {
                var stock = OutputInventory.FirstOrDefault(s => s.item == outReq.item);
                if (stock != null && stock.amount >= 100) return false;
            }

            return true;
        }

        protected virtual void StartProcessing()
        {
            isProcessing = true;
            processingProgress = 0;

            // Consume inputs
            foreach (var req in CurrentRecipe.inputs)
            {
                var stock = InputInventory.FirstOrDefault(s => s.item == req.item);
                if (stock != null) stock.amount -= req.amount;
            }
            InputInventory.RemoveAll(s => s.amount <= 0);
        }

        protected virtual void FinishProcessing()
        {
            isProcessing = false;
            processingProgress = 0;

            foreach (var outItem in CurrentRecipe.outputs)
            {
                var stock = OutputInventory.FirstOrDefault(s => s.item == outItem.item);
                if (stock != null) stock.amount += outItem.amount;
                else OutputInventory.Add(new ItemStack(outItem.item, outItem.amount));
            }
        }

        public virtual bool CanReceive(ItemDefinition item)
        {
            if (CurrentRecipe == null) return false;
            bool isAllowed = CurrentRecipe.inputs.Any(req => req.item == item);
            if (!isAllowed) return false;

            var stock = InputInventory.FirstOrDefault(s => s.item == item);
            return (stock == null) || (stock.amount < 100); // requested limit 100 per material buffer
        }

        public virtual void ReceiveItem(ConveyorItem item)
        {
            ItemDefinition type = item.Type;
            Destroy(item.gameObject); // visually consumed into the machine

            var stock = InputInventory.FirstOrDefault(s => s.item == type);
            if (stock != null) stock.amount++;
            else InputInventory.Add(new ItemStack(type, 1));
        }

        protected virtual void TryEjectOutput()
        {
            if (OutputInventory.Count == 0 || OutputInventory[0].amount <= 0) return;

            // Look for explicit ConnectionPoints of type Output
            ConnectionPoint[] ports = GetComponentsInChildren<ConnectionPoint>();
            var outputPorts = ports.Where(p => p.pointType == ConnectionPoint.PointType.Output).ToList();

            if (outputPorts.Count > 0)
            {
                foreach (var port in outputPorts)
                {
                    if (EjectFromPoint(port.transform.position, port.transform.forward)) return;
                }
            }
            else
            {
                // Fallback to transform forward
                EjectFromPoint(transform.position + transform.forward * (size.y * 0.5f + 0.1f) + Vector3.up * 0.5f, transform.forward);
            }
        }

        protected bool EjectFromPoint(Vector3 position, Vector3 direction)
        {
            Collider[] cols = Physics.OverlapSphere(position, 0.4f, LayerMask.GetMask("Interactable"));
            IItemReceiver receiver = null;

            foreach (var col in cols)
            {
                if (col.gameObject == gameObject) continue;
                receiver = col.GetComponent<IItemReceiver>() ?? col.GetComponentInParent<IItemReceiver>();
                if (receiver != null) break;
            }

            if (receiver != null && receiver.CanReceive(OutputInventory[0].item))
            {
                var outItemDef = OutputInventory[0].item;
                OutputInventory[0].amount--;
                if (OutputInventory[0].amount <= 0) OutputInventory.RemoveAt(0);

                ConveyorItem cItem = ConveyorItem.Spawn(outItemDef, position);
                receiver.ReceiveItem(cItem);
                return true;
            }
            return false;
        }
    }
}
