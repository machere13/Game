using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class KitchenBarController : MonoBehaviour
    {
        [SerializeField] MainScreenController mainScreen;
        [SerializeField] Button recipesButton;
        [SerializeField] Button ingredientsShopButton;
        [SerializeField] Button upgradesButton;

        void Awake()
        {
            Transform kitchenRoot = recipesButton != null ? recipesButton.transform.parent : null;
            if (kitchenRoot != null && kitchenRoot.GetComponent<KitchenUiFrontLayer>() == null)
                kitchenRoot.gameObject.AddComponent<KitchenUiFrontLayer>();
        }

        void Start()
        {
            if (ingredientsShopButton == null && recipesButton != null)
            {
                var t = recipesButton.transform.parent?.Find("ShopBtn");
                if (t != null)
                    ingredientsShopButton = t.GetComponent<Button>();
            }

            if (recipesButton != null)
                recipesButton.onClick.AddListener(() => mainScreen?.OpenRecipeBook());
            if (ingredientsShopButton != null)
                ingredientsShopButton.onClick.AddListener(() => mainScreen?.OpenIngredientsShop());
            if (upgradesButton != null)
                upgradesButton.onClick.AddListener(() => mainScreen?.OpenUpgrades());
        }
    }
}
