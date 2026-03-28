using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [DisallowMultipleComponent]
    public sealed class KitchenUiFrontLayer : MonoBehaviour
    {
        public const int SortOrder = 40;

        void Awake()
        {
            var c = GetComponent<Canvas>();
            if (c == null)
                c = gameObject.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = SortOrder;

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        void Start()
        {
            transform.SetAsLastSibling();
        }
    }
}
