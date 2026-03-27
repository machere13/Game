using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipScoreView : MonoBehaviour
    {
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] PancakeFlipConfig config;
        [SerializeField] Text scoreText;
        [SerializeField] Text rotationsPopupText;
        [SerializeField] float popupDuration = 1.8f;

        float _popupHideTime = -1f;

        void Start()
        {
            if (pancake != null)
                pancake.OnLanded += OnPancakeLanded;
            if (rotationsPopupText != null)
                rotationsPopupText.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (pancake != null)
                pancake.OnLanded -= OnPancakeLanded;
        }

        void Update()
        {
            RefreshScoreText();

            if (rotationsPopupText != null && rotationsPopupText.gameObject.activeSelf && Time.time >= _popupHideTime)
                rotationsPopupText.gameObject.SetActive(false);
        }

        void OnPancakeLanded(PancakeBehaviour.LandingResult result)
        {
            int xpPerRot = (config != null) ? config.xpPerRotation : 10;
            int earned = Mathf.Max(1, result.rotations) * xpPerRot;

            if (rotationsPopupText != null)
            {
                string label = result.rotations > 0
                    ? $"+{earned} XP  ({result.rotations}x)"
                    : $"+{earned} XP";
                rotationsPopupText.text = label;
                rotationsPopupText.gameObject.SetActive(true);
                _popupHideTime = Time.time + popupDuration;
            }
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
