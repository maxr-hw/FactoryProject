using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Factory.Core;
using TMPro;

namespace Factory.UI
{
    public class InGameMenuManager : MonoBehaviour
    {
        public static InGameMenuManager Instance { get; private set; }

        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject settingsPanel;
        
        private bool isMenuOpen = false;
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            if (menuPanel == null) BuildUI();
        }

        private void BuildUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Main Background Overlay
            GameObject overlay = new GameObject("PauseOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(canvas.transform, false);
            menuPanel = overlay;

            RectTransform overlayRT = overlay.GetComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = overlayRT.offsetMax = Vector2.zero;
            overlay.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);

            // Centered Card
            GameObject card = new GameObject("PauseCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            card.transform.SetParent(overlay.transform, false);
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(400, 500);
            cardRT.anchoredPosition = Vector2.zero;
            
            Image cardImg = card.GetComponent<Image>();
            cardImg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);
            card.AddComponent<UnityEngine.UI.Outline>().effectColor = new Color(1, 1, 1, 0.1f);

            // Container for buttons
            GameObject container = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            container.transform.SetParent(card.transform, false);
            RectTransform containerRT = container.GetComponent<RectTransform>();
            containerRT.anchorMin = Vector2.zero;
            containerRT.anchorMax = Vector2.one;
            containerRT.offsetMin = containerRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = container.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 25;
            vlg.padding = new RectOffset(50, 50, 60, 50);
            vlg.childAlignment = TextAnchor.UpperCenter;

            // Title
            GameObject titleGo = new GameObject("PauseTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(container.transform, false);
            TextMeshProUGUI titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.text = "PAUSED";
            titleText.fontSize = 36;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1, 0.8f, 0.2f);
            titleText.fontStyle = FontStyles.Bold;
            LayoutElement le = titleGo.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            // Spacing
            GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(container.transform, false);
            spacer.GetComponent<LayoutElement>().preferredHeight = 20;

            // Buttons
            CreateMenuButton("Continue", container.transform, Resume);
            CreateMenuButton("Settings", container.transform, OpenSettings);
            CreateMenuButton("Leave Session", container.transform, QuitToMainMenu);

            menuPanel.SetActive(false);
        }

        private void CreateMenuButton(string text, Transform parent, UnityEngine.Events.UnityAction action)
        {
            GameObject btnGo = new GameObject(text + "Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGo.transform.SetParent(parent, false);
            btnGo.GetComponent<LayoutElement>().preferredHeight = 50;
            
            Image img = btnGo.GetComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
            
            Button btn = btnGo.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.2f, 0.2f, 0.25f, 0.8f);
            cb.highlightedColor = new Color(0.3f, 0.5f, 0.8f, 1f);
            cb.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            cb.selectedColor = cb.normalColor;
            btn.colors = cb;
            
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(btnGo.transform, false);
            RectTransform textRT = textGo.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI t = textGo.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.alignment = TextAlignmentOptions.Center;
            t.fontSize = 20;
            t.color = Color.white;

            btn.onClick.AddListener(action);
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("[InGameMenuManager] Escape pressed!");
                ToggleMenu();
            }
        }

        public void ToggleMenu()
        {
            if (menuPanel == null) BuildUI();
            if (menuPanel == null) { Debug.LogError("[InGameMenuManager] Failed to BuildUI - No Canvas found?"); return; }

            isMenuOpen = !isMenuOpen;
            menuPanel.SetActive(isMenuOpen);
            if (isMenuOpen) menuPanel.transform.SetAsLastSibling();
            Debug.Log($"[InGameMenuManager] Menu toggled: {isMenuOpen}");

            if (isMenuOpen)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
                if (settingsPanel != null) settingsPanel.SetActive(false);
            }
        }

        public void Resume()
        {
            Debug.Log("[InGameMenuManager] Resume clicked");
            ToggleMenu();
        }

        public void OpenSettings()
        {
            if (SettingsUI.Instance != null) SettingsUI.Instance.Open();
        }

        public void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            Debug.Log("[InGameMenuManager] Leaving session...");
            // Load First Scene (index 0) as fallback for Main Menu
            SceneManager.LoadScene(0);
        }
    }
}
