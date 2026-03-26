using UnityEngine;
using UnityEditor;
using Factory.Contracts;
using Factory.Core;
using System.Collections.Generic;

namespace Factory.Editor
{
    public static class AssetGeneratorExt
    {
        [MenuItem("Factory/Debug/Generate Base Contracts")]
        public static void GenerateBaseContracts()
        {
            string path = "Assets/Resources/Factory/Contracts";
            if (!AssetDatabase.IsValidFolder(path))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.dataPath, "Resources/Factory/Contracts"));
                AssetDatabase.Refresh();
            }

            // --- Tier 1: Raw Materials ---
            ItemDefinition ironOre = GetItem("Iron Ore");
            ItemDefinition copperOre = GetItem("Copper Ore");
            ItemDefinition stone = GetItem("Stone");
            ItemDefinition limestone = GetItem("Limestone");

            CreateContract("Renault-o-matic", "We need raw iron for our chassis production. Vite!", 150f, 500, 1, new (ItemDefinition, int)[] { (ironOre, 25) });
            CreateContract("AirBus-tique", "Lightweight stone aggregates needed for our eco-hangars.", 240f, 800, 1, new (ItemDefinition, int)[] { (stone, 40) });
            CreateContract("SNCF-utur", "Electrification projects require copper. All stations go!", 180f, 650, 1, new (ItemDefinition, int)[] { (copperOre, 30) });
            CreateContract("LVMH-tech", "Limestone for our new luxury boutique's facade.", 200f, 700, 1, new (ItemDefinition, int)[] { (limestone, 50) });

            // --- Tier 2: Basic Processed ---
            ItemDefinition ironIngot = GetItem("Iron Ingot");
            ItemDefinition copperIngot = GetItem("Copper Ingot");
            ItemDefinition concrete = GetItem("Concrete");

            CreateContract("Peugeot-bot", "Standardized ingots for engine blocks.", 300f, 1200, 2, new (ItemDefinition, int)[] { (ironIngot, 20) });
            CreateContract("Bouygues-confort", "Foundation work requires high-grade concrete.", 350f, 1500, 2, new (ItemDefinition, int)[] { (concrete, 30) });

            // --- Tier 3: Intermediate Components ---
            ItemDefinition ironPlate = GetItem("Iron Plate");
            ItemDefinition screw = GetItem("Screw");
            ItemDefinition wire = GetItem("Wire");
            ItemDefinition cable = GetItem("Cable");

            CreateContract("Dassault-speed", "Plates and screws for the new Rafale prototype.", 400f, 2500, 3, new (ItemDefinition, int)[] { (ironPlate, 40), (screw, 200) });
            CreateContract("Orange-link", "Fiber projects are delayed. We need standard copper wire and cables.", 380f, 2200, 3, new (ItemDefinition, int)[] { (wire, 150), (cable, 50) });

            // --- Tier 4: Advanced Industrial ---
            ItemDefinition rotor = GetItem("Rotor");
            ItemDefinition motor = GetItem("Motor");
            ItemDefinition computer = GetItem("Computer");

            CreateContract("Tesla-france", "Massive turbine project. Rotors and Motors needed NOW.", 600f, 5000, 4, new (ItemDefinition, int)[] { (rotor, 15), (motor, 5) });
            CreateContract("Ariane-space", "Guidance systems requiring advanced computing power.", 900f, 12000, 5, new (ItemDefinition, int)[] { (computer, 10) });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Tiered Contracts Generated! Remember to run 'Factory > Setup Scene' to populate the manager.");
        }

        private static ItemDefinition GetItem(string name)
        {
            // Try both naming conventions
            string[] guids = AssetDatabase.FindAssets($"{name.Replace(" ", "")} t:ItemDefinition");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets($"{name.Replace(" ", "_")} t:ItemDefinition");
            
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(guids[0]));
            
            Debug.LogWarning($"Could not find item: {name}");
            return null;
        }

        private static void CreateContract(string company, string desc, float time, int reward, int difficulty, (ItemDefinition item, int amount)[] reqs)
        {
            ContractDefinition contract = ScriptableObject.CreateInstance<ContractDefinition>();
            contract.companyName = company;
            contract.description = desc;
            contract.timeLimit = time;
            contract.rewardMoney = reward;
            contract.difficultyRating = difficulty;
            contract.requiredItems = new List<ItemStack>();

            foreach (var r in reqs)
            {
                if (r.item != null)
                {
                    contract.requiredItems.Add(new ItemStack(r.item, r.amount));
                }
            }

            string assetPath = $"Assets/Resources/Factory/Contracts/{company.Replace("-", "").Replace(".", "").Replace(" ", "")}Contract.asset";
            AssetDatabase.CreateAsset(contract, assetPath);
            Debug.Log($"Created Contract: {assetPath}");
        }
    }
}
