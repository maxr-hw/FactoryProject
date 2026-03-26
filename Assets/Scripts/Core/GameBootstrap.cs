using UnityEngine;
using Factory.Core;
using Factory.Economy;
using Factory.Contracts;
using Factory.UI;

namespace Factory.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GridManager gridManager;
        [SerializeField] private BuildManager buildManager;
        [SerializeField] private EconomyManager economyManager;
        [SerializeField] private ContractManager contractManager;

        private void Start()
        {
            // Only add if not already present on an ACTIVE object in the scene
            if (FindAnyObjectByType<ContractPopupUI>(FindObjectsInactive.Exclude) == null)
            {
                gameObject.AddComponent<ContractPopupUI>();
                Debug.Log("[GameBootstrap] Added missing ContractPopupUI at runtime.");
            }

            if (FindAnyObjectByType<FactoryHUD>(FindObjectsInactive.Exclude) == null)
            {
                gameObject.AddComponent<FactoryHUD>();
                Debug.Log("[GameBootstrap] Added missing FactoryHUD at runtime.");
            }

            if (FindAnyObjectByType<MaterialsRecipesUI>(FindObjectsInactive.Exclude) == null)
            {
                gameObject.AddComponent<MaterialsRecipesUI>();
                Debug.Log("[GameBootstrap] Added missing MaterialsRecipesUI at runtime.");
            }

            // Settings UI
            if (FindAnyObjectByType<SettingsUI>(FindObjectsInactive.Exclude) == null)
            {
                var settings = gameObject.AddComponent<SettingsUI>();
                settings.BuildUI();
                Debug.Log("[GameBootstrap] Added missing SettingsUI at runtime.");
            }
            
            if (FindAnyObjectByType<InGameMenuManager>(FindObjectsInactive.Exclude) == null)
            {
                gameObject.AddComponent<InGameMenuManager>();
                Debug.Log("[GameBootstrap] Added missing InGameMenuManager at runtime.");
            }

            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
                // Try to add InputSystemUIInputModule first (new input system), then StandaloneInputModule (old)
                if (System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem") != null)
                {
                    eventSystem.AddComponent(System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"));
                }
                else
                {
                    eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
                Debug.Log("[GameBootstrap] Added missing EventSystem at runtime.");
            }

            Debug.Log("Factory Prototype Initialized.");
            Debug.Log("Controls: [Tab] Shop, [P] Recipes, [Esc] Menu, [B] Build Mode.");
        }
    }
}
