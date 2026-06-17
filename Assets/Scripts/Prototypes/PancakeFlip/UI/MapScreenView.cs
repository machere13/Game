using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class MapScreenView : MonoBehaviour
    {
        [SerializeField] Transform nodesContainer;
        [SerializeField] Button closeButton;

        static readonly Color CurrentColor = new Color(0.36f, 0.62f, 0.40f, 1f);
        static readonly Color UnlockedColor = new Color(0.86f, 0.78f, 0.60f, 1f);
        static readonly Color LockedColor = new Color(0.45f, 0.42f, 0.38f, 1f);

        void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            Rebuild();
        }

        void Rebuild()
        {
            if (nodesContainer == null) return;
            foreach (Transform child in nodesContainer)
                Destroy(child.gameObject);

            var s = GameSession.Instance;
            if (s == null || s.WorldMap == null || s.WorldMap.locations == null || s.Map == null) return;

            var font = PancakeFlipUiFonts.UiTmpFont;
            var locations = s.WorldMap.locations;
            for (int i = 0; i < locations.Length; i++)
            {
                var loc = locations[i];
                if (loc == null) continue;
                BuildNode(nodesContainer, s, i, loc, font);
            }
        }

        void BuildNode(Transform parent, GameSession s, int index, LocationConfig loc, TMP_FontAsset font)
        {
            bool unlocked = s.Map.CanTravelTo(index);
            bool current = s.Map.CurrentIndex == index;

            var node = new GameObject($"Node{index}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            node.transform.SetParent(parent, false);
            var img = node.GetComponent<Image>();
            img.color = current ? CurrentColor : (unlocked ? UnlockedColor : LockedColor);
            if (loc.mapIcon != null) { img.sprite = loc.mapIcon; img.preserveAspect = true; }
            var le = node.GetComponent<LayoutElement>();
            le.minWidth = 220f; le.preferredWidth = 220f;
            le.minHeight = 260f; le.preferredHeight = 260f;

            var btn = node.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = unlocked;
            int captured = index;
            btn.onClick.AddListener(() =>
            {
                var gs = GameSession.Instance;
                if (gs == null) return;
                gs.TravelTo(captured);
                gameObject.SetActive(false);
            });

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(node.transform, false);
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.04f, 0.04f);
            lrt.anchorMax = new Vector2(0.96f, 0.96f);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.fontSize = PancakeFlipUiTypography.ModalBody;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.enableWordWrapping = true;

            string status;
            if (!unlocked) status = "\nЗакрыто";
            else if (current) status = $"\n{s.Map.OrdersInCurrent()}/{s.Map.OrdersToUnlockCurrent()}";
            else status = "";
            label.text = loc.displayName + status;
        }

        public void SetRefs(Transform nodes, Button close)
        {
            nodesContainer = nodes;
            closeButton = close;
        }
    }
}
