using UnityEngine;
using UnityEditor;
using Factory.Core;
using Factory.UI;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

namespace Factory.Editor
{
    public class AudioSetupTool : EditorWindow
    {
        [MenuItem("Factory/Setup Audio System")]
        public static void SetupAudio()
        {
            // 1. Create or Find SoundLibrary
            string libraryPath = "Assets/Resources/Factory/SoundLibrary.asset";
            if (!Directory.Exists("Assets/Resources/Factory")) Directory.CreateDirectory("Assets/Resources/Factory");

            SoundLibrary library = AssetDatabase.LoadAssetAtPath<SoundLibrary>(libraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<SoundLibrary>();
                AssetDatabase.CreateAsset(library, libraryPath);
            }

            // 2. Find Music
            string musicDir = "Assets/Audio/Music/Game Ambiance";
            string menuMusicDir = "Assets/Audio/Music/Menu";
            
            library.playlist = new List<AudioClip>();
            string[] musicFiles = Directory.GetFiles(musicDir, "*.mp3");
            foreach (string file in musicFiles)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(file);
                if (clip != null) library.playlist.Add(clip);
            }

            // Assign Menu Music
            string[] menuMusicFiles = Directory.GetFiles(menuMusicDir, "*.mp3");
            if (menuMusicFiles.Length > 0)
            {
                library.menuMusic = AssetDatabase.LoadAssetAtPath<AudioClip>(menuMusicFiles[0]);
            }

            // 3. Assign SFX (based on the names I picked)
            string sfxDir = "Assets/Audio/SFX/CasualGameSounds";
            library.clickSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-01.wav"));
            library.openUISound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-02.wav"));
            library.rotateSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-16.wav"));
            library.placeSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-05.wav"));
            library.deleteSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-11.wav"));
            library.errorSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-20.wav"));
            library.closeUISound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-21.wav"));
            
            // New Contract Sounds
            library.contractStartedSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-09.wav"));
            library.contractCompletedSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(sfxDir, "DM-CGS-49.wav"));

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();

            // 4. Setup AudioManager in Scene
            AudioManager existingManager = GameObject.FindAnyObjectByType<AudioManager>();
            if (existingManager == null)
            {
                GameObject go = new GameObject("AudioManager");
                AudioManager manager = go.AddComponent<AudioManager>();
                
                // Use reflection or a public field if needed to set the library
                // Since I made it [SerializeField] private, I'll need to use SerializedObject
                SerializedObject so = new SerializedObject(manager);
                so.FindProperty("library").objectReferenceValue = library;
                so.ApplyModifiedProperties();
                
                Debug.Log("Created AudioManager in scene and assigned SoundLibrary.");
            }
            else
            {
                SerializedObject so = new SerializedObject(existingManager);
                so.FindProperty("library").objectReferenceValue = library;
                so.ApplyModifiedProperties();
                Debug.Log("Updated existing AudioManager with SoundLibrary.");
            }

            // 5. Auto-assign UIButtonSound to all buttons in scene
            Button[] allButtons = GameObject.FindObjectsByType<Button>(FindObjectsSortMode.None);
            int addedCount = 0;
            foreach (var btn in allButtons)
            {
                if (btn.GetComponent<UIButtonSound>() == null)
                {
                    btn.gameObject.AddComponent<UIButtonSound>();
                    addedCount++;
                }
            }
            Debug.Log($"Added UIButtonSound to {addedCount} buttons.");

            Debug.Log("Audio System Setup Complete!");
        }
    }
}
