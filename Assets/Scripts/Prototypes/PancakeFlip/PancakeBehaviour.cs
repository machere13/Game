using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PancakeBehaviour : MonoBehaviour
    {
        public enum State { OnPan, InFlight }
        public enum Side { A, B }

        [SerializeField] PancakeFlipConfig config;
        [SerializeField] Transform panCenter;

        Rigidbody2D _rb;
        State _state = State.OnPan;
        Side _currentSide = Side.A;

        float _totalRotationDegrees;
        int _fullRotations;
        float _lastAngleDeg;

        float _cookA;
        float _cookB;

        public State CurrentState => _state;
        public Side CurrentSide => _currentSide;
        public int FullRotations => _fullRotations;
        public float CookA => _cookA;
        public float CookB => _cookB;

        public event System.Action<LandingResult> OnLanded;

        public void ResetCooking()
        {
            _cookA = 0f;
            _cookB = 0f;
            _currentSide = Side.A;
        }

        public struct LandingResult
        {
            public int rotations;
            public Side sideDown;
            public float cookA;
            public float cookB;
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.None;
        }

        public void SetPanCenter(Transform center)
        {
            panCenter = center;
        }

        public void Throw(float verticalForce, float spinDegPerSec)
        {
            if (_state != State.OnPan) return;

            _state = State.InFlight;
            _totalRotationDegrees = 0f;
            _fullRotations = 0;
            _lastAngleDeg = _rb.rotation;

            _rb.gravityScale = config != null ? config.gravityScale : 1f;
            _rb.linearVelocity = new Vector2(0f, verticalForce);
            _rb.angularVelocity = -spinDegPerSec;
        }

        void FixedUpdate()
        {
            if (_state == State.InFlight)
            {
                float currentAngle = _rb.rotation;
                float delta = Mathf.DeltaAngle(_lastAngleDeg, currentAngle);
                _lastAngleDeg = currentAngle;
                _totalRotationDegrees += Mathf.Abs(delta);
                _fullRotations = Mathf.RoundToInt(_totalRotationDegrees / 360f);

                if (config != null && config.landingAssistStrength > 0.001f && panCenter != null)
                {
                    float dx = panCenter.position.x - transform.position.x;
                    _rb.linearVelocity = new Vector2(dx * config.landingAssistStrength, _rb.linearVelocity.y);
                }

                UpdateSideFromAngle();
            }
            else
            {
                CookCurrentSide();

                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.gravityScale = 0f;
                _rb.rotation = 0f;
                transform.rotation = Quaternion.identity;
            }
        }

        void UpdateSideFromAngle()
        {
            float angle = _rb.rotation % 360f;
            if (angle < 0f) angle += 360f;
            bool flipped = angle > 90f && angle < 270f;
            _currentSide = flipped ? Side.B : Side.A;
        }

        void CookCurrentSide()
        {
            if (config == null || config.cookTimePerSide <= 0f) return;
            float dt = Time.fixedDeltaTime / config.cookTimePerSide;
            if (_currentSide == Side.A)
                _cookA = Mathf.Clamp01(_cookA + dt);
            else
                _cookB = Mathf.Clamp01(_cookB + dt);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            if (_state != State.InFlight) return;
            if (!other.gameObject.TryGetComponent<PanBehaviour>(out _)) return;
            Land();
        }

        public void Land()
        {
            UpdateSideFromAngle();

            _state = State.OnPan;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.rotation = 0f;
            transform.rotation = Quaternion.identity;

            OnLanded?.Invoke(new LandingResult
            {
                rotations = _fullRotations,
                sideDown = _currentSide,
                cookA = _cookA,
                cookB = _cookB
            });
        }
    }
}
