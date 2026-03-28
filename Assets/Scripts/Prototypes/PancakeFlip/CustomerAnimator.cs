using System.Collections;
using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class CustomerAnimator : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sr;
        [SerializeField] Sprite[] personSprites;

        [Header("Walk Settings")]
        [SerializeField] float walkDuration = 1f;
        [SerializeField] float pickupPause = 0.3f;
        [SerializeField] float pickupScale = 1.15f;
        [SerializeField] float startX = 8f;
        [SerializeField] float targetX = 0f;
        [SerializeField] float exitX = -8f;

        float _baseScale;
        bool _busy;

        void Awake()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale.x;
            gameObject.SetActive(false);
        }

        public void Setup(float rightOffscreen, float counterX, float leftOffscreen)
        {
            startX = rightOffscreen;
            targetX = counterX;
            exitX = leftOffscreen;
        }

        public void PlayServe(int personIndex = -1)
        {
            if (_busy) return;
            gameObject.SetActive(true);

            if (personSprites != null && personSprites.Length > 0 && sr != null)
            {
                int idx = (personIndex >= 0 && personIndex < personSprites.Length)
                    ? personIndex : Random.Range(0, personSprites.Length);
                sr.sprite = personSprites[idx];
            }

            StartCoroutine(ServeRoutine());
        }

        IEnumerator ServeRoutine()
        {
            _busy = true;
            float y = transform.position.y;
            float z = transform.position.z;

            transform.position = new Vector3(startX, y, z);
            SetScale(_baseScale);

            yield return WalkX(startX, targetX, walkDuration, y, z);

            yield return ScalePop(_baseScale, _baseScale * pickupScale, pickupPause * 0.5f);
            yield return ScalePop(_baseScale * pickupScale, _baseScale, pickupPause * 0.5f);

            sr.flipX = true;
            yield return WalkX(targetX, exitX, walkDuration, y, z);
            sr.flipX = false;

            gameObject.SetActive(false);
            _busy = false;
        }

        IEnumerator WalkX(float from, float to, float dur, float y, float z)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float x = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, t / dur));
                transform.position = new Vector3(x, y, z);
                yield return null;
            }
            transform.position = new Vector3(to, y, z);
        }

        IEnumerator ScalePop(float from, float to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                SetScale(Mathf.Lerp(from, to, t / dur));
                yield return null;
            }
            SetScale(to);
        }

        void SetScale(float s) => transform.localScale = new Vector3(s, s, 1f);
    }
}
