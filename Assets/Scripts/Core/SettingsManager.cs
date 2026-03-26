using UnityEngine;
using System.Collections.Generic;

namespace Factory.Core
{
    [System.Serializable]
    public class GameSettings
    {
        // Audio
        public float masterVolume = 1.0f;
        public float musicVolume = 1.0f;
        public float sfxVolume = 1.0f;

        // Graphics
        public int qualityLevel = 2; // Medium
        public bool shadowsEnabled = true;

        // Display
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public int refreshRate = 60;
        public bool fullScreen = true;
        public bool vsync = true;
        public int resolutionIndex = -1; // Legacy or for dropdown tracking

        // Controls
        public float mouseSensitivity = 1.0f;
        public bool invertY = false;
    }

    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager _instance;
        public static SettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("SettingsManager");
                    _instance = go.AddComponent<SettingsManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public GameSettings settings = new GameSettings();

        private const string SETTINGS_KEY = "GameSettingsJSON";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(settings);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
            ApplySettings();
        }

        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                settings = JsonUtility.FromJson<GameSettings>(json);
            }
            ApplySettings();
        }

        public void ApplySettings()
        {
            // Apply Audio (this would likely involve an AudioMixer in a real project)
            AudioListener.volume = settings.masterVolume;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.UpdateVolumes();
            }

            // Apply Graphics
            QualitySettings.SetQualityLevel(settings.qualityLevel);
            
            // Apply Display
            if (settings.resolutionWidth > 0 && settings.resolutionHeight > 0)
            {
                Screen.SetResolution(settings.resolutionWidth, settings.resolutionHeight, settings.fullScreen, settings.refreshRate);
            }
            else if (settings.resolutionIndex != -1 && settings.resolutionIndex < Screen.resolutions.Length)
            {
                Resolution res = Screen.resolutions[settings.resolutionIndex];
                Screen.SetResolution(res.width, res.height, settings.fullScreen);
            }
            
            QualitySettings.vSyncCount = settings.vsync ? 1 : 0;
            QualitySettings.shadows = settings.shadowsEnabled ? ShadowQuality.All : ShadowQuality.Disable;

            Debug.Log($"Settings Applied: {settings.resolutionWidth}x{settings.resolutionHeight} @ {settings.refreshRate}Hz, Fullscreen: {settings.fullScreen}, VSync: {settings.vsync}");
        }
    }
}
