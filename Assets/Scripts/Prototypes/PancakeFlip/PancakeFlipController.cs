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
        [SerializeField] RectTransform inputZone;

        float _chargeTime;
        bool _isCharging;

        void Start()
        {
            if (pancake != null && pan != null)
                pancake.SetPanCenter(pan.PanCenter);
        }

        void Update()
        {
            if (config == null || pancake == null) return;

            if (pancake.CurrentState == PancakeBehaviour.State.InFlight)
            {
                _isCharging = false;
                _chargeTime = 0f;
                if (chargeIndicator != null)
                    chargeIndicator.SetCharge(0f);
                return;
            }

            bool pointerDown = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);
            bool pointerJustDown = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
            bool pointerJustUp = Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space);

            if (Input.touchCount > 0)
            {
                var t = Input.GetTouch(0);
                pointerDown = t.phase == TouchPhase.Began || t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved;
                pointerJustDown = t.phase == TouchPhase.Began;
                pointerJustUp = t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled;
            }

            if (pointerJustDown && !_isCharging)
            {
                _isCharging = true;
                _chargeTime = 0f;
                if (chargeIndicator != null)
                    chargeIndicator.SetCharge(0f);
            }

            if (pointerJustUp && _isCharging && pan != null)
            {
                float charge = Mathf.Min(1f, _chargeTime / config.maxHoldTime);
                charge = Mathf.Clamp01(charge);
                float force = config.ForceFromCharge(charge);
                float spin = config.SpinFromCharge(charge);
                pancake.Throw(force, spin);
                pan.PlayNod();
                _isCharging = false;
                _chargeTime = 0f;
                if (chargeIndicator != null)
                    chargeIndicator.SetCharge(0f);
            }

            if (_isCharging)
            {
                _chargeTime = Mathf.Min(_chargeTime + Time.deltaTime, config.maxHoldTime);
                float charge = Mathf.Min(1f, _chargeTime / config.maxHoldTime);
                if (chargeIndicator != null)
                    chargeIndicator.SetCharge(charge);
            }
        }

        public void OnPointerDown(BaseEventData eventData)
        {
            if (config == null || pancake == null) return;
            if (pancake.CurrentState == PancakeBehaviour.State.InFlight) return;
            if (inputZone != null && !IsInZone(eventData)) return;

            _isCharging = true;
            _chargeTime = 0f;
            if (chargeIndicator != null)
                chargeIndicator.SetCharge(0f);
        }

        public void OnPointerUp(BaseEventData eventData)
        {
            if (config == null || pancake == null || pan == null) return;
            if (pancake.CurrentState == PancakeBehaviour.State.InFlight) return;
            if (inputZone != null && !IsInZone(eventData)) return;

            if (!_isCharging) return;

            float charge = Mathf.Clamp01(_chargeTime / config.maxHoldTime);
            float force = config.ForceFromCharge(charge);
            float spin = config.SpinFromCharge(charge);

            pancake.Throw(force, spin);
            pan.PlayNod();
            _isCharging = false;
            _chargeTime = 0f;
            if (chargeIndicator != null)
                chargeIndicator.SetCharge(0f);
        }

        bool IsInZone(BaseEventData eventData)
        {
            if (inputZone == null) return true;
            if (eventData is PointerEventData ptr && RectTransformUtility.RectangleContainsScreenPoint(inputZone, ptr.position, ptr.pressEventCamera))
                return true;
            return false;
        }
    }
}
