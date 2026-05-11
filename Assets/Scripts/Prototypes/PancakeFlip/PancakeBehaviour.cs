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
        [Tooltip("Внешний вид физической стороны A (когда она смотрит на игрока — другая сторона на сковороде).")]
        [SerializeField] Sprite spriteFaceA;
        [Tooltip("Внешний вид физической стороны B.")]
        [SerializeField] Sprite spriteFaceB;

        Rigidbody2D _rb;
        Collider2D _col;
        Collider2D _panCol;
        SpriteRenderer _sr;
        Sprite _fallbackSprite;
        State _state = State.OnPan;
        Side _currentSide = Side.A;

        float _totalRotationDegrees;
        int _fullRotations;
        float _lastAngleDeg;
        float _flightSignedRotationSum;

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
            if (_state == State.OnPan)
                ApplyRestOnPanPose();
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
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null)
                _fallbackSprite = _sr.sprite;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.simulated = true;
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.None;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
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

            if (_state == State.OnPan)
                ApplyRestOnPanPose();
        }

        public void SetPanCenter(Transform center)
        {
            panCenter = center;
        }

        public void Throw(float verticalForce, float spinDegPerSec)
        {
            if (_state != State.OnPan) return;

            _rb.rotation = 0f;
            transform.rotation = Quaternion.identity;
            _lastAngleDeg = 0f;
            _flightSignedRotationSum = 0f;

            _state = State.InFlight;
            _totalRotationDegrees = 0f;
            _fullRotations = 0;

            if (_panCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _panCol, false);

            if (_sr != null)
                _sr.flipX = false;

            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.constraints = RigidbodyConstraints2D.None;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
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
                _flightSignedRotationSum += delta;
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
                ApplyRestOnPanPose();
            }
        }

        void UpdateSideFromAngle()
        {
            _currentSide = SideFromNetRotationDegrees(_flightSignedRotationSum);
        }

        static Side SideFromNetRotationDegrees(float totalDegrees)
        {
            float a = Mathf.Repeat(totalDegrees, 360f);
            if (a < 0f) a += 360f;
            // 90°…270° — перевёрнута «другая» сторона к сковороде; края 90/270 — боком, считаем как до переворота
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
            float currentAngle = _rb.rotation;
            float delta = Mathf.DeltaAngle(_lastAngleDeg, currentAngle);
            _flightSignedRotationSum += delta;
            _lastAngleDeg = currentAngle;

            UpdateSideFromAngle();

            _state = State.OnPan;

            if (_panCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _panCol, true);

            ApplyRestOnPanPose();

            OnLanded?.Invoke(new LandingResult
            {
                rotations = _fullRotations,
                sideDown = _currentSide,
                cookA = _cookA,
                cookB = _cookB
            });
        }

        void ApplyRestOnPanPose()
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.None;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.gravityScale = 0f;
            _rb.angularVelocity = 0f;
            _rb.linearVelocity = Vector2.zero;

            Sprite resA = spriteFaceA != null ? spriteFaceA : _fallbackSprite;
            Sprite resB = spriteFaceB != null ? spriteFaceB : _fallbackSprite;
            if (_sr != null)
            {
                bool sideAOnPan = _currentSide == Side.A;
                _sr.sprite = sideAOnPan ? resB : resA;

                bool sameArt = resA == resB;
                _sr.flipX = sameArt && _currentSide == Side.B;
            }

            _rb.MoveRotation(0f);
            _rb.MovePosition(_restPosition);
            _rb.rotation = 0f;
            transform.rotation = Quaternion.identity;
        }

        void LateUpdate()
        {
            if (_state != State.OnPan) return;
            if (Mathf.Approximately(_rb.rotation, 0f))
                return;
            _rb.MoveRotation(0f);
            transform.rotation = Quaternion.identity;
        }
    }
}
