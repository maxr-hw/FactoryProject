using Factory.Core;
using Factory.Factory;

namespace Factory.Contracts
{
    [System.Serializable]
    public class Contract
    {
        public string contractName;
        public ItemDefinition requiredItem;
        public int requiredAmount;
        public float timeLimit;
        public int reward;

        public bool isCompleted;
        public float timeRemaining;
        public int deliveredCount;
    }
}
