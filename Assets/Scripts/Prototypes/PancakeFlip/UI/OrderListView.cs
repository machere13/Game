using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class OrderListView : MonoBehaviour
    {
        [SerializeField] OrderCardView cardPrefab;
        [SerializeField] Transform container;
        [SerializeField] int maxCards = 4;

        readonly System.Collections.Generic.List<OrderCardView> _cards = new();

        void Start()
        {
            for (int i = 0; i < maxCards; i++)
            {
                var card = Instantiate(cardPrefab, container);
                card.gameObject.SetActive(false);
                _cards.Add(card);
            }
            Refresh();

            var s = GameSession.Instance;
            if (s != null)
            {
                s.Orders.OnChanged += Refresh;
                s.OnOrderSelected += _ => UpdateSelection();
            }
        }

        void OnDestroy()
        {
            var s = GameSession.Instance;
            if (s != null)
            {
                s.Orders.OnChanged -= Refresh;
                s.OnOrderSelected -= _ => UpdateSelection();
            }
        }

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
