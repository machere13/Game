using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace IdlePancake.Prototypes.PancakeFlip
{
    /// <summary>MouseMemoirs: исходный TTF для SDF; в билде — ссылка Font на GameSession.</summary>
    public static class PancakeFlipUiFonts
    {
        public const string EditorAssetPath = "Assets/Art/PancakeFlip/MouseMemoirs.ttf";

        const int TmpAtlasSize = 4096;
        const int TmpSamplingPointSize = 72;
        const int TmpAtlasPadding = 8;

        static Font _fromInspector;
        static Font _cached;
        static TMP_FontAsset _tmpCached;

        public static void Configure(Font fontFromGameSession)
        {
            _fromInspector = fontFromGameSession;
            _cached = null;
            _tmpCached = null;
        }

        public static Font UiFont
        {
            get
            {
                if (_fromInspector != null)
                    return _fromInspector;
                if (_cached != null)
                    return _cached;
                _cached = ResolveFontWithoutInspector();
                return _cached;
            }
        }

        public static TMP_FontAsset UiTmpFont
        {
            get
            {
                EnsureTmpFromUiFont();
                return _tmpCached;
            }
        }

        static Font ResolveFontWithoutInspector()
        {
#if UNITY_EDITOR
            var ed = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(EditorAssetPath);
            if (ed != null)
                return ed;
#endif
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        static void EnsureTmpFromUiFont()
        {
            if (_tmpCached != null) return;
            var src = UiFont;
            if (src == null) return;
            _tmpCached = TMP_FontAsset.CreateFontAsset(
                src,
                TmpSamplingPointSize,
                TmpAtlasPadding,
                GlyphRenderMode.SDFAA,
                TmpAtlasSize,
                TmpAtlasSize);
            if (_tmpCached != null)
                _tmpCached.name = "PancakeFlip_MouseMemoirs_Runtime_SDF";
        }

        /// <summary>Проставляет SDF-шрифт всем TMP на сцене и поднимает слишком мелкие размеры.</summary>
        public static void ApplyToAllTextsInLoadedScenes()
        {
            var f = UiTmpFont;
            if (f == null) return;
            float min = PancakeFlipUiTypography.MinimumLegacyHudFontSize;
            foreach (var t in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t == null) continue;
                t.font = f;
                if (t.fontSize > 0f && t.fontSize < min)
                    t.fontSize = min;
            }
        }
    }
}
