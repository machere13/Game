using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class CookingIndicatorView : MonoBehaviour
    {
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] PancakeFlipConfig config;

        [Header("Side A")]
        [SerializeField] Image barA;
        [SerializeField] Text labelA;

        [Header("Side B")]
        [SerializeField] Image barB;
        [SerializeField] Text labelB;

        [Header("Colors")]
        [SerializeField] Color rawColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] Color perfectColor = new Color(0.85f, 0.55f, 0.1f);
        [SerializeField] Color overcookedColor = new Color(0.25f, 0.12f, 0.05f);

        void Update()
        {
            if (pancake == null || config == null) return;

            UpdateBar(barA, labelA, pancake.CookA);
            UpdateBar(barB, labelB, pancake.CookB);
        }

        void UpdateBar(Image bar, Text label, float cook01)
        {
            if (bar != null)
            {
                bar.fillAmount = cook01;
                bar.color = GetCookColor(cook01);
            }

            if (label != null)
                label.text = GetCookLabel(cook01);
        }

        Color GetCookColor(float cook01)
        {
            if (cook01 < config.perfectMin)
                return Color.Lerp(rawColor, perfectColor, cook01 / Mathf.Max(0.01f, config.perfectMin));
            if (cook01 <= config.perfectMax)
                return perfectColor;
            float t = (cook01 - config.perfectMax) / Mathf.Max(0.01f, 1f - config.perfectMax);
            return Color.Lerp(perfectColor, overcookedColor, t);
        }

        string GetCookLabel(float cook01)
        {
            if (cook01 < config.perfectMin) return "Сырой";
            if (cook01 <= config.perfectMax) return "Готов";
            if (cook01 >= config.overcookedThreshold) return "Сгорел";
            return "Пережар";
        }
    }
}
