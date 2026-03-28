using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class MainScreenController : MonoBehaviour
    {
        [SerializeField] IngredientsScreenView ingredientsScreen;
        [SerializeField] PanUpgradeScreenView upgradeScreen;
        [SerializeField] Text statusText;
        [SerializeField] CustomerAnimator customerAnimator;

        public void OpenIngredients()
        {
            if (ingredientsScreen != null)
                ingredientsScreen.Open();
        }

        public void OpenUpgrades()
        {
            if (upgradeScreen != null)
                upgradeScreen.Open();
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
            if (!s.TryServe())
            {
                ShowStatus("Не удалось сдать блин");
            }
            else
            {
                ShowStatus("Заказ сдан!");
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
