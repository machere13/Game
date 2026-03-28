using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderListView : MonoBehaviour
    {
        [SerializeField] OrderCardView cardPrefab;
        [SerializeField] Transform container;
        [SerializeField] int maxCards = 3;
        [SerializeField] float slotHeight = 0.32f;
        [SerializeField] float gap = 0.02f;

        readonly System.Collections.Generic.List<OrderCardView> _cards = new();

        void Start()
        {
            for (int i = 0; i < maxCards; i++)
            {
                var card = Instantiate(cardPrefab, container);
                var rt = card.GetComponent<RectTransform>();

                float yMin = i * slotHeight + (i > 0 ? gap * 0.5f : 0);
                float yMax = (i + 1) * slotHeight - (i < maxCards - 1 ? gap * 0.5f : 0);

                rt.anchorMin = new Vector2(0, yMin);
                rt.anchorMax = new Vector2(1, yMax);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);

                card.gameObject.SetActive(false);
                _cards.Add(card);
            }

            Refresh();

            var s = GameSession.Instance;
            if (s != null)
            {
                s.Orders.OnChanged += Refresh;
                s.OnOrderSelected += OnOrderSelected;
            }
        }

        void OnDestroy()
        {
            var s = GameSession.Instance;
            if (s != null)
            {
                s.Orders.OnChanged -= Refresh;
                s.OnOrderSelected -= OnOrderSelected;
            }
        }

        void OnOrderSelected(Order _) => UpdateSelection();

        void Refresh()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            var visible = s.Orders.Visible;
            for (int i = 0; i < _cards.Count; i++)
            {
                if (i < visible.Count)
                    _cards[i].Bind(visible[i]);
                else
                    _cards[i].Bind(null);
            }
            UpdateSelection();
        }

        void UpdateSelection()
        {
            var s = GameSession.Instance;
            if (s == null) return;
            for (int i = 0; i < _cards.Count; i++)
            {
                var visible = s.Orders.Visible;
                bool sel = i < visible.Count && visible[i] == s.ActiveOrder;
                _cards[i].SetSelected(sel);
            }
        }
    }
}
