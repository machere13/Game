using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public static class ShopBuyButtonStyle
    {
        public static readonly Color BuyGreen = new Color(0.28f, 0.62f, 0.38f, 1f);
        public static readonly Color BuyRed = new Color(0.72f, 0.32f, 0.28f, 1f);

        public const int ButtonFontSize = 30;

        public const float PreferredButtonWidth = 200f;
        public const float PreferredButtonHeight = 76f;

        public static void Apply(Button btn, Text label, bool canBuy)
        {
            if (btn == null) return;
            btn.transition = Selectable.Transition.None;
            ApplyButtonLayout(btn);
            var img = btn.GetComponent<Image>();
            if (img != null)
                img.color = canBuy ? BuyGreen : BuyRed;
            if (label != null)
                label.fontSize = ButtonFontSize;
        }

        /// <summary>Одна и та же ширина/высота кнопки в ингредиентах и апгрейдах (LayoutElement + без растягивания по flexible).</summary>
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
    }
}
