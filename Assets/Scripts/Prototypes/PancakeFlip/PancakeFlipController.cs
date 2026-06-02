using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipController : MonoBehaviour
    {
        [SerializeField] PancakeFlipConfig config;
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] PanBehaviour pan;
        [SerializeField] ChargeIndicatorView chargeIndicator;

        float _chargeTime;
        bool _isCharging;
        bool _wasKeyOrMouseDown;

        void Start()
        {
            if (chargeIndicator == null)
                chargeIndicator = Object.FindFirstObjectByType<ChargeIndicatorView>();
        }

        void Update()
        {
            if (config == null || pancake == null) return;

            if (!pancake.IsActiveCooking)
            {
                _isCharging = false;
                _chargeTime = 0f;
                _wasKeyOrMouseDown = false;
                if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
                return;
            }

            if (pancake.CurrentState == PancakeBehaviour.State.InFlight)
            {
                _isCharging = false;
                _chargeTime = 0f;
                if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
                return;
            }

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool mouse = Input.GetMouseButton(0) && !overUI;
            bool space = Input.GetKey(KeyCode.Space);
            bool newInput = !overUI && GetSpaceOrMouseFromNewInputSystem();
            bool pointerDown = mouse || space || newInput;
            bool pointerJustDown = pointerDown && !_wasKeyOrMouseDown;
            bool pointerJustUp = !pointerDown && _wasKeyOrMouseDown;
            _wasKeyOrMouseDown = pointerDown;

            if (pointerJustDown && !_isCharging)
            {
                _isCharging = true;
                _chargeTime = 0f;
                if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
            }

            if (pointerJustUp && _isCharging)
            {
                ReleaseThrow(ChargeFromTime());
            }

            if (_isCharging)
            {
                _chargeTime += Time.deltaTime;
                if (chargeIndicator != null)
                    chargeIndicator.SetCharge(ChargeFromTime());

                if (_chargeTime >= config.maxHoldTime)
                {
                    ReleaseThrow(1f);
                }
            }
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            if (config == null || pancake == null || !pancake.IsActiveCooking || pancake.CurrentState == PancakeBehaviour.State.InFlight) return;
            if (!_isCharging) { _isCharging = true; _chargeTime = 0f; if (chargeIndicator != null) chargeIndicator.SetCharge(0f); }
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            if (config == null || pancake == null || !_isCharging || !pancake.IsActiveCooking) return;
            if (pancake.CurrentState == PancakeBehaviour.State.InFlight) return;
            ReleaseThrow(ChargeFromTime());
        }

        // Заряд (0..1) из времени удержания; защищён от maxHoldTime <= 0.
        float ChargeFromTime()
        {
            float hold = Mathf.Max(0.0001f, config != null ? config.maxHoldTime : 1f);
            return Mathf.Clamp01(_chargeTime / hold);
        }

        // Единая точка броска: убирает тройное дублирование (Update / max-hold / OnPointerUp).
        void ReleaseThrow(float charge01)
        {
            charge01 = Mathf.Clamp01(charge01);
            pancake.Throw(ForceForCharge(charge01), SpinForCharge(charge01));
            if (pan != null) pan.PlayNod();
            _isCharging = false;
            _chargeTime = 0f;
            if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
        }

        float ForceForCharge(float charge01)
        {
            var sess = GameSession.Instance;
            if (sess != null) return sess.ForceFromChargeWithUpgrades(charge01);
            return config.ForceFromCharge(charge01);
        }

        float SpinForCharge(float charge01)
        {
            var sess = GameSession.Instance;
            if (sess != null) return sess.SpinFromChargeWithUpgrades(charge01);
            return config.SpinFromCharge(charge01);
        }

        // Опрос нового Input System без рефлексии и без per-frame Type.GetType.
        // ENABLE_INPUT_SYSTEM определён, когда установлен пакет; иначе ветка компилируется в no-op.
        static bool GetSpaceOrMouseFromNewInputSystem()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.isPressed) return true;
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.isPressed;
#else
            return false;
#endif
        }
    }
}
