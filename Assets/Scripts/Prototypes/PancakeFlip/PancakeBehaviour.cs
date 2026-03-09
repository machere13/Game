using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public sealed class PancakeBehaviour : MonoBehaviour
    {
        public enum State { OnPan, InFlight }

        [SerializeField] PancakeFlipConfig config;
        [SerializeField] Transform panCenter;
        [SerializeField] float landingSnapSpeed = 15f;

        Rigidbody2D _rb;
        State _state = State.OnPan;
        float _totalRotationDegrees;
        int _fullRotations;
        float _lastAngleDeg;
        Vector2 _restPosition;
        Vector3 _baseScale;

        public State CurrentState => _state;
        public int FullRotations => _fullRotations;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.constraints = RigidbodyConstraints2D.None;
            _baseScale = transform.localScale;
            if (panCenter != null)
                _restPosition = panCenter.position;
            else
                _restPosition = transform.position;
        }

        public void SetPanCenter(Transform center)
        {
            panCenter = center;
            if (panCenter != null)
            {
                _restPosition = panCenter.position;
                if (_state == State.OnPan)
                    transform.position = _restPosition;
            }
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
            if (_state == State.OnPan && panCenter != null)
            {
                _restPosition = panCenter.position;
                _rb.position = _restPosition;
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.gravityScale = 0f;
                _rb.rotation = 0f;
                transform.rotation = Quaternion.identity;
                return;
            }

            if (_state != State.InFlight) return;

            float currentAngle = _rb.rotation;
            float delta = Mathf.DeltaAngle(_lastAngleDeg, currentAngle);
            _lastAngleDeg = currentAngle;
            _totalRotationDegrees += Mathf.Abs(delta);
            _fullRotations = Mathf.FloorToInt(_totalRotationDegrees / 360f);

            if (config != null && config.landingAssistStrength > 0.001f && panCenter != null)
            {
                float dx = panCenter.position.x - transform.position.x;
                _rb.linearVelocity = new Vector2(dx * config.landingAssistStrength, _rb.linearVelocity.y);
            }
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            if (_state != State.InFlight) return;
            if (!other.gameObject.TryGetComponent<PanBehaviour>(out _)) return;

            Land();
        }

        public void Land()
        {
            _state = State.OnPan;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;

            if (panCenter != null)
            {
                _restPosition = panCenter.position;
                _rb.position = _restPosition;
            }

            _rb.rotation = 0f;
            transform.rotation = Quaternion.identity;

            OnLanded?.Invoke(_fullRotations);
        }

        void Update()
        {
        }

        public event System.Action<int> OnLanded;
    }
}
