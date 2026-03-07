using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IdlePancake.Prototypes.PancakeFlip.Editor
{
#pragma warning disable 0618
    public static class PancakeFlipSceneSetup
    {
        [MenuItem("PancakeFlip/Setup Portrait Scene 9:16")]
        public static void SetupScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                cam.backgroundColor = new Color(0.95f, 0.9f, 0.85f);
            }

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var chargeGo = new GameObject("ChargeIndicator");
            chargeGo.transform.SetParent(canvasGo.transform, false);
            var chargeBg = chargeGo.AddComponent<Image>();
            chargeBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var chargeRect = chargeGo.GetComponent<RectTransform>();
            chargeRect.anchorMin = new Vector2(0.2f, 0.85f);
            chargeRect.anchorMax = new Vector2(0.8f, 0.92f);
            chargeRect.offsetMin = Vector2.zero;
            chargeRect.offsetMax = Vector2.zero;
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(chargeGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = Color.yellow;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 0f;
            var chargeView = chargeGo.AddComponent<ChargeIndicatorView>();
            SetSerialized(chargeView, "fillImage", fillImg);

            var inputZoneGo = new GameObject("InputZone");
            inputZoneGo.transform.SetParent(canvasGo.transform, false);
            var inputRect = inputZoneGo.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.offsetMin = Vector2.zero;
            inputRect.offsetMax = Vector2.zero;
            inputZoneGo.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
            var inputZoneComp = inputZoneGo.AddComponent<PancakeFlipInputZone>();

            var panGo = new GameObject("Pan");
            panGo.transform.position = new Vector3(0f, -3f, 0f);
            panGo.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            var panSr = panGo.AddComponent<SpriteRenderer>();
            panSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            panSr.color = new Color(0.4f, 0.25f, 0.1f);
            panSr.sortingOrder = 0;
            panGo.AddComponent<BoxCollider2D>();
            var pan = panGo.AddComponent<PanBehaviour>();

            var pancakeGo = new GameObject("Pancake");
            pancakeGo.transform.position = new Vector3(0f, -2.5f, 0f);
            pancakeGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var pancakeSr = pancakeGo.AddComponent<SpriteRenderer>();
            pancakeSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            pancakeSr.color = new Color(1f, 0.9f, 0.6f);
            pancakeSr.sortingOrder = 1;
            var pancakeRb = pancakeGo.AddComponent<Rigidbody2D>();
            pancakeRb.gravityScale = 0f;
            pancakeRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            pancakeGo.AddComponent<CircleCollider2D>();
            var pancakeBh = pancakeGo.AddComponent<PancakeBehaviour>();

            var controllerGo = new GameObject("PancakeFlipController");
            var controller = controllerGo.AddComponent<PancakeFlipController>();
            SetSerialized(controller, "pancake", pancakeBh);
            SetSerialized(controller, "pan", pan);
            SetSerialized(controller, "chargeIndicator", chargeView);
            SetSerialized(controller, "inputZone", inputRect);
            SetSerialized(inputZoneComp, "controller", controller);

            pancakeBh.SetPanCenter(pan.PanCenter);

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("PancakeFlip scene created. Create and assign PancakeFlipConfig (Create > IdlePancake > Prototypes > PancakeFlip Config) to PancakeFlipController and PancakeBehaviour.");
        }

        const string PanGuid = "e9107d6466d245d4d8b36637e369cbb4";
        const string PancakeGuid = "0534ba38780a54e4db8729cf9af9e4af";

        [MenuItem("PancakeFlip/Assign Pan and Pancake Sprites")]
        public static void AssignSprites()
        {
            AssetDatabase.Refresh();
            string panPath = "Assets/Art/PancakeFlip/Pan.png";
            string pancakePath = "Assets/Art/PancakeFlip/Pancake.png";
            ForceSpriteSingleAndReimport(panPath);
            ForceSpriteSingleAndReimport(pancakePath);
            Sprite panSprite = LoadSpriteByPathOrSearch(panPath, "Pan", PanGuid);
            Sprite pancakeSprite = LoadSpriteByPathOrSearch(pancakePath, "Pancake", PancakeGuid);

            bool panAssigned = false, pancakeAssigned = false;
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Pan")
                    {
                        var sr = t.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            if (panSprite != null) sr.sprite = panSprite;
                            t.localScale = new Vector3(0.4f, 0.4f, 1f);
                            sr.sortingOrder = 0;
                            EditorUtility.SetDirty(t);
                            panAssigned = true;
                        }
                    }
                    else if (t.name == "Pancake")
                    {
                        var sr = t.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            if (pancakeSprite != null) sr.sprite = pancakeSprite;
                            t.localScale = new Vector3(0.9f, 0.9f, 1f);
                            sr.sortingOrder = 1;
                            EditorUtility.SetDirty(t);
                            pancakeAssigned = true;
                        }
                    }
                }
            }

            if (panSprite == null)
            {
                bool exists = System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "Art/PancakeFlip/Pan.png"));
                if (exists)
                    Debug.LogWarning("PancakeFlip: Pan.png на диске есть, но спрайт не загрузился. В Project выбери Pan.png → Inspector → Texture Type: Sprite (2D and UI) → Apply.");
                else
                    Debug.LogWarning("PancakeFlip: Pan.png не найден. Путь к Assets: " + Application.dataPath + " — создай папки Art/PancakeFlip и положи туда Pan.png.");
            }
            else if (!panAssigned) Debug.LogWarning("PancakeFlip: в сцене нет объекта Pan со Sprite Renderer.");
            else Debug.Log("PancakeFlip: сковорода назначена.");
            if (pancakeSprite == null)
            {
                bool exists = System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "Art/PancakeFlip/Pancake.png"));
                if (exists)
                    Debug.LogWarning("PancakeFlip: Pancake.png на диске есть, но спрайт не загрузился. В Project выбери Pancake.png → Inspector → Texture Type: Sprite (2D and UI) → Apply.");
                else
                    Debug.LogWarning("PancakeFlip: Pancake.png не найден. Создай папки Art/PancakeFlip в Assets и положи туда Pancake.png.");
            }
            else if (!pancakeAssigned) Debug.LogWarning("PancakeFlip: в сцене нет объекта Pancake со Sprite Renderer.");
            else Debug.Log("PancakeFlip: блин назначен.");
        }

        static void ForceSpriteSingleAndReimport(string path)
        {
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, path.Replace("Assets/", "")))) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }
        }

        static Sprite LoadSpriteByPathOrSearch(string path, string nameForSearch, string knownGuid)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var o in all)
                if (o is Sprite s) return s;
            if (!string.IsNullOrEmpty(knownGuid))
            {
                var pathByGuid = AssetDatabase.GUIDToAssetPath(knownGuid);
                if (!string.IsNullOrEmpty(pathByGuid))
                {
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(pathByGuid);
                    if (sprite != null) return sprite;
                    all = AssetDatabase.LoadAllAssetsAtPath(pathByGuid);
                    foreach (var o in all)
                        if (o is Sprite s2) return s2;
                }
            }
            var guids = AssetDatabase.FindAssets(nameForSearch, new[] { "Assets" });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (!System.IO.Path.GetFileNameWithoutExtension(p).Equals(nameForSearch, System.StringComparison.OrdinalIgnoreCase)) continue;
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (sprite != null) return sprite;
                all = AssetDatabase.LoadAllAssetsAtPath(p);
                foreach (var o in all)
                    if (o is Sprite s2) return s2;
            }
            return null;
        }

        static void SetSerialized(Object obj, string propertyName, Object value)
        {
            var so = new SerializedObject(obj);
            var prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
