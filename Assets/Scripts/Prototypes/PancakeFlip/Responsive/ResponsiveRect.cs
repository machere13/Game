using UnityEngine;
using IdlePancake.PancakeFlip.ResponsiveCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class ResponsiveRect : MonoBehaviour
    {
        [SerializeField] Vector2 portraitMin;
        [SerializeField] Vector2 portraitMax;
        [SerializeField] Vector2 landscapeMin;
        [SerializeField] Vector2 landscapeMax;

        RectTransform _rt;

        public void Configure(Vector2 pMin, Vector2 pMax, Vector2 lMin, Vector2 lMax)
        {
            portraitMin = pMin; portraitMax = pMax;
            landscapeMin = lMin; landscapeMax = lMax;
        }

        void OnEnable()
        {
            _rt = (RectTransform)transform;
            var rl = ResponsiveLayout.Instance;
            if (rl != null)
            {
                rl.OnOrientationChanged += Apply;
                Apply(rl.Mode);
            }
            else
            {
                Apply(OrientationMode.Portrait);
            }
        }

        void OnDisable()
        {
            var rl = ResponsiveLayout.Instance;
            if (rl != null) rl.OnOrientationChanged -= Apply;
        }

        void Apply(OrientationMode mode)
        {
            if (_rt == null) _rt = (RectTransform)transform;
            bool land = mode == OrientationMode.Landscape;
            _rt.anchorMin = land ? landscapeMin : portraitMin;
            _rt.anchorMax = land ? landscapeMax : portraitMax;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
