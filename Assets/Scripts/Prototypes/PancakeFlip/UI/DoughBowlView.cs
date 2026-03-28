using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class DoughBowlView : MonoBehaviour
    {
        [SerializeField] IngredientConfig doughIngredient;
        [SerializeField] Button bowlButton;
        [SerializeField] Image bowlImage;
        [SerializeField] Sprite emptySprite;
        [SerializeField] Sprite fullSprite;

        void Start()
        {
            var s = GameSession.Instance;
            if (s != null)
                s.Inventory.OnChanged += Refresh;
            if (bowlButton != null)
                bowlButton.onClick.AddListener(OnBowlClicked);
            Refresh();
        }

        void OnDestroy()
        {
            var s = GameSession.Instance;
            if (s != null)
                s.Inventory.OnChanged -= Refresh;
            if (bowlButton != null)
                bowlButton.onClick.RemoveListener(OnBowlClicked);
        }

        void OnBowlClicked()
        {
            var s = GameSession.Instance;
            if (s == null || doughIngredient == null) return;
            s.TapDough(doughIngredient);
            Refresh();
        }

        void Refresh()
        {
            var s = GameSession.Instance;
            bool full = s != null && doughIngredient != null && s.Inventory.GetAmount(doughIngredient) > 0;
            if (bowlImage != null)
            {
                bowlImage.sprite = full ? fullSprite : emptySprite;
                bowlImage.enabled = bowlImage.sprite != null;
            }
            if (bowlButton != null)
                bowlButton.interactable = !full;
        }
    }
}
