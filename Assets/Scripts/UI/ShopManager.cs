using UnityEngine;
using Factory.Core;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace Factory.UI
{
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private List<MachineDefinition> availableMachines;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private Transform container;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeButton;

        private void Start()
        {
            if (shopPanel == null) BuildUI();
            else if (container != null) RefreshShop();
        }

        private void BuildUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Create Panel
            shopPanel = new GameObject("ShopPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            shopPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform panelRT = shopPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.3f, 0.2f);
            panelRT.anchorMax = new Vector2(0.7f, 0.8f);
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;

            shopPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Close Button
            GameObject closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(shopPanel.transform, false);
            RectTransform closeRT = closeGo.GetComponent<RectTransform>();
            closeRT.anchorMin = closeRT.anchorMax = new Vector2(1, 1);
            closeRT.anchoredPosition = new Vector2(-20, -20);
            closeRT.sizeDelta = new Vector2(30, 30);
            closeGo.GetComponent<Image>().color = Color.red;
            closeButton = closeGo.GetComponent<Button>();
            closeButton.onClick.AddListener(CloseShop);

            // Container
            GameObject scrollGo = new GameObject("ShopScroll", typeof(RectTransform), typeof(ScrollRect), typeof(Image), typeof(Mask));
            scrollGo.transform.SetParent(shopPanel.transform, false);
            RectTransform scrollRT = scrollGo.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRT.anchorMax = new Vector2(0.95f, 0.85f);
            scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            scrollGo.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(scrollGo.transform, false);
            container = contentGo.transform;
            RectTransform contentRT = contentGo.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 1);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.pivot = new Vector2(0.5f, 1);
            contentRT.offsetMin = new Vector2(0, -100);
            contentRT.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollGo.GetComponent<ScrollRect>().content = contentRT;
            scrollGo.GetComponent<ScrollRect>().horizontal = false;

            shopPanel.SetActive(false);
            RefreshShop();
        }

        private void RefreshShop()
        {
            if (container == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);

            // Load Machines
            if (availableMachines == null || availableMachines.Count == 0)
            {
                availableMachines = new List<MachineDefinition>(Resources.FindObjectsOfTypeAll<MachineDefinition>());
            }

            foreach (var machine in availableMachines)
            {
                CreateMachineCard(machine);
            }
        }

        private void CreateMachineCard(MachineDefinition def)
        {
            GameObject card = new GameObject(def.machineName + "Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup));
            card.transform.SetParent(container, false);
            card.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            
            RectTransform cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 80); // Ensure card has height

            HorizontalLayoutGroup hlg = card.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 10, 10);
            hlg.spacing = 20;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(card.transform, false);
            iconObj.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 50);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = def.machineIcon;
            if (img.sprite == null) img.color = Color.gray;

            // Info
            GameObject info = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
            info.transform.SetParent(card.transform, false);
            
            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(info.transform, false);
            TextMeshProUGUI nameTxt = nameObj.GetComponent<TextMeshProUGUI>();
            nameTxt.text = def.machineName;
            nameTxt.fontSize = 24;
            nameTxt.color = Color.white;

            GameObject costObj = new GameObject("Cost", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            costObj.transform.SetParent(info.transform, false);
            TextMeshProUGUI costTxt = costObj.GetComponent<TextMeshProUGUI>();
            costTxt.text = $"Cost: ${def.cost}";
            costTxt.fontSize = 18;
            costTxt.color = Color.green;

            Button btn = card.GetComponent<Button>();
            btn.onClick.AddListener(() => {
                if (BuildManager.Instance != null) BuildManager.Instance.SetSelectedBuilding(def);
                CloseShop();
            });
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
            {
                Debug.Log("[ShopManager] Tab key pressed!");
                ToggleShop();
            }

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && shopPanel != null && shopPanel.activeSelf)
            {
                CloseShop();
            }
        }

        public void ToggleShop()
        {
            if (shopPanel.activeSelf) CloseShop();
            else OpenShop();
        }

        public void OpenShop()
        {
            shopPanel.SetActive(true);
        }

        public void CloseShop()
        {
            shopPanel.SetActive(false);
        }
    }
}
