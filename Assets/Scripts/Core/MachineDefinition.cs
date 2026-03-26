using UnityEngine;
using Factory.Factory;

namespace Factory.Core
{
    public enum MachineType
    {
        None, Conveyor, Splitter, Merger, Constructor, Assembler, Smelter, Storage, Source, Delivery
    }

    [CreateAssetMenu(fileName = "NewMachineDefinition", menuName = "Factory/Machine Definition")]
    public class MachineDefinition : ScriptableObject
    {
        public string machineName;
        public int cost;
        public Vector2Int size = new Vector2Int(1, 1);
        public MachineType type;
        public GameObject prefab;
        public Sprite machineIcon;
        public Recipe defaultRecipe;
    }
}
