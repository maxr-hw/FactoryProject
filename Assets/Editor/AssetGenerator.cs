#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Factory.Core;
using System.Collections.Generic;

namespace Factory.EditorTools
{
    public class AssetGenerator
    {
        [MenuItem("Factory/Generate All Default Assets")]
        public static void GenerateAssets()
        {
            CreateFolders();

            // Create Items
            string[] items = new string[]
            {
                "Iron Ore", "Copper Ore", "Coal", "Quartz", "Limestone",
                "Iron Ingot", "Copper Ingot", "Steel Ingot", "Quartz Crystal", "Concrete",
                "Iron Plate", "Iron Rod", "Screw", "Wire", "Cable", "Steel Beam", "Steel Pipe",
                "Rotor", "Stator", "Motor", "Circuit Board", "Copper Plate", "Plastic",
                "Smart Plate", "Heavy Frame", "Computer"
            };

            Dictionary<string, ItemDefinition> itemMap = new Dictionary<string, ItemDefinition>();

            foreach (var itemName in items)
            {
                string id = itemName.Replace(" ", "");
                ItemDefinition itemDef = CreateItem(id, itemName);
                itemMap[itemName] = itemDef;
            }

            // Create Recipes
            CreateRecipe("Smelt Iron", 2f, new[] { (itemMap["Iron Ore"], 1) }, new[] { (itemMap["Iron Ingot"], 1) });
            CreateRecipe("Smelt Copper", 2f, new[] { (itemMap["Copper Ore"], 1) }, new[] { (itemMap["Copper Ingot"], 1) });
            CreateRecipe("Smelt Steel", 4f, new[] { (itemMap["Iron Ingot"], 1), (itemMap["Coal"], 1) }, new[] { (itemMap["Steel Ingot"], 1) });
            CreateRecipe("Make Quartz Crystal", 3f, new[] { (itemMap["Quartz"], 1) }, new[] { (itemMap["Quartz Crystal"], 1) });
            CreateRecipe("Make Concrete", 2f, new[] { (itemMap["Limestone"], 1) }, new[] { (itemMap["Concrete"], 1) });

            CreateRecipe("Make Iron Plate", 2f, new[] { (itemMap["Iron Ingot"], 1) }, new[] { (itemMap["Iron Plate"], 1) });
            CreateRecipe("Make Iron Rod", 2f, new[] { (itemMap["Iron Ingot"], 1) }, new[] { (itemMap["Iron Rod"], 1) });
            CreateRecipe("Make Screw", 1f, new[] { (itemMap["Iron Rod"], 1) }, new[] { (itemMap["Screw"], 1) });
            CreateRecipe("Make Wire", 1f, new[] { (itemMap["Copper Ingot"], 1) }, new[] { (itemMap["Wire"], 1) });
            CreateRecipe("Make Cable", 2f, new[] { (itemMap["Wire"], 1) }, new[] { (itemMap["Cable"], 1) });
            CreateRecipe("Make Steel Beam", 4f, new[] { (itemMap["Steel Ingot"], 1) }, new[] { (itemMap["Steel Beam"], 1) });
            CreateRecipe("Make Steel Pipe", 3f, new[] { (itemMap["Steel Ingot"], 1) }, new[] { (itemMap["Steel Pipe"], 1) });
            CreateRecipe("Make Copper Plate", 2f, new[] { (itemMap["Copper Ingot"], 1) }, new[] { (itemMap["Copper Plate"], 1) });
            CreateRecipe("Make Plastic", 3f, new[] { (itemMap["Coal"], 1) }, new[] { (itemMap["Plastic"], 1) });

            CreateRecipe("Make Rotor", 5f, new[] { (itemMap["Iron Rod"], 1), (itemMap["Screw"], 1) }, new[] { (itemMap["Rotor"], 1) });
            CreateRecipe("Make Stator", 6f, new[] { (itemMap["Wire"], 1), (itemMap["Steel Pipe"], 1) }, new[] { (itemMap["Stator"], 1) });
            CreateRecipe("Make Motor", 8f, new[] { (itemMap["Rotor"], 1), (itemMap["Stator"], 1) }, new[] { (itemMap["Motor"], 1) });
            CreateRecipe("Make Circuit Board", 6f, new[] { (itemMap["Copper Plate"], 1), (itemMap["Plastic"], 1) }, new[] { (itemMap["Circuit Board"], 1) });

            CreateRecipe("Make Smart Plate", 6f, new[] { (itemMap["Iron Plate"], 1), (itemMap["Screw"], 1) }, new[] { (itemMap["Smart Plate"], 1) });
            CreateRecipe("Make Heavy Frame", 10f, new[] { (itemMap["Steel Beam"], 1), (itemMap["Concrete"], 1) }, new[] { (itemMap["Heavy Frame"], 1) });
            CreateRecipe("Make Computer", 12f, new[] { (itemMap["Circuit Board"], 1), (itemMap["Cable"], 1) }, new[] { (itemMap["Computer"], 1) });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated all default Items and Recipes!");
        }

        private static void CreateFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Items"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Items");
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Recipes"))
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Recipes");
        }

        private static ItemDefinition CreateItem(string id, string name)
        {
            string path = $"Assets/ScriptableObjects/Items/{id}.asset";
            ItemDefinition existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (existing != null) return existing;

            ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.id = id;
            item.itemName = name;
            // leave icon null for now
            AssetDatabase.CreateAsset(item, path);
            return item;
        }

        private static Recipe CreateRecipe(string recipeName, float time, (ItemDefinition item, int count)[] inputs, (ItemDefinition item, int count)[] outputs)
        {
            string path = $"Assets/ScriptableObjects/Recipes/{recipeName.Replace(" ", "")}.asset";
            Recipe existing = AssetDatabase.LoadAssetAtPath<Recipe>(path);
            if (existing != null) return existing;

            Recipe recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.processingTime = time;

            recipe.inputs = new List<ItemStack>();
            foreach (var input in inputs)
                recipe.inputs.Add(new ItemStack(input.item, input.count));

            recipe.outputs = new List<ItemStack>();
            foreach (var output in outputs)
                recipe.outputs.Add(new ItemStack(output.item, output.count));

            AssetDatabase.CreateAsset(recipe, path);
            return recipe;
        }
    }
}
#endif
