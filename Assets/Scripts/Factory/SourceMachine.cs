using UnityEngine;
using Factory.Core;
using Factory.Economy;

namespace Factory.Factory
{
    public class SourceMachine : FactoryBuilding
    {
        public ItemDefinition itemToSpawn;
        public float spawnInterval = 2f;
        public int costPerSpawn = 1; // Money deducted per item generated

        private float spawnTimer = 0f;

        private void Awake()
        {
            // Ensure the machine is on the Interactable layer for SelectionManager
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            foreach (Transform t in transform) t.gameObject.layer = gameObject.layer;
        }

        private void Update()
        {
            if (itemToSpawn == null) return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                TrySpawnAndEject();
            }
        }

        private void TrySpawnAndEject()
        {
            // The machine is 2x2, so forward * 1.0f is exactly on the edge.
            // Conveyors are 1x1. Casting slightly ahead (1.1f) to find adjacent belt.
            Vector3 castPoint = transform.position + transform.forward * 1.1f;
            Collider[] cols = Physics.OverlapSphere(castPoint, 0.5f, LayerMask.GetMask("Interactable"));

            IItemReceiver receiver = null;
            foreach (var col in cols)
            {
                if (col.gameObject == gameObject) continue;
                receiver = col.GetComponent<IItemReceiver>();
                if (receiver == null) receiver = col.GetComponentInParent<IItemReceiver>();
                
                if (receiver != null) break;
            }

            if (receiver != null && receiver.CanReceive(itemToSpawn))
            {
                // Try spending money
                if (EconomyManager.Instance != null && !EconomyManager.Instance.CanAfford(costPerSpawn))
                    return; 

                EconomyManager.Instance?.Spend(costPerSpawn);

                // Spawn at the very edge so it enters the conveyor immediately
                Vector3 spawnPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
                ConveyorItem cItem = ConveyorItem.Spawn(itemToSpawn, spawnPos);
                receiver.ReceiveItem(cItem);
                
                spawnTimer = 0f;
            }
        }
    }
}
