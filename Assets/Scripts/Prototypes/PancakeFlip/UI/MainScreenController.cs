using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class MainScreenController : MonoBehaviour
    {
        [SerializeField] RecipeBookScreenView recipeBookScreen;
        [SerializeField] PanUpgradeScreenView upgradeScreen;
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] CustomerAnimator customerAnimator;
        [SerializeField] StoveView stove;
        [SerializeField] IngredientsScreenView ingredientsScreen;

        bool _shopResolved;

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

        public void OpenIngredientsShop()
        {
            // Ссылки берём из инспектора; если не назначены — ищем один раз и кешируем.
            if (!_shopResolved)
            {
                if (stove == null) stove = Object.FindFirstObjectByType<StoveView>();
                if (ingredientsScreen == null) ingredientsScreen = Object.FindFirstObjectByType<IngredientsScreenView>();
                _shopResolved = true;
            }

            if (stove != null)
                stove.Open();
            else if (ingredientsScreen != null)
                ingredientsScreen.Open();
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

            var pancake = s.Pancake;
            if (pancake != null)
            {
                float min = s.GetEffectivePerfectMin();
                if (pancake.CookA < min || pancake.CookB < min)
                {
                    ShowStatus(NotReadyMessage(pancake, min));
                    return;
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

        // Сообщение «не готов» с указанием недожаренных сторон — без хака со склейкой и .Trim().
        static string NotReadyMessage(PancakeBehaviour pancake, float min)
        {
            var sb = new System.Text.StringBuilder("Блин не готов!");
            if (pancake.CookA < min) sb.Append($" A:{pancake.CookA:P0}");
            if (pancake.CookB < min) sb.Append($" B:{pancake.CookB:P0}");
            return sb.ToString();
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
