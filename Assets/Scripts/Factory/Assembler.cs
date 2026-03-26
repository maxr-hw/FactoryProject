using Factory.Core;
using UnityEngine;

namespace Factory.Factory
{
    public class Assembler : Machine
    {
        // Fits 3x3. Receives 2 inputs, generates 1 output sequence.
        public void SetRecipe(Recipe r)
        {
            if (r.inputs.Count != 2) 
            {
                Debug.LogWarning("Assembler requires exactly 2 inputs for its recipe.");
                return;
            }
            CurrentRecipe = r;
        }
    }
}
