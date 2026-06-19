using System.Collections;
using TMPro;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class FloatingTextSpawner : MonoBehaviour
    {
        [SerializeField] float riseDistance = 90f;
        [SerializeField] float lifetime = 0.9f;

        // riseSign: +1 — текст поднимается вверх, -1 — опускается вниз (для попапов у верхней кромки).
        public void Spawn(string text, RectTransform anchor, Color color, float fontSize, float riseSign = 1f)
        {
            if (anchor == null || string.IsNullOrEmpty(text)) return;

            var go = new GameObject("FloatText", typeof(RectTransform));
            go.transform.SetParent(anchor, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.SetAsLastSibling();

            var tmp = go.AddComponent<TextMeshProUGUI>();
            var f = PancakeFlipUiFonts.UiTmpFont;
            if (f != null) tmp.font = f;
            tmp.text = text;
            tmp.color = color;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = false;
            tmp.fontStyle = FontStyles.Bold;

            StartCoroutine(Animate(rt, tmp, riseSign >= 0f ? 1f : -1f));
        }

        IEnumerator Animate(RectTransform rt, TextMeshProUGUI tmp, float riseSign)
        {
            Vector2 start = rt.anchoredPosition;
            Color baseColor = tmp.color;
            float e = 0f;
            while (e < lifetime && rt != null)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / lifetime);
                rt.anchoredPosition = start + Vector2.up * (riseDistance * k * riseSign);
                var c = baseColor; c.a = 1f - k;
                tmp.color = c;
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
        }
    }
}
