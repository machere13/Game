using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class IngredientsScreenView : MonoBehaviour
    {
        [SerializeField] Transform ingredientListContainer;
        [SerializeField] Transform builderContainer;
        [SerializeField] Button cookButton;
        [SerializeField] TextMeshProUGUI cookButtonLabel;
        [SerializeField] Button closeIconButton;
        [SerializeField] StoveView stove;

        const float IngredientIconSize = 88f;
        const float BuilderSlotSize = 110f;
        const float SlotRemoveSize = 48f;
        const float CoinIconSize = 56f;
        const float ActionButtonWidth = 200f;
        const string IconChildName = "Icon";

        static readonly Color SlotEmptyColor = new Color(0.85f, 0.78f, 0.65f, 0.5f);
        static readonly Color SlotFilledColor = new Color(1f, 0.94f, 0.78f, 0.95f);
        static readonly Color CookEnabledColor = new Color(0.86f, 0.55f, 0.18f, 1f);
        static readonly Color CookDisabledColor = new Color(0.55f, 0.5f, 0.45f, 1f);
        static readonly Color AddButtonColor = new Color(0.32f, 0.5f, 0.78f, 1f);

        readonly List<Button> _slotButtons = new List<Button>();

        // Кэш строк списка ингредиентов: строятся один раз, дальше только обновляются.
        // Раньше Rebuild() уничтожал и пересоздавал все строки на каждое изменение инвентаря/сборки.
        sealed class ActionButtonUi
        {
            public Button button;
            public Image background;
            public TextMeshProUGUI label;
        }

        sealed class IngredientRowUi
        {
            public IngredientConfig ingredient;
            public TextMeshProUGUI amountLabel;
            public ActionButtonUi add;
            public ActionButtonUi buy;
            public bool isFree;
        }

        readonly List<IngredientRowUi> _rows = new List<IngredientRowUi>();
        bool _rowsBuilt;

        void Start()
        {
            if (closeIconButton != null)
                closeIconButton.onClick.AddListener(OnClose);
            if (cookButton != null)
                cookButton.onClick.AddListener(OnCook);
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            var s = GameSession.Instance;
            if (s != null)
            {
                if (s.Build != null) s.Build.OnChanged += Rebuild;
                s.Inventory.OnChanged += Rebuild;
            }
        }

        void OnDisable()
        {
            var s = GameSession.Instance;
            if (s != null)
            {
                if (s.Build != null) s.Build.OnChanged -= Rebuild;
                s.Inventory.OnChanged -= Rebuild;
            }
        }

        void OnClose()
        {
            gameObject.SetActive(false);
            if (stove != null) stove.Close();
        }

        void OnCook()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            if (!s.TryCookFromBuild()) return;
            OnClose();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _rowsBuilt = false; // ингредиенты могли открыться после переезда — пересобираем строки
            Rebuild();
        }

        void Rebuild()
        {
            BuildBuilderArea();
            RefreshIngredientList();
            UpdateCookButton();
        }

        void BuildBuilderArea()
        {
            if (builderContainer == null) return;

            foreach (Transform child in builderContainer)
                Destroy(child.gameObject);
            _slotButtons.Clear();

            var s = GameSession.Instance;
            if (s == null || s.Build == null) return;

            for (int i = 0; i < PancakeBuild.MaxSlots; i++)
            {
                int slotIndex = i;
                IngredientConfig content = i < s.Build.Slots.Count ? s.Build.Slots[i] : null;
                CreateBuilderSlot(builderContainer, content, slotIndex);
            }
        }

        void CreateBuilderSlot(Transform parent, IngredientConfig ingredient, int slotIndex)
        {
            var slot = new GameObject($"Slot{slotIndex}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            slot.transform.SetParent(parent, false);
            var bg = slot.GetComponent<Image>();
            bg.color = ingredient != null ? SlotFilledColor : SlotEmptyColor;
            bg.raycastTarget = false;
            var le = slot.GetComponent<LayoutElement>();
            le.minWidth = BuilderSlotSize;
            le.preferredWidth = BuilderSlotSize;
            le.flexibleWidth = 0f;
            le.minHeight = BuilderSlotSize;
            le.preferredHeight = BuilderSlotSize;
            le.flexibleHeight = 0f;

            if (ingredient == null) return;

            var iconGo = new GameObject(IconChildName, typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(slot.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.08f, 0.08f);
            iconRt.anchorMax = new Vector2(0.92f, 0.92f);
            iconRt.offsetMin = iconRt.offsetMax = Vector2.zero;
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = ingredient.icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.enabled = ingredient.icon != null;

            var removeGo = new GameObject("Remove", typeof(RectTransform), typeof(Image), typeof(Button));
            removeGo.transform.SetParent(slot.transform, false);
            var rRt = removeGo.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(1f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(0.5f, 0.5f);
            rRt.anchoredPosition = new Vector2(-SlotRemoveSize * 0.15f, SlotRemoveSize * 0.15f);
            rRt.sizeDelta = new Vector2(SlotRemoveSize, SlotRemoveSize);
            var rImg = removeGo.GetComponent<Image>();
            var s = GameSession.Instance;
            var closeSpr = s != null ? s.CloseIcon : null;
            rImg.preserveAspect = true;
            if (closeSpr != null)
            {
                rImg.sprite = closeSpr;
                rImg.color = Color.white;
            }
            else
            {
                rImg.color = new Color(1f, 1f, 1f, 0f);
            }
            var rBtn = removeGo.GetComponent<Button>();
            rBtn.targetGraphic = rImg;
            rBtn.transition = Selectable.Transition.None;
            int captured = slotIndex;
            rBtn.onClick.AddListener(() =>
            {
                var gs = GameSession.Instance;
                if (gs == null || gs.Build == null) return;
                gs.Build.RemoveAt(captured);
            });
            _slotButtons.Add(rBtn);

            if (closeSpr == null)
            {
                var xGo = new GameObject("X", typeof(RectTransform));
                xGo.transform.SetParent(removeGo.transform, false);
                var xRt = xGo.GetComponent<RectTransform>();
                xRt.anchorMin = Vector2.zero; xRt.anchorMax = Vector2.one;
                xRt.offsetMin = xRt.offsetMax = Vector2.zero;
                var xTmp = xGo.AddComponent<TextMeshProUGUI>();
                var f = PancakeFlipUiFonts.UiTmpFont;
                if (f != null) xTmp.font = f;
                xTmp.text = "×";
                xTmp.color = new Color(0.78f, 0.18f, 0.18f, 1f);
                xTmp.alignment = TextAlignmentOptions.Center;
                xTmp.fontSize = 32f;
                xTmp.fontStyle = FontStyles.Bold;
                xTmp.raycastTarget = false;
            }
        }

        // Список ингредиентов статичен по составу (s.AllIngredients), поэтому строки создаём один раз,
        // а на каждое изменение только обновляем цифры/доступность кнопок — без Destroy/Instantiate.
        void RefreshIngredientList()
        {
            var s = GameSession.Instance;
            if (s == null || ingredientListContainer == null) return;

            if (!_rowsBuilt)
            {
                _rows.Clear();
                foreach (Transform child in ingredientListContainer)
                    Destroy(child.gameObject);

                foreach (var ing in OrderedIngredients(s))
                {
                    if (ing == null) continue;
                    _rows.Add(CreateIngredientRow(ingredientListContainer, s, ing));
                }
                _rowsBuilt = true;
            }

            foreach (var row in _rows)
                UpdateIngredientRow(s, row);
        }

        static void UpdateIngredientRow(GameSession s, IngredientRowUi row)
        {
            var ing = row.ingredient;
            if (row.amountLabel != null)
                row.amountLabel.text = $"{ing.displayName}  x{s.Inventory.GetAmount(ing)}";

            int inBuild = s.CountInBuild(ing);
            bool canAdd = s.Build != null && !s.Build.IsFull && s.Inventory.GetAmount(ing) > inBuild;
            if (row.add != null) row.add.button.interactable = canAdd;

            bool canBuy;
            string buyText;
            if (row.isFree)
            {
                buyText = "Заготовить";
                canBuy = !s.Inventory.IsAtCap(ing);
            }
            else
            {
                buyText = ing.coinCost.ToString();
                canBuy = s.Wallet.Coins >= ing.coinCost && !s.Inventory.IsAtCap(ing);
            }

            if (row.buy != null)
            {
                row.buy.button.interactable = canBuy;
                if (row.buy.label != null) row.buy.label.text = buyText;
                if (!row.isFree && row.buy.background != null)
                    row.buy.background.color = canBuy ? ShopBuyButtonStyle.BuyGreen : ShopBuyButtonStyle.BuyRed;
            }
        }

        static List<IngredientConfig> OrderedIngredients(GameSession s)
        {
            var list = new List<IngredientConfig>();
            if (s.AllIngredients == null) return list;
            foreach (var ing in s.AllIngredients)
            {
                if (ing == null) continue;
                if (ing.isDough) list.Insert(0, ing);
                else list.Add(ing);
            }
            return list;
        }

        static IngredientRowUi CreateIngredientRow(Transform parent, GameSession s, IngredientConfig ing)
        {
            var font = PancakeFlipUiFonts.UiTmpFont;

            var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowH = row.GetComponent<HorizontalLayoutGroup>();
            rowH.childControlWidth = true;
            rowH.childControlHeight = true;
            rowH.childForceExpandWidth = false;
            rowH.childForceExpandHeight = false;
            rowH.spacing = 12f;
            rowH.padding = new RectOffset(8, 8, 4, 4);
            rowH.childAlignment = TextAnchor.MiddleLeft;
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = 118f;
            rowLe.preferredHeight = 118f;

            var iconGo = new GameObject(IconChildName, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(row.transform, false);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.sprite = ing.icon;
            iconImg.preserveAspect = true;
            iconImg.enabled = ing.icon != null;
            iconImg.raycastTarget = false;
            var iconLe = iconGo.GetComponent<LayoutElement>();
            iconLe.minWidth = IngredientIconSize;
            iconLe.preferredWidth = IngredientIconSize;
            iconLe.flexibleWidth = 0f;
            iconLe.minHeight = IngredientIconSize;
            iconLe.preferredHeight = IngredientIconSize;
            iconLe.flexibleHeight = 0f;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(LayoutElement));
            labelGo.transform.SetParent(row.transform, false);
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            label.fontSize = PancakeFlipUiTypography.ShopIngredientRowLabel;
            label.color = new Color(0.18f, 0.15f, 0.12f);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            if (font != null) label.font = font;
            label.text = $"{ing.displayName}  x{s.Inventory.GetAmount(ing)}";
            var labelLe = labelGo.GetComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;

            bool isFree = ing.coinCost <= 0;

            var add = CreateActionButton(row.transform, "Добавить", AddButtonColor, font, withCoinIcon: false);
            add.button.onClick.AddListener(() =>
            {
                var gs = GameSession.Instance;
                if (gs != null) gs.TryAddToBuild(ing);
            });

            var buy = CreateActionButton(row.transform, isFree ? "Заготовить" : ing.coinCost.ToString(),
                isFree ? AddButtonColor : ShopBuyButtonStyle.BuyGreen, font, withCoinIcon: !isFree);
            buy.button.onClick.AddListener(() =>
            {
                var gs = GameSession.Instance;
                if (gs == null) return;
                gs.BuyIngredient(ing);
            });

            return new IngredientRowUi
            {
                ingredient = ing,
                amountLabel = label,
                add = add,
                buy = buy,
                isFree = isFree,
            };
        }

        static ActionButtonUi CreateActionButton(Transform parent, string text, Color bgColor, TMP_FontAsset font, bool withCoinIcon)
        {
            var btnGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            btnGo.transform.SetParent(parent, false);
            var img = btnGo.GetComponent<Image>();
            img.color = bgColor;
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            btnGo.AddComponent<ButtonJuice>();
            var le = btnGo.GetComponent<LayoutElement>();
            le.minWidth = ActionButtonWidth;
            le.preferredWidth = ActionButtonWidth;
            le.flexibleWidth = 0f;
            le.minHeight = ShopBuyButtonStyle.PreferredButtonHeight;
            le.preferredHeight = ShopBuyButtonStyle.PreferredButtonHeight;
            le.flexibleHeight = 0f;

            var txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txt = txtGo.AddComponent<TextMeshProUGUI>();
            if (font != null) txt.font = font;
            txt.fontSize = ShopBuyButtonStyle.ButtonFontSize;
            txt.color = Color.white;
            txt.raycastTarget = false;
            txt.enableWordWrapping = false;
            txt.text = text;
            var txtRt = txtGo.GetComponent<RectTransform>();

            if (withCoinIcon)
            {
                txt.alignment = TextAlignmentOptions.MidlineRight;
                txtRt.anchorMin = new Vector2(0f, 0f);
                txtRt.anchorMax = new Vector2(0.5f, 1f);
                txtRt.offsetMin = new Vector2(0f, 0f);
                txtRt.offsetMax = new Vector2(-4f, 0f);

                var coinGo = new GameObject("Coin", typeof(RectTransform), typeof(Image));
                coinGo.transform.SetParent(btnGo.transform, false);
                var coinImg = coinGo.GetComponent<Image>();
                var session = GameSession.Instance;
                coinImg.sprite = session != null ? session.CoinIcon : null;
                coinImg.preserveAspect = true;
                coinImg.raycastTarget = false;
                coinImg.enabled = coinImg.sprite != null;
                var coinRt = coinGo.GetComponent<RectTransform>();
                coinRt.anchorMin = new Vector2(0.5f, 0.5f);
                coinRt.anchorMax = new Vector2(0.5f, 0.5f);
                coinRt.pivot = new Vector2(0f, 0.5f);
                coinRt.anchoredPosition = new Vector2(4f, 0f);
                coinRt.sizeDelta = new Vector2(CoinIconSize, CoinIconSize);
            }
            else
            {
                txt.alignment = TextAlignmentOptions.Center;
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;
            }

            return new ActionButtonUi { button = btn, background = img, label = txt };
        }

        void UpdateCookButton()
        {
            if (cookButton == null) return;
            var s = GameSession.Instance;
            bool canCook = s != null && s.Build != null && s.Build.CanCook && !s.HasCookingPancake;
            cookButton.interactable = canCook;
            var img = cookButton.GetComponent<Image>();
            if (img != null)
                img.color = canCook ? CookEnabledColor : CookDisabledColor;
            if (cookButtonLabel != null)
            {
                var f = PancakeFlipUiFonts.UiTmpFont;
                if (f != null) cookButtonLabel.font = f;
                cookButtonLabel.text = s != null && s.HasCookingPancake ? "На сковороде" : "Готовить";
            }
        }
    }
}
