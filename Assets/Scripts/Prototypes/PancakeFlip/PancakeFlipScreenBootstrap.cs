using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipScreenBootstrap : MonoBehaviour
    {
        // Целевая зона 9:16 (портрет).
        const int PortraitW = 540;
        const int PortraitH = 960;

        void Awake()
        {
            // Мобилки — жёстко портрет.
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;

#if UNITY_STANDALONE
            // Десктоп — окно 9:16, оконный режим.
            Screen.SetResolution(PortraitW, PortraitH, FullScreenMode.Windowed);
#endif
        }
    }
}
