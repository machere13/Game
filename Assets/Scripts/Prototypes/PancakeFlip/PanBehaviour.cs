using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PanBehaviour : MonoBehaviour
    {
        [SerializeField] float nodAngle = 8f;
        [SerializeField] float nodDuration = 0.15f;

        Quaternion _restRotation;
        float _nodTimer;

        public Transform PanCenter => transform;

        void Awake()
        {
            _restRotation = transform.rotation;
        }

        void Update()
        {
            if (_nodTimer > 0f)
            {
                _nodTimer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(_nodTimer / nodDuration);
                float angle = Mathf.Sin(t * Mathf.PI) * nodAngle;
                transform.rotation = _restRotation * Quaternion.Euler(0f, 0f, angle);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, _restRotation, 10f * Time.deltaTime);
            }
        }

        public void PlayNod()
        {
            _nodTimer = nodDuration;
        }
    }
}
