using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Factory.Core;
using System.Linq;
using System.Collections.Generic;

namespace Factory.UI
{
    public class SettingsUI : MonoBehaviour
    {
        public static SettingsUI Instance { get; private set; }

        [Header("Audio")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;

        [Header("Controls")]
        public Slider sensitivitySlider;
        public Toggle invertYToggle;
        
        [Header("Graphics/Display (Procedural)")]
        public Toggle fullscreenToggle;
        public Toggle vsyncToggle;
        public Toggle shadowsToggle;
        public TMP_Dropdown resolutionDropdown; // Legacy
        public TMP_Dropdown qualityDropdown; // Legacy
        
        [Header("Keybinds (Visual Only)")]
        public TextMeshProUGUI keybindsPlaceholder;

        private GameObject settingsPanel;
        private List<Resolution> resolutions = new List<Resolution>();
        private int currentResIndex = 0;
        private int currentQualityIndex = 0;
        private TextMeshProUGUI resolutionText;
        private TextMeshProUGUI qualityText;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeResolutions();
        }

        private void InitializeResolutions()
        {
            resolutions = Screen.resolutions.Where(r => r.width >= 800).OrderByDescending(r => r.width).ThenByDescending(r => r.height).ToList();
            
            currentResIndex = 0;
            for (int i = 0; i < resolutions.Count; i++)
            {
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResIndex = i;
                    break;
                }
            }
            UpdateResolutionText();
        }

        private void UpdateResolutionText()
        {
            if (resolutionText == null || resolutions.Count == 0) return;
            var res = resolutions[currentResIndex];
            double hz = (double)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator;
            resolutionText.text = $"{res.width} x {res.height} @ {hz:F0}Hz";
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
        }

        public async void LoadCurrentSettings()
        {
            // Wait for instance if it's being created
            int attempts = 0;
            while (SettingsManager.Instance == null && attempts < 10) { await System.Threading.Tasks.Task.Delay(100); attempts++; }
            if (SettingsManager.Instance == null) return;

            var s = SettingsManager.Instance.settings;

            if (masterVolumeSlider) masterVolumeSlider.value = s.masterVolume;
            if (musicVolumeSlider) musicVolumeSlider.value = s.musicVolume;
            if (sfxVolumeSlider) sfxVolumeSlider.value = s.sfxVolume;

            currentQualityIndex = s.qualityLevel;
            if (qualityText) qualityText.text = QualitySettings.names[Mathf.Clamp(currentQualityIndex, 0, QualitySettings.names.Length - 1)];
            if (shadowsToggle) shadowsToggle.isOn = s.shadowsEnabled;

            if (fullscreenToggle) fullscreenToggle.isOn = s.fullScreen;
            if (vsyncToggle) vsyncToggle.isOn = s.vsync;
            
            if (resolutionText != null && resolutions.Count > 0)
            {
                for (int i = 0; i < resolutions.Count; i++)
                {
                    if (resolutions[i].width == s.resolutionWidth && resolutions[i].height == s.resolutionHeight)
                    {
                        currentResIndex = i;
                        UpdateResolutionText();
                        break;
                    }
                }
            }

            if (sensitivitySlider) sensitivitySlider.value = s.mouseSensitivity;
            if (invertYToggle) invertYToggle.isOn = s.invertY;
        }

        public void SaveAndApply()
        {
            if (SettingsManager.Instance == null) return;
            var s = SettingsManager.Instance.settings;

            if (masterVolumeSlider) s.masterVolume = masterVolumeSlider.value;
            if (musicVolumeSlider) s.musicVolume = musicVolumeSlider.value;
            if (sfxVolumeSlider) s.sfxVolume = sfxVolumeSlider.value;

            s.qualityLevel = currentQualityIndex;
            if (shadowsToggle) s.shadowsEnabled = shadowsToggle.isOn;

            if (fullscreenToggle) s.fullScreen = fullscreenToggle.isOn;
            if (vsyncToggle) s.vsync = vsyncToggle.isOn;

            if (resolutions.Count > 0 && currentResIndex < resolutions.Count)
            {
                var res = resolutions[currentResIndex];
                s.resolutionWidth = res.width;
                s.resolutionHeight = res.height;
                s.refreshRate = (int)((double)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);
            }

            if (sensitivitySlider) s.mouseSensitivity = sensitivitySlider.value;
            if (invertYToggle) s.invertY = invertYToggle.isOn;

            SettingsManager.Instance.SaveSettings();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        }

        public void Close()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void BuildUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Panel Background
            settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            settingsPanel.transform.SetParent(canvas.transform, false);
            RectTransform panelRT = settingsPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.2f, 0.1f);
            panelRT.anchorMax = new Vector2(0.8f, 0.9f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            settingsPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 0.98f);
            settingsPanel.AddComponent<UnityEngine.UI.Outline>().effectColor = new Color(1, 1, 1, 0.2f);

            // Title
            GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(settingsPanel.transform, false);
            var tRT = titleGo.GetComponent<RectTransform>();
            tRT.anchorMin = tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.anchoredPosition = new Vector2(0, -40);
            tRT.sizeDelta = new Vector2(400, 50);
            var t = titleGo.GetComponent<TextMeshProUGUI>();
            t.text = "SETTINGS";
            t.fontSize = 32;
            t.alignment = TextAlignmentOptions.Center;
            t.fontStyle = FontStyles.Bold;

            // Scroll Area
            GameObject scrollGo = new GameObject("ScrollArea", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollGo.transform.SetParent(settingsPanel.transform, false);
            var scrollRT = scrollGo.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.05f, 0.15f);
            scrollRT.anchorMax = new Vector2(0.95f, 0.85f);
            scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);
            scrollGo.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRT = contentGo.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = new Vector2(0, -500);
            contentRT.offsetMax = Vector2.zero;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.spacing = 20;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollGo.GetComponent<ScrollRect>().content = contentRT;
            scrollGo.GetComponent<ScrollRect>().horizontal = false;

            // Sections
            CreateSectionHeader(contentGo.transform, "DISPLAY");
            CreateResolutionSwitcher(contentGo.transform);
            InitializeResolutions();
            fullscreenToggle = CreateToggle(contentGo.transform, "Fullscreen");
            vsyncToggle = CreateToggle(contentGo.transform, "VSync");
            shadowsToggle = CreateToggle(contentGo.transform, "Shadows");
            CreateQualitySwitcher(contentGo.transform);

            CreateSectionHeader(contentGo.transform, "AUDIO");
            masterVolumeSlider = CreateSlider(contentGo.transform, "Master Volume", 0, 1);
            musicVolumeSlider = CreateSlider(contentGo.transform, "Music Volume", 0, 1);
            sfxVolumeSlider = CreateSlider(contentGo.transform, "SFX Volume", 0, 1);

            CreateSectionHeader(contentGo.transform, "CONTROLS");
            sensitivitySlider = CreateSlider(contentGo.transform, "Mouse Sensitivity", 0.1f, 5f);
            invertYToggle = CreateToggle(contentGo.transform, "Invert Y Axis");

            // Buttons Area
            GameObject btnsGo = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            btnsGo.transform.SetParent(settingsPanel.transform, false);
            var btnRT = btnsGo.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0, 0);
            btnRT.anchorMax = new Vector2(1, 0.12f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;

            var hlg = btnsGo.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 10, 10);
            hlg.spacing = 20;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;

            CreateActionButton("APPLY", btnsGo.transform, () => { SaveAndApply(); Close(); });
            CreateActionButton("BACK", btnsGo.transform, Close);

            settingsPanel.SetActive(false);
            LoadCurrentSettings();
        }

        private void CreateQualitySwitcher(Transform parent)
        {
            GameObject row = CreateRow(parent, "Quality");
            GameObject container = new GameObject("QualitySwitcher", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            container.transform.SetParent(row.transform, false);
            var hlg = container.GetComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.spacing = 10;

            string[] names = QualitySettings.names;
            GameObject txtGo = new GameObject("QualityText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            txtGo.transform.SetParent(container.transform, false);
            qualityText = txtGo.GetComponent<TextMeshProUGUI>();
            qualityText.alignment = TextAlignmentOptions.Center;
            qualityText.fontSize = 16;
            txtGo.GetComponent<LayoutElement>().preferredWidth = 150;

            System.Action updateUI = () => {
                qualityText.text = names[Mathf.Clamp(currentQualityIndex, 0, names.Length - 1)];
            };

            CreateArrowButton(container.transform, "<", () => {
                currentQualityIndex = (currentQualityIndex - 1 + names.Length) % names.Length;
                updateUI();
            });

            CreateArrowButton(container.transform, ">", () => {
                currentQualityIndex = (currentQualityIndex + 1) % names.Length;
                updateUI();
            });

            updateUI();
        }

        private void CreateSectionHeader(Transform parent, string text)
        {
            GameObject go = new GameObject("Header_" + text, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 24;
            t.color = new Color(1, 0.8f, 0.2f);
            t.fontStyle = FontStyles.Bold;
            t.margin = new Vector4(0, 10, 0, 5);
            go.GetComponent<LayoutElement>().preferredHeight = 40;
        }

        private void CreateResolutionSwitcher(Transform parent)
        {
            GameObject row = CreateRow(parent, "Resolution");
            
            GameObject container = new GameObject("ResSwitcher", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            container.transform.SetParent(row.transform, false);
            var hlg = container.GetComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.spacing = 10;

            CreateArrowButton(container.transform, "<", () => {
                currentResIndex = (currentResIndex - 1 + resolutions.Count) % resolutions.Count;
                UpdateResolutionText();
            });

            GameObject txtGo = new GameObject("ResText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            txtGo.transform.SetParent(container.transform, false);
            resolutionText = txtGo.GetComponent<TextMeshProUGUI>();
            resolutionText.alignment = TextAlignmentOptions.Center;
            resolutionText.fontSize = 16;
            txtGo.GetComponent<LayoutElement>().preferredWidth = 150;

            CreateArrowButton(container.transform, ">", () => {
                currentResIndex = (currentResIndex + 1) % resolutions.Count;
                UpdateResolutionText();
            });
        }

        private void CreateArrowButton(Transform parent, string text, UnityEngine.Events.UnityAction action)
        {
            GameObject go = new GameObject("Arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredWidth = 30;
            go.GetComponent<LayoutElement>().preferredHeight = 30;
            go.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);

            Button b = go.GetComponent<Button>();
            b.onClick.AddListener(action);

            GameObject tGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tGo.transform.SetParent(go.transform, false);
            var t = tGo.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 16;
            t.rectTransform.anchorMin = Vector2.zero;
            t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
        }

        private TMP_Dropdown CreateDropdown(Transform parent, string label)
        {
            // Deprecated for now, keeping signature for compatibility if needed elsewhere
            return null;
        }

        private Toggle CreateToggle(Transform parent, string label)
        {
            GameObject row = CreateRow(parent, label);
            GameObject tGo = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            tGo.transform.SetParent(row.transform, false);
            tGo.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            tGo.GetComponent<LayoutElement>().preferredWidth = 30;
            tGo.GetComponent<LayoutElement>().preferredHeight = 30;
            
            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(tGo.transform, false);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);

            GameObject check = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            check.transform.SetParent(bg.transform, false);
            var cRT = check.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.2f, 0.2f);
            cRT.anchorMax = new Vector2(0.8f, 0.8f);
            cRT.offsetMin = cRT.offsetMax = Vector2.zero;
            check.GetComponent<Image>().color = new Color(0.1f, 0.8f, 0.2f, 1f); // Nicer green

            Toggle t = tGo.GetComponent<Toggle>();
            t.targetGraphic = bg.GetComponent<Image>();
            t.graphic = check.GetComponent<Image>();
            return t;
        }

        private Slider CreateSlider(Transform parent, string label, float min, float max)
        {
            GameObject row = CreateRow(parent, label);
            GameObject sGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(LayoutElement));
            sGo.transform.SetParent(row.transform, false);
            sGo.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 30);
            sGo.GetComponent<LayoutElement>().preferredWidth = 200;
            sGo.GetComponent<LayoutElement>().preferredHeight = 30;

            GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sGo.transform, false);
            var bgRT = bg.GetComponent<RectTransform>();
            bgRT.anchorMin = new Vector2(0, 0.4f);
            bgRT.anchorMax = new Vector2(1, 0.6f);
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            bg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);

            GameObject fillArea = new GameObject("FillArea", typeof(RectTransform));
            fillArea.transform.SetParent(sGo.transform, false);
            var faRT = fillArea.GetComponent<RectTransform>();
            faRT.anchorMin = new Vector2(0, 0.4f);
            faRT.anchorMax = new Vector2(1, 0.6f);
            faRT.offsetMin = new Vector2(5, 0);
            faRT.offsetMax = new Vector2(-5, 0);

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            var fRT = fill.GetComponent<RectTransform>();
            fRT.anchorMin = Vector2.zero;
            fRT.anchorMax = Vector2.one;
            fRT.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.9f);

            GameObject handleArea = new GameObject("HandleArea", typeof(RectTransform));
            handleArea.transform.SetParent(sGo.transform, false);
            var haRT = handleArea.GetComponent<RectTransform>();
            haRT.anchorMin = new Vector2(0, 0);
            haRT.anchorMax = new Vector2(1, 1);
            haRT.offsetMin = new Vector2(10, 0);
            haRT.offsetMax = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var hRT = handle.GetComponent<RectTransform>();
            hRT.anchorMin = new Vector2(0.5f, 0.5f);
            hRT.anchorMax = new Vector2(0.5f, 0.5f);
            hRT.sizeDelta = new Vector2(20, 20);
            handle.GetComponent<Image>().color = Color.white;
            // Add a small circle-like look if possible, but solid is fine for now

            Slider s = sGo.GetComponent<Slider>();
            s.fillRect = fill.GetComponent<RectTransform>();
            s.handleRect = hRT;
            s.targetGraphic = handle.GetComponent<Image>();
            s.minValue = min;
            s.maxValue = max;
            return s;
        }

        private GameObject CreateRow(Transform parent, string labelText)
        {
            GameObject row = new GameObject("Row_" + labelText, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            row.GetComponent<LayoutElement>().preferredHeight = 50;
            
            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 0, 0);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            GameObject lGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            lGo.transform.SetParent(row.transform, false);
            lGo.GetComponent<LayoutElement>().preferredWidth = 200;
            var t = lGo.GetComponent<TextMeshProUGUI>();
            t.text = labelText;
            t.fontSize = 20;
            t.color = Color.white;
            return row;
        }

        private void CreateActionButton(string text, Transform parent, UnityEngine.Events.UnityAction action)
        {
            GameObject btnGo = new GameObject(text + "Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGo.transform.SetParent(parent, false);
            btnGo.GetComponent<LayoutElement>().preferredHeight = 50;
            btnGo.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 0.9f);
            
            Button btn = btnGo.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.3f, 0.5f, 0.8f, 1f);
            btn.colors = cb;
            btn.onClick.AddListener(action);

            GameObject tGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            tGo.transform.SetParent(btnGo.transform, false);
            var t = tGo.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 18;
            t.alignment = TextAlignmentOptions.Center;
            t.rectTransform.anchorMin = Vector2.zero;
            t.rectTransform.anchorMax = Vector2.one;
            t.rectTransform.offsetMin = t.rectTransform.offsetMax = Vector2.zero;
        }

        public void Open()
        {
            if (settingsPanel == null) BuildUI();
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                settingsPanel.transform.SetAsLastSibling();
                LoadCurrentSettings();
            }
        }
    }
}
