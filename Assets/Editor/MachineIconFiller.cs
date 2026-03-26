using UnityEngine;
using UnityEditor;
using Factory.Core;
using System.Linq;

public static class MachineIconFiller
{
    [MenuItem("Tools/Fill Machine Icons")]
    public static void FillIcons()
    {
        string fillerPath = "Assets/SlimUI/Modern Menu 1/Graphics/Icons/Gear 128px.png";
        Sprite fillerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fillerPath);
        
        if (fillerSprite == null)
        {
            // Try to load as Texture2D and get the sprite
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fillerPath);
            if (texture != null)
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(fillerPath);
                fillerSprite = assets.OfType<Sprite>().FirstOrDefault();
            }
        }

        if (fillerSprite == null)
        {
            Debug.LogError("Could not find filler sprite at " + fillerPath);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:MachineDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MachineDefinition def = AssetDatabase.LoadAssetAtPath<MachineDefinition>(path);
            if (def != null && def.machineIcon == null)
            {
                def.machineIcon = fillerSprite;
                EditorUtility.SetDirty(def);
                Debug.Log("Assigned icon to " + def.name);
            }
        }
        AssetDatabase.SaveAssets();
    }
}
