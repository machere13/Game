using System.Collections;
using TMPro;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipScoreView : MonoBehaviour
    {
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] PancakeFlipConfig config;
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI rotationsPopupText;
        [SerializeField] float popupDuration = 1.2f;
        [SerializeField] float popupRise = 90f;

        RectTransform _popupRt;
        Vector2 _popupBasePos;
        Color _popupBaseColor;
        Coroutine _popupCo;

        void Start()
        {
            if (pancake != null)
                pancake.OnLanded += OnPancakeLanded;
            if (rotationsPopupText != null)
            {
                _popupRt = rotationsPopupText.rectTransform;
                _popupBasePos = _popupRt.anchoredPosition;
                _popupBaseColor = rotationsPopupText.color;
                rotationsPopupText.gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            if (pancake != null)
                pancake.OnLanded -= OnPancakeLanded;
        }

        void Update()
        {
            RefreshScoreText();
        }

        void OnPancakeLanded(PancakeBehaviour.LandingResult result)
        {
            int xpPerRot = (config != null) ? config.xpPerRotation : 10;
            int earned = Mathf.Max(1, result.rotations) * xpPerRot;

            if (rotationsPopupText == null) return;

            rotationsPopupText.text = result.rotations > 0
                ? $"+{earned} XP  ({result.rotations}x)"
                : $"+{earned} XP";

            if (_popupCo != null) StopCoroutine(_popupCo);
            _popupCo = StartCoroutine(AnimatePopup());
        }

        IEnumerator AnimatePopup()
        {
            rotationsPopupText.gameObject.SetActive(true);
            _popupRt.anchoredPosition = _popupBasePos;

            float e = 0f;
            while (e < popupDuration)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / popupDuration);
                _popupRt.anchoredPosition = _popupBasePos + Vector2.up * (popupRise * k);
                var c = _popupBaseColor;
                c.a = 1f - k; // плавное затухание за время жизни
                rotationsPopupText.color = c;
                yield return null;
            }

            // Возврат в исходное состояние для следующего показа.
            rotationsPopupText.color = _popupBaseColor;
            _popupRt.anchoredPosition = _popupBasePos;
            rotationsPopupText.gameObject.SetActive(false);
            _popupCo = null;
        }

        void RefreshScoreText()
        {
            if (scoreText == null) return;
            var s = GameSession.Instance;
            if (s != null)
                scoreText.text = $"XP: {s.Wallet.TotalXp}";
        }
    }
}
