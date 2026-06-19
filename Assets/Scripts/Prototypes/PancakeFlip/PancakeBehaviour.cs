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
        Side _currentSide = Side.B;

        float _totalRotationDegrees;
        int _fullRotations;
        float _lastAngleDeg;
        float _flightSignedRotationSum;
        Side _sideAtThrow = Side.B;

        float _cookA;
        float _cookB;

        Vector2 _restPosition;

        public State CurrentState => _state;
        public Side CurrentSide => _currentSide;
        public int FullRotations => _fullRotations;
        public float CookA => _cookA;
        public float CookB => _cookB;

        public event System.Action<LandingResult> OnLanded;

        bool _activeCooking = true;
        public bool IsActiveCooking => _activeCooking;

        public void ResetCooking()
        {
            _cookA = 0f;
            _cookB = 0f;
            _currentSide = Side.B;
            if (_state == State.OnPan)
                ApplyRestOnPanPose();
        }

        public void SetActiveCooking(bool active)
        {
            _activeCooking = active;
            if (_sr != null) _sr.enabled = active;
            if (_col != null) _col.enabled = active;

            _cookA = 0f;
            _cookB = 0f;
            _currentSide = Side.B;
            _state = State.OnPan;
            _totalRotationDegrees = 0f;
            _fullRotations = 0;
            _flightSignedRotationSum = 0f;
            _lastAngleDeg = 0f;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _rb.gravityScale = 0f;
                _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            if (_panCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _panCol, true);

            if (active) ApplyRestOnPanPose();
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

        // Обновить позицию покоя (когда сцену переобрамляют под другой экран).
        public void SetRestPosition(Vector2 pos)
        {
            _restPosition = pos;
            if (_state == State.OnPan && _rb != null)
            {
                _rb.position = pos;
                transform.position = new Vector3(pos.x, pos.y, transform.position.z);
            }
        }

        // Лицо блина под текущий рецепт. Обратная сторона — то же изображение (отзеркаливается автоматически).
        // null — вернуть обычный блин (запасной спрайт).
        public void SetFaceArt(Sprite art)
        {
            var a = art != null ? art : _fallbackSprite;
            spriteFaceA = a;
            spriteFaceB = a;
            if (_state == State.OnPan)
                ApplyRestOnPanPose();
        }

        public void Throw(float verticalForce, float spinDegPerSec)
        {
            if (!_activeCooking) return;
            if (_state != State.OnPan) return;

            _rb.rotation = 0f;
            transform.rotation = Quaternion.identity;
            _lastAngleDeg = 0f;
            _flightSignedRotationSum = 0f;
            _sideAtThrow = _currentSide;

            _state = State.InFlight;
            _flightDuration = 0f;
            _totalRotationDegrees = 0f;
            _fullRotations = 0;

            if (_panCol != null && _col != null)
                Physics2D.IgnoreCollision(_col, _panCol, false);

            if (_sr != null)
            {
                _sr.flipX = false;
                _sr.flipY = false;
            }

            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.constraints = RigidbodyConstraints2D.None;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.gravityScale = config != null ? config.gravityScale : 1f;
            _rb.linearVelocity = new Vector2(0f, verticalForce);
            _rb.angularVelocity = -spinDegPerSec;
        }

        float _flightDuration;
        const float MaxFlightDuration = 6f;

        void FixedUpdate()
        {
            if (!_activeCooking) return;
            if (_state == State.InFlight)
            {
                _flightDuration += Time.fixedDeltaTime;
                if (_flightDuration > MaxFlightDuration)
                {
                    _flightDuration = 0f;
                    Land();
                    return;
                }

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
                _flightDuration = 0f;
                CookCurrentSide();
                ApplyRestOnPanPose();
            }
        }

        void UpdateSideFromAngle()
        {
            _currentSide = SideAfterRotation(_sideAtThrow, _flightSignedRotationSum);
        }

        // Сторона, лежащая на сковороде, определяется ОТНОСИТЕЛЬНО стороны на момент броска:
        // каждый чистый полуоборот (180°) переворачивает блин на другую сторону.
        // Полный оборот (360°) возвращает ту же сторону. Чётность полуоборотов решает всё.
        static Side SideAfterRotation(Side sideAtThrow, float netRotationDegrees)
        {
            int halfTurns = Mathf.RoundToInt(netRotationDegrees / 180f);
            bool flipped = (halfTurns & 1) != 0;
            if (!flipped) return sideAtThrow;
            return sideAtThrow == Side.A ? Side.B : Side.A;
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

                // Одинаковая картинка с двух сторон: обратную сторону показываем повёрнутой на 180°
                // (flipX+flipY), иначе у симметричного блина переворот незаметен.
                bool sameArt = resA == resB;
                // Стартовая сторона (B на сковороде) показывается натурально; повёрнута — другая.
                bool back = sameArt && _currentSide == Side.A;
                _sr.flipX = back;
                _sr.flipY = back;
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
