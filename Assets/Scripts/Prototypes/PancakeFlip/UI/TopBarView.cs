using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class TopBarView : MonoBehaviour
    {
        [SerializeField] Text coinsText;
        [SerializeField] Text levelText;

        void Update()
        {
            var s = GameSession.Instance;
            if (s == null) return;

            if (coinsText != null)
                coinsText.text = s.Wallet.Coins.ToString();
            if (levelText != null)
                levelText.text = $"Level {s.Wallet.Level}";
        }
    }
}
