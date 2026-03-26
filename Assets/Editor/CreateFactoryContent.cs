using UnityEngine;
using UnityEditor;
using Factory.Core;
using System.Collections.Generic;

/// <summary>
/// Creates all ItemDefinition and Recipe ScriptableObject assets for the Factory game.
/// Run via: Factory > Create All Items and Recipes
/// </summary>
public class CreateFactoryContent
{
    [MenuItem("Factory/Create All Items and Recipes")]
    public static void CreateAll()
    {
        // ── Folder setup ──────────────────────────────────────────────
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Factory");
        EnsureFolder("Assets/Resources/Factory/Items");
        EnsureFolder("Assets/Resources/Factory/Recipes");

        // ── Item Definitions ──────────────────────────────────────────
        //  (id, itemName, H  S  V)
        var itemsData = new (string id, string name, float h, float s, float v)[]
        {
            // Raw Materials
            ("iron_ore",       "Iron Ore",       0.03f, 0.70f, 0.55f),
            ("copper_ore",     "Copper Ore",     0.07f, 0.80f, 0.70f),
            ("coal",           "Coal",           0.00f, 0.00f, 0.12f),
            ("quartz",         "Quartz",         0.55f, 0.15f, 0.90f),
            ("limestone",      "Limestone",      0.12f, 0.10f, 0.80f),

            // Intermediate
            ("iron_ingot",     "Iron Ingot",     0.58f, 0.25f, 0.75f),
            ("copper_ingot",   "Copper Ingot",   0.07f, 0.80f, 0.85f),
            ("steel_ingot",    "Steel Ingot",    0.60f, 0.10f, 0.55f),
            ("quartz_crystal", "Quartz Crystal", 0.50f, 0.40f, 0.95f),
            ("concrete",       "Concrete",       0.10f, 0.08f, 0.55f),

            // Components
            ("iron_plate",     "Iron Plate",     0.58f, 0.30f, 0.65f),
            ("iron_rod",       "Iron Rod",       0.57f, 0.20f, 0.60f),
            ("screw",          "Screw",          0.60f, 0.12f, 0.50f),
            ("wire",           "Wire",           0.08f, 0.75f, 0.80f),
            ("cable",          "Cable",          0.06f, 0.90f, 0.60f),
            ("steel_beam",     "Steel Beam",     0.62f, 0.15f, 0.50f),
            ("steel_pipe",     "Steel Pipe",     0.60f, 0.18f, 0.60f),

            // Advanced Components
            ("rotor",          "Rotor",          0.33f, 0.55f, 0.75f),
            ("stator",         "Stator",         0.40f, 0.60f, 0.70f),
            ("motor",          "Motor",          0.45f, 0.70f, 0.65f),
            ("circuit_board",  "Circuit Board",  0.35f, 0.80f, 0.60f),

            // Products
            ("smart_plate",    "Smart Plate",    0.58f, 0.50f, 0.85f),
            ("heavy_frame",    "Heavy Frame",    0.62f, 0.40f, 0.45f),
            ("computer",       "Computer",       0.30f, 0.30f, 0.95f),
        };

        var items = new Dictionary<string, ItemDefinition>();
        foreach (var data in itemsData)
            items[data.id] = CreateItem(data.id, data.name, data.h, data.s, data.v);

        AssetDatabase.SaveAssets();

        // ── Recipes ──────────────────────────────────────────────────
        // Helper: (item_ref, amount)
        // Smelting
        Rec("Smelt_IronIngot",    new[]{I(items,"iron_ore",1)},     new[]{I(items,"iron_ingot",1)},     2f);
        Rec("Smelt_CopperIngot",  new[]{I(items,"copper_ore",1)},   new[]{I(items,"copper_ingot",1)},   2f);
        Rec("Smelt_SteelIngot",   new[]{I(items,"iron_ingot",1), I(items,"coal",1)}, new[]{I(items,"steel_ingot",1)}, 4f);
        Rec("Smelt_QuartzCrystal",new[]{I(items,"quartz",1)},       new[]{I(items,"quartz_crystal",1)}, 3f);
        Rec("Smelt_Concrete",     new[]{I(items,"limestone",1)},    new[]{I(items,"concrete",1)},       2f);

        // Constructor (1 input → 1 output)
        Rec("Construct_IronPlate", new[]{I(items,"iron_ingot",1)},  new[]{I(items,"iron_plate",1)},     2f);
        Rec("Construct_IronRod",   new[]{I(items,"iron_ingot",1)},  new[]{I(items,"iron_rod",1)},       2f);
        Rec("Construct_Screw",     new[]{I(items,"iron_rod",1)},    new[]{I(items,"screw",4)},          1f);
        Rec("Construct_Wire",      new[]{I(items,"copper_ingot",1)},new[]{I(items,"wire",2)},           1f);
        Rec("Construct_Cable",     new[]{I(items,"wire",2)},        new[]{I(items,"cable",1)},          2f);
        Rec("Construct_SteelBeam", new[]{I(items,"steel_ingot",1)}, new[]{I(items,"steel_beam",1)},    4f);
        Rec("Construct_SteelPipe", new[]{I(items,"steel_ingot",1)}, new[]{I(items,"steel_pipe",1)},    3f);

        // Assembler (2 inputs → 1 output)
        Rec("Assemble_Rotor",     new[]{I(items,"iron_rod",5), I(items,"screw",25)},      new[]{I(items,"rotor",1)},         5f);
        Rec("Assemble_Stator",    new[]{I(items,"wire",8), I(items,"steel_pipe",3)},      new[]{I(items,"stator",1)},        6f);
        Rec("Assemble_Motor",     new[]{I(items,"rotor",2), I(items,"stator",2)},         new[]{I(items,"motor",1)},         8f);
        Rec("Assemble_CircuitBoard", new[]{I(items,"iron_plate",2), I(items,"wire",4)},  new[]{I(items,"circuit_board",1)}, 6f);

        // Advanced
        Rec("Adv_SmartPlate",  new[]{I(items,"iron_plate",3), I(items,"screw",6)},           new[]{I(items,"smart_plate",1)},  6f);
        Rec("Adv_HeavyFrame",  new[]{I(items,"steel_beam",4), I(items,"concrete",3)},        new[]{I(items,"heavy_frame",1)}, 10f);
        Rec("Adv_Computer",    new[]{I(items,"circuit_board",2), I(items,"cable",4)},        new[]{I(items,"computer",1)},    12f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Factory] Created {itemsData.Length} items and 19 recipes in Assets/Resources/Factory/");
        EditorUtility.DisplayDialog("Done!", 
            $"Created {itemsData.Length} ItemDefinitions and 19 Recipes in\nAssets/Resources/Factory/", "Great!");
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private static ItemDefinition CreateItem(string id, string itemName, float h, float s, float v)
    {
        string path = $"Assets/Resources/Factory/Items/{itemName.Replace(" ", "_")}.asset";
        ItemDefinition existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        if (existing != null) return existing;   // skip if already exists

        var item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.id = id;
        item.itemName = itemName;
        item.itemColor = Color.HSVToRGB(h, s, v);
        AssetDatabase.CreateAsset(item, path);
        return item;
    }

    private static ItemStack I(Dictionary<string, ItemDefinition> dict, string id, int amount)
        => new ItemStack(dict[id], amount);

    private static void Rec(string recipeName, ItemStack[] inputs, ItemStack[] outputs, float time)
    {
        string path = $"Assets/Resources/Factory/Recipes/{recipeName}.asset";
        if (AssetDatabase.LoadAssetAtPath<Recipe>(path) != null) return;  // skip if exists

        var r = ScriptableObject.CreateInstance<Recipe>();
        r.inputs  = new System.Collections.Generic.List<ItemStack>(inputs);
        r.outputs = new System.Collections.Generic.List<ItemStack>(outputs);
        r.processingTime = time;
        AssetDatabase.CreateAsset(r, path);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent  = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string newName = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, newName);
        }
    }
}
