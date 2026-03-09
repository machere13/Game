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
            if (n == 0) return "0 оборотов";
            int mod10 = n % 10;
            int mod100 = n % 100;
            if (mod100 >= 11 && mod100 <= 19) return n + " оборотов";
            if (mod10 == 1) return n + " оборот";
            if (mod10 >= 2 && mod10 <= 4) return n + " оборота";
            return n + " оборотов";
        }
    }
}
