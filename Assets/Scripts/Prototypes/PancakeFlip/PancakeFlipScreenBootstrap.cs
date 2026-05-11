using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipScreenBootstrap : MonoBehaviour
    {
        const int RefW = 1080;
        const int RefH = 1920;

        void Awake()
        {
#if UNITY_STANDALONE
            if (Screen.width > Screen.height)
                Screen.SetResolution(RefW, RefH, FullScreenMode.Windowed);
#endif
        }
    }
}
