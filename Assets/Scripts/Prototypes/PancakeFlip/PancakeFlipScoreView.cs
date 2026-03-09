using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipScoreView : MonoBehaviour
    {
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] Text scoreText;
        [SerializeField] Text rotationsPopupText;
        [SerializeField] float popupDuration = 1.8f;

        int _score;
        float _popupHideTime = -1f;

        void Start()
        {
            if (pancake != null)
                pancake.OnLanded += OnPancakeLanded;
            if (rotationsPopupText != null)
                rotationsPopupText.gameObject.SetActive(false);
            RefreshScoreText();
        }

        void OnDestroy()
        {
            if (pancake != null)
                pancake.OnLanded -= OnPancakeLanded;
        }

        void Update()
        {
            if (rotationsPopupText != null && rotationsPopupText.gameObject.activeSelf && Time.time >= _popupHideTime)
                rotationsPopupText.gameObject.SetActive(false);
        }

        void OnPancakeLanded(int fullRotations)
        {
            _score += 1 + fullRotations;
            RefreshScoreText();

            if (rotationsPopupText != null)
            {
                rotationsPopupText.text = RotationsLabel(fullRotations);
                rotationsPopupText.gameObject.SetActive(true);
                _popupHideTime = Time.time + popupDuration;
            }
        }

        void RefreshScoreText()
        {
            if (scoreText != null)
                scoreText.text = "Очки: " + _score;
        }

        static string RotationsLabel(int n)
        {
            return n.ToString();
        }
    }
}
