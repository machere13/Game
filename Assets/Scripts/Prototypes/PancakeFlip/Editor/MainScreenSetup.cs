using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

namespace IdlePancake.Prototypes.PancakeFlip.Editor
{
#pragma warning disable 0618
    public static class MainScreenSetup
    {
        [MenuItem("PancakeFlip/Setup Full Main Screen")]
        public static void Setup()
        {
            if (Application.isPlaying) { Debug.LogWarning("Останови Play."); return; }

            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null) { Debug.LogWarning("Сначала создай сцену через PancakeFlip/Setup Portrait Scene."); return; }

            var pancake = Object.FindObjectOfType<PancakeBehaviour>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            PancakeFlipConfig flipConfig = FindConfig();

            var topBar = CreatePanel(canvas.transform, "TopBar",
                new Vector2(0f, 0.92f), new Vector2(1f, 1f), new Color(0.15f, 0.15f, 0.2f, 0.85f));
            var hlg = topBar.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(20, 20, 8, 8);
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;

            CreateLabel(topBar.transform, "CoinIcon", "\u00a4", font, 32, Color.yellow, 40);
            var coinText = CreateLabel(topBar.transform, "CoinText", "0", font, 36, Color.white, 150);
            var levelText = CreateLabel(topBar.transform, "LevelText", "\u0423\u0440. 1", font, 32, Color.white, 100);

            var xpBarBg = new GameObject("XpBarBG", typeof(RectTransform), typeof(Image));
            xpBarBg.transform.SetParent(topBar.transform, false);
            xpBarBg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f);
            var xpLe = xpBarBg.AddComponent<LayoutElement>();
            xpLe.flexibleWidth = 1;
            xpLe.preferredHeight = 30;

            var xpFill = new GameObject("XpFill", typeof(RectTransform), typeof(Image));
            xpFill.transform.SetParent(xpBarBg.transform, false);
            var xpFillRect = xpFill.GetComponent<RectTransform>();
            xpFillRect.anchorMin = Vector2.zero;
            xpFillRect.anchorMax = Vector2.one;
            xpFillRect.offsetMin = Vector2.zero;
            xpFillRect.offsetMax = Vector2.zero;
            var xpFillImg = xpFill.GetComponent<Image>();
            xpFillImg.color = new Color(0.3f, 0.8f, 1f);
            xpFillImg.type = Image.Type.Filled;
            xpFillImg.fillMethod = Image.FillMethod.Horizontal;
            xpFillImg.fillAmount = 0f;

            var topBarView = topBar.AddComponent<TopBarView>();
            SetSerialized(topBarView, "coinsText", coinText.GetComponent<Text>());
            SetSerialized(topBarView, "levelText", levelText.GetComponent<Text>());
            SetSerialized(topBarView, "xpFill", xpFillImg);

            var orderPanel = CreatePanel(canvas.transform, "OrderPanel",
                new Vector2(0f, 0.15f), new Vector2(0.25f, 0.92f), new Color(0.1f, 0.1f, 0.15f, 0.7f));

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            scrollGo.transform.SetParent(orderPanel.transform, false);
            FillParent(scrollGo);
            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.horizontal = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.childForceExpandHeight = false;
            scroll.content = contentRect;

            var cardPrefab = CreateOrderCardPrefab(contentGo.transform, font);
            cardPrefab.gameObject.SetActive(false);

            var orderListView = orderPanel.AddComponent<OrderListView>();
            SetSerialized(orderListView, "cardPrefab", cardPrefab);
            SetSerialized(orderListView, "container", contentGo.transform);

            var bottomBar = CreatePanel(canvas.transform, "BottomBar",
                new Vector2(0f, 0f), new Vector2(1f, 0.12f), new Color(0.15f, 0.12f, 0.1f, 0.9f));
            var bottomHlg = bottomBar.AddComponent<HorizontalLayoutGroup>();
            bottomHlg.padding = new RectOffset(20, 20, 10, 10);
            bottomHlg.spacing = 20;
            bottomHlg.childForceExpandWidth = true;

            var ingBtnGo = CreateButton(bottomBar.transform, "IngredientsBtn", "\u0418\u043d\u0433\u0440\u0435\u0434\u0438\u0435\u043d\u0442\u044b",
                font, new Color(0.2f, 0.55f, 0.2f));
            var upgBtnGo = CreateButton(bottomBar.transform, "UpgradesBtn", "\u0421\u043a\u043e\u0432\u043e\u0440\u043e\u0434\u0430",
                font, new Color(0.55f, 0.4f, 0.15f));
            var serveBtnGo = CreateButton(bottomBar.transform, "ServeBtn", "\u0421\u0434\u0430\u0442\u044c \u0431\u043b\u0438\u043d",
                font, new Color(0.7f, 0.2f, 0.15f));
            var serveBaseBtnGo = CreateButton(bottomBar.transform, "ServeBaseBtn", "\u0421\u0434\u0430\u0442\u044c \u0431\u0430\u0437\u043e\u0432\u044b\u0439",
                font, new Color(0.5f, 0.3f, 0.3f));

            var ingScreen = CreatePanel(canvas.transform, "IngredientsScreen",
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.9f), new Color(0.12f, 0.14f, 0.12f, 0.95f));
            var ingVlg = ingScreen.AddComponent<VerticalLayoutGroup>();
            ingVlg.padding = new RectOffset(20, 20, 20, 20);
            ingVlg.spacing = 10;
            ingVlg.childForceExpandHeight = false;

            var ingTitle = CreateLabel(ingScreen.transform, "Title", "\u0418\u043d\u0433\u0440\u0435\u0434\u0438\u0435\u043d\u0442\u044b", font, 40, Color.white, 0);
            ingTitle.AddComponent<LayoutElement>().preferredHeight = 60;

            var ingListGo = new GameObject("IngredientList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            ingListGo.transform.SetParent(ingScreen.transform, false);
            ingListGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ingListGo.GetComponent<VerticalLayoutGroup>().spacing = 8;
            ingListGo.AddComponent<LayoutElement>().flexibleHeight = 1;

            var ingCloseBtn = CreateButton(ingScreen.transform, "CloseBtn", "\u0417\u0430\u043a\u0440\u044b\u0442\u044c", font, new Color(0.6f, 0.2f, 0.2f));
            ingCloseBtn.AddComponent<LayoutElement>().preferredHeight = 60;

            var ingView = ingScreen.AddComponent<IngredientsScreenView>();
            SetSerialized(ingView, "ingredientListContainer", ingListGo.transform);
            SetSerialized(ingView, "closeButton", ingCloseBtn.GetComponent<Button>());

            var upgScreen = CreatePanel(canvas.transform, "UpgradeScreen",
                new Vector2(0.05f, 0.15f), new Vector2(0.95f, 0.9f), new Color(0.14f, 0.12f, 0.1f, 0.95f));
            var upgVlg = upgScreen.AddComponent<VerticalLayoutGroup>();
            upgVlg.padding = new RectOffset(20, 20, 20, 20);
            upgVlg.spacing = 10;
            upgVlg.childForceExpandHeight = false;

            var upgTitle = CreateLabel(upgScreen.transform, "Title", "\u0410\u043f\u0433\u0440\u0435\u0439\u0434\u044b \u0441\u043a\u043e\u0432\u043e\u0440\u043e\u0434\u044b", font, 40, Color.white, 0);
            upgTitle.AddComponent<LayoutElement>().preferredHeight = 60;

            var upgListGo = new GameObject("UpgradeList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            upgListGo.transform.SetParent(upgScreen.transform, false);
            upgListGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            upgListGo.GetComponent<VerticalLayoutGroup>().spacing = 8;
            upgListGo.AddComponent<LayoutElement>().flexibleHeight = 1;

            var upgCloseBtn = CreateButton(upgScreen.transform, "CloseBtn", "\u0417\u0430\u043a\u0440\u044b\u0442\u044c", font, new Color(0.6f, 0.2f, 0.2f));
            upgCloseBtn.AddComponent<LayoutElement>().preferredHeight = 60;

            var upgView = upgScreen.AddComponent<PanUpgradeScreenView>();
            SetSerialized(upgView, "upgradeListContainer", upgListGo.transform);
            SetSerialized(upgView, "closeButton", upgCloseBtn.GetComponent<Button>());

            var serveView = bottomBar.AddComponent<ServeButtonView>();
            SetSerialized(serveView, "serveButton", serveBtnGo.GetComponent<Button>());
            SetSerialized(serveView, "serveBaseButton", serveBaseBtnGo.GetComponent<Button>());

            var statusTextGo = CreateLabel(bottomBar.transform, "StatusText", "", font, 26, Color.yellow, 0);
            statusTextGo.AddComponent<LayoutElement>().flexibleWidth = 1;

            var controllerGo = new GameObject("MainScreenController");
            controllerGo.transform.SetParent(canvas.transform, false);
            var msc = controllerGo.AddComponent<MainScreenController>();
            SetSerialized(msc, "ingredientsScreen", ingView);
            SetSerialized(msc, "upgradeScreen", upgView);
            SetSerialized(msc, "statusText", statusTextGo.GetComponent<Text>());

            UnityEventTools.AddPersistentListener(ingBtnGo.GetComponent<Button>().onClick, msc.OpenIngredients);
            UnityEventTools.AddPersistentListener(upgBtnGo.GetComponent<Button>().onClick, msc.OpenUpgrades);
            UnityEventTools.AddPersistentListener(serveBtnGo.GetComponent<Button>().onClick, msc.Serve);
            UnityEventTools.AddPersistentListener(serveBaseBtnGo.GetComponent<Button>().onClick, msc.ServeBase);

            var sessionGo = GameObject.Find("GameSession");
            if (sessionGo == null)
            {
                sessionGo = new GameObject("GameSession");
                sessionGo.AddComponent<GameSession>();
            }
            var session = sessionGo.GetComponent<GameSession>();
            if (session != null)
            {
                if (pancake != null) SetSerialized(session, "pancake", pancake);
                if (flipConfig != null) SetSerialized(session, "flipConfig", flipConfig);
            }

            AutoAssignConfig(flipConfig);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("PancakeFlip: \u0413\u043b\u0430\u0432\u043d\u044b\u0439 \u044d\u043a\u0440\u0430\u043d \u0441\u043e\u0437\u0434\u0430\u043d. \u041d\u0430\u0437\u043d\u0430\u0447\u044c \u0432 GameSession: levelTable, startingRecipes, baseRecipe, allIngredients, allUpgrades.");
        }

        [MenuItem("PancakeFlip/Auto-assign Config to All Views")]
        public static void AutoAssignConfigMenu()
        {
            AutoAssignConfig(FindConfig());
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("PancakeFlip: config назначен на все View.");
        }

        [MenuItem("PancakeFlip/Create All Default Data and Assign")]
        public static void CreateDefaultData()
        {
            if (Application.isPlaying) { Debug.LogWarning("Останови Play."); return; }

            const string dir = "Assets/Data/PancakeFlip";
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets/Data", "PancakeFlip");

            var levelTable = GetOrCreate<LevelTableConfig>(dir, "LevelTable");

            var dough = GetOrCreate<IngredientConfig>(dir, "Тесто");
            dough.displayName = "Тесто";
            dough.coinCost = 0;
            dough.unlockLevel = 0;
            dough.infinite = true;
            EditorUtility.SetDirty(dough);

            var jam = GetOrCreate<IngredientConfig>(dir, "Варенье");
            jam.displayName = "Варенье";
            jam.coinCost = 5;
            jam.unlockLevel = 2;
            jam.infinite = false;
            EditorUtility.SetDirty(jam);

            var cheese = GetOrCreate<IngredientConfig>(dir, "Сыр");
            cheese.displayName = "Сыр";
            cheese.coinCost = 8;
            cheese.unlockLevel = 3;
            cheese.infinite = false;
            EditorUtility.SetDirty(cheese);

            var baseRecipe = GetOrCreate<RecipeConfig>(dir, "Базовый блин");
            baseRecipe.displayName = "Базовый блин";
            baseRecipe.unlockLevel = 0;
            baseRecipe.ingredients = new[] { new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 } };
            baseRecipe.rewardCoins = 3;
            baseRecipe.rewardXp = 5;
            EditorUtility.SetDirty(baseRecipe);

            var jamPancake = GetOrCreate<RecipeConfig>(dir, "Блин с вареньем");
            jamPancake.displayName = "Блин с вареньем";
            jamPancake.unlockLevel = 2;
            jamPancake.ingredients = new[]
            {
                new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                new RecipeConfig.IngredientSlot { ingredient = jam, amount = 1 }
            };
            jamPancake.rewardCoins = 12;
            jamPancake.rewardXp = 25;
            EditorUtility.SetDirty(jamPancake);

            var cheesePancake = GetOrCreate<RecipeConfig>(dir, "Блин с сыром");
            cheesePancake.displayName = "Блин с сыром";
            cheesePancake.unlockLevel = 3;
            cheesePancake.ingredients = new[]
            {
                new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                new RecipeConfig.IngredientSlot { ingredient = cheese, amount = 1 }
            };
            cheesePancake.rewardCoins = 18;
            cheesePancake.rewardXp = 35;
            EditorUtility.SetDirty(cheesePancake);

            var upg1 = GetOrCreate<PanUpgradeConfig>(dir, "Широкая норма");
            upg1.displayName = "Широкая норма";
            upg1.description = "Зона идеальной прожарки шире на 20%";
            upg1.coinCost = 30;
            upg1.unlockLevel = 1;
            upg1.effectType = PanUpgradeConfig.EffectType.WiderPerfectZone;
            upg1.effectValue = 1.2f;
            EditorUtility.SetDirty(upg1);

            var upg2 = GetOrCreate<PanUpgradeConfig>(dir, "Медленный пережар");
            upg2.displayName = "Медленный пережар";
            upg2.description = "Пережар наступает медленнее";
            upg2.coinCost = 50;
            upg2.unlockLevel = 2;
            upg2.effectType = PanUpgradeConfig.EffectType.SlowerOvercook;
            upg2.effectValue = 1.3f;
            EditorUtility.SetDirty(upg2);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var session = Object.FindObjectOfType<GameSession>();
            if (session != null)
            {
                SetSerialized(session, "levelTable", levelTable);
                SetSerialized(session, "baseRecipe", baseRecipe);
                SetSerializedArray(session, "startingRecipes", new Object[] { baseRecipe, jamPancake, cheesePancake });
                SetSerializedArray(session, "allIngredients", new Object[] { dough, jam, cheese });
                SetSerializedArray(session, "allUpgrades", new Object[] { upg1, upg2 });

                var flipCfg = FindConfig();
                if (flipCfg != null)
                    SetSerialized(session, "flipConfig", flipCfg);
            }
            else
            {
                Debug.LogWarning("GameSession не найден в сцене. Сначала запусти Setup Full Main Screen.");
            }

            AutoAssignConfig(FindConfig());

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("PancakeFlip: Все данные созданы в Assets/Data/PancakeFlip/ и назначены в GameSession.");
        }

        static T GetOrCreate<T>(string folder, string name) where T : ScriptableObject
        {
            string path = $"{folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var obj = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(obj, path);
            return obj;
        }

        static void SetSerializedArray(Object obj, string propertyName, Object[] values)
        {
            var so = new SerializedObject(obj);
            var prop = so.FindProperty(propertyName);
            if (prop == null || !prop.isArray) return;
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static PancakeFlipConfig FindConfig()
        {
            var guids = AssetDatabase.FindAssets("t:PancakeFlipConfig");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<PancakeFlipConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static void AutoAssignConfig(PancakeFlipConfig cfg)
        {
            if (cfg == null) { Debug.LogWarning("PancakeFlipConfig \u043d\u0435 \u043d\u0430\u0439\u0434\u0435\u043d \u0432 \u043f\u0440\u043e\u0435\u043a\u0442\u0435."); return; }

            var pancake = Object.FindObjectOfType<PancakeBehaviour>();
            if (pancake != null) SetSerialized(pancake, "config", cfg);

            var controller = Object.FindObjectOfType<PancakeFlipController>();
            if (controller != null) SetSerialized(controller, "config", cfg);

            var scoreView = Object.FindObjectOfType<PancakeFlipScoreView>();
            if (scoreView != null) SetSerialized(scoreView, "config", cfg);

            var cookingView = Object.FindObjectOfType<CookingIndicatorView>();
            if (cookingView != null) SetSerialized(cookingView, "config", cfg);

            var session = Object.FindObjectOfType<GameSession>();
            if (session != null) SetSerialized(session, "flipConfig", cfg);
        }

        static GameObject CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        static GameObject CreateLabel(Transform parent, string name, string text, Font font, int size, Color color, float width)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            if (font != null) t.font = font;
            if (width > 0)
            {
                var le = go.AddComponent<LayoutElement>();
                le.preferredWidth = width;
            }
            return go;
        }

        static GameObject CreateButton(Transform parent, string name, string label, Font font, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            go.GetComponent<Button>().targetGraphic = go.GetComponent<Image>();

            var txtGo = new GameObject("Text", typeof(RectTransform));
            txtGo.transform.SetParent(go.transform, false);
            FillParent(txtGo);
            var txt = txtGo.AddComponent<Text>();
            txt.text = label;
            txt.fontSize = 30;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            if (font != null) txt.font = font;

            return go;
        }

        static OrderCardView CreateOrderCardPrefab(Transform parent, Font font)
        {
            var card = new GameObject("OrderCard", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            card.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            card.AddComponent<LayoutElement>().preferredHeight = 130;

            var highlight = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlight.transform.SetParent(card.transform, false);
            FillParent(highlight);
            var hlImg = highlight.GetComponent<Image>();
            hlImg.color = new Color(1f, 0.85f, 0.3f, 0.3f);
            hlImg.enabled = false;

            var icon = new GameObject("RecipeIcon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(card.transform, false);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.05f, 0.55f);
            iconRect.anchorMax = new Vector2(0.4f, 0.95f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            icon.GetComponent<Image>().color = new Color(1f, 0.9f, 0.6f);

            var customer = new GameObject("CustomerIcon", typeof(RectTransform), typeof(Image));
            customer.transform.SetParent(card.transform, false);
            var custRect = customer.GetComponent<RectTransform>();
            custRect.anchorMin = new Vector2(0.5f, 0.55f);
            custRect.anchorMax = new Vector2(0.85f, 0.95f);
            custRect.offsetMin = Vector2.zero;
            custRect.offsetMax = Vector2.zero;
            customer.GetComponent<Image>().color = new Color(0.5f, 0.7f, 1f);

            var rewardGo = new GameObject("Reward", typeof(RectTransform));
            rewardGo.transform.SetParent(card.transform, false);
            var rewRect = rewardGo.GetComponent<RectTransform>();
            rewRect.anchorMin = new Vector2(0.05f, 0.05f);
            rewRect.anchorMax = new Vector2(0.65f, 0.45f);
            rewRect.offsetMin = Vector2.zero;
            rewRect.offsetMax = Vector2.zero;
            var rewText = rewardGo.AddComponent<Text>();
            rewText.fontSize = 22;
            rewText.color = new Color(0.4f, 0.9f, 0.4f);
            rewText.alignment = TextAnchor.MiddleLeft;
            if (font != null) rewText.font = font;

            var selectBtn = new GameObject("SelectBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            selectBtn.transform.SetParent(card.transform, false);
            FillParent(selectBtn);
            selectBtn.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);

            var dismissGo = new GameObject("DismissBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            dismissGo.transform.SetParent(card.transform, false);
            var disRect = dismissGo.GetComponent<RectTransform>();
            disRect.anchorMin = new Vector2(0.75f, 0.05f);
            disRect.anchorMax = new Vector2(0.95f, 0.45f);
            disRect.offsetMin = Vector2.zero;
            disRect.offsetMax = Vector2.zero;
            dismissGo.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0.3f, 0.8f);
            var disTxtGo = new GameObject("Text", typeof(RectTransform));
            disTxtGo.transform.SetParent(dismissGo.transform, false);
            FillParent(disTxtGo);
            var disTxt = disTxtGo.AddComponent<Text>();
            disTxt.text = "X";
            disTxt.fontSize = 22;
            disTxt.alignment = TextAnchor.MiddleCenter;
            disTxt.color = Color.white;
            if (font != null) disTxt.font = font;

            var cardView = card.AddComponent<OrderCardView>();
            SetSerialized(cardView, "recipeIcon", icon.GetComponent<Image>());
            SetSerialized(cardView, "customerIcon", customer.GetComponent<Image>());
            SetSerialized(cardView, "rewardText", rewText);
            SetSerialized(cardView, "selectButton", selectBtn.GetComponent<Button>());
            SetSerialized(cardView, "dismissButton", dismissGo.GetComponent<Button>());
            SetSerialized(cardView, "selectionHighlight", hlImg);

            return cardView;
        }

        static void FillParent(GameObject go)
        {
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
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
