using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Factory.Core;
using Factory.Factory;
using System.Collections.Generic;

namespace Factory.UI
{
    /// <summary>
    /// Self-contained UI that opens when clicking a SourceMachine (source).
    /// Lets the player pick which raw material/item the source will produce.
    /// </summary>
    public class SourceInteractUI : MonoBehaviour
    {
        public static SourceInteractUI Instance { get; private set; }

        private GameObject panel;
        private TextMeshProUGUI titleText;
        private TextMeshProUGUI currentItemText;
        private Transform itemScrollContent;
        private GameObject itemButtonTemplate;

        private SourceMachine currentSource;
        private List<ItemDefinition> allItems = new List<ItemDefinition>();

        // Colors
        private static readonly Color PanelBg   = new Color(0.08f, 0.10f, 0.12f, 0.97f);
        private static readonly Color HeaderBg   = new Color(0.14f, 0.16f, 0.20f, 1f);
        private static readonly Color BtnNormal  = new Color(0.18f, 0.20f, 0.26f, 1f);
        private static readonly Color BtnHover   = new Color(0.90f, 0.55f, 0.20f, 1f);
        private static readonly Color SelectedBg = new Color(0.90f, 0.55f, 0.20f, 1f);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            BuildPanel();
            ClosePanel();
        }

        private void Update()
        {
            if (panel != null && panel.activeSelf)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)
                    ClosePanel();
            }
        }

        // ─── Public API ───────────────────────────────────────────────
        public void OpenForSource(SourceMachine source)
        {
            currentSource = source;
            LoadItems();
            RefreshUI();
            panel.SetActive(true);
        }

        public void ClosePanel()
        {
            currentSource = null;
            if (panel != null) panel.SetActive(false);
        }

        // ─── Panel build ──────────────────────────────────────────────
        private void BuildPanel()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject cgo = new GameObject("UICanvas",
                    typeof(RectTransform), typeof(Canvas),
                    typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = cgo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            // Panel (left side, 340×520)
            panel = MakeRect("SourceInteractPanel", canvas.transform);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0, 0.5f);
            prt.anchorMax = new Vector2(0, 0.5f);
            prt.pivot = new Vector2(0, 0.5f);
            prt.anchoredPosition = new Vector2(20, 0);
            prt.sizeDelta = new Vector2(340, 520);
            AddImage(panel, PanelBg);

            // Header
            GameObject header = MakeRect("Header", panel.transform);
            RectTransform hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0, 1); hrt.anchorMax = new Vector2(1, 1);
            hrt.pivot = new Vector2(0.5f, 1);
            hrt.offsetMin = new Vector2(0, -48); hrt.offsetMax = Vector2.zero;
            AddImage(header, HeaderBg);

            titleText = MakeTMP("Title", header.transform, 16, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, Color.white);
            Stretch(titleText.rectTransform, 14, 0, -44, 0);

            // Close [×] button
            GameObject closeBtn = MakeRect("CloseBtn", header.transform);
            RectTransform crt = closeBtn.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(1, 0); crt.anchorMax = new Vector2(1, 1);
            crt.offsetMin = new Vector2(-42, 6); crt.offsetMax = new Vector2(-6, -6);
            AddImage(closeBtn, new Color(0.75f, 0.2f, 0.2f));
            Button closeBtnComp = closeBtn.AddComponent<Button>();
            var xt = MakeTMP("X", closeBtn.transform, 16, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            xt.text = "×";
            Stretch(xt.rectTransform);
            closeBtnComp.onClick.AddListener(ClosePanel);

            // Current item info
            currentItemText = MakeTMP("CurrentItem", panel.transform, 12, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, new Color(0.6f, 0.7f, 0.6f));
            var cur = currentItemText.rectTransform;
            cur.anchorMin = new Vector2(0, 1); cur.anchorMax = new Vector2(1, 1);
            cur.offsetMin = new Vector2(14, -82); cur.offsetMax = new Vector2(-14, -52);

            // Divider label
            var div = MakeTMP("Div", panel.transform, 10, FontStyles.Bold,
                TextAlignmentOptions.MidlineLeft, new Color(0.4f, 0.4f, 0.5f));
            div.text = "SELECT OUTPUT ITEM";
            var drt = div.rectTransform;
            drt.anchorMin = new Vector2(0, 1); drt.anchorMax = new Vector2(1, 1);
            drt.offsetMin = new Vector2(14, -104); drt.offsetMax = new Vector2(-14, -86);

            // Scroll view
            GameObject scrollObj = MakeRect("Scroll", panel.transform);
            RectTransform srt = scrollObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0, 0); srt.anchorMax = new Vector2(1, 1);
            srt.offsetMin = new Vector2(10, 10); srt.offsetMax = new Vector2(-10, -108);
            AddImage(scrollObj, new Color(0.05f, 0.06f, 0.08f, 0.6f));

            ScrollRect sr = scrollObj.AddComponent<ScrollRect>();
            sr.horizontal = false;

            GameObject viewport = MakeRect("Viewport", scrollObj.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            viewport.AddComponent<RectMask2D>();
            sr.viewport = viewport.GetComponent<RectTransform>();

            GameObject content = MakeRect("Content", viewport.transform);
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1); contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.offsetMin = Vector2.zero; contentRt.offsetMax = Vector2.zero;
            itemScrollContent = content.transform;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6; vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            sr.content = contentRt;

            itemButtonTemplate = BuildItemButtonTemplate();
        }

        private GameObject BuildItemButtonTemplate()
        {
            GameObject t = MakeRect("ItemBtnTemplate", null);
            LayoutElement le = t.AddComponent<LayoutElement>();
            le.minHeight = 44;

            AddImage(t, BtnNormal);
            Button btn = t.AddComponent<Button>();
            var cs = btn.colors;
            cs.normalColor = BtnNormal;
            cs.highlightedColor = BtnHover;
            cs.pressedColor = new Color(0.6f, 0.3f, 0.05f);
            btn.colors = cs;

            // Color swatch
            GameObject swatch = MakeRect("Swatch", t.transform);
            RectTransform swrt = swatch.GetComponent<RectTransform>();
            swrt.anchorMin = new Vector2(0, 0.1f); swrt.anchorMax = new Vector2(0, 0.9f);
            swrt.offsetMin = new Vector2(10, 0); swrt.offsetMax = new Vector2(36, 0);
            swrt.sizeDelta = new Vector2(26, 0);
            AddImage(swatch, Color.white);

            // Item name
            TextMeshProUGUI nameTmp = MakeTMP("ItemName", t.transform, 13, FontStyles.Normal,
                TextAlignmentOptions.MidlineLeft, Color.white);
            var nrt = nameTmp.rectTransform;
            nrt.anchorMin = new Vector2(0, 0); nrt.anchorMax = new Vector2(1, 1);
            nrt.offsetMin = new Vector2(46, 4); nrt.offsetMax = new Vector2(-10, -4);

            t.SetActive(false);
            return t;
        }

        // ─── Data + Refresh ───────────────────────────────────────────
        private void LoadItems()
        {
            allItems.Clear();
            var allDecls = Resources.LoadAll<ItemDefinition>("Factory/Items");
            if (allDecls == null || allDecls.Length == 0) allDecls = Resources.FindObjectsOfTypeAll<ItemDefinition>();

            // Find all recipes to see what they produce
            var recipes = Resources.LoadAll<Recipe>("Factory/Recipes");
            if (recipes == null || recipes.Length == 0) recipes = Resources.FindObjectsOfTypeAll<Recipe>();

            HashSet<ItemDefinition> producedItems = new HashSet<ItemDefinition>();
            foreach (var r in recipes)
            {
                if (r.outputs == null) continue;
                foreach (var output in r.outputs)
                {
                    if (output.item != null) producedItems.Add(output.item);
                }
            }

            // Only add items that are NOT produced by any recipe
            foreach (var item in allDecls)
            {
                if (!producedItems.Contains(item))
                {
                    allItems.Add(item);
                }
            }
        }

        private void RefreshUI()
        {
            if (currentSource == null) return;
            titleText.text = $"⚙ {currentSource.gameObject.name}";
            currentItemText.text = currentSource.itemToSpawn != null
                ? $"Currently outputting: {currentSource.itemToSpawn.itemName}"
                : "No item assigned — choose one below.";

            foreach (Transform child in itemScrollContent)
                if (child.gameObject != itemButtonTemplate) Destroy(child.gameObject);

            if (allItems.Count == 0)
            {
                var noItem = MakeTMP("NoItems", itemScrollContent, 12, FontStyles.Normal,
                    TextAlignmentOptions.Center, new Color(0.6f, 0.4f, 0.4f));
                noItem.text = "No items found.\nRun Factory → Create All Items and Recipes first.";
                noItem.gameObject.AddComponent<LayoutElement>().minHeight = 60;
                return;
            }

            foreach (var item in allItems)
            {
                GameObject entry = Instantiate(itemButtonTemplate, itemScrollContent);
                entry.SetActive(true);

                // Item name
                entry.transform.Find("ItemName").GetComponent<TextMeshProUGUI>().text = item.itemName;

                // Color swatch — use same color logic as ConveyorItem
                Color col = item.itemColor != Color.clear
                    ? item.itemColor
                    : ConveyorItem.GetItemColor(item);
                entry.transform.Find("Swatch").GetComponent<Image>().color = col;

                // Highlight if currently selected
                bool isCurrent = currentSource.itemToSpawn == item;
                if (isCurrent)
                {
                    entry.GetComponent<Image>().color = new Color(0.25f, 0.50f, 0.25f, 1f);
                }

                ItemDefinition capturedItem = item;
                SourceMachine capturedSrc = currentSource;
                entry.GetComponent<Button>().onClick.AddListener(() =>
                {
                    capturedSrc.itemToSpawn = capturedItem;
                    RefreshUI(); // update highlight
                });
            }
        }

        // ─── Helpers ──────────────────────────────────────────────────
        private static GameObject MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            return go;
        }
        private static void AddImage(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
        }
        private static TextMeshProUGUI MakeTMP(string name, Transform parent, float size,
            FontStyles style, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null) go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = size; tmp.fontStyle = style;
            tmp.alignment = align; tmp.color = color;
            tmp.enableWordWrapping = false;
            return tmp;
        }
        private static void Stretch(RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(r, t);
        }
    }
}
