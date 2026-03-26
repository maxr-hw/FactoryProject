using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Factory.Core;
using Factory.Economy;
using Factory.Contracts;
using Factory.UI;
using Factory.Factory;
using System.Collections.Generic;
using TMPro;

public class SetupScript
{
    [MenuItem("Factory/Setup Scene")]
    public static void Setup()
    {
        GameObject manager = GameObject.Find("Manager");
        if (manager == null) manager = GameObject.Find("Managers");
        if (manager == null) manager = new GameObject("Manager");

        EnsureComponent<GridManager>(manager);
        EnsureComponent<BuildManager>(manager);
        EnsureComponent<SelectionManager>(manager);
        EnsureComponent<EconomyManager>(manager);
        EnsureComponent<ContractManager>(manager);
        EnsureComponent<ContractPopupUI>(manager);
        EnsureComponent<FactoryHUD>(manager);
        EnsureComponent<GameBootstrap>(manager);

        GridManager grid = manager.GetComponent<GridManager>();
        BuildManager build = manager.GetComponent<BuildManager>();
        EconomyManager economy = manager.GetComponent<EconomyManager>();
        
        // Force starting money update in scene
        SerializedObject economySO = new SerializedObject(economy);
        economySO.FindProperty("startingMoney").intValue = 10000;
        economySO.ApplyModifiedProperties();

        ContractManager contract = manager.GetComponent<ContractManager>();
        GameBootstrap bootstrap = manager.GetComponent<GameBootstrap>();

        // Setup Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        }

        FactoryHUD hud = canvasObj.GetComponentInChildren<FactoryHUD>();
        if (hud == null)
        {
            GameObject hudObj = new GameObject("Factory HUD", typeof(RectTransform), typeof(FactoryHUD));
            hudObj.transform.SetParent(canvasObj.transform, false);
            hud = hudObj.GetComponent<FactoryHUD>();
            
            RectTransform hudRT = hud.GetComponent<RectTransform>();
            hudRT.anchorMin = new Vector2(0, 1);
            hudRT.anchorMax = new Vector2(0, 1);
            hudRT.pivot = new Vector2(0, 1);
            hudRT.anchoredPosition = new Vector2(20, -20);
            hudRT.sizeDelta = new Vector2(200, 50);
        }

        SerializedObject hudSO = new SerializedObject(hud);
        SerializedProperty moneyTextProp = hudSO.FindProperty("moneyText");
        if (moneyTextProp.objectReferenceValue == null)
        {
            GameObject moneyObj = new GameObject("MoneyText", typeof(RectTransform), typeof(TextMeshProUGUI));
            moneyObj.transform.SetParent(hud.transform, false);
            TextMeshProUGUI moneyTMP = moneyObj.GetComponent<TextMeshProUGUI>();
            moneyTMP.color = Color.black;
            moneyTMP.fontSize = 24;
            moneyTMP.text = "$0";
            moneyTextProp.objectReferenceValue = moneyTMP;
        }
        hudSO.ApplyModifiedProperties();

        ShopManager shop = canvasObj.GetComponentInChildren<ShopManager>(true);
        if (shop == null)
        {
            GameObject shopObj = new GameObject("Shop Manager", typeof(RectTransform), typeof(ShopManager));
            shopObj.transform.SetParent(canvasObj.transform, false);
            shop = shopObj.GetComponent<ShopManager>();
            
            GameObject panel = new GameObject("Shop Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(shopObj.transform, false);
            RectTransform panelRT = panel.GetComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(400, 500);
            panel.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            
            GameObject container = new GameObject("ShopContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            container.transform.SetParent(panel.transform, false);
            RectTransform contRT = container.GetComponent<RectTransform>();
            contRT.anchorMin = Vector2.zero;
            contRT.anchorMax = Vector2.one;
            contRT.sizeDelta = Vector2.zero;
            
            GameObject close = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button));
            close.transform.SetParent(panel.transform, false);
            RectTransform closeRT = close.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 1);
            closeRT.anchorMax = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-15, -15);
            closeRT.sizeDelta = new Vector2(30, 30);
            close.GetComponent<Image>().color = Color.red;

            panel.SetActive(false); // Start closed
        }

        // Setup Shop references
        // We use SerializedObject carefully here to ensure properties are saved
        SerializedObject shopSO = new SerializedObject(shop);
        SerializedProperty buttonPrefabProp = shopSO.FindProperty("buttonPrefab");
        if (buttonPrefabProp.objectReferenceValue == null) {
            string[] prefabs = AssetDatabase.FindAssets("UIButton t:Prefab");
            if (prefabs.Length > 0) {
                buttonPrefabProp.objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabs[0]));
            } else {
                GameObject tempButton = new GameObject("UIButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(ShopButton));
                RectTransform buttonRT = tempButton.GetComponent<RectTransform>();
                buttonRT.sizeDelta = new Vector2(160, 60);

                // Add VerticalLayoutGroup for stacking name and price
                VerticalLayoutGroup layout = tempButton.AddComponent<VerticalLayoutGroup>();
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = false;
                layout.childForceExpandWidth = true;
                layout.padding = new RectOffset(5, 5, 5, 5);
                layout.spacing = 2;

                GameObject nameObj = new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                nameObj.transform.SetParent(tempButton.transform, false);
                TextMeshProUGUI nameTMP = nameObj.GetComponent<TextMeshProUGUI>();
                nameTMP.color = Color.black;
                nameTMP.fontSize = 18;
                nameTMP.alignment = TextAlignmentOptions.Center;
                nameTMP.text = "Machine Name";
                
                GameObject priceObj = new GameObject("PriceText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                priceObj.transform.SetParent(tempButton.transform, false);
                TextMeshProUGUI priceTMP = priceObj.GetComponent<TextMeshProUGUI>();
                priceTMP.color = new Color(0.2f, 0.2f, 0.2f); // Dark gray for price
                priceTMP.fontSize = 14;
                priceTMP.alignment = TextAlignmentOptions.Center;
                priceTMP.text = "$0.00";
                
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(tempButton, "Assets/Prefabs/UIButton.prefab");
                buttonPrefabProp.objectReferenceValue = prefab;
                GameObject.DestroyImmediate(tempButton);
            }
        }
        if (shopSO.FindProperty("shopPanel").objectReferenceValue == null)
            shopSO.FindProperty("shopPanel").objectReferenceValue = shop.transform.Find("Shop Panel").gameObject;
        
        if (shopSO.FindProperty("container").objectReferenceValue == null)
            shopSO.FindProperty("container").objectReferenceValue = shop.transform.Find("Shop Panel/ShopContainer");
            
        if (shopSO.FindProperty("closeButton").objectReferenceValue == null)
            shopSO.FindProperty("closeButton").objectReferenceValue = shop.transform.Find("Shop Panel/Close").GetComponent<Button>();
        
        // Populate ShopManager + Fix missing item/recipe references
        string[] machineGuids = AssetDatabase.FindAssets("t:MachineDefinition");
        SerializedProperty listProp = shopSO.FindProperty("availableMachines");
        listProp.ClearArray();
        
        ItemDefinition ironOre = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Resources/Factory/Items/Iron_Ore.asset");
        Recipe ironSmelt = AssetDatabase.LoadAssetAtPath<Recipe>("Assets/Resources/Factory/Recipes/Smelt_IronIngot.asset");

        for (int i = 0; i < machineGuids.Length; i++) {
            MachineDefinition def = AssetDatabase.LoadAssetAtPath<MachineDefinition>(AssetDatabase.GUIDToAssetPath(machineGuids[i]));
            listProp.InsertArrayElementAtIndex(i);
            listProp.GetArrayElementAtIndex(i).objectReferenceValue = def;

            // Fix missing data based on machine name/type
            if (def != null)
            {
                if (def.type == MachineType.Source && def.prefab != null)
                {
                    // Update prefab default if possible
                    var src = def.prefab.GetComponent<SourceMachine>();
                    if (src != null && src.itemToSpawn == null)
                    {
                        src.itemToSpawn = ironOre;
                        EditorUtility.SetDirty(def.prefab);
                    }
                }
                if ((def.type == MachineType.Constructor || def.type == MachineType.Smelter) && def.defaultRecipe == null)
                {
                    def.defaultRecipe = ironSmelt;
                    EditorUtility.SetDirty(def);
                }
            }
        }
        shopSO.ApplyModifiedProperties();

        // Populate BuildManager catalog and materials
        SerializedObject buildSO = new SerializedObject(build);
        SerializedProperty catalogProp = buildSO.FindProperty("machineCatalog");
        catalogProp.ClearArray();
        for (int i = 0; i < machineGuids.Length; i++) {
            catalogProp.InsertArrayElementAtIndex(i);
            catalogProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<MachineDefinition>(AssetDatabase.GUIDToAssetPath(machineGuids[i]));
        }

        // Setup materials
        if (buildSO.FindProperty("ghostValidMaterial").objectReferenceValue == null)
            buildSO.FindProperty("ghostValidMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GhostValid.mat");
        if (buildSO.FindProperty("ghostInvalidMaterial").objectReferenceValue == null)
            buildSO.FindProperty("ghostInvalidMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GhostInvalid.mat");
        if (buildSO.FindProperty("deleteHighlightMaterial").objectReferenceValue == null)
            buildSO.FindProperty("deleteHighlightMaterial").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DeleteHighlight.mat");
        
        buildSO.ApplyModifiedProperties();

        // Populate ContractManager pool
        SerializedObject contractSO = new SerializedObject(contract);
        SerializedProperty poolProp = contractSO.FindProperty("contractPool");
        poolProp.ClearArray();
        string[] contractGuids = AssetDatabase.FindAssets("t:ContractDefinition");
        if (contractGuids.Length == 0)
        {
            Debug.LogWarning("No ContractDefinitions found in project! Contracts will not appear. Use 'Factory > Debug > Generate Base Contracts' first.");
        }
        for (int i = 0; i < contractGuids.Length; i++) {
            poolProp.InsertArrayElementAtIndex(i);
            poolProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ContractDefinition>(AssetDatabase.GUIDToAssetPath(contractGuids[i]));
        }
        contractSO.ApplyModifiedProperties();

        EditorUtility.SetDirty(contract);
        EditorUtility.SetDirty(manager);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Factory Project Setup/Update Complete.");
    }

    private static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp == null) 
        {
            Debug.Log($"[SetupScript] Adding component {typeof(T).Name} to {obj.name}");
            comp = obj.AddComponent<T>();
        }
        return comp;
    }
}
