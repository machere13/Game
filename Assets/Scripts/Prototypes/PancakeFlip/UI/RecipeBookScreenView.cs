using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class RecipeBookScreenView : MonoBehaviour
    {
        [SerializeField] Transform recipeListContainer;
        [SerializeField] TextMeshProUGUI bodyText;
        [SerializeField] Button closeButton;
        bool _runtimeUi;
        Transform _container;

        const float PancakeIconSize = 140f;
        const float IngredientIconSize = 56f;

        void Start()
        {
            if (_runtimeUi) return;
            if (closeButton != null)
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Rebuild();
        }

        Transform ResolveContainer()
        {
            if (_container != null) return _container;
            if (recipeListContainer != null) _container = recipeListContainer;
            else if (bodyText != null && bodyText.transform.parent != null) _container = bodyText.transform.parent;
            return _container;
        }

        void Rebuild()
        {
            var container = ResolveContainer();
            if (container == null) return;

            foreach (Transform child in container)
                Destroy(child.gameObject);

            var s = GameSession.Instance;
            if (s == null) return;

            var list = s.RecipeCatalog;
            if (list == null || list.Length == 0) return;

            foreach (var r in list)
            {
                if (r == null) continue;
                if (s.BaseRecipe != null && ReferenceEquals(r, s.BaseRecipe)) continue;
                BuildRecipeRow(container, r);
            }
        }

        static void BuildRecipeRow(Transform parent, RecipeConfig recipe)
        {
            var font = PancakeFlipUiFonts.UiTmpFont;

            var row = new GameObject("RecipeRow", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowBg = row.GetComponent<Image>();
            rowBg.raycastTarget = false;
            var rowSpot = GameSession.Instance != null ? GameSession.Instance.RecipeHudSpotSprite : null;
            if (rowSpot != null) { rowBg.sprite = rowSpot; rowBg.type = Image.Type.Sliced; rowBg.color = Color.white; }
            else rowBg.color = new Color(1f, 0.96f, 0.86f, 0.6f);
            var rowH = row.GetComponent<HorizontalLayoutGroup>();
            rowH.spacing = 28f;
            rowH.padding = new RectOffset(36, 16, 10, 10);
            rowH.childAlignment = TextAnchor.MiddleLeft;
            rowH.childControlWidth = true;
            rowH.childControlHeight = true;
            rowH.childForceExpandWidth = false;
            rowH.childForceExpandHeight = false;
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = PancakeIconSize + 24f;
            rowLe.preferredHeight = PancakeIconSize + 24f;

            var iconGo = new GameObject("PancakeIcon", typeof(RectTransform), typeof(LayoutElement));
            iconGo.transform.SetParent(row.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = recipe.icon;
            iconImg.enabled = recipe.icon != null;
            iconImg.preserveAspect = true; // блин по центру квадратной ячейки
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;
            var iconLe = iconGo.GetComponent<LayoutElement>();
            iconLe.minWidth = PancakeIconSize;
            iconLe.preferredWidth = PancakeIconSize;
            iconLe.flexibleWidth = 0f;
            iconLe.minHeight = PancakeIconSize;
            iconLe.preferredHeight = PancakeIconSize;
            iconLe.flexibleHeight = 0f;

            var infoGo = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            infoGo.transform.SetParent(row.transform, false);
            var infoV = infoGo.GetComponent<VerticalLayoutGroup>();
            infoV.spacing = 8f;
            infoV.childAlignment = TextAnchor.UpperLeft;
            infoV.childControlWidth = true;
            infoV.childControlHeight = true;
            infoV.childForceExpandWidth = true;
            infoV.childForceExpandHeight = false;
            var infoLe = infoGo.GetComponent<LayoutElement>();
            infoLe.flexibleWidth = 1f;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(infoGo.transform, false);
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            if (font != null) title.font = font;
            title.fontSize = PancakeFlipUiTypography.ModalBody;
            title.color = new Color(0.18f, 0.12f, 0.08f);
            title.alignment = TextAlignmentOptions.TopLeft;
            title.enableWordWrapping = false;
            title.raycastTarget = false;
            title.text = recipe.displayName;

            var ingredientsGo = new GameObject("Ingredients", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            ingredientsGo.transform.SetParent(infoGo.transform, false);
            var ingH = ingredientsGo.GetComponent<HorizontalLayoutGroup>();
            ingH.spacing = 10f;
            ingH.childAlignment = TextAnchor.MiddleLeft;
            ingH.childControlWidth = true;
            ingH.childControlHeight = true;
            ingH.childForceExpandWidth = false;
            ingH.childForceExpandHeight = false;
            var ingLe = ingredientsGo.GetComponent<LayoutElement>();
            ingLe.minHeight = IngredientIconSize + 4f;
            ingLe.preferredHeight = IngredientIconSize + 4f;

            if (recipe.ingredients != null)
            {
                foreach (var slot in recipe.ingredients)
                {
                    if (slot.ingredient == null) continue;
                    BuildIngredientPill(ingredientsGo.transform, slot.ingredient, slot.amount, font);
                }
            }
        }

        static void BuildIngredientPill(Transform parent, IngredientConfig ing, int amount, TMP_FontAsset font)
        {
            var pill = new GameObject("Pill", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            pill.transform.SetParent(parent, false);
            var ph = pill.GetComponent<HorizontalLayoutGroup>();
            ph.spacing = 4f;
            ph.childAlignment = TextAnchor.MiddleLeft;
            ph.childControlWidth = true;
            ph.childControlHeight = true;
            ph.childForceExpandWidth = false;
            ph.childForceExpandHeight = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(pill.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = ing.icon;
            iconImg.enabled = ing.icon != null;
            iconImg.preserveAspect = true;
            iconImg.color = Color.white;
            iconImg.raycastTarget = false;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.minWidth = IngredientIconSize;
            iconLe.preferredWidth = IngredientIconSize;
            iconLe.flexibleWidth = 0f;
            iconLe.minHeight = IngredientIconSize;
            iconLe.preferredHeight = IngredientIconSize;
            iconLe.flexibleHeight = 0f;

            var labelGo = new GameObject("Amount", typeof(RectTransform));
            labelGo.transform.SetParent(pill.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.fontSize = PancakeFlipUiTypography.ModalBody;
            label.color = new Color(0.2f, 0.16f, 0.12f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.text = "x" + amount;
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.minWidth = 40f;
            labelLe.preferredWidth = 40f;
        }

        public static RecipeBookScreenView EnsureUnderCanvas(Canvas canvas)
        {
            if (canvas == null) return null;

            var existing = canvas.GetComponentInChildren<RecipeBookScreenView>(true);
            if (existing != null && (existing.recipeListContainer != null || existing.bodyText != null) && existing.closeButton != null)
                return existing;

            if (existing != null)
                Object.Destroy(existing.gameObject);

            var font = PancakeFlipUiFonts.UiTmpFont;

            var root = new GameObject("RecipeBookScreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(canvas.transform, false);
            var rt = root.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.055f, 0.1f);
            rt.anchorMax = new Vector2(0.945f, 0.92f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0.96f, 0.94f, 0.89f, 0.99f);
            bg.raycastTarget = true;

            var modalCanvas = root.AddComponent<Canvas>();
            modalCanvas.overrideSorting = true;
            modalCanvas.sortingOrder = 100;
            root.AddComponent<GraphicRaycaster>();

            var header = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            header.transform.SetParent(root.transform, false);
            var hrt = header.GetComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 0.86f);
            hrt.anchorMax = new Vector2(1f, 1f);
            hrt.offsetMin = hrt.offsetMax = Vector2.zero;
            var hi = header.GetComponent<Image>();
            hi.color = new Color(0.32f, 0.46f, 0.4f, 1f);

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(header.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = titleRt.offsetMax = Vector2.zero;
            var titleT = titleGo.AddComponent<TextMeshProUGUI>();
            if (font != null) titleT.font = font;
            titleT.fontSize = PancakeFlipUiTypography.ModalHeaderTitle;
            titleT.color = Color.white;
            titleT.alignment = TextAlignmentOptions.Center;
            titleT.text = "Рецепты";
            titleT.raycastTarget = false;

            var listGo = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            listGo.transform.SetParent(root.transform, false);
            var listRt = listGo.GetComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0.04f, 0.14f);
            listRt.anchorMax = new Vector2(0.96f, 0.84f);
            listRt.offsetMin = listRt.offsetMax = Vector2.zero;
            var listV = listGo.GetComponent<VerticalLayoutGroup>();
            listV.spacing = 10f;
            listV.padding = new RectOffset(4, 4, 4, 8);
            listV.childAlignment = TextAnchor.UpperCenter;
            listV.childControlWidth = true;
            listV.childControlHeight = true;
            listV.childForceExpandWidth = true;
            listV.childForceExpandHeight = false;
            var listCsf = listGo.GetComponent<ContentSizeFitter>();
            listCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            listCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var closeRoot = new GameObject("CloseBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeRoot.transform.SetParent(root.transform, false);
            var crt = closeRoot.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.28f, 0.02f);
            crt.anchorMax = new Vector2(0.72f, 0.1f);
            crt.offsetMin = crt.offsetMax = Vector2.zero;
            var cimg = closeRoot.GetComponent<Image>();
            cimg.color = new Color(0.52f, 0.3f, 0.24f, 1f);
            var cbtn = closeRoot.GetComponent<Button>();
            cbtn.targetGraphic = cimg;

            var closeTxtGo = new GameObject("Text", typeof(RectTransform));
            closeTxtGo.transform.SetParent(closeRoot.transform, false);
            var ctxtRt = closeTxtGo.GetComponent<RectTransform>();
            ctxtRt.anchorMin = Vector2.zero;
            ctxtRt.anchorMax = Vector2.one;
            ctxtRt.offsetMin = ctxtRt.offsetMax = Vector2.zero;
            var ctxt = closeTxtGo.AddComponent<TextMeshProUGUI>();
            if (font != null) ctxt.font = font;
            ctxt.fontSize = PancakeFlipUiTypography.PrimaryButtonLabel;
            ctxt.color = Color.white;
            ctxt.alignment = TextAlignmentOptions.Center;
            ctxt.text = "Закрыть";
            ctxt.raycastTarget = false;

            var rb = root.AddComponent<RecipeBookScreenView>();
            rb._runtimeUi = true;
            rb.recipeListContainer = listGo.transform;
            rb.closeButton = cbtn;
            cbtn.onClick.AddListener(() => rb.gameObject.SetActive(false));
            rb.gameObject.SetActive(false);
            return rb;
        }
    }
}
