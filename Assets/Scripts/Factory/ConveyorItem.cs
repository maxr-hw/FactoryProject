using UnityEngine;
using Factory.Core;

namespace Factory.Factory
{
    /// <summary>
    /// A small colored 3D cube that slides along conveyors representing one item unit.
    /// </summary>
    public class ConveyorItem : MonoBehaviour
    {
        public ItemDefinition Type { get; private set; }

        private MeshRenderer meshRenderer;

        public void Initialize(ItemDefinition type)
        {
            Type = type;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            // If we have a custom model, we don't need to apply the color to the root MeshRenderer
            // as the model brings its own materials. Only apply to the procedural cube.
            if (Type != null && Type.modelPrefab != null)
            {
                // Optionally we could tint the model if needed, but for now we trust the FBX materials
                return;
            }

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) return;

            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

            Color col = Color.white;
            if (Type != null && Type.itemColor != Color.clear)
                col = Type.itemColor;
            else if (Type != null)
                col = ColorFromString(Type.itemName); // deterministic fallback per item name

            mat.color = col;
            mat.SetColor("_BaseColor", col);
            meshRenderer.material = mat;
        }

        /// <summary>
        /// Factory: spawn a new ConveyorItem at a world position, sized 0.35 units.
        /// No prefab needed.
        /// </summary>
        public static ConveyorItem Spawn(ItemDefinition type, Vector3 position, Transform parent = null)
        {
            GameObject go;
            
            if (type != null && type.modelPrefab != null)
            {
                go = Instantiate(type.modelPrefab);
                go.name = $"Item_{type.itemName}";
                // Custom models use 1.0f scale by default from Blender
                go.transform.localScale = Vector3.one * 1.25f;
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = type != null ? $"Item_{type.itemName}" : "Item";
                go.transform.localScale = Vector3.one * 0.35f;
            }

            go.transform.position = position;
            if (parent != null) go.transform.SetParent(parent);

            // Remove colliders so they don't block raycasts or physics
            foreach (var col in go.GetComponentsInChildren<Collider>())
            {
                Destroy(col);
            }
            
            go.layer = LayerMask.NameToLayer("Ignore Raycast");

            ConveyorItem item = go.AddComponent<ConveyorItem>();
            item.Initialize(type);
            return item;
        }

        // Public helper — returns the deterministic color for any item (used by SourceInteractUI swatches)
        public static Color GetItemColor(ItemDefinition item)
        {
            if (item == null) return Color.white;
            if (item.itemColor != Color.clear) return item.itemColor;
            return ColorFromString(item.name);
        }

        // Deterministic color from item name hash 
        private static Color ColorFromString(string s)
        {
            float h = (Mathf.Abs(s.GetHashCode()) % 1000) / 1000f;
            return Color.HSVToRGB(h, 0.8f, 0.95f);
        }
    }
}
