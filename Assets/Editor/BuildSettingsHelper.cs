using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public static class BuildSettingsHelper
{
    [MenuItem("Tools/Add MainMenu to Build Settings")]
    public static void AddMainMenu()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity/MainMenu.unity";
        var scenes = EditorBuildSettings.scenes.ToList();
        
        if (!scenes.Any(s => s.path == scenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            UnityEngine.Debug.Log("Added MainMenu to Build Settings.");
        }
        else
        {
            UnityEngine.Debug.Log("MainMenu already in Build Settings.");
        }
    }
}
