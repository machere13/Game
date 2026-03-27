using UnityEngine;
using UnityEngine.EventSystems;

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

            if (pointerJustUp && _isCharging && pan != null)
            {
                float charge = Mathf.Min(1f, _chargeTime / config.maxHoldTime);
                charge = Mathf.Clamp01(charge);
                pancake.Throw(config.ForceFromCharge(charge), config.SpinFromCharge(charge));
                pan.PlayNod();
                _isCharging = false;
                _chargeTime = 0f;
                if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
            }

            if (_isCharging)
            {
                _chargeTime += Time.deltaTime;
                float charge = Mathf.Clamp01(_chargeTime / config.maxHoldTime);
                if (chargeIndicator != null)
                    chargeIndicator.SetCharge(charge);

                if (_chargeTime >= config.maxHoldTime && pan != null)
                {
                    pancake.Throw(config.ForceFromCharge(1f), config.SpinFromCharge(1f));
                    pan.PlayNod();
                    _isCharging = false;
                    _chargeTime = 0f;
                    if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
                }
            }
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            if (config == null || pancake == null || pancake.CurrentState == PancakeBehaviour.State.InFlight) return;
            if (!_isCharging) { _isCharging = true; _chargeTime = 0f; if (chargeIndicator != null) chargeIndicator.SetCharge(0f); }
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            if (config == null || pancake == null || pan == null || !_isCharging) return;
            float charge = Mathf.Clamp01(_chargeTime / config.maxHoldTime);
            pancake.Throw(config.ForceFromCharge(charge), config.SpinFromCharge(charge));
            pan.PlayNod();
            _isCharging = false; _chargeTime = 0f;
            if (chargeIndicator != null) chargeIndicator.SetCharge(0f);
        }

        static bool GetSpaceOrMouseFromNewInputSystem()
        {
            try
            {
                var keyboardType = System.Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
                if (keyboardType == null) return false;
                var current = keyboardType.GetProperty("current")?.GetValue(null);
                if (current == null) return false;
                var spaceKey = current.GetType().GetProperty("spaceKey")?.GetValue(current);
                if (spaceKey == null) return false;
                var isPressed = spaceKey.GetType().GetProperty("isPressed")?.GetValue(spaceKey);
                if (isPressed is bool b && b) return true;
                var mouse = System.Type.GetType("UnityEngine.InputSystem.Mouse, Unity.InputSystem")?.GetProperty("current")?.GetValue(null);
                if (mouse == null) return false;
                var leftButton = mouse.GetType().GetProperty("leftButton")?.GetValue(mouse);
                if (leftButton == null) return false;
                var pressed = leftButton.GetType().GetProperty("isPressed")?.GetValue(leftButton);
                return pressed is bool p && p;
            }
            catch { return false; }
        }
    }
}
