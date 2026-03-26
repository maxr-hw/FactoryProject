using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Factory.Core;

namespace Factory.UI
{
    public class ShopButton : MonoBehaviour
    {
        private TextMeshProUGUI nameText;
        private TextMeshProUGUI costText;
        private Button button;

        private MachineDefinition definition;

        public void Initialize(MachineDefinition def)
        {
            definition = def;

            button = GetComponent<Button>();

            // Search specifically for TMPro texts
            TextMeshProUGUI[] tmpTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
            
            // Assign based on names if possible, otherwise by order
            foreach (var t in tmpTexts)
            {
                if (t.name.Contains("Name")) nameText = t;
                else if (t.name.Contains("Price")) costText = t;
            }

            // Fallback to order if names don't match
            if (nameText == null && tmpTexts.Length > 0) nameText = tmpTexts[0];
            if (costText == null && tmpTexts.Length > 1) costText = tmpTexts[1];

            // Apply consistent styling
            if (nameText != null)
            {
                nameText.text = def.machineName;
                nameText.color = Color.black;
            }
            if (costText != null)
            {
                costText.text = $"${def.cost}";
                costText.color = new Color(0.2f, 0.2f, 0.2f); // Dark gray
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void OnButtonClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

            if (BuildManager.Instance != null)
            {
                BuildManager.Instance.SetSelectedBuilding(definition);
            }

            // Close the shop so the player can place without UI obstruction
            ShopManager shop = GetComponentInParent<ShopManager>(true);
            if (shop != null) shop.CloseShop();
        }
    }
}
