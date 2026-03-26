using UnityEngine;

namespace Factory.Factory
{
    public abstract class PlaceableObject : MonoBehaviour
    {
        public string Name;
        public int Cost;
        
        // Base visuals handled by child classes or prefabs
        public virtual void OnPlaced() 
        {
            // Re-enable actual logic
        }
    }
}
