using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class TopBarView : MonoBehaviour
    {
        [SerializeField] Text coinsText;
        [SerializeField] Text levelText;
        [SerializeField] Image xpFill;

        void Update()
        {
            var s = GameSession.Instance;
            if (s == null) return;

            if (coinsText != null)
                coinsText.text = s.Wallet.Coins.ToString();
            if (levelText != null)
                levelText.text = $"Ур. {s.Wallet.Level}";
            if (xpFill != null)
                xpFill.fillAmount = s.Wallet.LevelProgress01();
        }
    }
}
