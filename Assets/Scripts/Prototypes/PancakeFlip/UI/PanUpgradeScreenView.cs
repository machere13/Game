using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PanUpgradeScreenView : MonoBehaviour
    {
        [SerializeField] Transform upgradeListContainer;
        [SerializeField] GameObject upgradeRowPrefab;
        [SerializeField] Button closeButton;

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
            var s = GameSession.Instance;
            if (s == null || upgradeListContainer == null) return;

            foreach (Transform child in upgradeListContainer)
                Destroy(child.gameObject);

            var upgrades = s.AllUpgrades;
            if (upgrades == null) return;

            foreach (var upg in upgrades)
            {
                if (upg == null) continue;

                var go = upgradeRowPrefab != null
                    ? Instantiate(upgradeRowPrefab, upgradeListContainer)
                    : CreateDefaultRow(upgradeListContainer);

                bool owned = s.Upgrades.IsOwned(upg);
                bool locked = upg.unlockLevel > s.Wallet.Level;

                var texts = go.GetComponentsInChildren<Text>();
                if (texts.Length >= 1)
                {
                    string status = owned ? " [куплено]" : locked ? $" [ур.{upg.unlockLevel}]" : "";
                    texts[0].text = $"{upg.displayName}{status}";
                }

                var btn = go.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.interactable = !owned && !locked;
                    var captured = upg;
                    btn.onClick.AddListener(() =>
                    {
                        s.BuyUpgrade(captured);
                        Rebuild();
                    });
                    var btnText = btn.GetComponentInChildren<Text>();
                    if (btnText != null)
                        btnText.text = owned ? "OK" : $"{upg.coinCost}c";
                }
            }
        }

        static GameObject CreateDefaultRow(Transform parent)
        {
            var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var rect = row.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 64f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(row.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.fontSize = 26;
            label.color = new Color(0.18f, 0.15f, 0.12f);
            if (font != null) label.font = font;
            var le = labelGo.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            var btnGo = new GameObject("Btn", typeof(RectTransform));
            btnGo.transform.SetParent(row.transform, false);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.55f, 0.42f, 0.22f, 1f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            var btnLe = btnGo.AddComponent<LayoutElement>();
            btnLe.preferredWidth = 140;

            var btnTxtGo = new GameObject("Text", typeof(RectTransform));
            btnTxtGo.transform.SetParent(btnGo.transform, false);
            var btnTxt = btnTxtGo.AddComponent<Text>();
            btnTxt.fontSize = 24;
            btnTxt.alignment = TextAnchor.MiddleCenter;
            btnTxt.color = Color.white;
            if (font != null) btnTxt.font = font;
            var btnTxtRect = btnTxtGo.GetComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.offsetMin = Vector2.zero;
            btnTxtRect.offsetMax = Vector2.zero;

            return row;
        }
    }
}
