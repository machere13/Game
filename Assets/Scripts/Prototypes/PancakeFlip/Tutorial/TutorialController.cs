using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdlePancake.PancakeFlip.TutorialCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class TutorialController : MonoBehaviour
    {
        public const string DoneKey = "pf_tutorial_done";

        [SerializeField] RectTransform orderPanelTarget;
        [SerializeField] RectTransform ingredientListTarget;
        [SerializeField] RectTransform cookButtonTarget;
        [SerializeField] IngredientsScreenView ingredientsScreen;

        sealed class Step
        {
            public string text;
            public RectTransform target;
            public Func<bool> isComplete;
            public bool blockInput;
        }

        readonly List<Step> _steps = new List<Step>();
        TutorialProgress _progress;
        bool _served;
        bool _running;

        RectTransform _root;
        Image _maskTop, _maskBottom, _maskLeft, _maskRight;
        TextMeshProUGUI _instruction;

        void Start()
        {
            if (PlayerPrefs.GetInt(DoneKey, 0) == 1) { gameObject.SetActive(false); return; }

            var s = GameSession.Instance;
            if (s != null) s.OnServed += OnServed;

            BuildOverlay();
            BuildSteps();
            _progress = new TutorialProgress(_steps.Count);
            _running = true;
            RefreshOverlay();
        }

        void OnDestroy()
        {
            var s = GameSession.Instance;
            if (s != null) s.OnServed -= OnServed;
        }

        void OnServed() => _served = true;

        void BuildSteps()
        {
            _steps.Clear();
            // Сначала готовим блин, заказ выбираем уже при сдаче.
            _steps.Add(new Step {
                text = "Нажми на плиту — открой ингредиенты",
                target = null, blockInput = false,
                isComplete = () => ingredientsScreen != null && ingredientsScreen.isActiveAndEnabled });
            _steps.Add(new Step {
                text = "Собери блин: добавь ингредиенты",
                target = ingredientListTarget, blockInput = true,
                isComplete = () => { var s = GameSession.Instance; return s != null && s.Build != null && s.Build.Slots.Count > 0; } });
            _steps.Add(new Step {
                text = "Поставь блин на сковороду — «Готовить»",
                target = cookButtonTarget, blockInput = true,
                isComplete = () => GameSession.Instance != null && GameSession.Instance.HasCookingPancake });
            _steps.Add(new Step {
                text = "Подбрось блин и прожарь обе стороны",
                target = null, blockInput = false,
                isComplete = () => GameSession.Instance != null && GameSession.Instance.IsPancakeCookedEnough() });
            _steps.Add(new Step {
                text = "Выбери заказ слева и дважды нажми — сдай блин",
                target = orderPanelTarget, blockInput = true,
                isComplete = () => _served });
        }

        void Update()
        {
            if (!_running || _progress == null) return;
            var step = Current();
            if (step != null && step.isComplete != null && step.isComplete())
            {
                _progress.Advance();
                if (_progress.IsComplete) { Finish(); return; }
                _served = false;
                RefreshOverlay();
            }
            else
            {
                LayoutMask(step);
            }
        }

        Step Current()
        {
            if (_progress == null || _progress.IsComplete) return null;
            if (_progress.CurrentIndex < 0 || _progress.CurrentIndex >= _steps.Count) return null;
            return _steps[_progress.CurrentIndex];
        }

        void RefreshOverlay()
        {
            var step = Current();
            if (step == null) return;
            if (_instruction != null) _instruction.text = step.text;
            LayoutMask(step);
        }

        void LayoutMask(Step step)
        {
            bool block = step != null && step.blockInput && step.target != null && step.target.gameObject.activeInHierarchy;
            SetMaskActive(block);
            if (!block) return;

            var c = new Vector3[4];
            step.target.GetWorldCorners(c);
            const float pad = 0.012f;
            float w = Mathf.Max(1f, Screen.width);
            float h = Mathf.Max(1f, Screen.height);
            float xMin = Mathf.Clamp01(Mathf.Min(c[0].x, c[1].x) / w - pad);
            float xMax = Mathf.Clamp01(Mathf.Max(c[2].x, c[3].x) / w + pad);
            float yMin = Mathf.Clamp01(Mathf.Min(c[0].y, c[3].y) / h - pad);
            float yMax = Mathf.Clamp01(Mathf.Max(c[1].y, c[2].y) / h + pad);

            SetAnchors(_maskTop, 0f, yMax, 1f, 1f);
            SetAnchors(_maskBottom, 0f, 0f, 1f, yMin);
            SetAnchors(_maskLeft, 0f, yMin, xMin, yMax);
            SetAnchors(_maskRight, xMax, yMin, 1f, yMax);
        }

        void SetMaskActive(bool on)
        {
            if (_maskTop) _maskTop.gameObject.SetActive(on);
            if (_maskBottom) _maskBottom.gameObject.SetActive(on);
            if (_maskLeft) _maskLeft.gameObject.SetActive(on);
            if (_maskRight) _maskRight.gameObject.SetActive(on);
        }

        static void SetAnchors(Image img, float xMin, float yMin, float xMax, float yMax)
        {
            if (img == null) return;
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        void Finish()
        {
            _running = false;
            PlayerPrefs.SetInt(DoneKey, 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }

        void BuildOverlay()
        {
            _root = (RectTransform)transform;
            _root.anchorMin = Vector2.zero; _root.anchorMax = Vector2.one;
            _root.offsetMin = _root.offsetMax = Vector2.zero;

            var maskColor = new Color(0f, 0f, 0f, 0.6f);
            _maskTop = MakeMask("MaskTop", maskColor);
            _maskBottom = MakeMask("MaskBottom", maskColor);
            _maskLeft = MakeMask("MaskLeft", maskColor);
            _maskRight = MakeMask("MaskRight", maskColor);

            var f = PancakeFlipUiFonts.UiTmpFont;

            var boxGo = new GameObject("Instruction", typeof(RectTransform), typeof(Image));
            boxGo.transform.SetParent(_root, false);
            var boxRt = (RectTransform)boxGo.transform;
            boxRt.anchorMin = new Vector2(0.12f, 0.85f);
            boxRt.anchorMax = new Vector2(0.88f, 0.95f);
            boxRt.offsetMin = boxRt.offsetMax = Vector2.zero;
            var boxImg = boxGo.GetComponent<Image>();
            boxImg.color = new Color(0.1f, 0.1f, 0.12f, 0.88f);
            boxImg.raycastTarget = false;

            var txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(boxRt, false);
            var trt = (RectTransform)txtGo.transform;
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(16, 8); trt.offsetMax = new Vector2(-16, -8);
            _instruction = txtGo.AddComponent<TextMeshProUGUI>();
            if (f != null) _instruction.font = f;
            _instruction.fontSize = 40f;
            _instruction.color = Color.white;
            _instruction.alignment = TextAlignmentOptions.Center;
            _instruction.raycastTarget = false;
            _instruction.enableWordWrapping = true;

            var skipGo = new GameObject("SkipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            skipGo.transform.SetParent(_root, false);
            var srt = (RectTransform)skipGo.transform;
            srt.anchorMin = new Vector2(0.80f, 0.965f);
            srt.anchorMax = new Vector2(0.99f, 0.998f);
            srt.offsetMin = srt.offsetMax = Vector2.zero;
            var skipImg = skipGo.GetComponent<Image>();
            skipImg.color = new Color(0.5f, 0.2f, 0.2f, 0.92f);
            var gs = GameSession.Instance;
            if (gs != null && gs.CancelButtonSprite != null)
            {
                skipImg.sprite = gs.CancelButtonSprite;
                skipImg.type = Image.Type.Sliced;
                skipImg.color = Color.white;
            }

            var skipTxtGo = new GameObject("Text", typeof(RectTransform));
            skipTxtGo.transform.SetParent(skipGo.transform, false);
            var skrt = (RectTransform)skipTxtGo.transform;
            skrt.anchorMin = Vector2.zero; skrt.anchorMax = Vector2.one;
            skrt.offsetMin = skrt.offsetMax = Vector2.zero;
            var skipTxt = skipTxtGo.AddComponent<TextMeshProUGUI>();
            if (f != null) skipTxt.font = f;
            skipTxt.text = "Пропустить";
            skipTxt.fontSize = 28f;
            skipTxt.color = Color.white;
            skipTxt.alignment = TextAlignmentOptions.Center;
            skipTxt.raycastTarget = false;

            skipGo.GetComponent<Button>().onClick.AddListener(Finish);

            boxGo.transform.SetAsLastSibling();
            skipGo.transform.SetAsLastSibling();
        }

        Image MakeMask(string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_root, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = true;
            return img;
        }
    }
}
