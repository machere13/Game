using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class MainScreenController : MonoBehaviour
    {
        [SerializeField] RecipeBookScreenView recipeBookScreen;
        [SerializeField] PanUpgradeScreenView upgradeScreen;
        [SerializeField] Text statusText;
        [SerializeField] CustomerAnimator customerAnimator;

        void Awake()
        {
            if (recipeBookScreen == null)
            {
                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    recipeBookScreen = RecipeBookScreenView.EnsureUnderCanvas(canvas);
            }
        }

        public void OpenRecipeBook()
        {
            if (recipeBookScreen != null)
                recipeBookScreen.Open();
        }

        public void OpenUpgrades()
        {
            if (upgradeScreen != null)
                upgradeScreen.Open();
        }

        /// <summary>Тот же экран, что и клик по плите — но из UI, без OnMouseDown по спрайту.</summary>
        public void OpenIngredientsShop()
        {
            var stove = Object.FindFirstObjectByType<StoveView>();
            if (stove != null)
                stove.Open();
            else
            {
                var shop = Object.FindFirstObjectByType<IngredientsScreenView>();
                shop?.Open();
            }
        }

        public void Serve()
        {
            var s = GameSession.Instance;
            if (s == null) return;

            if (s.ActiveOrder == null)
            {
                ShowStatus("Сначала выбери заказ слева!");
                return;
            }

            if (s.FlipConfig != null)
            {
                var pancake = Object.FindFirstObjectByType<PancakeBehaviour>();
                if (pancake != null)
                {
                    float min = s.FlipConfig.perfectMin;
                    if (pancake.CookA < min || pancake.CookB < min)
                    {
                        string sideA = pancake.CookA < min ? $"A:{pancake.CookA:P0}" : "";
                        string sideB = pancake.CookB < min ? $"B:{pancake.CookB:P0}" : "";
                        ShowStatus($"Блин не готов! {sideA} {sideB}".Trim());
                        return;
                    }
                }
            }

            int personIdx = s.ActiveOrder.PersonIndex;
            bool hadIngredients = s.ActiveOrder.Recipe == null || s.Inventory.HasIngredients(s.ActiveOrder.Recipe);
            if (!s.TryServe())
            {
                ShowStatus("Не удалось сдать блин");
            }
            else
            {
                ShowStatus(hadIngredients ? "Заказ сдан!" : "Сдано без ингредиентов (−50%)");
                if (customerAnimator != null)
                    customerAnimator.PlayServe(personIdx);
            }
        }

        public void ServeBase()
        {
            var s = GameSession.Instance;
            if (s == null) return;

            if (s.ActiveOrder == null)
            {
                ShowStatus("Сначала выбери заказ слева!");
                return;
            }

            if (!s.TryServeBase())
                ShowStatus("Блин не готов!");
            else
                ShowStatus("Базовый блин сдан!");
        }

        void ShowStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
            else
                Debug.Log($"[Serve] {msg}");
        }
    }
}
