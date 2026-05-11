using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PanUpgradeScreenView : MonoBehaviour
    {
        static readonly Color BuyGreen = new Color(0.28f, 0.62f, 0.38f, 1f);
        static readonly Color BuyRed = new Color(0.72f, 0.32f, 0.28f, 1f);
        static readonly Color OwnedGrey = new Color(0.5f, 0.5f, 0.52f, 1f);

        [SerializeField] Transform upgradeListContainer;
        [SerializeField] GameObject upgradeRowPrefab;
        [SerializeField] Button closeButton;
        [SerializeField] Sprite defaultPanIcon;

        void Start()
        {
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
            var s = GameSession.Instance;
            if (s == null || upgradeListContainer == null) return;

            foreach (Transform child in upgradeListContainer)
                Destroy(child.gameObject);

            var upgrades = s.AllUpgrades;
            if (upgrades == null) return;

            foreach (var upg in upgrades)
            {
                if (upg == null) continue;

                var go = upgradeRowPrefab != null
                    ? Instantiate(upgradeRowPrefab, upgradeListContainer)
                    : CreateDefaultRow(upgradeListContainer);

                BindRow(go, upg, s);
            }
        }

        void BindRow(GameObject rowGo, PanUpgradeConfig upg, GameSession s)
        {
            var iconImg = rowGo.transform.Find("Icon")?.GetComponent<Image>();
            var titleTxt = rowGo.transform.Find("Texts/Title")?.GetComponent<Text>();
            var descTxt = rowGo.transform.Find("Texts/Desc")?.GetComponent<Text>();
            var btn = rowGo.transform.Find("BuyBtn")?.GetComponent<Button>();
            var btnTxt = rowGo.transform.Find("BuyBtn/Text")?.GetComponent<Text>();

            if (iconImg != null)
            {
                var spr = upg.icon != null ? upg.icon : defaultPanIcon;
                iconImg.sprite = spr;
                iconImg.preserveAspect = true;
                iconImg.color = spr != null ? Color.white : new Color(0.85f, 0.85f, 0.85f);
            }

            if (titleTxt != null)
                titleTxt.text = upg.displayName;

            if (descTxt != null)
            {
                string d = string.IsNullOrWhiteSpace(upg.description) ? "Улучшение сковороды." : upg.description;
                descTxt.text = d;
            }

            if (btn == null) return;

            btn.transition = Selectable.Transition.None;

            bool owned = s.Upgrades.IsOwned(upg);
            bool locked = upg.unlockLevel > s.Wallet.Level;
            bool canAfford = s.Wallet.Coins >= upg.coinCost;
            bool canBuy = !owned && !locked && canAfford;

            if (btnTxt != null)
            {
                if (owned) btnTxt.text = "Куплено";
                else if (locked) btnTxt.text = $"Ур. {upg.unlockLevel}";
                else if (!canAfford) btnTxt.text = $"Нужно {upg.coinCost}¢";
                else btnTxt.text = "Купить";
            }

            Color bg;
            if (owned) bg = OwnedGrey;
            else if (canBuy) bg = BuyGreen;
            else bg = BuyRed;

            btn.interactable = canBuy;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = bg;

            btn.onClick.RemoveAllListeners();
            if (canBuy)
            {
                var captured = upg;
                btn.onClick.AddListener(() =>
                {
                    s.BuyUpgrade(captured);
                    Rebuild();
                });
            }
        }

        static GameObject CreateDefaultRow(Transform parent)
        {
            var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 14f;
            h.padding = new RectOffset(10, 10, 10, 10);
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;

            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = 132f;
            rowLe.preferredHeight = 132f;
            rowLe.flexibleWidth = 1f;

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(row.transform, false);
            var iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(108, 108);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 108;
            iconLe.preferredHeight = 108;
            iconLe.flexibleWidth = 0;
            iconLe.flexibleHeight = 0;

            var textsGo = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup));
            textsGo.transform.SetParent(row.transform, false);
            var v = textsGo.GetComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperLeft;
            v.spacing = 4f;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            var textsLe = textsGo.AddComponent<LayoutElement>();
            textsLe.flexibleWidth = 1f;
            textsLe.minWidth = 80f;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(textsGo.transform, false);
            var title = titleGo.AddComponent<Text>();
            title.fontSize = 26;
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.18f, 0.15f, 0.12f);
            title.alignment = TextAnchor.UpperLeft;
            if (font != null) title.font = font;
            var titleLe = titleGo.AddComponent<LayoutElement>();
            titleLe.minHeight = 34f;
            titleLe.flexibleWidth = 1f;

            var descGo = new GameObject("Desc", typeof(RectTransform));
            descGo.transform.SetParent(textsGo.transform, false);
            var desc = descGo.AddComponent<Text>();
            desc.fontSize = 20;
            desc.color = new Color(0.35f, 0.32f, 0.28f);
            desc.alignment = TextAnchor.UpperLeft;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Overflow;
            if (font != null) desc.font = font;
            var descLe = descGo.AddComponent<LayoutElement>();
            descLe.flexibleWidth = 1f;
            descLe.preferredHeight = 64f;

            var btnGo = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(row.transform, false);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = BuyGreen;
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 168f;
            btnLe.minWidth = 168f;
            btnLe.flexibleWidth = 0f;
            btnLe.preferredHeight = 72f;

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.fontSize = 22;
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
