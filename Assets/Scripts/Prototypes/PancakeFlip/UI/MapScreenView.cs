using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdlePancake.PancakeFlip.MapCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class MapScreenView : MonoBehaviour
    {
        [SerializeField] RectTransform markersContainer;
        [SerializeField] Sprite blockedSprite;
        [SerializeField] Sprite carSprite;
        [SerializeField] TextMeshProUGUI statusText;
        [SerializeField] Button closeButton;
        [SerializeField] GameObject buyModal;
        [SerializeField] TextMeshProUGUI buyTitleText;
        [SerializeField] TextMeshProUGUI buyCostText;
        [SerializeField] Button buyConfirmButton;
        [SerializeField] Button buyCloseButton;

        static readonly Color Green = new Color(0.30f, 0.65f, 0.30f);
        static readonly Color Yellow = new Color(0.85f, 0.68f, 0.20f);
        static readonly Color Red = new Color(0.70f, 0.25f, 0.22f);

        bool _hooked;
        bool _driving;

        void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            if (buyCloseButton != null) buyCloseButton.onClick.AddListener(CloseBuy);
            if (buyModal != null) buyModal.SetActive(false);
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            HookMap();
            CloseBuy();
            if (statusText != null) statusText.text = "";
            Rebuild();
        }

        void HookMap()
        {
            if (_hooked) return;
            var s = GameSession.Instance;
            if (s != null && s.Map != null) { s.Map.OnChanged += Rebuild; _hooked = true; }
        }

        void OnDisable()
        {
            var s = GameSession.Instance;
            if (s != null && s.Map != null) s.Map.OnChanged -= Rebuild;
            _hooked = false;
        }

        void Rebuild()
        {
            if (markersContainer == null) return;
            foreach (Transform ch in markersContainer) Destroy(ch.gameObject);

            var s = GameSession.Instance;
            if (s == null || s.WorldMap == null || s.WorldMap.locations == null || s.Map == null) return;

            int level = s.Wallet != null ? s.Wallet.Level : 1;
            var locs = s.WorldMap.locations;
            for (int i = 0; i < locs.Length; i++)
            {
                var loc = locs[i];
                if (loc == null) continue;
                BuildMarker(i, loc, s.Map.StateOf(i, level));
            }
        }

        void BuildMarker(int index, LocationConfig loc, CityState state)
        {
            var font = PancakeFlipUiFonts.UiTmpFont;

            var marker = new GameObject($"Marker{index}", typeof(RectTransform));
            marker.transform.SetParent(markersContainer, false);
            var mrt = (RectTransform)marker.transform;
            mrt.anchorMin = mrt.anchorMax = loc.mapPosition;
            mrt.pivot = new Vector2(0.5f, 0.5f);
            mrt.sizeDelta = new Vector2(220f, 250f);
            mrt.anchoredPosition = Vector2.zero;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(marker.transform, false);
            var irt = (RectTransform)iconGo.transform;
            irt.anchorMin = new Vector2(0.5f, 1f); irt.anchorMax = new Vector2(0.5f, 1f);
            irt.pivot = new Vector2(0.5f, 1f); irt.anchoredPosition = Vector2.zero;
            irt.sizeDelta = new Vector2(120f, 120f);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.sprite = (state == CityState.Locked && blockedSprite != null) ? blockedSprite : loc.mapIcon;
            iconImg.enabled = iconImg.sprite != null;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(marker.transform, false);
            var lrt = (RectTransform)labelGo.transform;
            lrt.anchorMin = new Vector2(0f, 0.32f); lrt.anchorMax = new Vector2(1f, 0.58f);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = loc.displayName;
            label.fontSize = 40f; label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
            label.enableWordWrapping = false;

            string btnText; Color btnColor;
            switch (state)
            {
                case CityState.Owned: btnText = "Войти"; btnColor = Green; break;
                case CityState.Buyable: btnText = "Купить"; btnColor = Yellow; break;
                default: btnText = "Закрыто"; btnColor = Red; break;
            }

            var btnGo = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(marker.transform, false);
            var brt = (RectTransform)btnGo.transform;
            brt.anchorMin = new Vector2(0.22f, 0.02f); brt.anchorMax = new Vector2(0.78f, 0.24f);
            brt.offsetMin = brt.offsetMax = Vector2.zero;
            var btnImg = btnGo.GetComponent<Image>();
            var gs = GameSession.Instance;
            Sprite btnSprite = gs == null ? null
                : state == CityState.Owned ? gs.SuccessButtonSprite
                : state == CityState.Buyable ? gs.ActionButtonSprite
                : gs.CancelButtonSprite;
            if (btnSprite != null) { btnImg.sprite = btnSprite; btnImg.type = Image.Type.Simple; btnImg.color = Color.white; }
            else btnImg.color = btnColor;
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;

            var btGo = new GameObject("Text", typeof(RectTransform));
            btGo.transform.SetParent(btnGo.transform, false);
            var btrt = (RectTransform)btGo.transform;
            btrt.anchorMin = Vector2.zero; btrt.anchorMax = Vector2.one; btrt.offsetMin = btrt.offsetMax = Vector2.zero;
            var bt = btGo.AddComponent<TextMeshProUGUI>();
            if (font != null) bt.font = font;
            bt.text = btnText; bt.fontSize = 26f; bt.color = Color.white;
            bt.alignment = TextAlignmentOptions.Center; bt.raycastTarget = false;

            int captured = index;
            int reqLevel = loc.requiredLevel;
            switch (state)
            {
                case CityState.Owned:
                    btn.onClick.AddListener(() =>
                    {
                        if (!_driving) StartCoroutine(DriveAndEnter(captured));
                    });
                    break;
                case CityState.Buyable:
                    btn.onClick.AddListener(() => OpenBuy(captured));
                    break;
                default:
                    btn.onClick.AddListener(() =>
                    {
                        if (statusText != null) statusText.text = $"Требуется уровень {reqLevel}";
                    });
                    break;
            }
        }

        IEnumerator DriveAndEnter(int target)
        {
            _driving = true;
            var s = GameSession.Instance;
            if (s == null || s.WorldMap == null || s.Map == null)
            {
                _driving = false;
                yield break;
            }

            var locs = s.WorldMap.locations;
            int from = s.Map.CurrentIndex;
            bool canDrive = carSprite != null && markersContainer != null
                && from != target
                && from >= 0 && from < locs.Length && target >= 0 && target < locs.Length
                && locs[from] != null && locs[target] != null;

            if (canDrive)
            {
                Vector2 fromPos = locs[from].mapPosition;
                Vector2 toPos = locs[target].mapPosition;

                var carGo = new GameObject("Car", typeof(RectTransform), typeof(Image));
                carGo.transform.SetParent(markersContainer, false);
                var crt = (RectTransform)carGo.transform;
                crt.pivot = new Vector2(0.5f, 0.5f);
                crt.sizeDelta = new Vector2(120f, 120f);
                crt.anchoredPosition = Vector2.zero;
                crt.localScale = toPos.x < fromPos.x ? new Vector3(-1f, 1f, 1f) : Vector3.one;
                var cimg = carGo.GetComponent<Image>();
                cimg.sprite = carSprite; cimg.preserveAspect = true; cimg.raycastTarget = false;
                crt.SetAsLastSibling();

                // Лёгкая дуга «по дороге»: контрольная точка смещена перпендикулярно курсу.
                Vector2 dir = toPos - fromPos;
                float len = dir.magnitude;
                Vector2 perp = len > 0.0001f ? new Vector2(-dir.y, dir.x) / len : Vector2.zero;
                Vector2 ctrl = (fromPos + toPos) * 0.5f + perp * 0.08f;

                const float dur = 1.6f;
                float e = 0f;
                while (e < dur)
                {
                    e += Time.deltaTime;
                    float k = Mathf.Clamp01(e / dur);
                    // smootherstep — плавный разгон и торможение с долгой «крейсерской» серединой.
                    float t = k * k * k * (k * (6f * k - 15f) + 10f);
                    float inv = 1f - t;
                    crt.anchorMin = crt.anchorMax =
                        inv * inv * fromPos + 2f * inv * t * ctrl + t * t * toPos;
                    yield return null;
                }
                Destroy(carGo);
            }

            s.TravelTo(target);
            _driving = false;
            gameObject.SetActive(false);
        }

        void OpenBuy(int index)
        {
            var s = GameSession.Instance;
            if (s == null || s.WorldMap == null || buyModal == null) return;
            var loc = s.WorldMap.locations[index];
            if (loc == null) return;

            buyModal.SetActive(true);
            buyModal.transform.SetAsLastSibling();
            if (buyTitleText != null) buyTitleText.text = loc.displayName;
            if (buyCostText != null) buyCostText.text = loc.cityCost.ToString();

            bool affordable = s.Wallet != null && s.Wallet.Coins >= loc.cityCost;
            if (buyConfirmButton != null)
            {
                buyConfirmButton.interactable = affordable;
                buyConfirmButton.onClick.RemoveAllListeners();
                buyConfirmButton.onClick.AddListener(() =>
                {
                    var gs = GameSession.Instance;
                    if (gs != null && gs.TryBuyCity(index)) { CloseBuy(); Rebuild(); }
                });
            }
        }

        void CloseBuy()
        {
            if (buyModal != null) buyModal.SetActive(false);
        }
    }
}
