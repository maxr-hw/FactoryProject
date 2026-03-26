using UnityEngine;
using Factory.Core;

namespace Factory.Factory
{
    /// <summary>
    /// Generic machine that processes items using a Recipe.
    /// Works for both single-input (Constructor) and multi-input (Assembler) recipes.
    /// This replaces any broken MachineProcessor script references on prefabs.
    /// </summary>
    [AddComponentMenu("Factory/Machine Processor")]
    public class MachineProcessor : Machine
    {
        [Tooltip("Optional: pre-assign a recipe in the Inspector without opening the shop UI.")]
        public Recipe defaultRecipe;

        [Tooltip("Time between attempts to eject output (seconds).")]
        public float ejectInterval = 0.5f;
        private float ejectTimer;

        protected override void Update()
        {
            base.Update();

            ejectTimer += Time.deltaTime;
            if (ejectTimer >= ejectInterval)
            {
                ejectTimer = 0f;
                TryEjectOutput();
            }
        }

        private void Start()
        {
            if (defaultRecipe != null && CurrentRecipe == null)
                SetRecipe(defaultRecipe);
        }

        public void SetRecipe(Recipe r)
        {
            CurrentRecipe = r;
            Debug.Log($"[{gameObject.name}] Recipe set to: {r.name}");
        }
    }
}
