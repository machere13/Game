using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderCardView : MonoBehaviour
    {
        [SerializeField] Text rewardText;
        [SerializeField] Button selectButton;
        [SerializeField] Image selectionHighlight;

        Order _order;
        bool _isSelected;
        float _lastClickTime;
        const float DoubleClickThreshold = 0.4f;

        public void Bind(Order order)
        {
            _order = order;
            _isSelected = false;
            if (order == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            if (rewardText != null)
            {
                string name = order.Recipe != null ? order.Recipe.displayName : "Блин";
                rewardText.text = $"{name}\n{order.RewardCoins}c  {order.RewardXp}xp";
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClick);
            }
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            if (selectionHighlight != null)
                selectionHighlight.enabled = selected;
        }

        void OnClick()
        {
            if (_order == null) return;
            var s = GameSession.Instance;
            if (s == null) return;

            float now = Time.unscaledTime;

            if (_isSelected && (now - _lastClickTime) < DoubleClickThreshold)
            {
                TryServe();
                _lastClickTime = 0f;
                return;
            }

            s.SelectOrder(_order);
            _lastClickTime = now;
        }

        void TryServe()
        {
            var s = GameSession.Instance;
            if (s == null || _order == null) return;

            var msc = Object.FindFirstObjectByType<MainScreenController>();
            if (msc != null)
                msc.Serve();
            else
                s.TryServe();
        }
    }
}
