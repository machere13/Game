using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(Button))]
    public sealed class ButtonJuice : MonoBehaviour
    {
        void Start()
        {
            var btn = GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(OnClick);
        }

        void OnClick()
        {
            Juice.PunchScale(this, transform, 0.14f, 0.16f);
            if (Sfx.Instance != null) Sfx.Instance.Click();
        }
    }
}
