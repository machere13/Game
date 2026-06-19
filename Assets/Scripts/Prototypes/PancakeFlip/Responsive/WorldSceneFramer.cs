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
        [SerializeField] Transform pan;
        [SerializeField] Transform pancake;
        [SerializeField] CustomerAnimator customer;

        const float BgOverscan = 1.24f;
        const float StoveColumnFill = 0.85f;
        // Доля высоты спрайта плиты, на которой находится конфорка (совпадает с BuildEverything).
        const float BurnerHeightFraction = 0.44f;

        // Базовая (запечённая в портрете) геометрия — захватывается один раз до первого ресейла.
        bool _captured;
        PancakeBehaviour _pancakeBh;
        float _baseBurnerY;
        float _basePanY;
        float _basePancakeY;

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
                // Захватываем базовую геометрию из ещё не изменённого (запечённого) масштаба плиты.
                if (!_captured)
                {
                    _baseBurnerY = BurnerYFor(stove.localScale.x, ortho);
                    _basePanY = pan != null ? pan.position.y : 0f;
                    _basePancakeY = pancake != null ? pancake.position.y : 0f;
                    _captured = true;
                }

                float sprW = stoveSr.sprite.bounds.size.x;
                float sc = (columnW * StoveColumnFill) / sprW;
                stove.localScale = Vector3.one * sc;
                float sprH = stoveSr.sprite.bounds.size.y * sc;
                Vector3 p = stove.position;
                stove.position = new Vector3(0f, -ortho + sprH * 0.5f - 0.2f, p.z);

                // Сдвигаем сковороду и блин на ту же дельту высоты конфорки, чтобы сохранить выравнивание.
                float delta = BurnerYFor(sc, ortho) - _baseBurnerY;
                if (pan != null)
                {
                    Vector3 pp = pan.position;
                    pan.position = new Vector3(pp.x, _basePanY + delta, pp.z);
                }
                if (pancake != null)
                {
                    Vector3 cp = pancake.position;
                    var np = new Vector3(cp.x, _basePancakeY + delta, cp.z);
                    pancake.position = np;
                    // Физика держит свою позицию покоя — обновляем и её, иначе блин снапнется обратно.
                    if (_pancakeBh == null) _pancakeBh = pancake.GetComponent<PancakeBehaviour>();
                    if (_pancakeBh != null) _pancakeBh.SetRestPosition(new Vector2(np.x, np.y));
                }
            }

            if (customer != null)
                customer.SetOffscreenBounds(camW * 0.5f + 2f, -camW * 0.5f - 2f);
        }

        // Мировая Y конфорки для заданного масштаба плиты (формула совпадает с BuildEverything).
        float BurnerYFor(float stoveScale, float ortho)
        {
            float sprH = stoveSr.sprite.bounds.size.y * stoveScale;
            float stoveY = -ortho + sprH * 0.5f - 0.2f;
            return stoveY + sprH * BurnerHeightFraction;
        }
    }
}
