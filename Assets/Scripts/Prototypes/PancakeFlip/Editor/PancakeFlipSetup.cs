using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

namespace IdlePancake.Prototypes.PancakeFlip.Editor
{
#pragma warning disable 0618
    public static class PancakeFlipSetup
    {
        const string ArtDir = "Assets/Art/PancakeFlip";
        const string DataDir = "Assets/Data/PancakeFlip";

        [MenuItem("PancakeFlip/Build Everything")]
        public static void BuildEverything()
        {
            if (Application.isPlaying) { Debug.LogWarning("Останови Play."); return; }

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            if (cam != null) { cam.orthographic = true; cam.orthographicSize = 5f; cam.backgroundColor = new Color(0.2f, 0.2f, 0.25f); }
            float ortho = 5f;

            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            ForceAllSprites();
            var bgSpr = LoadSprite("Background");
            var panSpr = LoadSprite("Pan");
            var pancakeSpr = LoadSprite("Pancake");
            var stoveClosedS = LoadSprite("image 5");
            var stoveOpenS = LoadSprite("image 4");
            var orderListSpr = LoadSprite("OrderList");
            var orderItemSpr = LoadSprite("OrderItem");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureFolder("Assets/Data");
            EnsureFolder(DataDir);
            var flipConfig = GetOrCreate<PancakeFlipConfig>(DataDir, "PancakeFlipConfig");
            var levelTable = GetOrCreate<LevelTableConfig>(DataDir, "LevelTable");
            var dough = CreateIngredient("Тесто", 0, 0, true);
            var jam = CreateIngredient("Варенье", 5, 2, false);
            var cheese = CreateIngredient("Сыр", 8, 3, false);
            var baseRecipe = CreateRecipe("Базовый блин", 0, 3, 5, new[] { new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 } });
            var jamRecipe = CreateRecipe("Блин с вареньем", 2, 12, 25, new[] {
                new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                new RecipeConfig.IngredientSlot { ingredient = jam, amount = 1 } });
            var cheeseRecipe = CreateRecipe("Блин с сыром", 3, 18, 35, new[] {
                new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                new RecipeConfig.IngredientSlot { ingredient = cheese, amount = 1 } });
            var upg1 = CreateUpgrade("Широкая норма", 30, 1, PanUpgradeConfig.EffectType.WiderPerfectZone, 1.2f);
            var upg2 = CreateUpgrade("Медленный пережар", 50, 2, PanUpgradeConfig.EffectType.SlowerOvercook, 1.3f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (bgSpr != null)
            {
                var bg = new GameObject("Background");
                var bgSr = bg.AddComponent<SpriteRenderer>(); bgSr.sprite = bgSpr; bgSr.sortingOrder = -100;
                float camH = ortho * 2f, camW = camH * (9f / 16f);
                float sc = Mathf.Max(camW / bgSpr.bounds.size.x, camH / bgSpr.bounds.size.y);
                bg.transform.localScale = Vector3.one * sc;
                bg.transform.position = new Vector3(0, 0, 10);
            }

            float stoveScale = 0.55f, burnerY = -1.5f;
            if (stoveClosedS != null)
            {
                var stoveGo = new GameObject("Stove");
                var stoveSr = stoveGo.AddComponent<SpriteRenderer>(); stoveSr.sprite = stoveClosedS; stoveSr.sortingOrder = -2;
                float sprW = stoveClosedS.bounds.size.x;
                float camW2 = ortho * 2f * (9f / 16f);
                stoveScale = (camW2 * 0.85f) / sprW;
                float sprH = stoveClosedS.bounds.size.y * stoveScale;
                float stoveY = -ortho + sprH * 0.5f - 0.2f;
                stoveGo.transform.localScale = Vector3.one * stoveScale;
                stoveGo.transform.position = new Vector3(0, stoveY, 0);
                burnerY = stoveY + sprH * 0.44f;
                var col = stoveGo.AddComponent<BoxCollider2D>(); col.isTrigger = true;
                stoveGo.AddComponent<StoveView>();
                SetField(stoveGo.GetComponent<StoveView>(), "spriteRenderer", stoveSr);
                SetField(stoveGo.GetComponent<StoveView>(), "closedSprite", stoveClosedS);
                if (stoveOpenS != null) SetField(stoveGo.GetComponent<StoveView>(), "openSprite", stoveOpenS);
            }

            const float worldScale = 0.25f;
            const float panLiftWorld = 0.12f;

            var panGo = new GameObject("Pan");
            panGo.transform.position = new Vector3(0, burnerY + panLiftWorld, 0);
            panGo.transform.localScale = Vector3.one * worldScale;
            var panSrC = panGo.AddComponent<SpriteRenderer>();
            panSrC.sprite = panSpr != null ? panSpr : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            panSrC.color = panSpr != null ? Color.white : new Color(0.4f, 0.25f, 0.1f);
            panSrC.sortingOrder = 0;
            panGo.AddComponent<BoxCollider2D>();
            var panBh = panGo.AddComponent<PanBehaviour>();

            const float pancakeY = -1.2f;

            var pcGo = new GameObject("Pancake");
            pcGo.transform.position = new Vector3(0, pancakeY, 0);
            pcGo.transform.localScale = Vector3.one * worldScale;
            var pcSr = pcGo.AddComponent<SpriteRenderer>();
            pcSr.sprite = pancakeSpr != null ? pancakeSpr : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            pcSr.color = pancakeSpr != null ? Color.white : new Color(1, 0.9f, 0.6f);
            pcSr.sortingOrder = 1;
            var rb = pcGo.AddComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            pcGo.AddComponent<CircleCollider2D>();
            var pcBh = pcGo.AddComponent<PancakeBehaviour>();
            pcBh.SetPanCenter(panBh.PanCenter);
            SetField(pcBh, "config", flipConfig);

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            var topBar = MkPanel(canvas.transform, "TopBar", V2(0, 0.95f), V2(1, 1), new Color(0.1f, 0.1f, 0.15f, 0.5f));
            var topHlg = topBar.AddComponent<HorizontalLayoutGroup>();
            topHlg.padding = new RectOffset(20, 20, 5, 5); topHlg.spacing = 15;
            topHlg.childAlignment = TextAnchor.MiddleCenter; topHlg.childForceExpandWidth = false;
            MkLabel(topBar.transform, "CoinIcon", "\u00a4", font, 28, Color.yellow, 35);
            var coinTxt = MkLabel(topBar.transform, "CoinText", "0", font, 32, Color.white, 120);
            var lvlTxt = MkLabel(topBar.transform, "LevelText", "Ур.1", font, 28, Color.white, 80);
            var xpBg = new GameObject("XpBg", typeof(RectTransform), typeof(Image));
            xpBg.transform.SetParent(topBar.transform, false);
            xpBg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f);
            var xpLe2 = xpBg.AddComponent<LayoutElement>(); xpLe2.flexibleWidth = 1; xpLe2.preferredHeight = 24;
            var xpFill = new GameObject("XpFill", typeof(RectTransform), typeof(Image));
            xpFill.transform.SetParent(xpBg.transform, false); Fill(xpFill);
            var xpImg = xpFill.GetComponent<Image>(); xpImg.color = new Color(0.3f, 0.8f, 1f);
            xpImg.type = Image.Type.Filled; xpImg.fillMethod = Image.FillMethod.Horizontal; xpImg.fillAmount = 0;
            var tbv = topBar.AddComponent<TopBarView>();
            SetField(tbv, "coinsText", coinTxt.GetComponent<Text>());
            SetField(tbv, "levelText", lvlTxt.GetComponent<Text>());
            SetField(tbv, "xpFill", xpImg);

            var orderPanel = MkPanel(canvas.transform, "OrderPanel", V2(0, 0.32f), V2(0.36f, 0.98f), new Color(0, 0, 0, 0));

            if (orderListSpr != null)
            {
                var rope = new GameObject("Rope", typeof(RectTransform), typeof(Image));
                rope.transform.SetParent(orderPanel.transform, false);
                var ropeR = rope.GetComponent<RectTransform>();
                ropeR.anchorMin = V2(0.08f, 0.15f); ropeR.anchorMax = V2(0.24f, 1.45f);
                ropeR.offsetMin = ropeR.offsetMax = Vector2.zero;
                var ropeI = rope.GetComponent<Image>(); ropeI.sprite = orderListSpr;
                ropeI.type = Image.Type.Simple; ropeI.preserveAspect = false; ropeI.raycastTarget = false;
            }

            var cardsContainer = new GameObject("Cards", typeof(RectTransform));
            cardsContainer.transform.SetParent(orderPanel.transform, false);
            var cardsRect = cardsContainer.GetComponent<RectTransform>();
            cardsRect.anchorMin = V2(0, 0); cardsRect.anchorMax = V2(0.75f, 1);
            cardsRect.offsetMin = cardsRect.offsetMax = Vector2.zero;

            var cardPrefab = MkOrderCard(cardsContainer.transform, font, orderItemSpr);
            cardPrefab.gameObject.SetActive(false);
            var olv = orderPanel.AddComponent<OrderListView>();
            SetField(olv, "cardPrefab", cardPrefab);
            SetField(olv, "container", cardsContainer.transform);

            var chargeGo = MkPanel(canvas.transform, "ChargeIndicator", V2(0.25f, 0.88f), V2(0.75f, 0.94f), new Color(0.15f, 0.15f, 0.15f, 0.7f));
            var fillGo2 = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo2.transform.SetParent(chargeGo.transform, false); Fill(fillGo2);
            var fillI = fillGo2.GetComponent<Image>(); fillI.color = Color.yellow;
            fillI.type = Image.Type.Filled; fillI.fillMethod = Image.FillMethod.Horizontal; fillI.fillAmount = 0;
            var cv = chargeGo.AddComponent<ChargeIndicatorView>(); SetField(cv, "fillImage", fillI);

            var popup = MkLabel(canvas.transform, "RotationsPopup", "", font, 52, new Color(1, 0.95f, 0.7f), 0);
            var popupR = popup.GetComponent<RectTransform>();
            popupR.anchorMin = V2(0.5f, 0.5f); popupR.anchorMax = V2(0.5f, 0.5f);
            popupR.sizeDelta = new Vector2(400, 100);
            popup.GetComponent<Text>().fontStyle = FontStyle.Bold;

            var scoreUI = new GameObject("ScoreUI", typeof(RectTransform));
            scoreUI.transform.SetParent(canvas.transform, false);
            var sv = scoreUI.AddComponent<PancakeFlipScoreView>();
            SetField(sv, "pancake", pcBh); SetField(sv, "config", flipConfig);
            SetField(sv, "rotationsPopupText", popup.GetComponent<Text>());

            var cookRoot = MkPanel(canvas.transform, "CookingIndicators", V2(0.88f, 0.35f), V2(0.97f, 0.7f), new Color(0, 0, 0, 0));
            MkCookBar(cookRoot, "A", V2(0, 0.55f), V2(1, 1), font, out Image barA, out Text lblA);
            MkCookBar(cookRoot, "B", V2(0, 0), V2(1, 0.45f), font, out Image barB, out Text lblB);
            var civ = cookRoot.AddComponent<CookingIndicatorView>();
            SetField(civ, "pancake", pcBh); SetField(civ, "config", flipConfig);
            SetField(civ, "barA", barA); SetField(civ, "labelA", lblA);
            SetField(civ, "barB", barB); SetField(civ, "labelB", lblB);

            var ingScr = MkPanel(canvas.transform, "IngredientsScreen", V2(0.05f, 0.18f), V2(0.95f, 0.93f), new Color(0.1f, 0.12f, 0.1f, 0.95f));
            var iVlg = ingScr.AddComponent<VerticalLayoutGroup>(); iVlg.padding = new RectOffset(20, 20, 20, 20); iVlg.spacing = 10; iVlg.childForceExpandHeight = false;
            MkLabel(ingScr.transform, "Title", "Ингредиенты", font, 38, Color.white, 0).AddComponent<LayoutElement>().preferredHeight = 50;
            var iList = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            iList.transform.SetParent(ingScr.transform, false);
            iList.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            iList.GetComponent<VerticalLayoutGroup>().spacing = 8;
            iList.AddComponent<LayoutElement>().flexibleHeight = 1;
            var iCloseBtn = MkButton(ingScr.transform, "CloseBtn", "Закрыть", font, new Color(0.6f, 0.2f, 0.2f));
            iCloseBtn.AddComponent<LayoutElement>().preferredHeight = 55;
            var isv = ingScr.AddComponent<IngredientsScreenView>();
            SetField(isv, "ingredientListContainer", iList.transform);
            SetField(isv, "closeButton", iCloseBtn.GetComponent<Button>());

            var stoveV = Object.FindObjectOfType<StoveView>();
            if (stoveV != null) { SetField(stoveV, "ingredientsScreen", isv); SetField(isv, "stove", stoveV); }

            var upgScr = MkPanel(canvas.transform, "UpgradeScreen", V2(0.05f, 0.18f), V2(0.95f, 0.93f), new Color(0.12f, 0.1f, 0.08f, 0.95f));
            var uVlg = upgScr.AddComponent<VerticalLayoutGroup>(); uVlg.padding = new RectOffset(20, 20, 20, 20); uVlg.spacing = 10; uVlg.childForceExpandHeight = false;
            MkLabel(upgScr.transform, "Title", "Апгрейды сковороды", font, 38, Color.white, 0).AddComponent<LayoutElement>().preferredHeight = 50;
            var uList = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            uList.transform.SetParent(upgScr.transform, false);
            uList.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            uList.GetComponent<VerticalLayoutGroup>().spacing = 8;
            uList.AddComponent<LayoutElement>().flexibleHeight = 1;
            var uCloseBtn = MkButton(upgScr.transform, "CloseBtn", "Закрыть", font, new Color(0.6f, 0.2f, 0.2f));
            uCloseBtn.AddComponent<LayoutElement>().preferredHeight = 55;
            var usv = upgScr.AddComponent<PanUpgradeScreenView>();
            SetField(usv, "upgradeListContainer", uList.transform);
            SetField(usv, "closeButton", uCloseBtn.GetComponent<Button>());

            var ctrlGo = new GameObject("PancakeFlipController");
            var ctrl = ctrlGo.AddComponent<PancakeFlipController>();
            SetField(ctrl, "pancake", pcBh); SetField(ctrl, "pan", panBh);
            SetField(ctrl, "chargeIndicator", cv); SetField(ctrl, "config", flipConfig);

            var sessGo = new GameObject("GameSession");
            var sess = sessGo.AddComponent<GameSession>();
            SetField(sess, "flipConfig", flipConfig); SetField(sess, "levelTable", levelTable);
            SetField(sess, "pancake", pcBh); SetField(sess, "baseRecipe", baseRecipe);
            SetFieldArr(sess, "startingRecipes", new Object[] { baseRecipe, jamRecipe, cheeseRecipe });
            SetFieldArr(sess, "allIngredients", new Object[] { dough, jam, cheese });
            SetFieldArr(sess, "allUpgrades", new Object[] { upg1, upg2 });

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("PancakeFlip: всё создано одной кнопкой.");
        }

        static IngredientConfig CreateIngredient(string n, int cost, int lvl, bool inf)
        {
            var o = GetOrCreate<IngredientConfig>(DataDir, n);
            o.displayName = n; o.coinCost = cost; o.unlockLevel = lvl; o.infinite = inf;
            EditorUtility.SetDirty(o); return o;
        }
        static RecipeConfig CreateRecipe(string n, int lvl, int coins, int xp, RecipeConfig.IngredientSlot[] ing)
        {
            var o = GetOrCreate<RecipeConfig>(DataDir, n);
            o.displayName = n; o.unlockLevel = lvl; o.rewardCoins = coins; o.rewardXp = xp; o.ingredients = ing;
            EditorUtility.SetDirty(o); return o;
        }
        static PanUpgradeConfig CreateUpgrade(string n, int cost, int lvl, PanUpgradeConfig.EffectType t, float v)
        {
            var o = GetOrCreate<PanUpgradeConfig>(DataDir, n);
            o.displayName = n; o.coinCost = cost; o.unlockLevel = lvl; o.effectType = t; o.effectValue = v;
            EditorUtility.SetDirty(o); return o;
        }
        static T GetOrCreate<T>(string folder, string name) where T : ScriptableObject
        {
            string p = $"{folder}/{name}.asset";
            var e = AssetDatabase.LoadAssetAtPath<T>(p);
            if (e != null) return e;
            var o = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(o, p); return o;
        }

        static void ForceAllSprites()
        {
            foreach (var n in new[] { "Background", "OrderList", "OrderItem", "image 5", "image 4", "Pan", "Pancake" })
            {
                string p = $"{ArtDir}/{n}.png";
                if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, p.Replace("Assets/", "")))) continue;
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                if (imp == null) continue;
                bool c = false;
                if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; c = true; }
                if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; c = true; }
                if (c) imp.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }
        static Sprite LoadSprite(string n)
        {
            string p = $"{ArtDir}/{n}.png";
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
            if (s != null) return s;
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(p)) if (o is Sprite sp) return sp;
            return null;
        }

        static Vector2 V2(float x, float y) => new(x, y);
        static GameObject MkPanel(Transform p, string n, Vector2 amin, Vector2 amax, Color c)
        {
            var g = new GameObject(n, typeof(RectTransform), typeof(Image));
            g.transform.SetParent(p, false);
            var r = g.GetComponent<RectTransform>(); r.anchorMin = amin; r.anchorMax = amax;
            r.offsetMin = r.offsetMax = Vector2.zero;
            g.GetComponent<Image>().color = c; return g;
        }
        static GameObject MkLabel(Transform p, string n, string txt, Font f, int sz, Color c, float w)
        {
            var g = new GameObject(n, typeof(RectTransform));
            g.transform.SetParent(p, false);
            var t = g.AddComponent<Text>(); t.text = txt; t.fontSize = sz; t.color = c;
            t.alignment = TextAnchor.MiddleCenter; if (f) t.font = f;
            if (w > 0) { var le = g.AddComponent<LayoutElement>(); le.preferredWidth = w; }
            return g;
        }
        static GameObject MkButton(Transform p, string n, string lbl, Font f, Color c)
        {
            var g = new GameObject(n, typeof(RectTransform), typeof(Image), typeof(Button));
            g.transform.SetParent(p, false);
            g.GetComponent<Image>().color = c; g.GetComponent<Button>().targetGraphic = g.GetComponent<Image>();
            var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(g.transform, false); Fill(t);
            var tx = t.AddComponent<Text>(); tx.text = lbl; tx.fontSize = 28; tx.alignment = TextAnchor.MiddleCenter;
            tx.color = Color.white; if (f) tx.font = f;
            return g;
        }
        static void Fill(GameObject g)
        {
            var r = g.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        static OrderCardView MkOrderCard(Transform p, Font f, Sprite itemSpr)
        {
            var card = new GameObject("OrderCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(p, false);
            var cardImg = card.GetComponent<Image>();
            if (itemSpr != null) { cardImg.sprite = itemSpr; cardImg.color = Color.white; }
            else cardImg.color = new Color(0.95f, 0.9f, 0.8f, 0.95f);

            var hl = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            hl.transform.SetParent(card.transform, false); Fill(hl);
            var hlI = hl.GetComponent<Image>(); hlI.color = new Color(1, 0.85f, 0.3f, 0.35f); hlI.enabled = false;

            var rew = new GameObject("Reward", typeof(RectTransform));
            rew.transform.SetParent(card.transform, false); Anch(rew, 0.08f, 0.1f, 0.92f, 0.9f);
            var rt = rew.AddComponent<Text>(); rt.fontSize = 28; rt.color = new Color(0.2f, 0.15f, 0.1f);
            rt.alignment = TextAnchor.MiddleCenter; rt.fontStyle = FontStyle.Bold; if (f) rt.font = f;

            var sel = new GameObject("SelectBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            sel.transform.SetParent(card.transform, false); Fill(sel); sel.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);

            var cv2 = card.AddComponent<OrderCardView>();
            SetField(cv2, "rewardText", rt);
            SetField(cv2, "selectButton", sel.GetComponent<Button>());
            SetField(cv2, "selectionHighlight", hlI);
            return cv2;
        }
        static void Anch(GameObject g, float xmin, float ymin, float xmax, float ymax)
        {
            var r = g.GetComponent<RectTransform>();
            r.anchorMin = V2(xmin, ymin); r.anchorMax = V2(xmax, ymax);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        static void MkCookBar(GameObject root, string side, Vector2 amin, Vector2 amax, Font f, out Image bar, out Text lbl)
        {
            var bg = MkPanel(root.transform, $"Bar{side}_BG", amin, amax, new Color(0.12f, 0.12f, 0.12f, 0.6f));
            var fill = new GameObject($"Bar{side}", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bg.transform, false); Fill(fill);
            bar = fill.GetComponent<Image>(); bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Vertical; bar.fillOrigin = 0; bar.fillAmount = 0;
            bar.color = new Color(1, 0.95f, 0.8f);
            var lg = MkLabel(bg.transform, $"Lbl{side}", side, f, 20, Color.white, 0);
            var lr = lg.GetComponent<RectTransform>();
            lr.anchorMin = V2(0, 0); lr.anchorMax = V2(1, 0.2f);
            lr.offsetMin = lr.offsetMax = Vector2.zero;
            lbl = lg.GetComponent<Text>();
        }

        static void SetField(Object obj, string prop, Object val)
        {
            var so = new SerializedObject(obj); var p = so.FindProperty(prop);
            if (p != null) { p.objectReferenceValue = val; so.ApplyModifiedPropertiesWithoutUndo(); }
        }
        static void SetFieldArr(Object obj, string prop, Object[] vals)
        {
            var so = new SerializedObject(obj); var p = so.FindProperty(prop);
            if (p == null || !p.isArray) return;
            p.arraySize = vals.Length;
            for (int i = 0; i < vals.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = vals[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var name = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
