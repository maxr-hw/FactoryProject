using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Factory.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Factory.UI
{
    public class MaterialsRecipesUI : MonoBehaviour
    {
        public static MaterialsRecipesUI Instance { get; private set; }

        [Header("UI References")]
        private GameObject panel;
        private Transform leftColumnContent;
        private Transform rightColumnContent;
        private ItemDefinition selectedMaterial;
        
        private List<ItemDefinition> allItems = new List<ItemDefinition>();
        private List<Recipe> allRecipes = new List<Recipe>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            if (panel == null) BuildUI();
            panel.SetActive(false);
        }

        private void Update()
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && (keyboard.pKey.wasPressedThisFrame)) // Changed to P
            {
                Debug.Log("[MaterialsRecipesUI] Toggle key pressed (P)!");
                ToggleMenu();
            }
        }

        public void ToggleMenu()
        {
            if (panel == null) BuildUI();
            if (panel == null) { Debug.LogError("[MaterialsRecipesUI] Failed to BuildUI - No Canvas found?"); return; }
            
            bool isOpen = !panel.activeSelf;
            panel.SetActive(isOpen);
            if (isOpen) panel.transform.SetAsLastSibling();
            Debug.Log($"[MaterialsRecipesUI] Menu toggled: {isOpen}");

            if (isOpen)
            {
                RefreshData(); // Changed from RefreshLists()
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        private void BuildUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            panel = new GameObject("MaterialsRecipesMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var pRT = panel.GetComponent<RectTransform>();
            pRT.anchorMin = new Vector2(0.5f, 0.5f);
            pRT.anchorMax = new Vector2(0.5f, 0.5f);
            pRT.pivot = new Vector2(0.5f, 0.5f);
            pRT.anchoredPosition = Vector2.zero;
            pRT.sizeDelta = new Vector2(900, 600); // Wider for two columns
            panel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.98f);

            // Title
            GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(panel.transform, false);
            var tRT = titleObj.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0.5f, 1);
            tRT.anchorMax = new Vector2(0.5f, 1);
            tRT.anchoredPosition = new Vector2(0, -30);
            tRT.sizeDelta = new Vector2(400, 50);
            var titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "MATERIALS & RECIPES";
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Columns Container
            GameObject columnsObj = new GameObject("Columns", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            columnsObj.transform.SetParent(panel.transform, false);
            var cRT = columnsObj.GetComponent<RectTransform>();
            cRT.anchorMin = Vector2.zero;
            cRT.anchorMax = Vector2.one;
            cRT.offsetMin = new Vector2(20, 20);
            cRT.offsetMax = new Vector2(-20, -70); // Space for title

            var hlg = columnsObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // --- LEFT COLUMN (Materials) ---
            GameObject leftCol = CreateColumn("MaterialsList", columnsObj.transform, out leftColumnContent);
            
            // --- RIGHT COLUMN (Recipes) ---
            GameObject rightCol = CreateColumn("RecipesDetails", columnsObj.transform, out rightColumnContent);

            // Close hint
            GameObject hintObj = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintObj.transform.SetParent(panel.transform, false);
            var hint = hintObj.GetComponent<TextMeshProUGUI>();
            hint.text = "Press 'P' to Close"; // Updated hint
            hint.fontSize = 12;
            hint.alignment = TextAlignmentOptions.Center;
            hint.rectTransform.anchoredPosition = new Vector2(0, -235);

            panel.SetActive(false);
            RefreshData();
        }

        private GameObject CreateColumn(string name, Transform parent, out Transform content)
        {
            GameObject colGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            colGo.transform.SetParent(parent, false);
            colGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.3f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(colGo.transform, false);
            RectTransform vRT = viewport.GetComponent<RectTransform>();
            vRT.anchorMin = Vector2.zero;
            vRT.anchorMax = Vector2.one;
            vRT.sizeDelta = Vector2.zero;
            viewport.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);

            GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);
            content = contentGo.transform;
            RectTransform contRT = contentGo.GetComponent<RectTransform>();
            contRT.anchorMin = new Vector2(0, 1);
            contRT.anchorMax = new Vector2(1, 1);
            contRT.anchoredPosition = Vector2.zero;
            contRT.sizeDelta = new Vector2(0, 0);

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.spacing = 5;
            vlg.childControlHeight = true;
            vlg.childForceExpandHeight = false;

            var csf = contentGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = colGo.GetComponent<ScrollRect>();
            sr.content = contRT;
            sr.viewport = vRT;
            sr.horizontal = false;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

            // Scrollbar
            GameObject sbGo = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
            sbGo.transform.SetParent(colGo.transform, false);
            RectTransform sbRT = sbGo.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(1, 0);
            sbRT.anchorMax = new Vector2(1, 1);
            sbRT.pivot = new Vector2(1, 1);
            sbRT.sizeDelta = new Vector2(15, 0);
            sbGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            GameObject slidingArea = new GameObject("SlidingArea", typeof(RectTransform));
            slidingArea.transform.SetParent(sbGo.transform, false);
            RectTransform saRT = slidingArea.GetComponent<RectTransform>();
            saRT.anchorMin = Vector2.zero;
            saRT.anchorMax = Vector2.one;
            saRT.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(slidingArea.transform, false);
            RectTransform hRT = handle.GetComponent<RectTransform>();
            hRT.sizeDelta = Vector2.zero;
            handle.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);

            Scrollbar sb = sbGo.GetComponent<Scrollbar>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.handleRect = hRT;
            sr.verticalScrollbar = sb;

            return colGo;
        }

        private void RefreshData()
        {
            allItems = Resources.LoadAll<ItemDefinition>("Factory/Items").ToList();
            allRecipes = Resources.LoadAll<Recipe>("Factory/Recipes").ToList();
            
            PopulateMaterials();
            PopulateRecipes(selectedMaterial); // Use current filter
        }

        private void PopulateMaterials()
        {
            foreach (Transform child in leftColumnContent) Destroy(child.gameObject);

            // Add "All Recipes" button
            CreateMaterialButton(null, "SHOW ALL RECIPES");

            foreach (var item in allItems.OrderBy(i => i.itemName ?? i.name))
            {
                CreateMaterialButton(item, item.itemName);
            }
        }

        private void CreateMaterialButton(ItemDefinition item, string label)
        {
            GameObject btnGo = new GameObject(label + "Btn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            btnGo.transform.SetParent(leftColumnContent, false);
            btnGo.GetComponent<LayoutElement>().preferredHeight = 60;
            
            // Background Visuals
            Image bgImg = btnGo.GetComponent<Image>();
            bgImg.color = (item == selectedMaterial) ? new Color(0.15f, 0.35f, 0.65f, 0.9f) : new Color(0.25f, 0.25f, 0.25f, 0.7f);
            
            HorizontalLayoutGroup hlg = btnGo.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 10, 10);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObj.transform.SetParent(btnGo.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(40, 40);
            var i = iconObj.GetComponent<Image>();
            if (item != null && item.icon != null)
            {
                i.sprite = item.icon;
                i.color = Color.white;
            }
            else
            {
                // Fallback for "All Recipes" or missing icons
                i.color = (item == null) ? new Color(1, 0.8f, 0.2f, 0.8f) : (item.itemColor != Color.clear ? item.itemColor : new Color(0.5f, 0.5f, 0.5f, 0.8f));
            }

            // Label
            GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            var t = txtGo.GetComponent<TextMeshProUGUI>();
            t.text = label;
            t.fontSize = 18;
            t.alignment = TextAlignmentOptions.Left;
            t.fontStyle = (item == selectedMaterial) ? FontStyles.Bold : FontStyles.Normal;

            Button btn = btnGo.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1, 1, 1, 0.15f);
            cb.pressedColor = new Color(0.3f, 0.6f, 0.9f, 0.5f);
            btn.colors = cb;

            btn.onClick.AddListener(() => {
                selectedMaterial = item;
                PopulateMaterials(); // Refresh highlight
                PopulateRecipes(item);
            });
        }

        private void PopulateRecipes(ItemDefinition filter)
        {
            foreach (Transform child in rightColumnContent) Destroy(child.gameObject);

            var recipesToShow = (filter == null) 
                ? allRecipes 
                : allRecipes.Where(r => r.outputs != null && r.outputs.Any(o => o.item == filter)).ToList();

            if (recipesToShow.Count == 0)
            {
                CreateLabel(rightColumnContent, filter == null ? "No recipes found in project." : "No recipes found for this material.");
                return;
            }

            foreach (var recipe in recipesToShow.OrderBy(r => r.name)) // Added order by name
            {
                CreateRecipeEntry(recipe);
            }
        }

        private void CreateRecipeEntry(Recipe recipe)
        {
            GameObject entry = new GameObject("RecipeEntry", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            entry.transform.SetParent(rightColumnContent, false);
            entry.GetComponent<LayoutElement>().preferredHeight = 120; // Fixed height for recipe entries
            
            // Visuals
            Image img = entry.GetComponent<Image>();
            img.color = new Color(1, 1, 1, 0.08f);
            
            Button btn = entry.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(1, 1, 1, 0.08f);
            cb.highlightedColor = new Color(1, 1, 1, 0.15f);
            cb.pressedColor = new Color(1, 1, 1, 0.25f);
            btn.colors = cb;
            
            var vlg = entry.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 8;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            var title = CreateLabel(entry.transform, $"<color=#ffcc00>{recipe.name}</color> <color=#999999>({recipe.processingTime}s)</color>");
            title.fontSize = 18;
            title.fontStyle = FontStyles.Bold;

            StringBuilder sb = new StringBuilder();
            sb.Append("<color=#aaaaaa>Inputs: </color>");
            if (recipe.inputs == null || recipe.inputs.Count == 0) sb.Append("None");
            else sb.Append(string.Join(", ", recipe.inputs.Select(i => $"<color=#ffffff>{i.item?.itemName ?? "???"}</color> x{i.amount}")));

            sb.Append("\n<color=#aaaaaa>Outputs: </color>");
            if (recipe.outputs == null || recipe.outputs.Count == 0) sb.Append("None");
            else sb.Append(string.Join(", ", recipe.outputs.Select(o => $"<color=#00ff00>{o.item?.itemName ?? "???"}</color> x{o.amount}")));

            var stats = CreateLabel(entry.transform, sb.ToString());
            stats.fontSize = 14;
            stats.lineSpacing = 10;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text)
        {
            GameObject go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = 14;
            return t;
        }
    }
}
