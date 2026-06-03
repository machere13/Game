using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(PancakeBehaviour))]
    public sealed class PancakePlasticity : MonoBehaviour
    {
        [Header("Приземление (главный эффект)")]
        [Tooltip("Насколько сильно блин сплющивается в момент удара (0..1)")]
        [Range(0f, 0.9f)] public float landingSquash = 0.4f;
        [Tooltip("Скорость колебаний после удара, Гц")]
        public float landingFrequency = 7f;
        [Tooltip("Затухание колебаний (больше — быстрее успокаивается)")]
        public float landingDamping = 6f;

        [Header("Бросок (вытяжка вверх)")]
        [Range(0f, 0.9f)] public float throwStretch = 0.28f;
        public float throwFrequency = 5f;
        public float throwDamping = 7f;

        [Header("Покой на сковороде (дыхание)")]
        [Tooltip("Амплитуда лёгкого желейного покачивания при готовке")]
        [Range(0f, 0.2f)] public float idleWobble = 0.05f;
        public float idleSpeed = 2.5f;

        [Header("Ограничители")]
        [Range(0.2f, 1f)] public float minScaleFactor = 0.5f;
        [Range(1f, 2f)] public float maxScaleFactor = 1.6f;
        [Tooltip("Доля «сохранения объёма»: насколько ось X компенсирует сжатие по Y")]
        [Range(0f, 1f)] public float volumePreserve = 0.6f;

        PancakeBehaviour _pancake;
        Vector3 _baseScale;

        float _impulseAmp;
        float _impulseFreq;
        float _impulseDamp;
        float _impulseTime;

        float _idlePhase;
        PancakeBehaviour.State _prevState;

        void Awake()
        {
            _pancake = GetComponent<PancakeBehaviour>();
            _baseScale = transform.localScale;
            _impulseAmp = 0f;
            _impulseTime = 999f;
        }

        void OnEnable()
        {
            if (_pancake != null)
            {
                _pancake.OnLanded += OnLanded;
                _prevState = _pancake.CurrentState;
            }
        }

        void OnDisable()
        {
            if (_pancake != null)
                _pancake.OnLanded -= OnLanded;
            transform.localScale = _baseScale;
        }

        void OnLanded(PancakeBehaviour.LandingResult result)
        {
            float intensity = Mathf.Clamp01(0.7f + result.rotations * 0.1f);
            Kick(landingSquash * intensity, landingFrequency, landingDamping);
        }

        void Kick(float amp, float freq, float damp)
        {
            _impulseAmp = amp;
            _impulseFreq = Mathf.Max(0.01f, freq);
            _impulseDamp = Mathf.Max(0f, damp);
            _impulseTime = 0f;
        }

        void LateUpdate()
        {
            var state = _pancake != null ? _pancake.CurrentState : PancakeBehaviour.State.OnPan;
            if (state != _prevState)
            {
                if (_prevState == PancakeBehaviour.State.OnPan && state == PancakeBehaviour.State.InFlight)
                    Kick(-throwStretch, throwFrequency, throwDamping);
                _prevState = state;
            }

            bool active = _pancake == null || _pancake.IsActiveCooking;
            if (!active)
            {
                transform.localScale = _baseScale;
                return;
            }

            float s = 0f;

            if (Mathf.Abs(_impulseAmp) > 0.0001f)
            {
                _impulseTime += Time.deltaTime;
                float env = Mathf.Exp(-_impulseDamp * _impulseTime);
                if (env < 0.001f)
                {
                    _impulseAmp = 0f;
                }
                else
                {
                    s += _impulseAmp * env * Mathf.Cos(2f * Mathf.PI * _impulseFreq * _impulseTime);
                }
            }

            if (state == PancakeBehaviour.State.OnPan && idleWobble > 0f)
            {
                _idlePhase += Time.deltaTime * idleSpeed;
                s += idleWobble * Mathf.Sin(_idlePhase);
            }

            float factorY = Mathf.Clamp(1f - s, minScaleFactor, maxScaleFactor);
            float factorX = Mathf.Clamp(1f + s * volumePreserve, minScaleFactor, maxScaleFactor);

            transform.localScale = new Vector3(_baseScale.x * factorX, _baseScale.y * factorY, _baseScale.z);
        }
    }
}
