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
                    bool canAfford = s.Wallet.Coins >= ing.coinCost;
                    var btnText = buyBtn.GetComponentInChildren<Text>();
                    if (btnText != null)
                        btnText.text = canAfford ? $"Купить — {ing.coinCost}¢" : $"Нужно {ing.coinCost}¢";

                    ShopBuyButtonStyle.Apply(buyBtn, btnText, canAfford);
                    buyBtn.interactable = canAfford;

                    buyBtn.onClick.RemoveAllListeners();
                    if (canAfford)
                    {
                        var captured = ing;
                        buyBtn.onClick.AddListener(() =>
                        {
                            s.BuyIngredient(captured);
                            Rebuild();
                        });
                    }
                }
            }
        }

        static GameObject CreateDefaultRow(Transform parent)
        {
            var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var rect = row.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 88f);
            var rowH = row.GetComponent<HorizontalLayoutGroup>();
            rowH.childControlWidth = true;
            rowH.childControlHeight = true;
            rowH.childForceExpandWidth = false;
            rowH.childForceExpandHeight = false;

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
            btnImg.color = ShopBuyButtonStyle.BuyGreen;
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.transition = Selectable.Transition.None;
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.minWidth = ShopBuyButtonStyle.PreferredButtonWidth;
            btnLe.preferredWidth = ShopBuyButtonStyle.PreferredButtonWidth;
            btnLe.flexibleWidth = 0f;
            btnLe.minHeight = ShopBuyButtonStyle.PreferredButtonHeight;
            btnLe.preferredHeight = ShopBuyButtonStyle.PreferredButtonHeight;
            btnLe.flexibleHeight = 0f;

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.fontSize = ShopBuyButtonStyle.ButtonFontSize;
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
