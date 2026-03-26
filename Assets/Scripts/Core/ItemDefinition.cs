using UnityEngine;

namespace Factory.Core
{
    [CreateAssetMenu(fileName = "NewItemDefinition", menuName = "Factory/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        public string id;
        public string itemName;
        public Sprite icon;
        [Tooltip("Color used for the item cube on conveyors. Leave black for auto-generated color.")]
        public Color itemColor = Color.clear;

        [Tooltip("Universal 3D model for this item on conveyors.")]
        public GameObject modelPrefab;
    }
}
