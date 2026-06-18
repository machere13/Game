using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IdlePancake.PancakeFlip.JuiceCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    /// <summary>Presentational tween helpers. Stateless; coroutines run on a passed host MonoBehaviour.</summary>
    public static class Juice
    {
        public static void PunchScale(MonoBehaviour host, Transform t, float strength, float duration)
        {
            if (host == null || t == null || !host.isActiveAndEnabled) return;
            host.StartCoroutine(PunchScaleRoutine(t, strength, duration));
        }

        static IEnumerator PunchScaleRoutine(Transform t, float strength, float duration)
        {
            Vector3 baseScale = t.localScale;
            float e = 0f;
            while (e < duration && t != null)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / duration);
                t.localScale = baseScale * (1f + JuiceCurves.PunchScale01(k) * strength);
                yield return null;
            }
            if (t != null) t.localScale = baseScale;
        }

        public static void Shake(MonoBehaviour host, Transform t, float amplitude, float duration)
        {
            if (host == null || t == null || !host.isActiveAndEnabled) return;
            host.StartCoroutine(ShakeRoutine(t, amplitude, duration));
        }

        static IEnumerator ShakeRoutine(Transform t, float amplitude, float duration)
        {
            Vector3 basePos = t.localPosition;
            float e = 0f;
            while (e < duration && t != null)
            {
                e += Time.deltaTime;
                float damp = 1f - Mathf.Clamp01(e / duration);
                Vector2 off = Random.insideUnitCircle * (amplitude * damp);
                t.localPosition = basePos + new Vector3(off.x, off.y, 0f);
                yield return null;
            }
            if (t != null) t.localPosition = basePos;
        }

        public static void FlashColor(MonoBehaviour host, Graphic g, Color flash, float duration)
        {
            if (host == null || g == null || !host.isActiveAndEnabled) return;
            host.StartCoroutine(FlashRoutine(g, flash, duration));
        }

        static IEnumerator FlashRoutine(Graphic g, Color flash, float duration)
        {
            Color baseColor = g.color;
            float e = 0f;
            while (e < duration && g != null)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / duration);
                g.color = Color.Lerp(flash, baseColor, k);
                yield return null;
            }
            if (g != null) g.color = baseColor;
        }
    }
}
