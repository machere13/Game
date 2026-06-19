using TMPro;
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

            var font = PancakeFlipUiFonts.UiTmpFont;

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

        static void AddSectionTitle(Transform parent, string title, TMP_FontAsset font)
        {
            var go = new GameObject("Section", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = PancakeFlipUiTypography.SectionBlockMinHeight;
            le.preferredHeight = PancakeFlipUiTypography.SectionBlockMinHeight;
            le.flexibleWidth = 1f;
            var tgo = new GameObject("Text", typeof(RectTransform));
            tgo.transform.SetParent(go.transform, false);
            var txt = tgo.AddComponent<TextMeshProUGUI>();
            txt.text = title;
            txt.fontSize = PancakeFlipUiTypography.SectionHeading;
            txt.fontStyle = FontStyles.Bold;
            txt.color = new Color(0.2f, 0.16f, 0.12f);
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            txt.enableWordWrapping = false;
            txt.raycastTarget = false;
            if (font != null) txt.font = font;
            var rt = tgo.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(14f, 0f);
            rt.offsetMax = Vector2.zero;
        }

        void CreateStatTrackRow(Transform parent, PanStatTrackConfig track, GameSession s, TMP_FontAsset font)
        {
            var row = new GameObject("StatRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 10f;
            h.padding = new RectOffset(12, 28, 10, 10);
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = PancakeFlipUiTypography.StatRowMinHeight;
            rowLe.preferredHeight = PancakeFlipUiTypography.StatRowMinHeight;
            rowLe.flexibleWidth = 1f;

            // Иконка у характеристик не нужна — сразу текст.
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
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = track.displayName;
            title.fontSize = PancakeFlipUiTypography.ListRowTitle;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color(0.18f, 0.15f, 0.12f);
            title.alignment = TextAlignmentOptions.TopLeft;
            title.enableWordWrapping = true;
            title.raycastTarget = false;
            if (font != null) title.font = font;
            titleGo.AddComponent<LayoutElement>().minHeight = PancakeFlipUiTypography.TitleLineMinHeight;

            var descGo = new GameObject("Desc", typeof(RectTransform));
            descGo.transform.SetParent(textsGo.transform, false);
            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = string.IsNullOrEmpty(track.description) ? " — " : track.description;
            desc.fontSize = PancakeFlipUiTypography.ListRowDescription;
            desc.color = new Color(0.38f, 0.34f, 0.3f);
            desc.alignment = TextAlignmentOptions.TopLeft;
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Overflow;
            desc.raycastTarget = false;
            if (font != null) desc.font = font;
            descGo.AddComponent<LayoutElement>().preferredHeight = PancakeFlipUiTypography.DescPreferredHeight;

            // Правая колонка: шкала уровней сверху, кнопка под ней.
            var rightGo = new GameObject("Right", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            rightGo.transform.SetParent(row.transform, false);
            var rightV = rightGo.GetComponent<VerticalLayoutGroup>();
            rightV.spacing = 10f;
            rightV.childAlignment = TextAnchor.MiddleCenter;
            rightV.childControlWidth = true;
            rightV.childControlHeight = true;
            rightV.childForceExpandWidth = false;
            rightV.childForceExpandHeight = false;
            var rightLe = rightGo.GetComponent<LayoutElement>();
            rightLe.minWidth = ShopBuyButtonStyle.PreferredButtonWidth;
            rightLe.preferredWidth = ShopBuyButtonStyle.PreferredButtonWidth;
            rightLe.flexibleWidth = 0f;

            var cellsGo = new GameObject("Cells", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cellsGo.transform.SetParent(rightGo.transform, false);
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
            btnGo.transform.SetParent(rightGo.transform, false);
            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = ShopBuyButtonStyle.BuyGreen;
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            ShopBuyButtonStyle.ApplyButtonLayout(btn);

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<TextMeshProUGUI>();
            btnTxt.fontSize = ShopBuyButtonStyle.ButtonFontSize;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;
            btnTxt.raycastTarget = false;
            if (font != null) btnTxt.font = font;
            var r = btnTxtGo.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;

            BindStatRow(row, track, s, btn, btnTxt);
        }

        void BindStatRow(GameObject row, PanStatTrackConfig track, GameSession s, Button btn, TextMeshProUGUI btnTxt)
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

            bool showCoin = !maxed && !locked;
            if (btnTxt != null)
            {
                if (maxed) btnTxt.text = "Максимум";
                else if (locked) btnTxt.text = $"Ур. {track.unlockLevel}";
                else btnTxt.text = cost.ToString();
            }

            ShopBuyButtonStyle.Apply(btn, btnTxt, canBuy);
            ShopBuyButtonStyle.SetCoinIcon(btn, showCoin);
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

        void CreatePanTierRow(Transform parent, PanTierConfig tier, GameSession s, TMP_FontAsset font)
        {
            var row = new GameObject("PanRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var h = row.GetComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleLeft;
            h.spacing = 14f;
            h.padding = new RectOffset(0, 28, 10, 10);
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            var rowLe = row.GetComponent<LayoutElement>();
            rowLe.minHeight = PancakeFlipUiTypography.PanRowMinHeight;
            rowLe.preferredHeight = PancakeFlipUiTypography.PanRowMinHeight;
            rowLe.flexibleWidth = 1f;

            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(row.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = tier.icon != null ? tier.icon : defaultPanIcon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.minWidth = 200f;
            iconLe.preferredWidth = 200f;
            iconLe.flexibleWidth = 0f;
            iconLe.minHeight = 160f;
            iconLe.preferredHeight = 160f;
            iconLe.flexibleHeight = 0f;

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
            var title = titleGo.AddComponent<TextMeshProUGUI>();
            title.text = tier.displayName;
            title.fontSize = PancakeFlipUiTypography.ListRowTitle;
            title.fontStyle = FontStyles.Bold;
            title.color = new Color(0.18f, 0.15f, 0.12f);
            title.alignment = TextAlignmentOptions.TopLeft;
            title.enableWordWrapping = true;
            title.raycastTarget = false;
            if (font != null) title.font = font;
            titleGo.AddComponent<LayoutElement>().minHeight = PancakeFlipUiTypography.TitleLineMinHeight;

            var descGo = new GameObject("Desc", typeof(RectTransform));
            descGo.transform.SetParent(textsGo.transform, false);
            var desc = descGo.AddComponent<TextMeshProUGUI>();
            desc.text = string.IsNullOrEmpty(tier.description) ? " — " : tier.description;
            desc.fontSize = PancakeFlipUiTypography.ListRowDescription;
            desc.color = new Color(0.38f, 0.34f, 0.3f);
            desc.alignment = TextAlignmentOptions.TopLeft;
            desc.enableWordWrapping = true;
            desc.overflowMode = TextOverflowModes.Overflow;
            desc.raycastTarget = false;
            if (font != null) desc.font = font;
            descGo.AddComponent<LayoutElement>().preferredHeight = PancakeFlipUiTypography.DescPreferredHeight;

            var btnGo = new GameObject("BuyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(row.transform, false);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnGo.GetComponent<Image>();
            ShopBuyButtonStyle.ApplyButtonLayout(btn);

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<TextMeshProUGUI>();
            btnTxt.fontSize = ShopBuyButtonStyle.ButtonFontSize;
            btnTxt.alignment = TextAlignmentOptions.Center;
            btnTxt.color = Color.white;
            btnTxt.raycastTarget = false;
            if (font != null) btnTxt.font = font;
            var rr = btnTxtGo.GetComponent<RectTransform>();
            rr.anchorMin = Vector2.zero;
            rr.anchorMax = Vector2.one;
            rr.offsetMin = rr.offsetMax = Vector2.zero;

            BindPanTierRow(tier, s, btn, btnTxt);
        }

        void BindPanTierRow(PanTierConfig tier, GameSession s, Button btn, TextMeshProUGUI btnTxt)
        {
            bool owned = s.Upgrades.IsPanOwned(tier);
            var equipped = s.EquippedPanTier;
            bool isEquipped = equipped == tier;
            bool locked = tier.unlockLevel > s.Wallet.Level;
            bool canBuy = !tier.isStarter && !owned && !locked && s.Wallet.Coins >= tier.coinCost;

            if (tier.isStarter)
            {
                if (btnTxt != null)
                    btnTxt.text = isEquipped ? "Надета" : "Надеть";
                bool canEquipStarter = !isEquipped;
                ShopBuyButtonStyle.Apply(btn, btnTxt, canEquipStarter);
                ShopBuyButtonStyle.SetCoinIcon(btn, false);
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
                    else btnTxt.text = tier.coinCost.ToString();
                }
                ShopBuyButtonStyle.Apply(btn, btnTxt, canBuy);
                ShopBuyButtonStyle.SetCoinIcon(btn, !locked);
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
                ShopBuyButtonStyle.SetCoinIcon(btn, false);
                btn.interactable = false;
                btn.onClick.RemoveAllListeners();
                return;
            }

            if (btnTxt != null) btnTxt.text = "Надеть";
            ShopBuyButtonStyle.Apply(btn, btnTxt, true);
            ShopBuyButtonStyle.SetCoinIcon(btn, false);
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
