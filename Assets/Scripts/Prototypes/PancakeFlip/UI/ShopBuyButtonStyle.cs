using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public static class ShopBuyButtonStyle
    {
        public static readonly Color BuyGreen = new Color(0.28f, 0.62f, 0.38f, 1f);
        public static readonly Color BuyRed = new Color(0.72f, 0.32f, 0.28f, 1f);

        public const float ButtonFontSize = PancakeFlipUiTypography.ActionButton;

        public const float PreferredButtonWidth = 268f;
        public const float PreferredButtonHeight = 104f;

        public static void Apply(Button btn, TextMeshProUGUI label, bool canBuy)
        {
            if (btn == null) return;
            btn.transition = Selectable.Transition.None;
            ApplyButtonLayout(btn);
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                var session = GameSession.Instance;
                var spr = session == null ? null : (canBuy ? session.SuccessButtonSprite : session.CancelButtonSprite);
                if (spr != null) { img.sprite = spr; img.type = Image.Type.Sliced; img.color = Color.white; }
                else img.color = canBuy ? BuyGreen : BuyRed;
            }
            if (label != null)
            {
                label.fontSize = ButtonFontSize;
                var f = PancakeFlipUiFonts.UiTmpFont;
                if (f != null) label.font = f;
            }
        }

        public static void ApplyButtonLayout(Button btn)
        {
            if (btn == null) return;
            var le = btn.GetComponent<LayoutElement>();
            if (le == null)
                le = btn.gameObject.AddComponent<LayoutElement>();
            le.minWidth = PreferredButtonWidth;
            le.preferredWidth = PreferredButtonWidth;
            le.flexibleWidth = 0f;
            le.minHeight = PreferredButtonHeight;
            le.preferredHeight = PreferredButtonHeight;
            le.flexibleHeight = 0f;
        }

        public const float CoinIconSize = 56f;
        const float CoinIconRightPad = 16f;
        const float TextCoinSpacing = 8f;

        public static void SetCoinIcon(Button btn, bool show)
        {
            if (btn == null) return;
            var existing = btn.transform.Find("CoinIcon");
            var textTr = btn.transform.Find("Text");
            var textRt = textTr != null ? textTr as RectTransform : null;

            if (!show)
            {
                if (existing != null) existing.gameObject.SetActive(false);
                if (textRt != null)
                {
                    textRt.anchorMin = Vector2.zero;
                    textRt.anchorMax = Vector2.one;
                    textRt.offsetMin = Vector2.zero;
                    textRt.offsetMax = Vector2.zero;
                }
                return;
            }

            var session = GameSession.Instance;
            var sprite = session != null ? session.CoinIcon : null;

            GameObject coinGo;
            if (existing != null) coinGo = existing.gameObject;
            else
            {
                coinGo = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
                coinGo.transform.SetParent(btn.transform, false);
            }
            coinGo.SetActive(true);
            var rt = coinGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(TextCoinSpacing * 0.5f, 0f);
            rt.sizeDelta = new Vector2(CoinIconSize, CoinIconSize);
            var img = coinGo.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.enabled = sprite != null;

            if (textRt != null)
            {
                textRt.anchorMin = new Vector2(0f, 0f);
                textRt.anchorMax = new Vector2(0.5f, 1f);
                textRt.offsetMin = new Vector2(0f, 0f);
                textRt.offsetMax = new Vector2(-TextCoinSpacing * 0.5f, 0f);
                var tmp = textRt.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null) tmp.alignment = TMPro.TextAlignmentOptions.MidlineRight;
            }
        }
    }
}
