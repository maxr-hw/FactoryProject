using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Factory.Contracts;
using Factory.Core;
using System.Collections.Generic;
using System.Text;

namespace Factory.UI
{
    public class ContractPopupUI : MonoBehaviour
    {
        public static ContractPopupUI Instance { get; private set; }

        private GameObject panel;
        private TextMeshProUGUI companyText;
        private TextMeshProUGUI descriptionText;
        private TextMeshProUGUI requirementsText;
        private TextMeshProUGUI rewardText;
        private TextMeshProUGUI timerText;

        private Button acceptButton;
        private Button declineButton;

        private Queue<ContractDefinition> offerQueue = new Queue<ContractDefinition>();
        private ContractDefinition currentOffer;

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

        private void OnEnable()
        {
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractOffered -= ShowContractOffer; // Prevent double sub
                ContractManager.Instance.OnContractOffered += ShowContractOffer;
            }
        }

        private void OnDisable()
        {
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractOffered -= ShowContractOffer;
            }
        }

        private void Start()
        {
            // Fallback for first initialization
            if (ContractManager.Instance != null)
            {
                ContractManager.Instance.OnContractOffered -= ShowContractOffer;
                ContractManager.Instance.OnContractOffered += ShowContractOffer;
            }
        }

        public void ShowContractOffer(ContractDefinition contract)
        {
            Debug.Log($"[ContractPopupUI] Offer received for {contract.companyName}");
            if (panel == null) BuildUI();

            if (panel == null)
            {
                Debug.LogError("[ContractPopupUI] BuildUI failed to create panel!");
                return;
            }

            if (currentOffer != null || panel.activeSelf)
            {
                offerQueue.Enqueue(contract);
                return;
            }

            DisplayOffer(contract);
        }

        private void DisplayOffer(ContractDefinition contract)
        {
            // Panel check/build moved to ShowContractOffer
            if (panel == null) return; 

            currentOffer = contract;
            companyText.text = contract.companyName;
            descriptionText.text = contract.description;
            rewardText.text = $"Reward: ${contract.rewardMoney}";
            timerText.text = $"Time: {contract.timeLimit}s";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Requirements:");
            foreach (var req in contract.requiredItems)
            {
                sb.AppendLine($"- {req.amount}x {req.item.itemName}");
            }
            requirementsText.text = sb.ToString();

            panel.SetActive(true);
        }

        private void Accept()
        {
            if (currentOffer != null)
            {
                ContractManager.Instance.AcceptContract(currentOffer);
                CloseAndCheckQueue();
            }
        }

        private void Decline()
        {
            CloseAndCheckQueue();
        }

        private void CloseAndCheckQueue()
        {
            panel.SetActive(false);
            currentOffer = null;

            if (offerQueue.Count > 0)
            {
                DisplayOffer(offerQueue.Dequeue());
            }
        }

        private void BuildUI()
        {
            if (panel != null) return;

            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                // Try to find any canvas, even inactive
                canvas = Resources.FindObjectsOfTypeAll<Canvas>().Length > 0 ? Resources.FindObjectsOfTypeAll<Canvas>()[0] : null;
            }

            if (canvas == null)
            {
                Debug.LogError("[ContractPopupUI] Failed to find a Canvas! Skipping UI build.");
                return;
            }

            Debug.Log("[ContractPopupUI] Building UI under Canvas: " + canvas.name);

            // Layer for UI
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer == -1) uiLayer = 5;

            // Main Panel
            panel = new GameObject("ContractPopup", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            panel.layer = uiLayer;
            RectTransform pRT = panel.GetComponent<RectTransform>();
            pRT.sizeDelta = new Vector2(400, 350);
            panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            // Title/Company
            GameObject titleObj = new GameObject("Company", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(panel.transform, false);
            titleObj.layer = uiLayer;
            companyText = titleObj.GetComponent<TextMeshProUGUI>();
            companyText.fontSize = 24;
            companyText.fontStyle = FontStyles.Bold;
            companyText.alignment = TextAlignmentOptions.Center;
            companyText.color = new Color(0.9f, 0.6f, 0.2f);
            companyText.rectTransform.anchoredPosition = new Vector2(0, 140);
            companyText.rectTransform.sizeDelta = new Vector2(380, 40);

            // Description
            GameObject descObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(panel.transform, false);
            descObj.layer = uiLayer;
            descriptionText = descObj.GetComponent<TextMeshProUGUI>();
            descriptionText.fontSize = 14;
            descriptionText.alignment = TextAlignmentOptions.TopLeft;
            descriptionText.rectTransform.anchoredPosition = new Vector2(0, 80);
            descriptionText.rectTransform.sizeDelta = new Vector2(360, 60);

            // Requirements
            GameObject reqObj = new GameObject("Requirements", typeof(RectTransform), typeof(TextMeshProUGUI));
            reqObj.transform.SetParent(panel.transform, false);
            reqObj.layer = uiLayer;
            requirementsText = reqObj.GetComponent<TextMeshProUGUI>();
            requirementsText.fontSize = 14;
            requirementsText.alignment = TextAlignmentOptions.TopLeft;
            requirementsText.rectTransform.anchoredPosition = new Vector2(0, 0);
            requirementsText.rectTransform.sizeDelta = new Vector2(360, 80);

            // Reward
            GameObject rewObj = new GameObject("Reward", typeof(RectTransform), typeof(TextMeshProUGUI));
            rewObj.transform.SetParent(panel.transform, false);
            rewObj.layer = uiLayer;
            rewardText = rewObj.GetComponent<TextMeshProUGUI>();
            rewardText.fontSize = 16;
            rewardText.color = Color.green;
            rewardText.rectTransform.anchoredPosition = new Vector2(-80, -80);
            rewardText.rectTransform.sizeDelta = new Vector2(180, 30);

            // Timer
            GameObject timeObj = new GameObject("Timer", typeof(RectTransform), typeof(TextMeshProUGUI));
            timeObj.transform.SetParent(panel.transform, false);
            timeObj.layer = uiLayer;
            timerText = timeObj.GetComponent<TextMeshProUGUI>();
            timerText.fontSize = 16;
            timerText.color = Color.cyan;
            timerText.rectTransform.anchoredPosition = new Vector2(80, -80);
            timerText.rectTransform.sizeDelta = new Vector2(180, 30);

            // Accept Button
            GameObject accObj = new GameObject("AcceptButton", typeof(RectTransform), typeof(Image), typeof(Button));
            accObj.transform.SetParent(panel.transform, false);
            accObj.layer = uiLayer;
            accObj.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.2f);
            accObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, -140);
            accObj.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 40);
            acceptButton = accObj.GetComponent<Button>();
            acceptButton.onClick.AddListener(Accept);
            AddTextToButton(accObj, "ACCEPT", uiLayer);

            // Decline Button
            GameObject decObj = new GameObject("DeclineButton", typeof(RectTransform), typeof(Image), typeof(Button));
            decObj.transform.SetParent(panel.transform, false);
            decObj.layer = uiLayer;
            decObj.GetComponent<Image>().color = new Color(0.6f, 0.2f, 0.2f);
            decObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(100, -140);
            decObj.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 40);
            declineButton = decObj.GetComponent<Button>();
            declineButton.onClick.AddListener(Decline);
            AddTextToButton(decObj, "DECLINE", uiLayer);

            panel.SetActive(false);
        }

        private void AddTextToButton(GameObject btn, string text, int uiLayer)
        {
            GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btn.transform, false);
            txtObj.layer = uiLayer;
            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.anchorMin = Vector2.zero;
            tmp.rectTransform.anchorMax = Vector2.one;
            tmp.rectTransform.sizeDelta = Vector2.zero;
        }
    }
}
