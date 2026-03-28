using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class ServeButtonView : MonoBehaviour
    {
        [SerializeField] Button serveButton;
        [SerializeField] Button serveBaseButton;
        [SerializeField] Text statusText;

        void Start()
        {
            if (serveButton != null)
                serveButton.onClick.AddListener(OnServe);
            if (serveBaseButton != null)
                serveBaseButton.onClick.AddListener(OnServeBase);
        }

        void Update()
        {
            var s = GameSession.Instance;
            if (s == null) return;

            bool hasOrder = s.ActiveOrder != null;
            if (serveButton != null) serveButton.interactable = hasOrder;
            if (serveBaseButton != null)
                serveBaseButton.gameObject.SetActive(hasOrder && s.BaseRecipe != null);

            if (statusText != null)
            {
                if (!hasOrder)
                    statusText.text = "Выбери заказ";
                else
                    statusText.text = "";
            }
        }

        void OnServe()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            if (!s.TryServe())
            {
                if (statusText == null) return;
                var p = Object.FindFirstObjectByType<PancakeBehaviour>();
                float min = s.FlipConfig != null ? s.FlipConfig.perfectMin : 0.4f;
                if (p != null && (p.CookA < min || p.CookB < min))
                    statusText.text = "Блин не готов!";
                else if (s.ActiveOrder != null && s.ActiveOrder.Recipe != null && !s.Inventory.HasIngredients(s.ActiveOrder.Recipe))
                    statusText.text = "Нет ингредиентов — «Магазин» или плита";
                else
                    statusText.text = "Не удалось сдать";
            }
        }

        void OnServeBase()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            if (!s.TryServeBase())
            {
                if (statusText == null) return;
                var p = Object.FindFirstObjectByType<PancakeBehaviour>();
                float min = s.FlipConfig != null ? s.FlipConfig.perfectMin : 0.4f;
                if (p != null && (p.CookA < min || p.CookB < min))
                    statusText.text = "Блин не готов!";
                else if (s.BaseRecipe != null && !s.Inventory.HasIngredients(s.BaseRecipe))
                    statusText.text = "Нет ингредиентов (нужен базовый набор)";
                else
                    statusText.text = "Не удалось сдать";
            }
        }
    }
}
