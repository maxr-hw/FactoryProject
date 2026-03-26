using UnityEngine;
using UnityEngine.InputSystem;
using Factory.Factory;
using Factory.UI;

namespace Factory.Core
{
    public class SelectionManager : MonoBehaviour
    {
        private void Start()
        {
            // Auto-spawn UIs if missing
            if (MachineInteractUI.Instance == null)
                new GameObject("MachineInteractUI").AddComponent<MachineInteractUI>();

            if (SourceInteractUI.Instance == null)
                new GameObject("SourceInteractUI").AddComponent<SourceInteractUI>();
        }

        private void Update()
        {
            // Suppress clicks during placement
            if (BuildManager.Instance != null && BuildManager.Instance.HasActiveGhost) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            // Ignore clicks over UI elements
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Interactable")))
            {
                CloseAllPanels();
                return;
            }

            // SourceMachine (Source) → SourceInteractUI
            SourceMachine src = hit.collider.GetComponentInParent<SourceMachine>();
            if (src != null)
            {
                MachineInteractUI.Instance?.ClosePanel();
                SourceInteractUI.Instance?.OpenForSource(src);
                return;
            }

            // Any other Machine → MachineInteractUI
            Machine m = hit.collider.GetComponentInParent<Machine>();
            if (m != null)
            {
                SourceInteractUI.Instance?.ClosePanel();
                MachineInteractUI.Instance?.OpenForMachine(m);
                return;
            }

            CloseAllPanels();
        }

        private static void CloseAllPanels()
        {
            MachineInteractUI.Instance?.ClosePanel();
            SourceInteractUI.Instance?.ClosePanel();
        }
    }
}

