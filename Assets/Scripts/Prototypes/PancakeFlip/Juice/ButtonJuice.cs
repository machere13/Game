using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IdlePancake.PancakeFlip.JuiceCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonJuice : MonoBehaviour
    {
        const float Strength = 0.14f;
        const float Duration = 0.16f;

        Vector3 _baseScale = Vector3.one;
        bool _captured;
        Coroutine _punch;

        void Start()
        {
            if (!_captured) { _baseScale = transform.localScale; _captured = true; }
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            // Гасим предыдущий пунч и всегда стартуем от исходного масштаба —
            // иначе при частых кликах анимации складываются и кнопка раздувается.
            if (_punch != null) StopCoroutine(_punch);
            transform.localScale = _baseScale;
            if (isActiveAndEnabled) _punch = StartCoroutine(Punch());

            if (Sfx.Instance != null) Sfx.Instance.Click();
        }

        IEnumerator Punch()
        {
            float e = 0f;
            while (e < Duration)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / Duration);
                transform.localScale = _baseScale * (1f + JuiceCurves.PunchScale01(k) * Strength);
                yield return null;
            }
            transform.localScale = _baseScale;
            _punch = null;
        }
    }
}
