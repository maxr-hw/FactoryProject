using UnityEngine;
using System.Collections.Generic;

namespace Factory.Factory
{
    public abstract class FactoryBuilding : MonoBehaviour
    {
        public string machineName;
        public int cost;
        public Vector2Int gridPosition;
        public Vector2Int size = Vector2Int.one;
        public Direction facingDirection;

        protected DirectionIndicator indicator;

        // Base logic for connections if needed
        public virtual void OnPlaced() 
        {
            // Auto-spawn direction indicator if none exists
            if (indicator == null) 
            {
                GameObject go = new GameObject("DirectionIndicator");
                go.transform.SetParent(transform, false);
                indicator = go.AddComponent<DirectionIndicator>();
            }
        }

        public virtual void OnRemoved()
        {
            // Logic for cleanup
        }
    }
}
