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

            if (_isCharging)
            {
                _chargeTime = Mathf.Min(_chargeTime + Time.deltaTime, config.maxHoldTime);
                float charge = Mathf.Clamp01(_chargeTime / config.maxHoldTime);
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
