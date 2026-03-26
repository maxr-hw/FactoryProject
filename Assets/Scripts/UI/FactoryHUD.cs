using UnityEngine;
using TMPro;
using Factory.Economy;
using Factory.Contracts;
using Factory.Core;
using System.Collections.Generic;
using System.Text;

namespace Factory.UI
{
    public class FactoryHUD : MonoBehaviour
    {
        public static FactoryHUD Instance { get; private set; }

        [Header("UI Containers")]
        public RectTransform topLeftContainer;
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI nextOfferText;

        [Header("Contract HUD Settings")]
        public float entryWidth = 250f;
        public float progressBarHeight = 10f;
        
        private GameObject contractContainer;
        private Dictionary<ContractManager.ActiveContract, ContractUIRefs> activeUIEntries = new Dictionary<ContractManager.ActiveContract, ContractUIRefs>();

        private class ContractUIRefs
        {
            public GameObject entryObj;
            public TextMeshProUGUI titleText;
            public Dictionary<ItemDefinition, ProgressBarRefs> progressBars = new Dictionary<ItemDefinition, ProgressBarRefs>();
        }

        private class ProgressBarRefs
        {
            public RectTransform fillRect;
            public TextMeshProUGUI amountText;
        }

        [Header("Bottom Bar")]
        public Transform hotbarContainer; // Can be filled with UI icons for 1-9
        public Transform bottomBar; // Added for the new button

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
                return;
            }
        }

        private void Start()
        {
            InitializeHUD();

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnMoneyChanged += UpdateMoneyUI;
                UpdateMoneyUI();
            }

            // We update in Update() now for smooth timers, but sub to events for structure changes
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractUpdated += RefreshContractEntries;
                ContractManager.Instance.OnContractCompleted += RefreshContractEntries;
                ContractManager.Instance.OnContractFailed += RefreshContractEntries;
            }
        }

        private void InitializeHUD()
        {
            if (topLeftContainer == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject go = new GameObject("HUD_TopLeft", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup));
                    topLeftContainer = go.GetComponent<RectTransform>();
                    topLeftContainer.SetParent(canvas.transform, false);
                    topLeftContainer.anchorMin = new Vector2(0, 1);
                    topLeftContainer.anchorMax = new Vector2(0, 1);
                    topLeftContainer.pivot = new Vector2(0, 1);
                    topLeftContainer.anchoredPosition = new Vector2(20, -20);
                    topLeftContainer.sizeDelta = new Vector2(300, 600);
                    
                    var vlg = go.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                    vlg.childAlignment = TextAnchor.UpperLeft;
                    vlg.childControlHeight = true;
                    vlg.childControlWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.spacing = 10;

                    var csf = go.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                    csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                    
                    go.SetActive(true);
                }
            }
            else
            {
                topLeftContainer.gameObject.SetActive(true);
            }

            if (moneyText == null && topLeftContainer != null)
            {
                GameObject moneyObj = new GameObject("MoneyText", typeof(RectTransform), typeof(TextMeshProUGUI));
                moneyObj.transform.SetParent(topLeftContainer, false);
                moneyText = moneyObj.GetComponent<TextMeshProUGUI>();
                moneyText.fontSize = 24;
                moneyText.color = Color.green;
                moneyText.fontStyle = FontStyles.Bold; // Make it bold
                moneyText.gameObject.SetActive(true);
                
                // Add shadow for visibility on light backgrounds
                var shadow = moneyText.gameObject.AddComponent<UnityEngine.UI.Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.5f);
                shadow.effectDistance = new Vector2(2, -2);
            }
            else if (moneyText != null)
            {
                moneyText.gameObject.SetActive(true);
            }

            if (nextOfferText == null && topLeftContainer != null)
            {
                GameObject nextOfferObj = new GameObject("NextOfferText", typeof(RectTransform), typeof(TextMeshProUGUI));
                nextOfferObj.transform.SetParent(topLeftContainer, false);
                nextOfferText = nextOfferObj.GetComponent<TextMeshProUGUI>();
                nextOfferText.fontSize = 14;
                nextOfferText.color = Color.black; // Changed to black as requested for visibility
                nextOfferText.gameObject.SetActive(true);
            }
            else if (nextOfferText != null)
            {
                nextOfferText.gameObject.SetActive(true);
            }
            
            // Re-use or create contract container
            contractContainer = new GameObject("ContractList", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup));
            contractContainer.transform.SetParent(topLeftContainer, false);
            var cvlg = contractContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            cvlg.childAlignment = TextAnchor.UpperLeft;
            cvlg.childControlHeight = true;
            cvlg.childControlWidth = true;
            cvlg.childForceExpandHeight = false;
            cvlg.spacing = 15;

            var ccsf = contractContainer.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            ccsf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            if (bottomBar == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject go = new GameObject("HUD_Bottom", typeof(RectTransform), typeof(UnityEngine.UI.HorizontalLayoutGroup));
                    bottomBar = go.transform;
                    bottomBar.SetParent(canvas.transform, false);
                    RectTransform rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0);
                    rt.anchorMax = new Vector2(0.5f, 0);
                    rt.pivot = new Vector2(0.5f, 0);
                    rt.anchoredPosition = new Vector2(0, 20);
                    rt.sizeDelta = new Vector2(1000, 60);

                    var hlg = go.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                    hlg.childAlignment = TextAnchor.MiddleCenter;
                    hlg.spacing = 20;
                    hlg.childControlHeight = false;
                    hlg.childControlWidth = false;
                }
            }

            if (bottomBar != null && bottomBar.Find("RecipesButton") == null)
            {
                GameObject recipesBtnObj = new GameObject("RecipesButton", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
                recipesBtnObj.transform.SetParent(bottomBar, false);
                recipesBtnObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 45);
                recipesBtnObj.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.4f, 0.6f, 0.9f);
                
                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(recipesBtnObj.transform, false);
                TextMeshProUGUI t = textObj.GetComponent<TextMeshProUGUI>();
                t.text = "RECIPES (P)";
                t.fontSize = 18;
                t.alignment = TextAlignmentOptions.Center;
                t.rectTransform.anchorMin = Vector2.zero;
                t.rectTransform.anchorMax = Vector2.one;
                t.rectTransform.sizeDelta = Vector2.zero;
                
                recipesBtnObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {
                    if (MaterialsRecipesUI.Instance != null) MaterialsRecipesUI.Instance.ToggleMenu();
                });
            }
        }

        private void Update()
        {
            // Robustness check: if canvas or container is lost (e.g. scene load), re-init
            if (topLeftContainer == null || !topLeftContainer.gameObject.activeInHierarchy)
            {
                InitializeHUD();
            }

            UpdateContractList();
        }

        private void OnDestroy()
        {
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.OnMoneyChanged -= UpdateMoneyUI;

            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractUpdated -= RefreshContractEntries;
                ContractManager.Instance.OnContractCompleted -= RefreshContractEntries;
                ContractManager.Instance.OnContractFailed -= RefreshContractEntries;
            }
        }

        private string GetNormalizedName(ItemDefinition item)
        {
            if (item == null) return "Unknown";
            string raw = string.IsNullOrEmpty(item.itemName) ? item.name : item.itemName;
            return raw.Replace("_", " ").Trim();
        }

        public void UpdateMoney(int amount)
        {
            if (moneyText != null)
                moneyText.text = $"${EconomyManager.Instance.CurrentMoney}";
        }

        private void UpdateMoneyUI()
        {
            if (moneyText != null)
            {
                moneyText.text = $"${EconomyManager.Instance.CurrentMoney}";
                moneyText.gameObject.SetActive(true);
            }
        }

        private void RefreshContractEntries()
        {
            // Clear dead entries
            List<ContractManager.ActiveContract> currentActive = ContractManager.Instance.GetActiveContracts();
            List<ContractManager.ActiveContract> toRemove = new List<ContractManager.ActiveContract>();

            foreach (var key in activeUIEntries.Keys)
            {
                if (!currentActive.Contains(key)) toRemove.Add(key);
            }

            foreach (var key in toRemove)
            {
                if (activeUIEntries[key].entryObj != null) Destroy(activeUIEntries[key].entryObj);
                activeUIEntries.Remove(key);
            }

            // Add new entries
            foreach (var ac in currentActive)
            {
                if (!activeUIEntries.ContainsKey(ac))
                {
                    CreateContractUIEntry(ac);
                }
            }
        }

        private void CreateContractUIEntry(ContractManager.ActiveContract ac)
        {
            GameObject entry = new GameObject($"Contract_{ac.definition.companyName}", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup), typeof(UnityEngine.UI.Image));
            entry.transform.SetParent(contractContainer.transform, false);
            entry.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.4f);
            
            var vlg = entry.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            vlg.padding = new UnityEngine.RectOffset(10, 10, 10, 10);
            vlg.spacing = 8;
            vlg.childControlHeight = true; // Let it control height of children
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = entry.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;

            ContractUIRefs refs = new ContractUIRefs();
            refs.entryObj = entry;

            // Title line
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(entry.transform, false);
            refs.titleText = titleObj.GetComponent<TextMeshProUGUI>();
            refs.titleText.fontSize = 16;
            refs.titleText.fontStyle = FontStyles.Bold;

            // Item Progress Bars
            foreach (var req in ac.definition.requiredItems)
            {
                GameObject barContainer = new GameObject($"Bar_{req.item.itemName}", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup));
                barContainer.transform.SetParent(entry.transform, false);
                
                // Label
                GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(barContainer.transform, false);
                var labelText = labelObj.GetComponent<TextMeshProUGUI>();
                labelText.text = req.item.itemName;
                labelText.fontSize = 12;

                // Bar Background
                GameObject bg = new GameObject("BarBG", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.LayoutElement));
                bg.transform.SetParent(barContainer.transform, false);
                bg.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                var le = bg.GetComponent<UnityEngine.UI.LayoutElement>();
                le.preferredHeight = progressBarHeight;
                le.minHeight = progressBarHeight;
                le.flexibleWidth = 1;

                // Bar Fill
                GameObject fill = new GameObject("BarFill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                fill.transform.SetParent(bg.transform, false);
                fill.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.7f, 0.2f);
                RectTransform fillRT = fill.GetComponent<RectTransform>();
                fillRT.anchorMin = Vector2.zero;
                fillRT.anchorMax = new Vector2(0, 1); // Start at 0 width
                fillRT.pivot = new Vector2(0, 0.5f);
                fillRT.anchoredPosition = Vector2.zero;
                fillRT.sizeDelta = Vector2.zero;

                // Amount Text
                GameObject amountObj = new GameObject("Amount", typeof(RectTransform), typeof(TextMeshProUGUI));
                amountObj.transform.SetParent(bg.transform, false);
                var amountText = amountObj.GetComponent<TextMeshProUGUI>();
                amountText.fontSize = 11;
                amountText.alignment = TextAlignmentOptions.Center;
                amountText.rectTransform.anchorMin = Vector2.zero;
                amountText.rectTransform.anchorMax = Vector2.one;
                amountText.rectTransform.sizeDelta = Vector2.zero;

                refs.progressBars[req.item] = new ProgressBarRefs { fillRect = fillRT, amountText = amountText };
            }

            activeUIEntries[ac] = refs;
        }

        private void UpdateContractList()
        {
            if (ContractManager.Instance == null) return;

            // Ensure our UI mapping stays in sync if entries are added/removed
            RefreshContractEntries();

            foreach (var pair in activeUIEntries)
            {
                var ac = pair.Key;
                var refs = pair.Value;

                if (refs.entryObj == null) continue;

                // Update Title (Company + Timer + Reward)
                refs.titleText.text = $"{ac.definition.companyName} <color=#000000>{Mathf.CeilToInt(ac.timeRemaining)}s</color> <color=#00ff00>${ac.scaledReward}</color>";

                // Update Progress Bars
                foreach (var req in ac.definition.requiredItems)
                {
                    if (refs.progressBars.TryGetValue(req.item, out var bar))
                    {
                        // Match by name as we do in ContractManager to handle asset duplicates
                        string itemName = GetNormalizedName(req.item);
                        int delivered = ac.deliveredItems.ContainsKey(itemName) ? ac.deliveredItems[itemName] : 0;
                        int target = ac.targetAmounts.ContainsKey(req.item) ? ac.targetAmounts[req.item] : req.amount;
                        float progress = Mathf.Clamp01((float)delivered / target);
                        
                        bar.fillRect.anchorMax = new Vector2(progress, 1);
                        bar.amountText.text = $"{delivered}/{target}";
                    }
                }
            }

            // Update Next Offer Timer
            if (nextOfferText != null)
            {
                if (ContractManager.Instance.GetActiveContracts().Count < 2)
                {
                    nextOfferText.text = $"Niveau {ContractManager.Instance.CurrentLevel} | Prochain contrat : <color=#000000>{Mathf.CeilToInt(ContractManager.Instance.NextOfferTimer)}s</color>";
                }
                else
                {
                    nextOfferText.text = $"Niveau {ContractManager.Instance.CurrentLevel} | Maximum de contrats actifs";
                }
            }
        }
    }
}
