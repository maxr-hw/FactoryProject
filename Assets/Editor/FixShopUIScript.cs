using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Factory.UI;

public class FixShopUIScript
{
    [MenuItem("Factory/Fix Shop UI")]
    public static void FixShopUI()
    {
        // 1. Find the Shop Manager
        ShopManager shopManager = Object.FindObjectOfType<ShopManager>(true);
        if (shopManager == null)
        {
            Debug.LogError("Could not find ShopManager in the scene.");
            return;
        }

        // 2. Setup Shop Panel
        Transform shopPanel = shopManager.transform.Find("Shop Panel");
        if (shopPanel == null)
        {
            GameObject panelObj = new GameObject("Shop Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObj.transform.SetParent(shopManager.transform, false);
            
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(400, 600);
            
            panelObj.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.9f); // Dark background
            shopPanel = panelObj.transform;
            
            // Add close button if new
            GameObject closeBtn = new GameObject("Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeBtn.transform.SetParent(shopPanel, false);
            RectTransform cRt = closeBtn.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(1, 1);
            cRt.anchorMax = new Vector2(1, 1);
            cRt.pivot = new Vector2(1, 1);
            cRt.anchoredPosition = new Vector2(0, 0);
            cRt.sizeDelta = new Vector2(30, 30);
            closeBtn.GetComponent<Image>().color = Color.red;
        }
        
        // 3. Setup Shop Container inside Shop Panel
        Transform shopContainer = shopPanel.Find("ShopContainer");
        if (shopContainer == null)
        {
            GameObject containerObj = new GameObject("ShopContainer", typeof(RectTransform));
            containerObj.transform.SetParent(shopPanel, false);
            shopContainer = containerObj.transform;
        }

        RectTransform srT = shopContainer.GetComponent<RectTransform>();
        srT.anchorMin = new Vector2(0, 0);
        srT.anchorMax = new Vector2(1, 1);
        srT.offsetMin = new Vector2(20, 20); // Margins
        srT.offsetMax = new Vector2(-20, -50); // Top margin for title/close

        // Add Vertical Layout Group to container
        VerticalLayoutGroup vlg = shopContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = shopContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 10f;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        // Content Size Fitter (Optional, but good if it's inside a scroll rect - but here it's constrained by panel)
        // If it isn't inside a Scroll Rect, leaving it out so layout matches Panel height

        // 4. Update the Button Prefab
        SerializedObject shopSO = new SerializedObject(shopManager);
        SerializedProperty buttonPrefabProp = shopSO.FindProperty("buttonPrefab");
        SerializedProperty containerProp = shopSO.FindProperty("container");

        containerProp.objectReferenceValue = shopContainer;
        shopSO.ApplyModifiedProperties();

        GameObject prefab = buttonPrefabProp.objectReferenceValue as GameObject;
        if (prefab != null)
        {
            /// Need to get prefab path, instantiate, modify, overwrite
            string path = AssetDatabase.GetAssetPath(prefab);
            if (!string.IsNullOrEmpty(path))
            {
                GameObject inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                
                // Add Horizontal Layout
                HorizontalLayoutGroup hlg = inst.GetComponent<HorizontalLayoutGroup>();
                if (hlg == null) hlg = inst.AddComponent<HorizontalLayoutGroup>();
                hlg.childControlHeight = true;
                hlg.childControlWidth = true;
                hlg.childForceExpandHeight = true;
                hlg.childForceExpandWidth = true;
                hlg.padding = new RectOffset(5, 5, 5, 5);
                hlg.spacing = 5;

                // Set Text Properties
                TextMeshProUGUI tmp = inst.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.color = Color.black;
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = 14;
                    tmp.fontSizeMax = 36;
                    tmp.alignment = TextAlignmentOptions.CenterGeoAligned; // Centered
                }

                // Make sure it has a ShopButton component
                if (inst.GetComponent<ShopButton>() == null)
                {
                    inst.AddComponent<ShopButton>();
                }
                
                // Enforce minimum height via Layout Element so VLG respects it
                LayoutElement le = inst.GetComponent<LayoutElement>();
                if (le == null) le = inst.AddComponent<LayoutElement>();
                le.minHeight = 50f;

                PrefabUtility.SaveAsPrefabAsset(inst, path);
                GameObject.DestroyImmediate(inst);
            }
        }

        EditorUtility.SetDirty(shopManager);
        Debug.Log("Shop UI Fixed: Layouts applied, button prefab fully updated with black text and horizontal layout.");
    }
}
