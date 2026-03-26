using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Factory.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private string gameSceneName = "SampleScene";
        
        private GameObject menuCanvas;
        private GameObject menuPanel;

        private void Start()
        {
            // Ensure EventSystem exists
            if (FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length == 0)
            {
                // Use InputSystemUIInputModule for compatibility with the new Input System
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            }

            // Disable existing canvases to avoid clutter, but preserve the new ones
            Canvas[] existing = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var c in existing)
            {
                if (c.gameObject.name != "MainMenuCanvas" && c.gameObject.name != "SettingsCanvas")
                {
                    c.gameObject.SetActive(false);
                }
            }
            
            BuildUI();
        }

        private void BuildUI()
        {
            // Create Canvas
            menuCanvas = new GameObject("MainMenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = menuCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = menuCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Create Background Panel
            menuPanel = new GameObject("MainMenuPanel", typeof(RectTransform), typeof(Image));
            menuPanel.transform.SetParent(menuCanvas.transform, false);
            var panelRT = menuPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.sizeDelta = Vector2.zero;
            
            menuPanel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.08f, 1f);

            // Create Center Container
            GameObject centerGo = new GameObject("CenterContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            centerGo.transform.SetParent(menuPanel.transform, false);
            var centerRT = centerGo.GetComponent<RectTransform>();
            centerRT.anchorMin = new Vector2(0.5f, 0.5f);
            centerRT.anchorMax = new Vector2(0.5f, 0.5f);
            centerRT.pivot = new Vector2(0.5f, 0.5f);
            centerRT.sizeDelta = new Vector2(400, 0);

            var vlg = centerGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            var csf = centerGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create Title
            GameObject titleGo = new GameObject("GameTitle", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            titleGo.transform.SetParent(centerGo.transform, false);
            var titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "FACTORY PROJECT";
            titleText.fontSize = 72;
            titleText.color = new Color(1f, 0.8f, 0.2f);
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            
            titleGo.GetComponent<LayoutElement>().preferredHeight = 150;

            // Add Space
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(centerGo.transform, false);
            spacer.GetComponent<LayoutElement>().preferredHeight = 50;

            // Create Buttons
            CreateMenuButton(centerGo.transform, "PLAY", () => StartGame());
            CreateMenuButton(centerGo.transform, "SETTINGS", () => OpenSettings());
            CreateMenuButton(centerGo.transform, "EXIT", () => QuitGame());
        }

        private void CreateMenuButton(Transform parent, string label, System.Action onClick)
        {
            GameObject btnGo = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGo.transform.SetParent(parent, false);
            
            var le = btnGo.GetComponent<LayoutElement>();
            le.preferredHeight = 60;
            le.preferredWidth = 300;

            var img = btnGo.GetComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            
            var btn = btnGo.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick());
            
            // Text
            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRT = txtGo.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;
            
            var txt = txtGo.GetComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 24;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;

            // Hover effects (simulated with transition)
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            cb.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            cb.pressedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
            btn.colors = cb;
        }

        public void StartGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenSettings()
        {
            if (SettingsUI.Instance != null)
            {
                SettingsUI.Instance.Open();
            }
            else
            {
                Debug.LogError("SettingsUI Instance not found!");
            }
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
