using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class SimpleBackgroundColor : MonoBehaviour
    {
        [Header("Камера")]
        [SerializeField] Color cameraBackground = new Color(0.98f, 0.95f, 0.9f);

        [Header("Пол (опционально)")]
        [SerializeField] Color floorColor = new Color(0.6f, 0.5f, 0.4f);
        [SerializeField] string[] floorObjectNames = { "Plane", "Ground", "Floor", "Quad" };

        void Start()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = cameraBackground;
            }

            foreach (string name in floorObjectNames)
            {
                var go = GameObject.Find(name);
                if (go == null) continue;
                var r = go.GetComponent<Renderer>();
                if (r != null && r.sharedMaterial != null)
                {
                    r.material.color = floorColor;
                    break;
                }
            }
        }
    }
}
