using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class RecipeBookScreenView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI bodyText;
        [SerializeField] Button closeButton;
        bool _runtimeUi;

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

        void Rebuild()
        {
            if (bodyText == null) return;
            var uiTmp = PancakeFlipUiFonts.UiTmpFont;
            if (uiTmp != null) bodyText.font = uiTmp;
            bodyText.fontSize = PancakeFlipUiTypography.ModalBody;

            var s = GameSession.Instance;
            if (s == null)
            {
                bodyText.text = "";
                return;
            }

            var list = s.RecipeCatalog;
            if (list == null || list.Length == 0)
            {
                bodyText.text = "";
                return;
            }

            var sb = new StringBuilder();
            int n = 1;
            foreach (var r in list)
            {
                if (r == null) continue;
                if (s.BaseRecipe != null && ReferenceEquals(r, s.BaseRecipe)) continue;

                sb.Append(n++).Append(". ").AppendLine(r.displayName);
                sb.Append("   Ингредиенты: ");
                if (r.ingredients != null && r.ingredients.Length > 0)
                {
                    bool first = true;
                    foreach (var slot in r.ingredients)
                    {
                        if (slot.ingredient == null) continue;
                        if (!first) sb.Append(", ");
                        first = false;
                        sb.Append(slot.amount).Append(' ').Append(slot.ingredient.displayName);
                    }
                }
                sb.AppendLine().AppendLine();
            }

            bodyText.text = sb.ToString().TrimEnd();
        }

        public static RecipeBookScreenView EnsureUnderCanvas(Canvas canvas)
        {
            if (canvas == null) return null;

            var existing = canvas.GetComponentInChildren<RecipeBookScreenView>(true);
            if (existing != null && existing.bodyText != null && existing.closeButton != null)
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

            var bodyGo = new GameObject("Body", typeof(RectTransform));
            bodyGo.transform.SetParent(root.transform, false);
            var brt = bodyGo.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.04f, 0.14f);
            brt.anchorMax = new Vector2(0.96f, 0.84f);
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var bodyT = bodyGo.AddComponent<TextMeshProUGUI>();
            if (font != null) bodyT.font = font;
            bodyT.fontSize = PancakeFlipUiTypography.ModalBody;
            bodyT.color = new Color(0.15f, 0.12f, 0.1f);
            bodyT.alignment = TextAlignmentOptions.TopLeft;
            bodyT.enableWordWrapping = true;
            bodyT.overflowMode = TextOverflowModes.Overflow;
            bodyT.raycastTarget = false;

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
            rb.bodyText = bodyT;
            rb.closeButton = cbtn;
            cbtn.onClick.AddListener(() => rb.gameObject.SetActive(false));
            rb.gameObject.SetActive(false);
            return rb;
        }
    }
}
