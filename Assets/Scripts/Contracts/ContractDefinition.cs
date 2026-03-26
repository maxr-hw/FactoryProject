using UnityEngine;
using System.Collections.Generic;
using Factory.Core;

namespace Factory.Contracts
{
    [CreateAssetMenu(fileName = "NewContract", menuName = "Factory/Contract")]
    public class ContractDefinition : ScriptableObject
    {
        public string companyName;
        [TextArea] public string description;
        
        public int difficultyRating = 1;
        public List<ItemStack> requiredItems;
        public float timeLimit = 120f;
        public int rewardMoney;

        // Optional: array of MachineDefinitions or Recipes to unlock upon completion
        public MachineDefinition[] unlocksMachines;
        public Recipe[] unlocksRecipes;
    }
}
