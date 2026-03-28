using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderCardView : MonoBehaviour
    {
        [SerializeField] Image recipeImage;
        [SerializeField] Image rewardBg;
        [SerializeField] Text coinText;
        [SerializeField] Text xpText;
        [SerializeField] Image personIcon;
        [SerializeField] Button selectButton;
        [SerializeField] Image selectionHighlight;

        [Header("Person Icons")]
        [SerializeField] Sprite[] personIconSprites;

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

            if (recipeImage != null && order.Recipe != null && order.Recipe.icon != null)
            {
                recipeImage.sprite = order.Recipe.icon;
                recipeImage.enabled = true;
            }
            else if (recipeImage != null)
                recipeImage.enabled = false;

            if (coinText != null)
                coinText.text = $"+{order.RewardCoins}";
            if (xpText != null)
                xpText.text = $"+{order.RewardXp}";

            if (personIcon != null && personIconSprites != null && order.PersonIndex < personIconSprites.Length)
            {
                personIcon.sprite = personIconSprites[order.PersonIndex];
                personIcon.enabled = true;
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

        public Order BoundOrder => _order;

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
