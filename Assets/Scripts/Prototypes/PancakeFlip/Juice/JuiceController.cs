using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class JuiceController : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] FloatingTextSpawner floatText;
        [SerializeField] CoinFlySpawner coinFly;
        [SerializeField] RectTransform walletAnchor;
        [SerializeField] RectTransform profileAnchor;
        [SerializeField] RectTransform centerAnchor;
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] Graphic profileGraphic;

        static readonly Color CoinColor = new Color(1f, 0.85f, 0.25f);
        static readonly Color XpColor = new Color(0.55f, 0.8f, 1f);
        static readonly Color FlashTint = new Color(1f, 1f, 0.7f);

        Wallet _wallet;
        int _lastCoins, _lastXp;
        bool _hooked;

        void Start() => Hook();

        void Hook()
        {
            if (_hooked) return;
            var s = GameSession.Instance;
            if (s == null) return;

            _wallet = s.Wallet;
            if (_wallet != null)
            {
                _lastCoins = _wallet.Coins;
                _lastXp = _wallet.TotalXp;
                _wallet.OnChanged += OnWalletChanged;
                _wallet.OnLevelUp += OnLevelUp;
            }
            if (pancake == null) pancake = s.Pancake;
            if (pancake != null) pancake.OnLanded += OnLanded;
            s.OnPancakeStarted += OnPancakeStarted;
            s.OnNewLocationUnlocked += OnUnlocked;
            _hooked = true;
        }

        void OnDestroy()
        {
            if (_wallet != null)
            {
                _wallet.OnChanged -= OnWalletChanged;
                _wallet.OnLevelUp -= OnLevelUp;
            }
            if (pancake != null) pancake.OnLanded -= OnLanded;
            var s = GameSession.Instance;
            if (s != null)
            {
                s.OnPancakeStarted -= OnPancakeStarted;
                s.OnNewLocationUnlocked -= OnUnlocked;
            }
        }

        void OnLanded(PancakeBehaviour.LandingResult r)
        {
            if (pancake != null) Juice.PunchScale(this, pancake.transform, 0.22f, 0.2f);
            if (cam != null)
            {
                float amp = Mathf.Clamp(r.rotations * 0.04f, 0f, 0.25f);
                if (amp > 0.001f) Juice.Shake(this, cam.transform, amp, 0.18f);
            }
            if (floatText != null && centerAnchor != null && r.rotations > 0)
                floatText.Spawn($"x{r.rotations}!", centerAnchor, FlashTint, 64f);
            if (Sfx.Instance != null) Sfx.Instance.PlayFlip(1f + Mathf.Clamp(r.rotations, 0, 8) * 0.06f);
        }

        void OnPancakeStarted()
        {
            if (Sfx.Instance != null) Sfx.Instance.PlayCook();
        }

        void OnWalletChanged()
        {
            if (_wallet == null) return;
            int dCoins = _wallet.Coins - _lastCoins;
            int dXp = _wallet.TotalXp - _lastXp;
            _lastCoins = _wallet.Coins;
            _lastXp = _wallet.TotalXp;

            if (dCoins > 0)
            {
                if (coinFly != null && centerAnchor != null && walletAnchor != null)
                    coinFly.Fly(centerAnchor, walletAnchor, Mathf.Clamp(dCoins / 5 + 1, 1, 5));
                if (floatText != null && walletAnchor != null)
                    floatText.Spawn($"+{dCoins}", walletAnchor, CoinColor, 48f);
                if (Sfx.Instance != null) Sfx.Instance.PlayServe();
            }
            if (dXp > 0 && floatText != null && profileAnchor != null)
                floatText.Spawn($"+{dXp} xp", profileAnchor, XpColor, 40f);
        }

        void OnLevelUp(int level)
        {
            if (profileGraphic != null) Juice.FlashColor(this, profileGraphic, FlashTint, 0.5f);
            if (floatText != null && profileAnchor != null)
                floatText.Spawn("LEVEL UP", profileAnchor, FlashTint, 56f);
            if (cam != null) Juice.Shake(this, cam.transform, 0.2f, 0.25f);
            if (Sfx.Instance != null) Sfx.Instance.PlayLevelUp();
        }

        void OnUnlocked()
        {
            if (floatText != null && centerAnchor != null)
                floatText.Spawn("Новая локация!", centerAnchor, FlashTint, 56f);
            if (cam != null) Juice.Shake(this, cam.transform, 0.2f, 0.25f);
            if (Sfx.Instance != null) Sfx.Instance.PlayUnlock();
        }
    }
}
