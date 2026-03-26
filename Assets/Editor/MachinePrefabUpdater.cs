using UnityEngine;
using UnityEditor;
using Factory.Core;
using System.IO;
using System.Collections.Generic;

namespace Factory.Editor
{
    public class MachinePrefabUpdater : EditorWindow
    {
        [MenuItem("Factory/Update Machine Models")]
        public static void UpdateMachineModels()
        {
            UpdateAllMachinePrefabs();
        }

        [MenuItem("Factory/Update Item Models")]
        public static void UpdateItemModels()
        {
            UpdateAllItemDefinitions();
        }

        private static void UpdateAllMachinePrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:MachineDefinition");
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MachineDefinition def = AssetDatabase.LoadAssetAtPath<MachineDefinition>(path);

                if (def == null || def.prefab == null) continue;

                if (UpdatePrefabForMachine(def))
                {
                    updatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Machine Update", $"Successfully updated {updatedCount} machine prefabs with Blender assets.", "OK");
        }

        private static void UpdateAllItemDefinitions()
        {
            string[] guids = AssetDatabase.FindAssets("t:ItemDefinition");
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemDefinition def = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);

                if (def == null) continue;

                if (UpdateItemDefinition(def))
                {
                    updatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Item Update", $"Successfully updated {updatedCount} item definitions with Blender assets.", "OK");
        }

        private static bool UpdatePrefabForMachine(MachineDefinition def)
        {
            string machineName = def.machineName;
            // Handle special naming if necessary (e.g. Smelter uses Test_Smelter)
            string searchName = machineName == "Smelter" ? "Test_Smelter" : machineName;
            
            // Search for FBX in Blender Assets/MachineName folder
            string blenderFolderPath = $"Assets/Blender Assets/{machineName}";
            if (!AssetDatabase.IsValidFolder(blenderFolderPath))
            {
                // Fallback to searching the whole Blender Assets folder
                blenderFolderPath = "Assets/Blender Assets";
            }

            string[] assetGuids = AssetDatabase.FindAssets($"{searchName} t:Model", new[] { blenderFolderPath });
            if (assetGuids.Length == 0) return false;

            string modelPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            Mesh modelMesh = ExtractMeshFromModel(modelPrefab);

            if (modelMesh == null)
            {
                Debug.LogWarning($"[MachinePrefabUpdater] Could not find mesh in model: {modelPath}");
                return false;
            }

            // Load the prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(def.prefab);
            GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);

            // Find the "Body" child
            Transform body = prefabContents.transform.Find("Body");
            if (body == null)
            {
                // If no Body, maybe it's the root itself or we need to find MeshFilter
                MeshFilter mf = prefabContents.GetComponentInChildren<MeshFilter>();
                if (mf != null) body = mf.transform;
            }

            if (body != null)
            {
                MeshFilter mf = body.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    mf.sharedMesh = modelMesh;
                    
                    // Also try to assign materials if they exist in the model's sub-assets
                    MeshRenderer mr = body.GetComponent<MeshRenderer>();
                    MeshRenderer modelRenderer = modelPrefab.GetComponentInChildren<MeshRenderer>();
                    if (mr != null && modelRenderer != null)
                    {
                        mr.sharedMaterials = modelRenderer.sharedMaterials;
                    }

                    Debug.Log($"[MachinePrefabUpdater] Updated model for {machineName} using {modelPath}");
                    PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
                    PrefabUtility.UnloadPrefabContents(prefabContents);
                    return true;
                }
            }

            PrefabUtility.UnloadPrefabContents(prefabContents);
            return false;
        }

        private static bool UpdateItemDefinition(ItemDefinition def)
        {
            string itemName = def.itemName;
            
            // Search for FBX in Blender Assets/ItemName folder
            string blenderFolderPath = $"Assets/Blender Assets/{itemName}";
            if (!AssetDatabase.IsValidFolder(blenderFolderPath))
            {
                blenderFolderPath = "Assets/Blender Assets";
            }

            string[] assetGuids = AssetDatabase.FindAssets($"{itemName} t:Model", new[] { blenderFolderPath });
            if (assetGuids.Length == 0) return false;

            string modelPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);

            if (modelPrefab != null)
            {
                if (def.modelPrefab != modelPrefab)
                {
                    def.modelPrefab = modelPrefab;
                    EditorUtility.SetDirty(def);
                    Debug.Log($"[MachinePrefabUpdater] Updated model for item {itemName} using {modelPath}");
                    return true;
                }
            }

            return false;
        }

        private static Mesh ExtractMeshFromModel(GameObject model)
        {
            MeshFilter mf = model.GetComponentInChildren<MeshFilter>();
            if (mf != null) return mf.sharedMesh;
            
            SkinnedMeshRenderer smr = model.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null) return smr.sharedMesh;

            return null;
        }
    }
}
