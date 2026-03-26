using UnityEngine;
using UnityEditor;
using Factory.Core;
using Factory.Factory;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Creates prefab GameObjects for all 9 machine types and their MachineDefinition ScriptableObjects.
/// Run via: Factory > Create Machine Prefabs
/// </summary>
public class CreateMachinePrefabs
{
    // (name, type, size, color, cost, hasScript)
    private static readonly (string name, MachineType type, Vector2Int size, Color color, int cost)[] Machines =
    {
        ("Conveyor",           MachineType.Conveyor,     new Vector2Int(1,1), new Color(0.90f, 0.75f, 0.30f), 10),
        ("Splitter",           MachineType.Splitter,     new Vector2Int(1,1), new Color(0.30f, 0.80f, 0.90f), 80),
        ("Merger",             MachineType.Merger,       new Vector2Int(1,1), new Color(0.80f, 0.30f, 0.90f), 80),
        ("Constructor",        MachineType.Constructor,  new Vector2Int(2,2), new Color(0.35f, 0.70f, 0.35f), 200),
        ("Assembler",          MachineType.Assembler,    new Vector2Int(3,3), new Color(0.25f, 0.50f, 0.80f), 500),
        ("Smelter",            MachineType.Smelter,      new Vector2Int(2,2), new Color(0.40f, 0.40f, 0.45f), 250),
        ("Storage Container",  MachineType.Storage,      new Vector2Int(2,2), new Color(0.80f, 0.60f, 0.25f), 150),
        ("Source",             MachineType.Source,       new Vector2Int(2,2), new Color(0.50f, 0.85f, 0.55f), 300),
        ("Delivery",           MachineType.Delivery,     new Vector2Int(2,2), new Color(0.85f, 0.40f, 0.40f), 300),
    };

    [MenuItem("Factory/Create Machine Prefabs")]
    public static void CreateAll()
    {
        EnsureFolder("Assets/Resources/Factory");
        EnsureFolder("Assets/Resources/Factory/MachineDefinitions");
        EnsureFolder("Assets/Prefabs/Machines");

        foreach (var m in Machines)
            CreateMachinePrefab(m.name, m.type, m.size, m.color, m.cost);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Factory] Created 9 machine prefabs and MachineDefinitions.");
        EditorUtility.DisplayDialog("Done!",
            "Created 9 machine prefabs in Assets/Prefabs/Machines/\nand MachineDefinitions in Assets/Resources/Factory/MachineDefinitions/",
            "OK");
    }

    private static void CreateMachinePrefab(string machineName, MachineType type,
        Vector2Int size, Color color, int cost)
    {
        string prefabPath = $"Assets/Prefabs/Machines/{machineName}.prefab";
        string defPath    = $"Assets/Resources/Factory/MachineDefinitions/{machineName}.asset";

        // ── Build the root GameObject ─────────────────────────────
        GameObject root = new GameObject(machineName);

        // ── Body child (scaled visual) ───────────────────────────
        GameObject body = new GameObject("Body");
        body.transform.SetParent(root.transform);
        body.transform.localScale = new Vector3(size.x, 0.5f, size.y);
        body.transform.localPosition = new Vector3(0, 0.25f, 0);

        MeshFilter mf = body.AddComponent<MeshFilter>();
        mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        MeshRenderer mr = body.AddComponent<MeshRenderer>();
        
        // Assign a distinct material - Force URP Lit or Standard
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = $"{machineName}_Mat";
        mat.color = color;
        if (shader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.5f);
        }
        
        // Save material as a persistent asset so it doesn't disappear
        AssetDatabase.CreateAsset(mat, $"Assets/Prefabs/Machines/{machineName}_Mat.mat");
        mr.sharedMaterial = mat;

        // Top directional indicator (child)
        GameObject indicator = new GameObject("Indicator");
        indicator.transform.SetParent(root.transform);
        indicator.transform.localScale = new Vector3(0.15f, 0.05f, size.y * 0.5f);
        indicator.transform.localPosition = new Vector3(0, 0.55f, size.y * 0.15f);
        
        MeshFilter indMf = indicator.AddComponent<MeshFilter>();
        indMf.sharedMesh = mf.sharedMesh; 
        
        MeshRenderer indMr = indicator.AddComponent<MeshRenderer>();
        Material indMat = new Material(shader);
        indMat.name = $"{machineName}_IndicatorMat";
        indMat.color = Color.white;
        if (shader.name.Contains("Universal Render Pipeline")) indMat.SetColor("_BaseColor", Color.white);
        
        AssetDatabase.CreateAsset(indMat, $"Assets/Prefabs/Machines/{machineName}_IndMat.mat");
        indMr.sharedMaterial = indMat;

        // Collider on root for raycasting (InteractableLayer)
        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size   = new Vector3(size.x, 0.6f, size.y);
        col.center = new Vector3(0, 0.3f, 0);
        root.layer = LayerMask.NameToLayer("Interactable");

        // Add the correct MonoBehaviour script based on type
        switch (type)
        {
            case MachineType.Conveyor:    root.AddComponent<ConveyorSegment>(); break;
            case MachineType.Splitter:    root.AddComponent<Splitter>(); break;
            case MachineType.Merger:      root.AddComponent<Merger>(); break;
            case MachineType.Constructor: root.AddComponent<Constructor>(); break;
            case MachineType.Assembler:   root.AddComponent<Assembler>(); break;
            case MachineType.Smelter:     root.AddComponent<MachineProcessor>(); break;
            case MachineType.Storage:     root.AddComponent<StorageContainer>(); break;
            case MachineType.Source:      root.AddComponent<SourceMachine>(); break;
            case MachineType.Delivery:    root.AddComponent<DeliveryMachine>(); break;
        }

        FactoryBuilding building = root.GetComponent<FactoryBuilding>();
        if (building != null) building.size = size;

        // Add 3D space UI indicator for facing direction
        root.AddComponent<DirectionIndicator>();

        // ── Save as prefab ────────────────────────────────────────
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        // ── Create MachineDefinition SO ───────────────────────────
        MachineDefinition existingDef = AssetDatabase.LoadAssetAtPath<MachineDefinition>(defPath);
        if (existingDef == null)
        {
            MachineDefinition def = ScriptableObject.CreateInstance<MachineDefinition>();
            def.machineName = machineName.Replace("_", " ");
            def.type = type;
            def.size = size;
            def.cost = cost;
            def.prefab = prefabAsset;
            AssetDatabase.CreateAsset(def, defPath);
        }
        else
        {
            existingDef.prefab = prefabAsset;
            EditorUtility.SetDirty(existingDef);
        }
    }

    private static void EnsureFolder(string path)
    {
        string[] folders = path.Split('/');
        string currentPath = folders[0];
        for (int i = 1; i < folders.Length; i++)
        {
            string folderPath = currentPath + "/" + folders[i];
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }
            currentPath = folderPath;
        }
    }
}
