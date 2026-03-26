using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Factory.Factory;
using Factory.Core;
using System.Collections.Generic;

namespace Factory.UI
{
    /// <summary>
    /// Self-contained machine interaction panel. Opens when clicking a placed Machine.
    /// Builds its own Canvas panel at runtime — no external prefab references required.
    /// </summary>
    public class MachineInteractUI : MonoBehaviour
    {
        public static MachineInteractUI Instance { get; private set; }

        // ── Runtime-built UI elements ──────────────────────────────
        private GameObject panel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI statusText;
        private Transform recipeScrollContent;
        private GameObject recipeButtonPrefab;

        private Machine currentMachine;
        private List<Recipe> availableRecipes = new List<Recipe>();

        // Colors
        private static readonly Color PanelBg    = new Color(0.10f, 0.11f, 0.14f, 0.97f);
        private static readonly Color HeaderBg   = new Color(0.16f, 0.17f, 0.22f, 1f);
        private static readonly Color BtnNormal  = new Color(0.22f, 0.23f, 0.30f, 1f);
        private static readonly Color BtnHover   = new Color(0.32f, 0.55f, 0.90f, 1f);
        private static readonly Color InputColor = new Color(0.30f, 0.73f, 0.45f, 1f);
        private static readonly Color OutputColor= new Color(0.90f, 0.55f, 0.20f, 1f);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            BuildPanel();
            ClosePanel();
        }

        private void Update()
        {
            // Close on Escape / Right-click
            if (panel != null && panel.activeSelf)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
                {
                    ClosePanel();
                }
            }
        }

        // ─────────────────────────── Public API ───────────────────
        public void OpenForMachine(Machine m)
        {
            currentMachine = m;
            LoadRecipesForMachine(m);
            RefreshUI();
            panel.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayOpenUI();
        }

        public void ClosePanel()
        {
            currentMachine = null;
            if (panel != null) panel.SetActive(false);
        }

        // ─────────────────────────── UI Build ─────────────────────
        private void BuildPanel()
        {
            // Find or create Canvas
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject cgo = new GameObject("UICanvas",
                    typeof(RectTransform), typeof(Canvas),
                    typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = cgo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            // Root panel (right side, 380×580)
            panel = MakeRect("MachineInteractPanel", canvas.transform);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(1, 0.5f);
            prt.anchorMax = new Vector2(1, 0.5f);
            prt.pivot     = new Vector2(1, 0.5f);
            prt.anchoredPosition = new Vector2(-20, 0);
            prt.sizeDelta = new Vector2(380, 580);
            AddImage(panel, PanelBg, true, 12f);

            // Header
            GameObject header = MakeRect("Header", panel.transform);
            SetAnchored(header, 0, 1, 1, 1, 0, -50, 50, 0);
            AddImage(header, HeaderBg, true, 10f);

            titleText = MakeTMP("TitleText", header.transform, 20, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, Color.white);
            var trt = titleText.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(16, 0); trt.offsetMax = new Vector2(-50, 0);

            // Close button [X]
            GameObject closeBtn = MakeRect("CloseBtn", header.transform);
            SetAnchored(closeBtn, 1, 0, 1, 1, -44, 6, -6, -6);
            AddImage(closeBtn, new Color(0.8f, 0.2f, 0.2f, 1f), true, 6f);
            Button closeBtnComp = closeBtn.AddComponent<Button>();
            var closeText = MakeTMP("X", closeBtn.transform, 16, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            closeText.text = "×";
            stretch(closeText.GetComponent<RectTransform>());
            closeBtnComp.onClick.AddListener(ClosePanel);

            // Status label
            statusText = MakeTMP("StatusText", panel.transform, 13, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
                new Color(0.6f, 0.6f, 0.7f));
            var srt = statusText.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 1); srt.anchorMax = new Vector2(1, 1);
            srt.offsetMin = new Vector2(16, -90); srt.offsetMax = new Vector2(-16, -55);

            // Divider label "SELECT RECIPE"
            var divLabel = MakeTMP("DivLabel", panel.transform, 11, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, new Color(0.45f, 0.45f, 0.55f));
            divLabel.text = "SELECT RECIPE";
            var drt = divLabel.GetComponent<RectTransform>();
            drt.anchorMin = new Vector2(0, 1); drt.anchorMax = new Vector2(1, 1);
            drt.offsetMin = new Vector2(16, -115); drt.offsetMax = new Vector2(-16, -95);

            // Scroll view for recipes
            GameObject scrollObj = MakeRect("RecipeScroll", panel.transform);
            var sscrt = scrollObj.GetComponent<RectTransform>();
            sscrt.anchorMin = new Vector2(0, 0); sscrt.anchorMax = new Vector2(1, 1);
            sscrt.offsetMin = new Vector2(12, 12); sscrt.offsetMax = new Vector2(-12, -120);
            AddImage(scrollObj, new Color(0.07f, 0.08f, 0.10f, 0.6f), true, 8f);

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;

            GameObject viewport = MakeRect("Viewport", scrollObj.transform);
            stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            sr.viewport = viewport.GetComponent<RectTransform>();

            GameObject content = MakeRect("Content", viewport.transform);
            var crt2 = content.GetComponent<RectTransform>();
            crt2.anchorMin = new Vector2(0, 1); crt2.anchorMax = new Vector2(1, 1);
            crt2.pivot = new Vector2(0.5f, 1);
            crt2.offsetMin = Vector2.zero; crt2.offsetMax = Vector2.zero;
            recipeScrollContent = content.transform;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8; vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = crt2;

            // Build the recipe button prefab (not a real prefab, just a template GO)
            recipeButtonPrefab = BuildRecipeButtonTemplate();
        }

        // ─────────────────────────── Recipe Template ──────────────
        private GameObject BuildRecipeButtonTemplate()
        {
            // We clone this for each recipe entry
            GameObject t = MakeRect("RecipeBtnTemplate", null);
            LayoutElement le = t.AddComponent<LayoutElement>();
            le.minHeight = 68;

            AddImage(t, BtnNormal, true, 8f);

            Button btn = t.AddComponent<Button>();
            var cs = btn.colors;
            cs.normalColor      = BtnNormal;
            cs.highlightedColor = BtnHover;
            cs.pressedColor     = new Color(0.18f, 0.18f, 0.25f);
            cs.selectedColor    = BtnHover;
            btn.colors = cs;

            // Recipe name
            TextMeshProUGUI nameTmp = MakeTMP("RecipeName", t.transform, 15, FontStyles.Bold,
                TextAlignmentOptions.TopLeft, Color.white);
            var nrt = nameTmp.GetComponent<RectTransform>();
            nrt.anchorMin = new Vector2(0, 1); nrt.anchorMax = new Vector2(1, 1);
            nrt.offsetMin = new Vector2(12, -28); nrt.offsetMax = new Vector2(-12, 0);

            // Inputs label
            TextMeshProUGUI inTmp = MakeTMP("Inputs", t.transform, 11, FontStyles.Normal,
                TextAlignmentOptions.TopLeft, InputColor);
            var irt = inTmp.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0, 0); irt.anchorMax = new Vector2(0.5f, 1);
            irt.offsetMin = new Vector2(12, 6); irt.offsetMax = new Vector2(-4, -30);

            // Outputs label
            TextMeshProUGUI outTmp = MakeTMP("Outputs", t.transform, 11, FontStyles.Normal,
                TextAlignmentOptions.TopLeft, OutputColor);
            var ort = outTmp.GetComponent<RectTransform>();
            ort.anchorMin = new Vector2(0.5f, 0); ort.anchorMax = new Vector2(1, 1);
            ort.offsetMin = new Vector2(4, 6); ort.offsetMax = new Vector2(-12, -30);

            t.SetActive(false); // hide template
            return t;
        }

        // ─────────────────────────── Data refresh ─────────────────
        private void LoadRecipesForMachine(Machine m)
        {
            availableRecipes.Clear();
            // Use Resources.LoadAll for more reliable loading
            var all = Resources.LoadAll<Recipe>("Factory/Recipes");
            
            bool isForge = m.gameObject.name.ToLower().Contains("forge") || m.gameObject.name.ToLower().Contains("smelter");

            foreach (var r in all)
            {
                if (isForge)
                {
                    // Only show smelting recipes for the forge
                    if (r.name.ToLower().Contains("smelt"))
                        availableRecipes.Add(r);
                    continue;
                }

                // MachineProcessor accepts all recipes; typed machines are filtered by input count
                if (m is Constructor && r.inputs.Count != 1) continue;
                if (m is Assembler   && r.inputs.Count != 2) continue;
                
                // Don't show smelting recipes in normal constructors/assemblers if we have a specialized machine
                if (r.name.ToLower().Contains("smelt")) continue;

                availableRecipes.Add(r);
            }
        }

        private void RefreshUI()
        {
            if (currentMachine == null) return;

            titleText.text = currentMachine.gameObject.name;

            // Status line
            if (currentMachine.CurrentRecipe != null)
                statusText.text = $"⚙ Recipe: {currentMachine.CurrentRecipe.name}  |  {currentMachine.InputInventory.Count} inputs buffered";
            else
                statusText.text = "No recipe assigned — choose one below.";

            // Clear old recipe entries
            foreach (Transform child in recipeScrollContent)
                if (child.gameObject != recipeButtonPrefab) Destroy(child.gameObject);

            // Populate
            if (availableRecipes.Count == 0)
            {
                var noRecipe = MakeTMP("NoRecipe", recipeScrollContent, 13, FontStyles.Normal,
                    TextAlignmentOptions.Center, new Color(0.6f, 0.4f, 0.4f));
                noRecipe.text = "No compatible recipes found.\n(Put Recipe SOs in a Resources folder)";
                LayoutElement le2 = noRecipe.gameObject.AddComponent<LayoutElement>();
                le2.minHeight = 60;
                return;
            }

            foreach (var recipe in availableRecipes)
            {
                GameObject entry = Instantiate(recipeButtonPrefab, recipeScrollContent);
                entry.SetActive(true);

                // Name
                entry.transform.Find("RecipeName").GetComponent<TextMeshProUGUI>().text = recipe.name;

                // Inputs
                string inStr = "▶ IN:\n";
                foreach (var s in recipe.inputs) inStr += $"  {s.amount}× {(s.item ? s.item.itemName : "?")} \n";
                entry.transform.Find("Inputs").GetComponent<TextMeshProUGUI>().text = inStr.TrimEnd();

                // Outputs
                string outStr = "▶ OUT:\n";
                foreach (var s in recipe.outputs) outStr += $"  {s.amount}× {(s.item ? s.item.itemName : "?")} \n";
                entry.transform.Find("Outputs").GetComponent<TextMeshProUGUI>().text = outStr.TrimEnd();

                // On click: assign recipe to any machine type
                Recipe capturedRecipe = recipe;
                Machine capturedMachine = currentMachine;
                entry.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (capturedMachine is Constructor c)         c.SetRecipe(capturedRecipe);
                    else if (capturedMachine is Assembler a)      a.SetRecipe(capturedRecipe);
                    else if (capturedMachine is MachineProcessor p) p.SetRecipe(capturedRecipe);
                    else capturedMachine.CurrentRecipe = capturedRecipe; // fallback for any Machine
                    RefreshUI();
                });
            }
        }

        // ─────────────────────────── Helpers ──────────────────────
        private static GameObject MakeRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }

        private static void AddImage(GameObject go, Color color, bool raycastTarget = true, float cornerRadius = 0f)
        {
            Image img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycastTarget;
        }

        private static TextMeshProUGUI MakeTMP(string name, Transform parent, float size,
            FontStyles style, TextAlignmentOptions align, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static void SetAnchored(GameObject go, float axMin, float ayMin, float axMax, float ayMax,
            float ox1, float oy1, float ox2, float oy2)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(axMin, ayMin);
            rt.anchorMax = new Vector2(axMax, ayMax);
            rt.offsetMin = new Vector2(ox1, oy1);
            rt.offsetMax = new Vector2(ox2, oy2);
        }

        private static void stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
    }
}
