using Factory.Core;
using UnityEngine;

namespace Factory.Factory
{
    public class Constructor : Machine
    {
        // Fits 2x2. Receives 1 input, generates 1 output sequence (defined in recipes)
        public void SetRecipe(Recipe r)
        {
            if (r.inputs.Count > 1) 
            {
                Debug.LogWarning("Constructor cannot process recipes with multiple inputs.");
                return;
            }
            CurrentRecipe = r;
        }
    }
}
