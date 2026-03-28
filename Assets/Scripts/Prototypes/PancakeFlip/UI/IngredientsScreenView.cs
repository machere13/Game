using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class IngredientsScreenView : MonoBehaviour
    {
        [SerializeField] Transform ingredientListContainer;
        [SerializeField] GameObject ingredientRowPrefab;
        [SerializeField] Button closeButton;
        [SerializeField] StoveView stove;

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnClose);
            gameObject.SetActive(false);
        }

        void OnClose()
        {
            gameObject.SetActive(false);
            if (stove != null) stove.Close();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Rebuild();
        }

        void Rebuild()
        {
            var s = GameSession.Instance;
            if (s == null || ingredientListContainer == null) return;

            foreach (Transform child in ingredientListContainer)
                Destroy(child.gameObject);

            var ingredients = s.AllIngredients;
            if (ingredients == null) return;

            foreach (var ing in ingredients)
            {
                if (ing == null) continue;
                if (s.DoughIngredient != null && ing == s.DoughIngredient) continue;
                if (ing.unlockLevel > s.Wallet.Level) continue;

                var go = ingredientRowPrefab != null
                    ? Instantiate(ingredientRowPrefab, ingredientListContainer)
                    : CreateDefaultRow(ingredientListContainer);

                var texts = go.GetComponentsInChildren<Text>();
                if (texts.Length >= 1)
                    texts[0].text = $"{ing.displayName}  x{s.Inventory.GetAmount(ing)}";

                var buyBtn = go.GetComponentInChildren<Button>();
                if (buyBtn != null)
                {
                    var captured = ing;
                    buyBtn.onClick.AddListener(() =>
                    {
                        s.BuyIngredient(captured);
                        Rebuild();
                    });
                    var btnText = buyBtn.GetComponentInChildren<Text>();
                    if (btnText != null) btnText.text = $"Купить ({ing.coinCost}c)";
                }
            }
        }

        static GameObject CreateDefaultRow(Transform parent)
        {
            var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var rect = row.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 64f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(row.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.fontSize = 26;
            label.color = new Color(0.18f, 0.15f, 0.12f);
            if (font != null) label.font = font;
            var le = labelGo.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            var btnGo = new GameObject("Btn", typeof(RectTransform));
            btnGo.transform.SetParent(row.transform, false);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.35f, 0.62f, 0.38f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 180;

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.fontSize = 24;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            if (font != null) btnTxt.font = font;
            var btnTxtRect = btnTxtGo.GetComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.offsetMin = Vector2.zero;
            btnTxtRect.offsetMax = Vector2.zero;

            return row;
        }
    }
}
