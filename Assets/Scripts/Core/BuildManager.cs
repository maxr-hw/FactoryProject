using UnityEngine;
using UnityEngine.InputSystem;
using Factory.Factory;
using System.Collections.Generic;
using System.Linq;
using Factory.Economy;

namespace Factory.Core
{
    public class BuildManager : MonoBehaviour
    {
        public static BuildManager Instance { get; private set; }

        [SerializeField] private LayerMask terrainLayer;
        [SerializeField] private Material ghostValidMaterial;
        [SerializeField] private Material ghostInvalidMaterial;
        [SerializeField] private Material deleteHighlightMaterial;

        public enum BuildMode { None, Placement, Delete }
        private BuildMode currentMode = BuildMode.None;

        // References to all placeable definitions (can be populated via Inspector or loaded)
        public List<MachineDefinition> machineCatalog;

        private GameObject currentGhost;
        private MachineDefinition currentDefinition;
        private Direction currentDirection = Direction.North;
        private bool isPlacementValid;

        /// <summary>True while a ghost building is being positioned for placement.</summary>
        public bool HasActiveGhost => currentGhost != null;
        public BuildMode CurrentMode => currentMode;

        private FactoryBuilding hoveredDeleteBuilding;
        private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

        // How close (world units) a ghost connection point must be to snap
        private const float SnapRange = 1.5f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            EnsureGhostMaterials();
        }

        /// <summary>
        /// Creates simple semi-transparent fallback materials at runtime if none
        /// are wired in the Inspector. This means the feature works out-of-the-box.
        /// </summary>
        private void EnsureGhostMaterials()
        {
            if (ghostValidMaterial == null)
            {
                ghostValidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                SetupTransparentMaterial(ghostValidMaterial, new Color(0.2f, 1f, 0.2f, 0.35f));
            }
            if (ghostInvalidMaterial == null)
            {
                ghostInvalidMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                SetupTransparentMaterial(ghostInvalidMaterial, new Color(1f, 0.15f, 0.15f, 0.45f));
            }
            if (deleteHighlightMaterial == null)
            {
                deleteHighlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                SetupTransparentMaterial(deleteHighlightMaterial, new Color(1f, 0.5f, 0f, 0.5f)); // Orange highlight
            }
        }

        private static void SetupTransparentMaterial(Material mat, Color color)
        {
            mat.SetFloat("_Surface",  1);
            mat.SetFloat("_Mode",     3);
            mat.SetInt("_SrcBlend",   (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend",   (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite",     0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.color = color;
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color",     color);
        }

        public void SetSelectedBuilding(MachineDefinition definition)
        {
            if (currentGhost != null) Destroy(currentGhost);

            currentDefinition = definition;
            if (currentDefinition != null && currentDefinition.prefab != null)
            {
                currentMode = BuildMode.Placement;
                currentGhost = Instantiate(currentDefinition.prefab);
                currentDirection = Direction.North;
                // Start with ghost assuming invalid until first frame updates it
                isPlacementValid = false;
                PrepareGhost();
            }
            else if (currentDefinition != null)
            {
                Debug.LogWarning($"[BuildManager] Cannot spawn ghost: Prefab is null on definition {currentDefinition.machineName}");
            }
        }

        /// <summary>
        /// Disables all colliders on the ghost and applies the initial ghost material.
        /// Called once when a ghost is first spawned.
        /// </summary>
        private void PrepareGhost()
        {
            // Disable colliders so the ghost doesn't interfere with physics/raycasts
            foreach (var col in currentGhost.GetComponentsInChildren<Collider>(true))
                col.enabled = false;

            // Set it to its own layer so terrain raycast still works
            currentGhost.layer = LayerMask.NameToLayer("Ignore Raycast");
            foreach (Transform child in currentGhost.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            ApplyGhostMaterial(ghostInvalidMaterial);
        }

        private void Update()
        {
            HandleHotkeys();

            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                ToggleDeleteMode();
            }

            if (currentMode == BuildMode.Placement && currentGhost != null)
            {
                UpdateGhostPosition();

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    // Ignore clicks on UI elements (like shop buttons)
                    if (UnityEngine.EventSystems.EventSystem.current != null && 
                        UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                    {
                        return;
                    }

                    TryPlaceBuilding();
                }

                if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    CancelPlacement();
                }

                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    RotateGhost();
                }
            }
            else if (currentMode == BuildMode.Delete)
            {
                UpdateDeleteHighlight();

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    if (hoveredDeleteBuilding != null)
                    {
                        ConfirmDeleteBuilding(hoveredDeleteBuilding);
                    }
                }

                if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    ExitDeleteMode();
                }
            }
        }

        private void ToggleDeleteMode()
        {
            if (currentMode == BuildMode.Delete)
            {
                ExitDeleteMode();
            }
            else
            {
                CancelPlacement();
                currentMode = BuildMode.Delete;
            }
        }

        private void ExitDeleteMode()
        {
            ClearDeleteHighlight();
            currentMode = BuildMode.None;
        }

        private void UpdateDeleteHighlight()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Interactable")))
            {
                FactoryBuilding building = hit.collider.GetComponentInParent<FactoryBuilding>();
                if (building != hoveredDeleteBuilding)
                {
                    ClearDeleteHighlight();
                    hoveredDeleteBuilding = building;
                    if (hoveredDeleteBuilding != null)
                    {
                        ApplyDeleteHighlight(hoveredDeleteBuilding);
                    }
                }
            }
            else
            {
                ClearDeleteHighlight();
            }
        }

        private void ApplyDeleteHighlight(FactoryBuilding building)
        {
            originalMaterials.Clear();
            foreach (var renderer in building.GetComponentsInChildren<Renderer>())
            {
                originalMaterials[renderer] = renderer.sharedMaterials;
                Material[] highlighted = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < highlighted.Length; i++) highlighted[i] = deleteHighlightMaterial;
                renderer.sharedMaterials = highlighted;
            }
        }

        private void ClearDeleteHighlight()
        {
            if (hoveredDeleteBuilding != null)
            {
                foreach (var kvp in originalMaterials)
                {
                    if (kvp.Key != null) kvp.Key.sharedMaterials = kvp.Value;
                }
                originalMaterials.Clear();
                hoveredDeleteBuilding = null;
            }
        }

        private void ConfirmDeleteBuilding(FactoryBuilding building)
        {
            // Refund 50%
            int refund = Mathf.FloorToInt(building.cost * 0.5f);
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.Earn(refund);
            }

            // Remove from grid
            if (GridManager.Instance != null)
            {
                // We need to use the rotated size if it was rotated. 
                // Currently size in FactoryBuilding is the base size.
                // For now use base size, but ideally we'd track current rotated size.
                // Actually, most machines are square or 1x1. For the rest, we should check rotation.
                Vector2Int sizeToDelete = building.size;
                if (building.facingDirection == Direction.East || building.facingDirection == Direction.West)
                {
                    sizeToDelete = new Vector2Int(building.size.y, building.size.x);
                }
                GridManager.Instance.RemoveBuilding(building.gridPosition, sizeToDelete);
            }

            building.OnRemoved();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDelete();
            
            Destroy(building.gameObject);
            hoveredDeleteBuilding = null;
            originalMaterials.Clear();
        }

        private void HandleHotkeys()
        {
            if (machineCatalog == null || machineCatalog.Count == 0) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Conveyor);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Splitter);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Merger);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Constructor);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Assembler);
            if (Keyboard.current.digit6Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Storage);
            if (Keyboard.current.digit8Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Source);
            if (Keyboard.current.digit9Key.wasPressedThisFrame) SelectFromCatalog(MachineType.Delivery);
        }

        private void SelectFromCatalog(MachineType type)
        {
            var def = machineCatalog.Find(m => m.type == type);
            if (def != null)
            {
                SetSelectedBuilding(def);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
            }
        }

        private void RotateGhost()
        {
            currentDirection = (Direction)(((int)currentDirection + 1) % 4);
            currentGhost.transform.rotation = Quaternion.Euler(0, GetRotationAngle(currentDirection), 0);
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRotate();
            
            UpdateGhostVisuals(); // Re-check bounds since rotated size might matter
        }

        private float GetRotationAngle(Direction dir)
        {
            switch (dir)
            {
                case Direction.North: return 0;
                case Direction.East: return 90;
                case Direction.South: return 180;
                case Direction.West: return 270;
            }
            return 0;
        }

        private Vector2Int GetRotatedSize()
        {
            if (currentDefinition == null) return Vector2Int.one;
            if (currentDirection == Direction.East || currentDirection == Direction.West)
            {
                return new Vector2Int(currentDefinition.size.y, currentDefinition.size.x);
            }
            return currentDefinition.size;
        }

        private void UpdateGhostPosition()
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayer))
            {
                currentGhost.SetActive(true);
                Vector2Int size = GetRotatedSize();
                float cellSize = GridManager.Instance.CellSize;
                float offsetX = (Mathf.Floor(size.x * 0.5f) + 0.5f) * cellSize;
                float offsetZ = (Mathf.Floor(size.y * 0.5f) + 0.5f) * cellSize;
                Vector3 offset = new Vector3(offsetX, 0, offsetZ);
                
                Vector3 snapped = GridManager.Instance.GetNearestPointOnGrid(hit.point - offset);
                currentGhost.transform.position = snapped + offset;

                // Try to snap ghost connection points to nearby placed connection points
                Vector3 snapOffset = TrySnapToConnection();
                if (snapOffset != Vector3.zero)
                {
                    currentGhost.transform.position += snapOffset;
                    snapped = currentGhost.transform.position - offset;
                }

                // Track validity
                Vector2Int gridPos = GridManager.Instance.WorldToGridPos(snapped);
                bool occupied = GridManager.Instance.IsAreaOccupied(gridPos, size);
                bool affordable = EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(currentDefinition.cost);
                
                bool newValidity = !occupied && affordable;
                if (newValidity != isPlacementValid)
                {
                    isPlacementValid = newValidity;
                    ApplyGhostMaterial(isPlacementValid ? ghostValidMaterial : ghostInvalidMaterial);
                }
            }
            else
            {
                // If not hitting terrain, hide the ghost or mark invalid
                currentGhost.SetActive(false);
                isPlacementValid = false;
            }
        }

        /// <summary>
        /// Searches all placed ConnectionPoints in the scene for the closest
        /// match (ghost Output <-> placed Input, or ghost Input <-> placed Output).
        /// Returns the world-space delta needed to snap the ghost so the matched
        /// points coincide, or Vector3.zero if nothing is within SnapRange.
        /// </summary>
        private Vector3 TrySnapToConnection()
        {
            ConnectionPoint[] ghostPts = currentGhost.GetComponentsInChildren<ConnectionPoint>();
            if (ghostPts.Length == 0) return Vector3.zero;

            // All placed (non-ghost) ConnectionPoints
            ConnectionPoint[] allPlaced = Object.FindObjectsByType<ConnectionPoint>(FindObjectsSortMode.None)
                .Where(p => p.gameObject.scene.IsValid()             // scene object, not prefab
                         && !currentGhost.transform.IsChildOf(p.transform.root) // exclude ghost itself
                         && p.transform.root.GetComponent<FactoryBuilding>() != null) // only placed buildings
                .ToArray();

            float bestDist = SnapRange;
            Vector3 bestDelta = Vector3.zero;

            foreach (var ghostPt in ghostPts)
            {
                // Snap ghost OUTPUT -> placed INPUT, or ghost INPUT -> placed OUTPUT
                ConnectionPoint.PointType wantedType = ghostPt.pointType == ConnectionPoint.PointType.Output
                    ? ConnectionPoint.PointType.Input
                    : ConnectionPoint.PointType.Output;

                foreach (var placed in allPlaced)
                {
                    if (placed.pointType != wantedType) continue;

                    float dist = Vector3.Distance(ghostPt.transform.position, placed.transform.position);
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        // delta = how much we need to shift the ghost so ghostPt lands on placed
                        bestDelta = placed.transform.position - ghostPt.transform.position;
                    }
                }
            }

            return bestDelta;
        }

        private void TryPlaceBuilding()
        {
            if (!isPlacementValid) return;

            Vector2Int size = GetRotatedSize();
            float cellSize = GridManager.Instance.CellSize;
            Vector3 centerPos = currentGhost.transform.position;
            float offsetX = (Mathf.Floor(size.x * 0.5f) + 0.5f) * cellSize;
            float offsetZ = (Mathf.Floor(size.y * 0.5f) + 0.5f) * cellSize;
            Vector3 bottomLeftPos = centerPos - new Vector3(offsetX, 0, offsetZ);
            Vector2Int gridPos = GridManager.Instance.WorldToGridPos(bottomLeftPos);

            EconomyManager.Instance.Spend(currentDefinition.cost);
            GameObject placed = Instantiate(currentDefinition.prefab, currentGhost.transform.position, currentGhost.transform.rotation);
            placed.layer = LayerMask.NameToLayer("Interactable");
            
            FactoryBuilding building = placed.GetComponent<FactoryBuilding>();
            if (building != null)
            {
                building.gridPosition = gridPos;
                building.facingDirection = currentDirection;
                GridManager.Instance.RegisterBuilding(gridPos, size, building);
                building.OnPlaced();
                
                if (AudioManager.Instance != null) AudioManager.Instance.PlayPlace();
            }

            // Keep ghost active for multiple placements
            UpdateGhostVisuals();
        }

        private void CancelPlacement()
        {
            if (currentGhost != null) Destroy(currentGhost);
            currentDefinition = null;
            currentMode = BuildMode.None;
        }

        private void UpdateGhostVisuals()
        {
            if (currentGhost == null || !currentGhost.activeSelf) return;

            ApplyGhostMaterial(isPlacementValid ? ghostValidMaterial : ghostInvalidMaterial);
            foreach (var col in currentGhost.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
            }
        }

        private void ApplyGhostMaterial(Material mat)
        {
            if (currentGhost == null || mat == null) return;
            foreach (var renderer in currentGhost.GetComponentsInChildren<Renderer>())
            {
                renderer.material = mat;
            }
        }
    }
}
