using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class ChargeIndicatorView : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] float blinkPeriod = 0.5f;
        [SerializeField] Color baseColor = new Color(1f, 0.9f, 0.3f);

        float _charge = -1f;
        CanvasGroup _canvasGroup;

        void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            var myImage = GetComponent<Image>();
            if (myImage != null) myImage.raycastTarget = false;

            if (fillImage == null)
            {
                fillImage = GetComponent<Image>();
                if (fillImage == null)
                    fillImage = GetComponentInChildren<Image>();
            }

            if (fillImage != null)
            {
                fillImage.raycastTarget = false;
                // Заливка слева направо: открываем спрайт по горизонтали от левого края.
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            }

            SetCharge(0f);
        }

        void Update()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = _charge > 0.001f ? 1f : 0f;

            if (fillImage == null) return;

            // На полном заряде — лёгкое мерцание прозрачностью (не перекрашиваем спрайт).
            if (_charge >= 0.999f)
            {
                float blink = (Mathf.Sin(Time.time * (2f * Mathf.PI / blinkPeriod)) + 1f) * 0.5f;
                var c = fillImage.color;
                c.a = 0.7f + 0.3f * blink;
                fillImage.color = c;
            }
            else
            {
                var c = fillImage.color;
                c.a = 1f;
                fillImage.color = c;
            }
        }

        public void SetCharge(float charge01)
        {
            _charge = Mathf.Clamp01(charge01);

            if (_charge > 0.001f && !gameObject.activeSelf)
                gameObject.SetActive(true);
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = _charge > 0.001f ? 1f : 0f;

            if (fillImage == null) return;
            fillImage.fillAmount = _charge;
        }

        public void SetVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = visible;
            }
            else
                gameObject.SetActive(visible);
        }
    }
}
