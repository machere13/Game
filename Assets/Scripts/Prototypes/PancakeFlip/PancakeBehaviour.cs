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
        Collider2D _col;
        Collider2D _panCol;
        State _state = State.OnPan;
        Side _currentSide = Side.A;

        float _totalRotationDegrees;
        int _fullRotations;
        float _lastAngleDeg;
        float _throwStartRotationDeg;

        float _cookA;
        float _cookB;

        Vector2 _restPosition;

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
            _col = GetComponent<Collider2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.None;
        }

        void Start()
        {
            _restPosition = _rb.position;

            if (panCenter != null)
            {
                _panCol = panCenter.GetComponent<Collider2D>();
                if (_panCol != null && _col != null)
                    Physics2D.IgnoreCollision(_col, _panCol, true);
            }
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
            _throwStartRotationDeg = _rb.rotation;

            if (_panCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _panCol, false);

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

                _rb.position = _restPosition;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.gravityScale = 0f;
                _rb.rotation = 0f;
                transform.rotation = Quaternion.identity;
            }
        }

        void UpdateSideFromAngle()
        {
            float net = Mathf.DeltaAngle(_throwStartRotationDeg, _rb.rotation);
            _currentSide = SideFromNetRotationDegrees(net);
        }

        static Side SideFromNetRotationDegrees(float netDegrees)
        {
            float a = Mathf.Repeat(netDegrees, 360f);
            if (a < 0f) a += 360f;
            bool flipped = a > 90f && a < 270f;
            return flipped ? Side.B : Side.A;
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
            if (_rb.linearVelocity.y > 0f) return;
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
            _rb.position = _restPosition;
            transform.rotation = Quaternion.identity;

            if (_panCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _panCol, true);

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
