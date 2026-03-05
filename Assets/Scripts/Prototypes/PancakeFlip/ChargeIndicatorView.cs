using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class ChargeIndicatorView : MonoBehaviour
    {
        [SerializeField] Image fillImage;
        [SerializeField] float blinkPeriod = 0.5f;

        float _charge = -1f;

        void Update()
        {
            if (fillImage == null) return;
            if (_charge >= 0.999f)
            {
                float blink = (Mathf.Sin(Time.time * (2f * Mathf.PI / blinkPeriod)) + 1f) * 0.5f;
                fillImage.color = Color.Lerp(new Color(1f, 1f, 0.6f), new Color(1f, 1f, 1f), 0.3f + 0.4f * blink);
            }
            else
            {
                fillImage.color = Color.white;
            }
        }

        public void SetCharge(float charge01)
        {
            _charge = Mathf.Clamp01(charge01);
            if (fillImage != null)
                fillImage.fillAmount = _charge;
        }
    }
}
