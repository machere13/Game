using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class ServeButtonView : MonoBehaviour
    {
        [SerializeField] Button serveButton;
        [SerializeField] Button serveBaseButton;
        [SerializeField] TextMeshProUGUI statusText;

        void Start()
        {
            if (serveButton != null)
                serveButton.onClick.AddListener(OnServe);
            if (serveBaseButton != null)
                serveBaseButton.onClick.AddListener(OnServeBase);
            RefreshState();
        }

        void OnEnable()
        {
            var s = GameSession.Instance;
            if (s != null)
            {
                s.OnOrderSelected += OnOrderChanged;
                s.OnServed += RefreshState;
                if (s.Orders != null) s.Orders.OnChanged += RefreshState;
            }
            RefreshState();
        }

        void OnDisable()
        {
            var s = GameSession.Instance;
            if (s != null)
            {
                s.OnOrderSelected -= OnOrderChanged;
                s.OnServed -= RefreshState;
                if (s.Orders != null) s.Orders.OnChanged -= RefreshState;
            }
        }

        void OnOrderChanged(Order _) => RefreshState();

        // Раньше это писалось каждый кадр в Update (лишний TMP-rebuild). Теперь — только по событиям.
        void RefreshState()
        {
            var s = GameSession.Instance;
            if (s == null) return;

            bool hasOrder = s.ActiveOrder != null;
            if (serveButton != null) serveButton.interactable = hasOrder;
            if (serveBaseButton != null)
                serveBaseButton.gameObject.SetActive(hasOrder && s.BaseRecipe != null);

            if (statusText != null)
                statusText.text = hasOrder ? "" : "Выбери заказ";
        }

        void OnServe()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            if (!s.TryServe() && statusText != null)
                statusText.text = s.ActiveOrder != null && s.ActiveOrder.Recipe != null && !s.Inventory.HasIngredients(s.ActiveOrder.Recipe)
                    ? "Нет ингредиентов — «Магазин» или плита"
                    : ServeFailMessage(s);
        }

        void OnServeBase()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            if (!s.TryServeBase() && statusText != null)
                statusText.text = s.BaseRecipe != null && !s.Inventory.HasIngredients(s.BaseRecipe)
                    ? "Нет ингредиентов (нужен базовый набор)"
                    : ServeFailMessage(s);
        }

        static string ServeFailMessage(GameSession s)
        {
            return !s.IsPancakeCookedEnough() ? "Блин не готов!" : "Не удалось сдать";
        }
    }
}
