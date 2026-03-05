using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace IdlePancake.Prototypes.PancakeFlip.Editor
{
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
            var panSr = panGo.AddComponent<SpriteRenderer>();
            panSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            panSr.color = new Color(0.4f, 0.25f, 0.1f);
            panGo.AddComponent<BoxCollider2D>();
            var pan = panGo.AddComponent<PanBehaviour>();

            var pancakeGo = new GameObject("Pancake");
            pancakeGo.transform.position = new Vector3(0f, -2.5f, 0f);
            var pancakeSr = pancakeGo.AddComponent<SpriteRenderer>();
            pancakeSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            pancakeSr.color = new Color(1f, 0.9f, 0.6f);
            pancakeSr.transform.localScale = new Vector3(1.2f, 0.3f, 1f);
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
