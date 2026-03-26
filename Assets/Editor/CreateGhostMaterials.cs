using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates the two ghost placement materials used by BuildManager at edit time.
/// Run via: Factory > Create Ghost Materials
/// </summary>
public class CreateGhostMaterials
{
    [MenuItem("Factory/Create Ghost Materials")]
    public static void Create()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        CreateMaterial("GhostValid",   new Color(0.2f, 1f,   0.2f, 0.35f), true,  "Assets/Materials/GhostValid.mat");
        CreateMaterial("GhostInvalid", new Color(1f,   0.1f, 0.1f, 0.45f), true, "Assets/Materials/GhostInvalid.mat");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Ghost materials created in Assets/Materials/");
    }

    private static void CreateMaterial(string name, Color color, bool transparent, string path)
    {
        // Use Built-in RP Standard shader with transparent mode, or URP Lit if available
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = name;

        if (transparent)
        {
            // Standard transparent blend  
            mat.SetFloat("_Surface",  1); // 0=Opaque, 1=Transparent (URP)
            mat.SetFloat("_Blend",    0); // Alpha blend
            mat.SetFloat("_Mode",     3); // Transparent (Built-in)
            mat.SetInt("_SrcBlend",   (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend",   (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite",     0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        mat.color = color;
        mat.SetColor("_BaseColor", color);   // URP
        mat.SetColor("_Color",     color);   // Built-in

        AssetDatabase.CreateAsset(mat, path);
    }
}
