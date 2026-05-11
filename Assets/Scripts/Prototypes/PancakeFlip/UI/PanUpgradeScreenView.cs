using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PanUpgradeScreenView : MonoBehaviour
    {
        [SerializeField] Transform upgradeListContainer;
        [SerializeField] Button closeButton;
        [SerializeField] Sprite defaultPanIcon;

        static readonly Color CellFilled = new Color(0.42f, 0.62f, 0.40f, 1f);
        static readonly Color CellEmpty = new Color(0.88f, 0.86f, 0.82f, 1f);

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

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            AddSectionTitle(upgradeListContainer, "Характеристики сковороды", font);
            var tracks = s.StatTracks;
            if (tracks != null)
            {
                foreach (var t in tracks)
                {
                    if (t == null) continue;
                    CreateStatTrackRow(upgradeListContainer, t, s, font);
                }
            }

            AddSectionTitle(upgradeListContainer, "Сковороды", font);
            var tiers = s.PanTiers;
            if (tiers != null)
            {
                foreach (var tier in tiers)
                {
                    if (tier == null) continue;
                    CreatePanTierRow(upgradeListContainer, tier, s, font);
                }
            }
        }

        static void AddSectionTitle(Transform parent, string title, Font font)
        {
            var go = new GameObject("Section", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = 40f;
            le.preferredHeight = 40f;
            le.flexibleWidth = 1f;
            var tgo = new GameObject("Text", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var txt = tgo.AddComponent<Text>();
            txt.text = title;
            txt.fontSize = 28;
            txt.fontStyle = FontStyle.Bold;
            txt.color = new Color(0.2f, 0.16f, 0.12f);
            txt.alignment = TextAnchor.MiddleLeft;
            if (font != null) txt.font = font;
            var rt = tgo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(14f, 0f);
            rt.offsetMax = Vector2.zero;
        }

        void CreateStatTrackRow(Transform parent, PanStatTrackConfig track, GameSession s, Font font)
        {
            var row = new GameObject("StatRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 10f;
            h.padding = new RectOffset(12, 12, 10, 10);
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = 128f;
            rowLe.preferredHeight = 128f;
            rowLe.flexibleWidth = 1f;

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(row.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = track.icon != null ? track.icon : defaultPanIcon;
            iconImg.preserveAspect = true;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 72f;
            iconLe.preferredHeight = 72f;

            var textsGo = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup));
            textsGo.transform.SetParent(row.transform, false);
            var v = textsGo.GetComponent<VerticalLayoutGroup>();
            v.spacing = 4f;
            v.childControlHeight = true;
            v.childControlWidth = true;
            var textsLe = textsGo.AddComponent<LayoutElement>();
            textsLe.flexibleWidth = 1f;
            textsLe.minWidth = 60f;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(textsGo.transform, false);
            var title = titleGo.AddComponent<Text>();
            title.text = track.displayName;
            title.fontSize = 24;
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.18f, 0.15f, 0.12f);
            title.alignment = TextAnchor.UpperLeft;
            if (font != null) title.font = font;
            titleGo.AddComponent<LayoutElement>().minHeight = 28f;

            var descGo = new GameObject("Desc", typeof(RectTransform));
            descGo.transform.SetParent(textsGo.transform, false);
            var desc = descGo.AddComponent<Text>();
            desc.text = string.IsNullOrEmpty(track.description) ? "—" : track.description;
            desc.fontSize = 18;
            desc.color = new Color(0.38f, 0.34f, 0.3f);
            desc.alignment = TextAnchor.UpperLeft;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Overflow;
            if (font != null) desc.font = font;
            descGo.AddComponent<LayoutElement>().preferredHeight = 44f;

            var cellsGo = new GameObject("Cells", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cellsGo.transform.SetParent(row.transform, false);
            var ch = cellsGo.GetComponent<HorizontalLayoutGroup>();
            ch.spacing = 6f;
            ch.childAlignment = TextAnchor.MiddleCenter;
            ch.childControlWidth = true;
            ch.childControlHeight = true;
            ch.childForceExpandWidth = false;
            ch.childForceExpandHeight = false;
            var cellsLe = cellsGo.AddComponent<LayoutElement>();
            cellsLe.preferredWidth = 5f * 28f + 4f * 6f;
            cellsLe.minWidth = cellsLe.preferredWidth;

            for (int i = 0; i < PanUpgradeState.StatMaxLevel; i++)
            {
                var c = new GameObject($"C{i}", typeof(RectTransform));
                c.transform.SetParent(cellsGo.transform, false);
                var img = c.AddComponent<Image>();
                img.color = CellEmpty;
                var cle = c.AddComponent<LayoutElement>();
                cle.preferredWidth = 28f;
                cle.preferredHeight = 28f;
            }

            var btnGo = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(row.transform, false);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = ShopBuyButtonStyle.BuyGreen;
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            ShopBuyButtonStyle.ApplyButtonLayout(btn);

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.fontSize = ShopBuyButtonStyle.ButtonFontSize;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            if (font != null) btnTxt.font = font;
            var r = btnTxtGo.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;

            BindStatRow(row, track, s, btn, btnTxt);
        }

        void BindStatRow(GameObject row, PanStatTrackConfig track, GameSession s, Button btn, Text btnTxt)
        {
            int level = s.Upgrades.GetStatLevel(track.effectType);
            var cells = row.transform.Find("Cells");
            if (cells != null)
            {
                for (int i = 0; i < PanUpgradeState.StatMaxLevel; i++)
                {
                    var c = cells.GetChild(i).GetComponent<Image>();
                    if (c != null)
                        c.color = i < level ? CellFilled : CellEmpty;
                }
            }

            bool maxed = level >= PanUpgradeState.StatMaxLevel;
            bool locked = track.unlockLevel > s.Wallet.Level;
            int cost = maxed ? 0 : track.CostForNextLevel(level);
            bool canAfford = !maxed && !locked && s.Wallet.Coins >= cost;
            bool canBuy = !maxed && !locked && canAfford;

            if (btnTxt != null)
            {
                if (maxed) btnTxt.text = "Максимум";
                else if (locked) btnTxt.text = $"Ур. {track.unlockLevel}";
                else if (!canAfford) btnTxt.text = $"— {cost}¢";
                else btnTxt.text = $"Купить — {cost}¢";
            }

            ShopBuyButtonStyle.Apply(btn, btnTxt, canBuy);
            btn.interactable = canBuy;
            btn.onClick.RemoveAllListeners();
            if (canBuy)
            {
                var cap = track;
                btn.onClick.AddListener(() =>
                {
                    s.TryBuyStatStep(cap);
                    Rebuild();
                });
            }
        }

        void CreatePanTierRow(Transform parent, PanTierConfig tier, GameSession s, Font font)
        {
            var row = new GameObject("PanRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 10f;
            h.padding = new RectOffset(12, 12, 10, 10);
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = 118f;
            rowLe.preferredHeight = 118f;
            rowLe.flexibleWidth = 1f;

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(row.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = tier.icon != null ? tier.icon : defaultPanIcon;
            iconImg.preserveAspect = true;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.preferredWidth = 88f;
            iconLe.preferredHeight = 88f;

            var textsGo = new GameObject("Texts", typeof(RectTransform), typeof(VerticalLayoutGroup));
            textsGo.transform.SetParent(row.transform, false);
            var textV = textsGo.GetComponent<VerticalLayoutGroup>();
            textV.spacing = 4f;
            textV.childControlHeight = true;
            textV.childControlWidth = true;
            var textsLe = textsGo.AddComponent<LayoutElement>();
            textsLe.flexibleWidth = 1f;
            textsLe.minWidth = 60f;

            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(textsGo.transform, false);
            var title = titleGo.AddComponent<Text>();
            title.text = tier.displayName;
            title.fontSize = 24;
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(0.18f, 0.15f, 0.12f);
            title.alignment = TextAnchor.UpperLeft;
            if (font != null) title.font = font;
            titleGo.AddComponent<LayoutElement>().minHeight = 28f;

            var descGo = new GameObject("Desc", typeof(RectTransform));
            descGo.transform.SetParent(textsGo.transform, false);
            var desc = descGo.AddComponent<Text>();
            desc.text = string.IsNullOrEmpty(tier.description) ? "—" : tier.description;
            desc.fontSize = 18;
            desc.color = new Color(0.38f, 0.34f, 0.3f);
            desc.alignment = TextAnchor.UpperLeft;
            desc.horizontalOverflow = HorizontalWrapMode.Wrap;
            desc.verticalOverflow = VerticalWrapMode.Overflow;
            if (font != null) desc.font = font;
            descGo.AddComponent<LayoutElement>().preferredHeight = 44f;

            var btnGo = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(row.transform, false);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnGo.GetComponent<Image>();
            ShopBuyButtonStyle.ApplyButtonLayout(btn);

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.fontSize = ShopBuyButtonStyle.ButtonFontSize;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            if (font != null) btnTxt.font = font;
            var rr = btnTxtGo.GetComponent<RectTransform>();
            rr.anchorMin = Vector2.zero;
            rr.anchorMax = Vector2.one;
            rr.offsetMin = rr.offsetMax = Vector2.zero;

            BindPanTierRow(tier, s, btn, btnTxt);
        }

        void BindPanTierRow(PanTierConfig tier, GameSession s, Button btn, Text btnTxt)
        {
            bool owned = s.Upgrades.IsPanOwned(tier);
            var equipped = s.EquippedPanTier;
            bool isEquipped = equipped == tier;
            bool locked = tier.unlockLevel > s.Wallet.Level;
            bool canBuy = !tier.isStarter && !owned && !locked && s.Wallet.Coins >= tier.coinCost;
            bool canEquip = owned && !isEquipped;

            if (tier.isStarter)
            {
                if (btnTxt != null)
                    btnTxt.text = isEquipped ? "Надета" : "Надеть";
                bool canEquipStarter = !isEquipped;
                ShopBuyButtonStyle.Apply(btn, btnTxt, canEquipStarter);
                btn.interactable = canEquipStarter;
                btn.onClick.RemoveAllListeners();
                if (canEquipStarter)
                {
                    var cap = tier;
                    btn.onClick.AddListener(() =>
                    {
                        s.EquipPanTier(cap);
                        Rebuild();
                    });
                }
                return;
            }

            if (!owned)
            {
                if (btnTxt != null)
                {
                    if (locked) btnTxt.text = $"Ур. {tier.unlockLevel}";
                    else if (!canBuy && !locked) btnTxt.text = $"— {tier.coinCost}¢";
                    else btnTxt.text = $"Купить — {tier.coinCost}¢";
                }
                ShopBuyButtonStyle.Apply(btn, btnTxt, canBuy);
                btn.interactable = canBuy;
                btn.onClick.RemoveAllListeners();
                if (canBuy)
                {
                    var cap = tier;
                    btn.onClick.AddListener(() =>
                    {
                        s.TryBuyPanTier(cap);
                        Rebuild();
                    });
                }
                return;
            }

            if (isEquipped)
            {
                if (btnTxt != null) btnTxt.text = "Надета";
                ShopBuyButtonStyle.Apply(btn, btnTxt, false);
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
                return;
            }

            if (btnTxt != null) btnTxt.text = "Надеть";
            ShopBuyButtonStyle.Apply(btn, btnTxt, true);
            btn.interactable = true;
            btn.onClick.RemoveAllListeners();
            var c = tier;
            btn.onClick.AddListener(() =>
            {
                s.EquipPanTier(c);
                Rebuild();
             });
        }
    }
}
