using System;
using UnityEngine;
using IdlePancake.PancakeFlip.ResponsiveCore;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [DefaultExecutionOrder(-200)]
    public sealed class ResponsiveLayout : MonoBehaviour
    {
        public const float ColumnAspect = 9f / 16f;

        public static ResponsiveLayout Instance { get; private set; }

        public OrientationMode Mode { get; private set; }
        public event Action<OrientationMode> OnOrientationChanged;

        int _lastW, _lastH;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _lastW = Screen.width;
            _lastH = Screen.height;
            Mode = OrientationResolver.Initial(Aspect());
        }

        void Start()
        {
            OnOrientationChanged?.Invoke(Mode);
        }

        void Update()
        {
            if (Screen.width == _lastW && Screen.height == _lastH) return;
            _lastW = Screen.width;
            _lastH = Screen.height;
            Mode = OrientationResolver.Resolve(Aspect(), Mode);
            OnOrientationChanged?.Invoke(Mode);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        float Aspect() => Screen.height <= 0 ? 1f : (float)Screen.width / Screen.height;

        public PlayColumnMetrics ColumnMetrics(float orthoSize) =>
            PlayColumnCalculator.Compute(orthoSize, Aspect(), ColumnAspect);
    }
}
