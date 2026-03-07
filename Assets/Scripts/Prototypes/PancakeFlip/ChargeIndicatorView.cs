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
        float _baseScaleX = 1f;
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
                var rt = fillImage.rectTransform;
                _baseScaleX = Mathf.Abs(rt.localScale.x);
                if (_baseScaleX <= 0f) _baseScaleX = 1f;

                fillImage.color = baseColor;
            }

            SetCharge(0f);
        }

        void Update()
        {
            if (_canvasGroup != null)
                _canvasGroup.alpha = _charge > 0.001f ? 1f : 0f;

            if (fillImage == null) return;

            if (_charge >= 0.999f)
            {
                float blink = (Mathf.Sin(Time.time * (2f * Mathf.PI / blinkPeriod)) + 1f) * 0.5f;
                fillImage.color = Color.Lerp(baseColor, Color.white, 0.3f + 0.4f * blink);
            }
            else
            {
                fillImage.color = baseColor;
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

            var rt = fillImage.rectTransform;
            var s = rt.localScale;
            float x = Mathf.Lerp(0.0001f, _baseScaleX, _charge);
            rt.localScale = new Vector3(x, s.y, s.z);
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
