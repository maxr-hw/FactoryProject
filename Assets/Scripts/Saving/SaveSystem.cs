using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Factory.Core;
using Factory.Economy;
using Factory.Contracts;
using Factory.Factory;
using System.Linq;

namespace Factory.Saving
{
    [System.Serializable]
    public class FactorySaveData
    {
        public int currentMoney;
        public List<ContractSaveData> activeContracts = new List<ContractSaveData>();
        public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
    }

    [System.Serializable]
    public class ContractSaveData
    {
        public string companyName;
        public float timeRemaining;
        public List<ItemStackSaveData> deliveredItems = new List<ItemStackSaveData>();
    }

    [System.Serializable]
    public class BuildingSaveData
    {
        public Vector2Int gridPos;
        public Direction facing;
        public string machineType; // ID or Type name
        public string recipeId;
        public List<ItemStackSaveData> inputs = new List<ItemStackSaveData>();
        public List<ItemStackSaveData> outputs = new List<ItemStackSaveData>();
    }

    [System.Serializable]
    public class ItemStackSaveData
    {
        public string itemId;
        public int amount;
    }

    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private string savePath;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            savePath = Path.Combine(Application.persistentDataPath, "factory_save.json");
        }

        public void SaveGame()
        {
            FactorySaveData data = new FactorySaveData();
            
            if (EconomyManager.Instance != null) data.currentMoney = EconomyManager.Instance.CurrentMoney;
            
            if (ContractManager.Instance != null)
            {
                var activeList = ContractManager.Instance.GetActiveContracts();
                foreach (var ac in activeList)
                {
                    ContractSaveData cData = new ContractSaveData();
                    cData.companyName = ac.definition.companyName;
                    cData.timeRemaining = ac.timeRemaining;
                    
                    foreach (var kvp in ac.deliveredItems)
                    {
                        cData.deliveredItems.Add(new ItemStackSaveData { 
                            itemId = kvp.Key, 
                            amount = kvp.Value 
                        });
                    }
                    data.activeContracts.Add(cData);
                }
            }

            if (GridManager.Instance != null)
            {
                // Grab all buildings from grid. To avoid duplicates (multi-tile structures register on multiple tiles),
                // we'll keep a hashset of processed instance IDs.
                HashSet<int> processed = new HashSet<int>();

                // A properly exposed method to get all unique buildings from grid would be better, 
                // but since we can access them by scanning the grid space or storing a list in GridManager...
                // For this prototype, let's assume GridManager has `GetAllBuildings()`.
                // Actually, I should add `GetAllBuildings()` to GridManager.
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"Game Saved to {savePath}");
        }

        public void LoadGame()
        {
            if (!File.Exists(savePath))
            {
                Debug.LogWarning("No save file found.");
                return;
            }

            string json = File.ReadAllText(savePath);
            FactorySaveData data = JsonUtility.FromJson<FactorySaveData>(json);

            // Needs specialized logic to clear the map, restore money/contracts, and instantiate buildings.
            Debug.Log("Load game logic triggered - data parsed.");
        }
    }
}
