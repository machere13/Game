using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class CoinFlySpawner : MonoBehaviour
    {
        [SerializeField] RectTransform parentCanvas;
        [SerializeField] Sprite coinSprite;
        [SerializeField] float flyDuration = 0.5f;
        [SerializeField] float coinSize = 64f;
        const int MaxCoins = 5;

        public void Fly(RectTransform from, RectTransform to, int count)
        {
            if (parentCanvas == null || from == null || to == null) return;
            int n = Mathf.Clamp(count, 1, MaxCoins);
            for (int i = 0; i < n; i++)
                StartCoroutine(FlyOne(from.position, to, i * 0.06f, i == n - 1));
        }

        IEnumerator FlyOne(Vector3 fromWorld, RectTransform to, float delay, bool last)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            var go = new GameObject("FlyCoin", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parentCanvas, false);
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(coinSize, coinSize);
            var img = go.GetComponent<Image>();
            img.sprite = coinSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.enabled = coinSprite != null;

            Vector3 startWorld = fromWorld + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.2f, 0.2f), 0f);
            rt.position = startWorld;

            float e = 0f;
            while (e < flyDuration && rt != null && to != null)
            {
                e += Time.deltaTime;
                float k = Mathf.Clamp01(e / flyDuration);
                float ease = 1f - (1f - k) * (1f - k); // ease-out
                rt.position = Vector3.Lerp(startWorld, to.position, ease);
                yield return null;
            }
            if (rt != null) Destroy(rt.gameObject);
            if (last && to != null) Juice.PunchScale(this, to, 0.25f, 0.2f);
        }
    }
}
