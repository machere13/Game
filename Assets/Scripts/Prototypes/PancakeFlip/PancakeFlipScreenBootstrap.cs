using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class PancakeFlipScreenBootstrap : MonoBehaviour
    {
        const int DesktopW = 1280;
        const int DesktopH = 720;

        void Awake()
        {
#if UNITY_STANDALONE
            // Desktop is a landscape platform. Start in a resizable landscape window;
            // ResponsiveLayout handles any later resize.
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ||
                Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                return;
            Screen.SetResolution(DesktopW, DesktopH, FullScreenMode.Windowed);
#endif
        }
    }
}
