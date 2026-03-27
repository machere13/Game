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
                if (statusText != null)
                    statusText.text = "Блин не готов!";
            }
        }

        void OnServeBase()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            if (!s.TryServeBase())
            {
                if (statusText != null)
                    statusText.text = "Блин не готов!";
            }
        }
    }
}
