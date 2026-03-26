using UnityEngine;
using Factory.Factory;
using System.Collections.Generic;

namespace Factory.Core
{
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Factory/Recipe")]
    public class Recipe : ScriptableObject
    {
        public List<ItemStack> inputs = new List<ItemStack>();
        public List<ItemStack> outputs = new List<ItemStack>();
        public float processingTime = 3f;
    }
}
