using UnityEngine;
using System;
using System.Collections.Generic;
using Factory.Core;
using Factory.Economy;

namespace Factory.Contracts
{
    public class ContractManager : MonoBehaviour
    {
        public static ContractManager Instance { get; private set; }

        public List<ContractDefinition> contractPool;
        
        [System.Serializable]
        public class ActiveContract
        {
            public ContractDefinition definition;
            public Dictionary<string, int> deliveredItems = new Dictionary<string, int>(); // Switched to string key
            public Dictionary<ItemDefinition, int> targetAmounts = new Dictionary<ItemDefinition, int>();
            public float timeRemaining;
            public int scaledReward;
        }

        private List<ActiveContract> activeContracts = new List<ActiveContract>();
        private float nextOfferTimer;
        public float NextOfferTimer => nextOfferTimer;
        private bool firstOfferMade = false;

        public int completedContractsCount = 0;
        public int CurrentLevel => (completedContractsCount / 5) + 1;

        public Action OnContractUpdated;
        public Action OnContractCompleted;
        public Action OnContractFailed;
        public Action<ContractDefinition> OnContractOffered;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // First offer arrives in 15-30 seconds
            nextOfferTimer = UnityEngine.Random.Range(15f, 30f);

            // Robust check: if pool is empty, load from Resources
            if (contractPool == null || contractPool.Count == 0)
            {
                var loaded = Resources.LoadAll<ContractDefinition>("Factory/Contracts");
                if (loaded != null && loaded.Length > 0)
                {
                    contractPool = new List<ContractDefinition>(loaded);
                    Debug.Log($"[ContractManager] Auto-loaded {contractPool.Count} contracts from Resources.");
                }
                else
                {
                    Debug.LogWarning("[ContractManager] Contract pool is empty and no contracts found in Resources/Factory/Contracts!");
                }
            }
        }

        private void Update()
        {
            // Debug Log every few seconds to check status
            if (Time.frameCount % 300 == 0) // Roughly every 5-6 seconds
            {
                //Debug.Log($"[ContractManager] Active: {activeContracts.Count}, Next Offer In: {nextOfferTimer:F1}s, Pool Size: {(contractPool != null ? contractPool.Count : 0)}");
            }

            // Update timers for active contracts
            for (int i = activeContracts.Count - 1; i >= 0; i--)
            {
                var ac = activeContracts[i];
                ac.timeRemaining -= Time.deltaTime;
                if (ac.timeRemaining <= 0)
                {
                    FailContract(ac);
                }
            }

            // Offer logic
            if (activeContracts.Count < 2)
            {
                nextOfferTimer -= Time.deltaTime;
                if (nextOfferTimer <= 0)
                {
                    Debug.Log("[ContractManager] Timer reached 0, attempting to offer contract...");
                    if (TryOfferRandomContract())
                    {
                        firstOfferMade = true;
                        // Reset timer for next offer (2-3 minutes)
                        nextOfferTimer = UnityEngine.Random.Range(120f, 180f);
                        Debug.Log($"[ContractManager] Contract offered. Next offer in {nextOfferTimer:F1}s.");
                    }
                    else
                    {
                        // Retry sooner if we couldn't offer anything (pool empty or all active)
                        nextOfferTimer = 10f;
                    }
                }
            }

            if (activeContracts.Count > 0) OnContractUpdated?.Invoke();
        }

        private bool TryOfferRandomContract()
        {
            if (contractPool == null || contractPool.Count == 0) return false;
            
            // Filters based on level: prefer contracts with difficulty close to CurrentLevel
            // We allow contracts up to CurrentLevel + 1 or always allow level 1
            var available = contractPool.FindAll(c => 
                !activeContracts.Exists(ac => ac.definition == c) && 
                (c.difficultyRating <= CurrentLevel + 1 || c.difficultyRating == 1)
            );

            if (available.Count > 0)
            {
                // Weighted selection or just random from available
                OfferContract(available[UnityEngine.Random.Range(0, available.Count)]);
                return true;
            }
            return false;
        }

        public List<ActiveContract> GetActiveContracts() => activeContracts;
        
        public void OfferContract(ContractDefinition contract)
        {
            Debug.Log($"[ContractManager] Offering contract from company: {contract.companyName}");
            OnContractOffered?.Invoke(contract);
        }

        public void AcceptContract(ContractDefinition contract)
        {
            if (activeContracts.Count >= 2) return;

            ActiveContract ac = new ActiveContract();
            ac.definition = contract;
            
            // Scaling logic based on level
            float levelMultiplier = 1f + (CurrentLevel - 1) * 0.2f; // +20% per level
            float timeMultiplier = 1f + (CurrentLevel - 1) * 0.3f;  // +30% time per level (as requested, more time for harder ones)

            ac.timeRemaining = contract.timeLimit * timeMultiplier;
            ac.scaledReward = Mathf.RoundToInt(contract.rewardMoney * levelMultiplier);
            
            foreach (var req in contract.requiredItems)
            {
                if (req.item == null) continue;
                // Quantities also scale with level (min 1)
                int scaledAmount = Mathf.Max(1, Mathf.RoundToInt(req.amount * levelMultiplier));
                
                // Use Name as key for delivery tracking to avoid reference issues
                string itemName = GetNormalizedName(req.item);
                ac.deliveredItems[itemName] = 0;
                ac.targetAmounts[req.item] = scaledAmount;
            }
            
            activeContracts.Add(ac);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayContractStarted();
            OnContractUpdated?.Invoke();
        }

        private string GetNormalizedName(ItemDefinition item)
        {
            if (item == null) return "Unknown";
            // Match based on itemName field if set, otherwise fallback to asset name, removing spaces/underscores
            string raw = string.IsNullOrEmpty(item.itemName) ? item.name : item.itemName;
            return raw.Replace("_", " ").Trim();
        }

        public void HandleItemDelivered(ItemDefinition itemDelivered)
        {
            if (activeContracts.Count == 0 || itemDelivered == null) return;

            string deliveredName = GetNormalizedName(itemDelivered);

            foreach (var ac in activeContracts)
            {
                if (ac.deliveredItems.ContainsKey(deliveredName))
                {
                    ac.deliveredItems[deliveredName]++;
                    OnContractUpdated?.Invoke();

                    if (CheckCompletion(ac))
                    {
                        CompleteContract(ac);
                        return; // Done with this delivery
                    }
                }
            }
        }

        private bool CheckCompletion(ActiveContract ac)
        {
            foreach (var req in ac.definition.requiredItems)
            {
                string reqName = GetNormalizedName(req.item);
                int target = ac.targetAmounts.ContainsKey(req.item) ? ac.targetAmounts[req.item] : req.amount;
                if (!ac.deliveredItems.ContainsKey(reqName) || ac.deliveredItems[reqName] < target) return false;
            }
            return true;
        }

        private void FailContract(ActiveContract ac)
        {
            activeContracts.Remove(ac);
            OnContractFailed?.Invoke();
            OnContractUpdated?.Invoke();
            Debug.Log($"Contract with {ac.definition.companyName} Failed! Time limit exceeded.");
        }

        private void CompleteContract(ActiveContract ac)
        {
            completedContractsCount++;
            EconomyManager.Instance.Earn(ac.scaledReward);

            if (ac.definition.unlocksMachines != null && BuildManager.Instance != null)
            {
                foreach (var m in ac.definition.unlocksMachines)
                {
                    if (!BuildManager.Instance.machineCatalog.Contains(m))
                    {
                        BuildManager.Instance.machineCatalog.Add(m);
                    }
                }
            }

            activeContracts.Remove(ac);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayContractCompleted();
            OnContractCompleted?.Invoke();
            OnContractUpdated?.Invoke();
            
            Debug.Log($"Contract with '{ac.definition.companyName}' Completed! Rewarded ${ac.scaledReward}. Total completed: {completedContractsCount}");
        }
    }
}
