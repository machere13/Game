using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderCardView : MonoBehaviour
    {
        [SerializeField] Image recipeIcon;
        [SerializeField] Image customerIcon;
        [SerializeField] Text rewardText;
        [SerializeField] Button selectButton;
        [SerializeField] Button dismissButton;
        [SerializeField] Image selectionHighlight;

        Order _order;

        public void Bind(Order order)
        {
            _order = order;
            if (order == null)
            {
                gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);

            if (recipeIcon != null && order.Recipe != null && order.Recipe.icon != null)
                recipeIcon.sprite = order.Recipe.icon;

            if (rewardText != null)
                rewardText.text = $"{order.RewardCoins}c  {order.RewardXp}xp";

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnSelect);
            }
            if (dismissButton != null)
            {
                dismissButton.onClick.RemoveAllListeners();
                dismissButton.onClick.AddListener(OnDismiss);
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
                selectionHighlight.enabled = selected;
        }

        void OnSelect()
        {
            if (_order != null)
                GameSession.Instance?.SelectOrder(_order);
        }

        void OnDismiss()
        {
            if (_order != null)
                GameSession.Instance?.DismissOrder(_order);
        }
    }
}
