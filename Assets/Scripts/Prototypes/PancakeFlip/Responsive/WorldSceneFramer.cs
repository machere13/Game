using UnityEngine;
using IdlePancake.PancakeFlip.ResponsiveCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [DefaultExecutionOrder(-150)]
    public sealed class WorldSceneFramer : MonoBehaviour
    {
        [SerializeField] Camera cam;
        [SerializeField] SpriteRenderer background;
        [SerializeField] SpriteRenderer bottomPanel;
        [SerializeField] Transform stove;
        [SerializeField] SpriteRenderer stoveSr;
        [SerializeField] CustomerAnimator customer;

        const float BgOverscan = 1.24f;
        const float StoveColumnFill = 0.85f;

        void OnEnable()
        {
            var rl = ResponsiveLayout.Instance;
            if (rl != null) rl.OnOrientationChanged += OnChanged;
            Reframe();
        }

        void OnDisable()
        {
            var rl = ResponsiveLayout.Instance;
            if (rl != null) rl.OnOrientationChanged -= OnChanged;
        }

        void OnChanged(OrientationMode _) => Reframe();

        void Reframe()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            float ortho = cam.orthographicSize;
            float camH = ortho * 2f;
            float camW = camH * cam.aspect;

            var metrics = PlayColumnCalculator.Compute(ortho, cam.aspect, ResponsiveLayout.ColumnAspect);
            float columnW = metrics.WorldWidth;

            if (background != null && background.sprite != null)
            {
                Vector3 s = background.sprite.bounds.size;
                float sc = Mathf.Max(camW / s.x, camH / s.y) * BgOverscan;
                background.transform.localScale = Vector3.one * sc;
            }

            if (bottomPanel != null && bottomPanel.sprite != null)
            {
                float bpSc = camW / bottomPanel.sprite.bounds.size.x;
                bottomPanel.transform.localScale = Vector3.one * bpSc;
                float bpH = bottomPanel.sprite.bounds.size.y * bpSc;
                Vector3 p = bottomPanel.transform.position;
                bottomPanel.transform.position = new Vector3(0f, -ortho + bpH * 0.5f, p.z);
            }

            if (stove != null && stoveSr != null && stoveSr.sprite != null)
            {
                float sprW = stoveSr.sprite.bounds.size.x;
                float sc = (columnW * StoveColumnFill) / sprW;
                stove.localScale = Vector3.one * sc;
                float sprH = stoveSr.sprite.bounds.size.y * sc;
                Vector3 p = stove.position;
                stove.position = new Vector3(0f, -ortho + sprH * 0.5f - 0.2f, p.z);
            }

            if (customer != null)
                customer.SetOffscreenBounds(camW * 0.5f + 2f, -camW * 0.5f - 2f);
        }
    }
}
