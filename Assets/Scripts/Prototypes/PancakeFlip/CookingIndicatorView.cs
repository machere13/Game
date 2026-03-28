using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    /// <summary>
    /// Превью стороны на UI: один спрайт блина; степень прожарки — через умножающий тинт (сырое → белый → подгоревшее).
    /// </summary>
    public sealed class CookingIndicatorView : MonoBehaviour
    {
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] PancakeFlipConfig config;

        [Header("Side A (верх)")]
        [SerializeField] Image pancakeA;

        [Header("Side B (низ)")]
        [SerializeField] Image pancakeB;

        [Header("Тинт поверх текстуры")]
        [SerializeField] Color rawTint = new Color(0.82f, 0.76f, 0.68f);
        [Tooltip("Цвет в зоне perfectMin…perfectMax — чуть теплее белого, чтобы окно «можно сдавать» читалось")]
        [SerializeField] Color readyTint = new Color(1f, 0.98f, 0.94f);
        [SerializeField] Color overcookedTint = new Color(0.32f, 0.18f, 0.1f);

        void Update()
        {
            if (pancake == null || config == null) return;

            ApplyTint(pancakeA, pancake.CookA);
            ApplyTint(pancakeB, pancake.CookB);
        }

        void ApplyTint(Image img, float cook01)
        {
            if (img == null) return;
            img.color = GetPancakeTint(cook01);
        }

        /// <summary>Двойной SmoothStep — мягче края, без резких скачков.</summary>
        static float EaseInOut01(float x)
        {
            x = Mathf.Clamp01(x);
            return Mathf.SmoothStep(0f, 1f, Mathf.SmoothStep(0f, 1f, x));
        }

        Color GetPancakeTint(float cook01)
        {
            float pm = config.perfectMin;
            float pM = config.perfectMax;

            if (cook01 < pm)
            {
                float u = cook01 / Mathf.Max(0.001f, pm);
                float t = EaseInOut01(u);
                return Color.Lerp(rawTint, readyTint, t);
            }

            if (cook01 <= pM)
                return readyTint;

            float burnU = (cook01 - pM) / Mathf.Max(0.001f, 1f - pM);
            float burnT = EaseInOut01(burnU);
            return Color.Lerp(readyTint, overcookedTint, burnT);
        }
    }
}
