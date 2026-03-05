using UnityEngine;
using UnityEngine.EventSystems;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class PancakeFlipInputZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] PancakeFlipController controller;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (controller != null)
                controller.OnPointerDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (controller != null)
                controller.OnPointerUp(eventData);
        }
    }
}
