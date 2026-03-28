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
        const string OutputScenePath = "Assets/Scenes/PancakeFlip.unity";

        [MenuItem("PancakeFlip/Build Everything")]
        public static void BuildEverything()
        {
            if (Application.isPlaying) { Debug.LogWarning("Останови Play."); return; }

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 5f;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.38f, 0.46f, 0.54f);
            }
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
            var profileSpr = LoadSprite("Profile");
            var walletSpr = LoadSprite("Wallet");
            var backPanSpr = LoadSprite("BackPan");
            var frontPanSpr = LoadSprite("FrontPan");
            var bottomPanelSpr = LoadSprite("BottomPanel");
            var person1 = LoadSprite("Person1");
            var person2 = LoadSprite("Person2");
            var person3 = LoadSprite("Person3");
            var person4 = LoadSprite("Person4");
            var person1Icon = LoadSprite("Person1Icon");
            var person2Icon = LoadSprite("Person2Icon");
            var person3Icon = LoadSprite("Person3Icon");
            var rewardInfoSpr = LoadSprite("RewardInfo");
            var commonPancakeSpr = LoadSprite("CommonPancake");
            var cheeseHamPancakeSpr = LoadSprite("CheeseHamPancake");
            var chocoStrawberrySpr = LoadSprite("ChocolateStrawberryPancake");
            var xpIconSpr = LoadSprite("XPIcon");
            var pancakeSideUiSpr = LoadSprite("PancakeSide");
            var doughEmptySpr = LoadSprite("Empty");
            var doughFullSpr = LoadSprite("Full");
            var receiptBtnSpr = LoadSprite("ReceiptButton");
            var panUpgradeBtnSpr = LoadSprite("PanUpgradeButton");
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            EnsureFolder("Assets/Data");
            EnsureFolder(DataDir);
            var flipConfig = GetOrCreate<PancakeFlipConfig>(DataDir, "PancakeFlipConfig");
            var levelTable = GetOrCreate<LevelTableConfig>(DataDir, "LevelTable");

            var dough = CreateIngredient("Тесто", 0, 0, false);
            var salami = CreateIngredient("Салями", 5, 1, false);
            var cheese = CreateIngredient("Сыр", 5, 1, false);
            var banana = CreateIngredient("Банан", 4, 2, false);
            var chocolate = CreateIngredient("Шоколад", 6, 2, false);
            var mushroom = CreateIngredient("Гриб", 4, 3, false);
            var strawberry = CreateIngredient("Клубника", 5, 3, false);

            var baseRecipe = CreateRecipe("Обычный блин", 0, 5, 10,
                new[] { new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 } },
                commonPancakeSpr);
            var cheeseHamRecipe = CreateRecipe("Блин с салями и сыром", 1, 20, 35,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = salami, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = cheese, amount = 1 }
                }, cheeseHamPancakeSpr);
            var bananaChocoRecipe = CreateRecipe("Блин с бананом и шоколадом", 2, 18, 30,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = banana, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = chocolate, amount = 1 }
                }, null);
            var mushroomRecipe = CreateRecipe("Грибной блин", 3, 22, 40,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = mushroom, amount = 2 }
                }, null);
            var strawberryChocoRecipe = CreateRecipe("Блин с клубникой и шоколадом", 3, 25, 45,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = strawberry, amount = 2 },
                    new RecipeConfig.IngredientSlot { ingredient = chocolate, amount = 1 }
                }, chocoStrawberrySpr);

            var upg1 = CreateUpgrade("Широкая норма", 30, 1, PanUpgradeConfig.EffectType.WiderPerfectZone, 1.2f);
            var upg2 = CreateUpgrade("Медленный пережар", 50, 2, PanUpgradeConfig.EffectType.SlowerOvercook, 1.3f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            float camH = ortho * 2f, camW = camH * (9f / 16f);

            // Background (sortingOrder -100) — overscan + сдвиг вверх, чтобы не было щели под clear color
            if (bgSpr != null)
            {
                var bg = new GameObject("Background");
                var bgSr = bg.AddComponent<SpriteRenderer>(); bgSr.sprite = bgSpr; bgSr.sortingOrder = -100;
                const float bgOverscan = 1.24f;
                float sc = Mathf.Max(camW / bgSpr.bounds.size.x, camH / bgSpr.bounds.size.y) * bgOverscan;
                bg.transform.localScale = Vector3.one * sc;
                bg.transform.position = new Vector3(0, 0.38f, 10);
            }

            // BottomPanel (sortingOrder -3, behind stove, in front of persons)
            if (bottomPanelSpr != null)
            {
                var bpGo = new GameObject("BottomPanel");
                var bpSr = bpGo.AddComponent<SpriteRenderer>(); bpSr.sprite = bottomPanelSpr; bpSr.sortingOrder = -3;
                float bpSc = camW / bottomPanelSpr.bounds.size.x;
                bpGo.transform.localScale = Vector3.one * bpSc;
                float bpH = bottomPanelSpr.bounds.size.y * bpSc;
                bpGo.transform.position = new Vector3(0, -ortho + bpH * 0.5f, 0);
            }

            // Customer (sortingOrder -4, behind BottomPanel)
            float rightOffscreen = camW * 0.5f + 2f;
            float leftOffscreen = -camW * 0.5f - 2f;
            float counterX = 0f;
            var custGo = new GameObject("Customer");
            var custSr = custGo.AddComponent<SpriteRenderer>(); custSr.sortingOrder = -4;
            custGo.transform.localScale = Vector3.one * 0.35f;
            custGo.transform.position = new Vector3(rightOffscreen, -1.5f, 0);
            var custAnim = custGo.AddComponent<CustomerAnimator>();
            SetField(custAnim, "sr", custSr);
            {
                var so = new SerializedObject(custAnim);
                so.FindProperty("startX").floatValue = rightOffscreen;
                so.FindProperty("targetX").floatValue = counterX;
                so.FindProperty("exitX").floatValue = leftOffscreen;
                var arr = so.FindProperty("personSprites");
                if (arr != null)
                {
                    arr.arraySize = 4;
                    arr.GetArrayElementAtIndex(0).objectReferenceValue = person1;
                    arr.GetArrayElementAtIndex(1).objectReferenceValue = person2;
                    arr.GetArrayElementAtIndex(2).objectReferenceValue = person3;
                    arr.GetArrayElementAtIndex(3).objectReferenceValue = person4;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Stove (sortingOrder -2)
            float stoveScale = 0.55f, burnerY = -1.5f;
            if (stoveClosedS != null)
            {
                var stoveGo = new GameObject("Stove");
                var stoveSr = stoveGo.AddComponent<SpriteRenderer>(); stoveSr.sprite = stoveClosedS; stoveSr.sortingOrder = -2;
                float sprW = stoveClosedS.bounds.size.x;
                stoveScale = (camW * 0.85f) / sprW;
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

            // Pan: parent GO with collider + PanBehaviour, two child SpriteRenderers
            const float worldScale = 0.25f;
            const float panLiftWorld = 0.12f;

            var panGo = new GameObject("Pan");
            panGo.transform.position = new Vector3(0, burnerY + panLiftWorld, 0);
            panGo.transform.localScale = Vector3.one * worldScale;
            panGo.AddComponent<BoxCollider2D>();
            var panBh = panGo.AddComponent<PanBehaviour>();

            // BackPan (sortingOrder 0) — behind pancake
            var backPanGo = new GameObject("BackPan");
            backPanGo.transform.SetParent(panGo.transform, false);
            var backPanSr = backPanGo.AddComponent<SpriteRenderer>();
            if (backPanSpr != null) { backPanSr.sprite = backPanSpr; backPanSr.color = Color.white; }
            else if (panSpr != null) { backPanSr.sprite = panSpr; backPanSr.color = Color.white; }
            backPanSr.sortingOrder = 0;

            // FrontPan (sortingOrder 2) — in front of pancake
            var frontPanGo = new GameObject("FrontPan");
            frontPanGo.transform.SetParent(panGo.transform, false);
            var frontPanSr = frontPanGo.AddComponent<SpriteRenderer>();
            if (frontPanSpr != null) { frontPanSr.sprite = frontPanSpr; frontPanSr.color = Color.white; }
            frontPanSr.sortingOrder = 2;

            // Pancake (sortingOrder 1) — between BackPan and FrontPan
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

            Transform uiRoot = canvas.transform;

            var profileGo = new GameObject("ProfileIcon", typeof(RectTransform), typeof(Image));
            profileGo.transform.SetParent(uiRoot, false);
            var profileR = profileGo.GetComponent<RectTransform>();
            profileR.anchorMin = V2(0.82f, 0.91f); profileR.anchorMax = V2(0.97f, 0.99f);
            profileR.offsetMin = profileR.offsetMax = Vector2.zero;
            var profileImg = profileGo.GetComponent<Image>();
            if (profileSpr != null) { profileImg.sprite = profileSpr; profileImg.preserveAspect = true; }
            else profileImg.color = new Color(0.8f, 0.3f, 0.3f);
            profileImg.raycastTarget = false;

            var lvlTxt = new GameObject("LevelText", typeof(RectTransform));
            lvlTxt.transform.SetParent(profileGo.transform, false);
            Anch(lvlTxt, -0.2f, -0.42f, 1.2f, 0f);
            var lvlT = lvlTxt.AddComponent<Text>(); lvlT.text = "Level 1"; lvlT.fontSize = 36;
            lvlT.alignment = TextAnchor.MiddleCenter; lvlT.color = Color.white; lvlT.fontStyle = FontStyle.Bold;
            if (font) lvlT.font = font;

            var walletGo = new GameObject("WalletIcon", typeof(RectTransform), typeof(Image));
            walletGo.transform.SetParent(uiRoot, false);
            var walletR = walletGo.GetComponent<RectTransform>();
            walletR.anchorMin = V2(0.82f, 0.805f); walletR.anchorMax = V2(0.97f, 0.865f);
            walletR.offsetMin = walletR.offsetMax = Vector2.zero;
            var walletImg = walletGo.GetComponent<Image>();
            if (walletSpr != null) { walletImg.sprite = walletSpr; walletImg.preserveAspect = true; }
            else walletImg.color = new Color(0.2f, 0.7f, 0.2f);
            walletImg.raycastTarget = false;

            var coinTxt = new GameObject("CoinText", typeof(RectTransform));
            coinTxt.transform.SetParent(walletGo.transform, false);
            Anch(coinTxt, -1.5f, 0f, 0f, 1f);
            var coinT = coinTxt.AddComponent<Text>(); coinT.text = "0"; coinT.fontSize = 36;
            coinT.alignment = TextAnchor.MiddleRight; coinT.color = Color.white; coinT.fontStyle = FontStyle.Bold;
            if (font) coinT.font = font;

            var tbvGo = new GameObject("TopBarView", typeof(RectTransform));
            tbvGo.transform.SetParent(uiRoot, false);
            var tbv = tbvGo.AddComponent<TopBarView>();
            SetField(tbv, "coinsText", coinT);
            SetField(tbv, "levelText", lvlT);

            // Панель до верха экрана (+ чуть выше), чтобы OrderList уходил за верхний край
            var orderPanel = MkPanel(uiRoot, "OrderPanel", V2(0, 0.42f), V2(0.44f, 1.02f), new Color(0, 0, 0, 0));

            if (orderListSpr != null)
            {
                var rope = new GameObject("Rope", typeof(RectTransform), typeof(Image));
                rope.transform.SetParent(orderPanel.transform, false);
                var ropeR = rope.GetComponent<RectTransform>();
                ropeR.anchorMin = V2(-0.08f, 0f);
                ropeR.anchorMax = V2(0.48f, 1.06f);
                ropeR.offsetMin = ropeR.offsetMax = Vector2.zero;
                var ropeI = rope.GetComponent<Image>(); ropeI.sprite = orderListSpr;
                ropeI.type = Image.Type.Simple; ropeI.preserveAspect = false; ropeI.raycastTarget = false;
            }

            var cardsContainer = new GameObject("Cards", typeof(RectTransform));
            cardsContainer.transform.SetParent(orderPanel.transform, false);
            var cardsRect = cardsContainer.GetComponent<RectTransform>();
            cardsRect.anchorMin = V2(0.1f, 0); cardsRect.anchorMax = V2(1f, 1f);
            cardsRect.offsetMin = cardsRect.offsetMax = Vector2.zero;

            var cardPrefab = MkOrderCard(cardsContainer.transform, font, orderItemSpr, rewardInfoSpr,
                new[] { person1Icon, person2Icon, person3Icon }, walletSpr, xpIconSpr);
            cardPrefab.gameObject.SetActive(false);
            var olv = orderPanel.AddComponent<OrderListView>();
            SetField(olv, "cardPrefab", cardPrefab);
            SetField(olv, "container", cardsContainer.transform);
            SetFloatField(olv, "slotHeight", 0.285f);
            SetFloatField(olv, "gap", 0.004f);

            var chargeGo = MkPanel(uiRoot, "ChargeIndicator", V2(0.25f, 0.84f), V2(0.75f, 0.9f), new Color(0.15f, 0.15f, 0.15f, 0.7f));
            var fillGo2 = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo2.transform.SetParent(chargeGo.transform, false); Fill(fillGo2);
            var fillI = fillGo2.GetComponent<Image>(); fillI.color = Color.yellow;
            fillI.type = Image.Type.Filled; fillI.fillMethod = Image.FillMethod.Horizontal; fillI.fillAmount = 0;
            var cv = chargeGo.AddComponent<ChargeIndicatorView>(); SetField(cv, "fillImage", fillI);

            var popup = MkLabel(uiRoot, "RotationsPopup", "", font, 52, new Color(1, 0.95f, 0.7f), 0);
            var popupR = popup.GetComponent<RectTransform>();
            popupR.anchorMin = V2(0.5f, 0.5f); popupR.anchorMax = V2(0.5f, 0.5f);
            popupR.sizeDelta = new Vector2(400, 100);
            popup.GetComponent<Text>().fontStyle = FontStyle.Bold;

            var scoreUI = new GameObject("ScoreUI", typeof(RectTransform));
            scoreUI.transform.SetParent(uiRoot, false);
            var sv = scoreUI.AddComponent<PancakeFlipScoreView>();
            SetField(sv, "pancake", pcBh); SetField(sv, "config", flipConfig);
            SetField(sv, "rotationsPopupText", popup.GetComponent<Text>());

            var cookRoot = MkPanel(uiRoot, "CookingIndicators", V2(0.74f, 0.34f), V2(0.97f, 0.81f), new Color(0, 0, 0, 0));
            cookRoot.GetComponent<Image>().raycastTarget = false;
            MkCookPancakePreview(cookRoot, "A", V2(0, 0.53f), V2(1, 0.778f), pancakeSideUiSpr, font, out Image imgA);
            MkCookPancakePreview(cookRoot, "B", V2(0, 0.232f), V2(1, 0.498f), pancakeSideUiSpr, font, out Image imgB);
            var civ = cookRoot.AddComponent<CookingIndicatorView>();
            SetField(civ, "pancake", pcBh); SetField(civ, "config", flipConfig);
            SetField(civ, "pancakeA", imgA); SetField(civ, "pancakeB", imgB);

            var recipeBookScr = MkPanel(uiRoot, "RecipeBookScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.96f, 0.94f, 0.89f, 0.99f));
            recipeBookScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(recipeBookScr);
            var rbSh = recipeBookScr.AddComponent<Shadow>(); rbSh.effectDistance = new Vector2(3, -4); rbSh.effectColor = new Color(0, 0, 0, 0.28f);
            MkPanel(recipeBookScr.transform, "Header", V2(0f, 0.86f), V2(1f, 1f), new Color(0.32f, 0.46f, 0.4f, 1f));
            var rbTitle = MkLabel(recipeBookScr.transform, "Title", "Рецепты", font, 22, Color.white, 0);
            var rbTitleRt = rbTitle.GetComponent<RectTransform>();
            rbTitleRt.anchorMin = V2(0.04f, 0.86f); rbTitleRt.anchorMax = V2(0.96f, 0.98f);
            rbTitleRt.offsetMin = rbTitleRt.offsetMax = Vector2.zero;
            rbTitle.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            MkVerticalScrollArea(recipeBookScr.transform, "Scroll", V2(0.03f, 0.13f), V2(0.97f, 0.84f), out RectTransform rbListContent);
            var rbTextGo = new GameObject("Body", typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter), typeof(LayoutElement));
            rbTextGo.transform.SetParent(rbListContent, false);
            var rbLe = rbTextGo.GetComponent<LayoutElement>();
            rbLe.flexibleWidth = 1f;
            rbLe.minHeight = 80f;
            var rbTxt = rbTextGo.GetComponent<Text>();
            rbTxt.font = font;
            rbTxt.fontSize = 22;
            rbTxt.color = new Color(0.15f, 0.12f, 0.1f);
            rbTxt.alignment = TextAnchor.UpperLeft;
            rbTxt.horizontalOverflow = HorizontalWrapMode.Wrap;
            rbTxt.verticalOverflow = VerticalWrapMode.Overflow;
            var rbTxtRt = rbTextGo.GetComponent<RectTransform>();
            rbTxtRt.anchorMin = new Vector2(0f, 1f);
            rbTxtRt.anchorMax = new Vector2(1f, 1f);
            rbTxtRt.pivot = new Vector2(0.5f, 1f);
            rbTxtRt.sizeDelta = Vector2.zero;
            rbTxtRt.anchoredPosition = Vector2.zero;
            var rbCsf = rbTextGo.GetComponent<ContentSizeFitter>();
            rbCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            rbCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rbCloseBtn = MkButton(recipeBookScr.transform, "CloseBtn", "Закрыть", font, new Color(0.52f, 0.3f, 0.24f, 1f));
            var rbCloseRt = rbCloseBtn.GetComponent<RectTransform>();
            rbCloseRt.anchorMin = V2(0.28f, 0.02f); rbCloseRt.anchorMax = V2(0.72f, 0.1f);
            rbCloseRt.offsetMin = rbCloseRt.offsetMax = Vector2.zero;
            var rbsv = recipeBookScr.AddComponent<RecipeBookScreenView>();
            SetField(rbsv, "bodyText", rbTxt);
            SetField(rbsv, "closeButton", rbCloseBtn.GetComponent<Button>());

            var ingScr = MkPanel(uiRoot, "IngredientsScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.96f, 0.94f, 0.89f, 0.99f));
            ingScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(ingScr);
            var ingSh = ingScr.AddComponent<Shadow>(); ingSh.effectDistance = new Vector2(3, -4); ingSh.effectColor = new Color(0, 0, 0, 0.28f);
            MkPanel(ingScr.transform, "Header", V2(0f, 0.86f), V2(1f, 1f), new Color(0.32f, 0.46f, 0.4f, 1f));
            var ingTitle = MkLabel(ingScr.transform, "Title", "Ингредиенты", font, 22, Color.white, 0);
            var ingTitleRt = ingTitle.GetComponent<RectTransform>();
            ingTitleRt.anchorMin = V2(0.04f, 0.86f); ingTitleRt.anchorMax = V2(0.96f, 0.98f);
            ingTitleRt.offsetMin = ingTitleRt.offsetMax = Vector2.zero;
            ingTitle.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            MkVerticalScrollArea(ingScr.transform, "Scroll", V2(0.03f, 0.13f), V2(0.97f, 0.84f), out RectTransform iListContent);
            var iCloseBtn = MkButton(ingScr.transform, "CloseBtn", "Закрыть", font, new Color(0.52f, 0.3f, 0.24f, 1f));
            var iCloseRt = iCloseBtn.GetComponent<RectTransform>();
            iCloseRt.anchorMin = V2(0.28f, 0.02f); iCloseRt.anchorMax = V2(0.72f, 0.1f);
            iCloseRt.offsetMin = iCloseRt.offsetMax = Vector2.zero;
            var isv = ingScr.AddComponent<IngredientsScreenView>();
            SetField(isv, "ingredientListContainer", iListContent);
            SetField(isv, "closeButton", iCloseBtn.GetComponent<Button>());

            var stoveV = Object.FindObjectOfType<StoveView>();
            if (stoveV != null) { SetField(stoveV, "ingredientsScreen", isv); SetField(isv, "stove", stoveV); }

            var upgScr = MkPanel(uiRoot, "UpgradeScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.98f, 0.95f, 0.9f, 0.99f));
            upgScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(upgScr);
            var upgSh = upgScr.AddComponent<Shadow>(); upgSh.effectDistance = new Vector2(3, -4); upgSh.effectColor = new Color(0, 0, 0, 0.28f);
            MkPanel(upgScr.transform, "Header", V2(0f, 0.86f), V2(1f, 1f), new Color(0.45f, 0.32f, 0.22f, 1f));
            var upgTitle = MkLabel(upgScr.transform, "Title", "Прокачка сковороды", font, 22, Color.white, 0);
            var upgTitleRt = upgTitle.GetComponent<RectTransform>();
            upgTitleRt.anchorMin = V2(0.04f, 0.86f); upgTitleRt.anchorMax = V2(0.96f, 0.98f);
            upgTitleRt.offsetMin = upgTitleRt.offsetMax = Vector2.zero;
            upgTitle.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            MkVerticalScrollArea(upgScr.transform, "Scroll", V2(0.03f, 0.13f), V2(0.97f, 0.84f), out RectTransform uListContent);
            var uCloseBtn = MkButton(upgScr.transform, "CloseBtn", "Закрыть", font, new Color(0.52f, 0.3f, 0.24f, 1f));
            var uCloseRt = uCloseBtn.GetComponent<RectTransform>();
            uCloseRt.anchorMin = V2(0.28f, 0.02f); uCloseRt.anchorMax = V2(0.72f, 0.1f);
            uCloseRt.offsetMin = uCloseRt.offsetMax = Vector2.zero;
            var usv = upgScr.AddComponent<PanUpgradeScreenView>();
            SetField(usv, "upgradeListContainer", uListContent);
            SetField(usv, "closeButton", uCloseBtn.GetComponent<Button>());

            var ctrlGo = new GameObject("PancakeFlipController");
            var ctrl = ctrlGo.AddComponent<PancakeFlipController>();
            SetField(ctrl, "pancake", pcBh); SetField(ctrl, "pan", panBh);
            SetField(ctrl, "chargeIndicator", cv); SetField(ctrl, "config", flipConfig);

            var sessGo = new GameObject("GameSession");
            var sess = sessGo.AddComponent<GameSession>();
            SetField(sess, "flipConfig", flipConfig); SetField(sess, "levelTable", levelTable);
            SetField(sess, "pancake", pcBh); SetField(sess, "baseRecipe", baseRecipe);
            SetFieldArr(sess, "startingRecipes", new Object[] { baseRecipe, cheeseHamRecipe, bananaChocoRecipe, mushroomRecipe, strawberryChocoRecipe });
            SetFieldArr(sess, "allIngredients", new Object[] { dough, salami, cheese, banana, chocolate, mushroom, strawberry });
            SetField(sess, "doughIngredient", dough);
            SetFieldArr(sess, "allUpgrades", new Object[] { upg1, upg2 });

            var mscGo = new GameObject("MainScreenController");
            var msc = mscGo.AddComponent<MainScreenController>();
            SetField(msc, "recipeBookScreen", rbsv);
            SetField(msc, "upgradeScreen", usv);
            SetField(msc, "customerAnimator", custAnim);

            var kitchenBottom = new GameObject("KitchenBottom", typeof(RectTransform), typeof(KitchenUiFrontLayer));
            kitchenBottom.transform.SetParent(uiRoot, false);
            var kitchenBottomRt = kitchenBottom.GetComponent<RectTransform>();
            kitchenBottomRt.anchorMin = Vector2.zero;
            kitchenBottomRt.anchorMax = Vector2.one;
            kitchenBottomRt.offsetMin = kitchenBottomRt.offsetMax = Vector2.zero;

            var recipesBtnGo = MkKitchenSpriteButton(kitchenBottom.transform, "RecipesBtn", V2(0.05f, 0.04f), V2(0.17f, 0.18f), receiptBtnSpr);
            var upgradesBtnGo = MkKitchenSpriteButton(kitchenBottom.transform, "UpgradesBtn", V2(0.83f, 0.04f), V2(0.95f, 0.18f), panUpgradeBtnSpr);

            var doughBowlRoot = new GameObject("DoughBowl", typeof(RectTransform), typeof(Image), typeof(Button));
            doughBowlRoot.transform.SetParent(kitchenBottom.transform, false);
            var doughRt = doughBowlRoot.GetComponent<RectTransform>();
            doughRt.anchorMin = V2(0.04f, 0.10f); doughRt.anchorMax = V2(0.2f, 0.31f);
            doughRt.offsetMin = doughRt.offsetMax = Vector2.zero;
            var doughImg = doughBowlRoot.GetComponent<Image>();
            doughImg.sprite = doughEmptySpr;
            doughImg.preserveAspect = true;
            doughImg.color = Color.white;
            doughImg.type = Image.Type.Simple;
            doughImg.raycastTarget = true;
            var doughBtn = doughBowlRoot.GetComponent<Button>();
            doughBtn.targetGraphic = doughImg;
            var dbv = doughBowlRoot.AddComponent<DoughBowlView>();
            SetField(dbv, "doughIngredient", dough);
            SetField(dbv, "bowlButton", doughBtn);
            SetField(dbv, "bowlImage", doughImg);
            SetField(dbv, "emptySprite", doughEmptySpr);
            SetField(dbv, "fullSprite", doughFullSpr);

            var kHud = new GameObject("KitchenHUD", typeof(RectTransform));
            kHud.transform.SetParent(uiRoot, false);
            var kHudRt = kHud.GetComponent<RectTransform>();
            kHudRt.anchorMin = kHudRt.anchorMax = Vector2.zero;
            kHudRt.sizeDelta = Vector2.zero;
            var kBar = kHud.AddComponent<KitchenBarController>();
            SetField(kBar, "mainScreen", msc);
            SetField(kBar, "recipesButton", recipesBtnGo.GetComponent<Button>());
            SetField(kBar, "upgradesButton", upgradesBtnGo.GetComponent<Button>());
            kitchenBottom.transform.SetAsLastSibling();

            EnsureFolder("Assets/Scenes");
            var active = EditorSceneManager.GetActiveScene();
            if (!EditorSceneManager.SaveScene(active, OutputScenePath))
            {
                Debug.LogError($"PancakeFlip: SaveScene не удалось — путь «{OutputScenePath}». Проверь права и что не открыт Play.");
                return;
            }

            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);

            var sceneAsset = AssetDatabase.LoadAssetAtPath<Object>(OutputScenePath);
            if (sceneAsset != null)
                EditorGUIUtility.PingObject(sceneAsset);

            Debug.Log($"PancakeFlip: сцена записана и сразу открыта: {OutputScenePath} (иерархия обновлена).");
        }

        [MenuItem("PancakeFlip/Open PancakeFlip Scene")]
        public static void OpenPancakeFlipScene()
        {
            if (Application.isPlaying) { Debug.LogWarning("Останови Play."); return; }
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.dataPath, "Scenes/PancakeFlip.unity")))
            {
                Debug.LogWarning("Файла сцены ещё нет — сначала PancakeFlip → Build Everything.");
                return;
            }
            EditorSceneManager.OpenScene(OutputScenePath, OpenSceneMode.Single);
            var o = AssetDatabase.LoadAssetAtPath<Object>(OutputScenePath);
            if (o != null) EditorGUIUtility.PingObject(o);
        }

        static IngredientConfig CreateIngredient(string n, int cost, int lvl, bool inf)
        {
            var o = GetOrCreate<IngredientConfig>(DataDir, n);
            o.displayName = n; o.coinCost = cost; o.unlockLevel = lvl; o.infinite = inf;
            EditorUtility.SetDirty(o); return o;
        }
        static RecipeConfig CreateRecipe(string n, int lvl, int coins, int xp, RecipeConfig.IngredientSlot[] ing, Sprite icon = null)
        {
            var o = GetOrCreate<RecipeConfig>(DataDir, n);
            o.displayName = n; o.unlockLevel = lvl; o.rewardCoins = coins; o.rewardXp = xp; o.ingredients = ing;
            o.icon = icon;
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
            foreach (var n in new[] { "Background", "OrderList", "OrderItem", "image 5", "image 4", "Pan", "Pancake", "PancakeSide", "Empty", "Full", "ReceiptButton", "PanUpgradeButton", "Profile", "Wallet", "BackPan", "FrontPan", "BottomPanel", "Person1", "Person2", "Person3", "Person4", "Person1Icon", "Person2Icon", "Person3Icon", "RewardInfo", "CommonPancake", "CheeseHamPancake", "ChocolateStrawberryPancake", "XPIcon" })
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

        /// <summary>Модалки выше слоя кухни (<see cref="KitchenUiFrontLayer.SortOrder"/>).</summary>
        static void AddModalCanvasLayer(GameObject go)
        {
            var c = go.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 100;
            go.AddComponent<GraphicRaycaster>();
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

        static void MkVerticalScrollArea(Transform parent, string name, Vector2 amin, Vector2 amax, out RectTransform contentOut)
        {
            var scrollGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            var sRt = scrollGo.GetComponent<RectTransform>();
            sRt.anchorMin = amin; sRt.anchorMax = amax; sRt.offsetMin = sRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0.78f, 0.74f, 0.68f, 0.45f);

            var vp = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            vp.transform.SetParent(scrollGo.transform, false);
            var vpRt = vp.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(8, 6); vpRt.offsetMax = new Vector2(-8, -6);
            vp.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            vp.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(vp.transform, false);
            var cRt = content.GetComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, 0f);
            var cv = content.GetComponent<ContentSizeFitter>();
            cv.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            cv.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vl = content.GetComponent<VerticalLayoutGroup>();
            vl.spacing = 10;
            vl.padding = new RectOffset(4, 4, 4, 8);
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.childForceExpandWidth = true;

            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.viewport = vpRt;
            sr.content = cRt;
            sr.horizontal = false;
            sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped;
            sr.scrollSensitivity = 28f;

            contentOut = cRt;
        }

        static GameObject MkKitchenSpriteButton(Transform parent, string name, Vector2 amin, Vector2 amax, Sprite spr)
        {
            var g = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            g.transform.SetParent(parent, false);
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = amin; rt.anchorMax = amax; rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = g.GetComponent<Image>();
            img.sprite = spr;
            img.preserveAspect = true;
            img.color = Color.white;
            img.type = Image.Type.Simple;
            img.raycastTarget = true;
            var btn = g.GetComponent<Button>();
            btn.targetGraphic = img;
            return g;
        }

        static OrderCardView MkOrderCard(Transform p, Font f, Sprite itemSpr, Sprite rewardSpr,
            Sprite[] personIcons, Sprite coinSpr, Sprite xpSpr)
        {
            var root = new GameObject("OrderCard", typeof(RectTransform));
            root.transform.SetParent(p, false);

            var rewardBg = new GameObject("RewardBg", typeof(RectTransform), typeof(Image));
            rewardBg.transform.SetParent(root.transform, false);
            var rbImg = rewardBg.GetComponent<Image>();
            if (rewardSpr != null) { rbImg.sprite = rewardSpr; rbImg.color = Color.white; rbImg.preserveAspect = true; }
            else rbImg.color = new Color(0.9f, 0.85f, 0.6f);
            Anch(rewardBg, 0.4f, 0.4f, 1.24f, 1.24f);

            var coinTxtGo = new GameObject("CoinTxt", typeof(RectTransform));
            coinTxtGo.transform.SetParent(rewardBg.transform, false); Anch(coinTxtGo, 0.06f, 0.62f, 0.52f, 0.78f);
            var coinT = coinTxtGo.AddComponent<Text>(); coinT.fontSize = 30; coinT.color = new Color(0.2f, 0.15f, 0.1f);
            coinT.alignment = TextAnchor.MiddleRight; coinT.fontStyle = FontStyle.Bold; if (f) coinT.font = f;

            var coinIco = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
            coinIco.transform.SetParent(rewardBg.transform, false); Anch(coinIco, 0.52f, 0.63f, 0.66f, 0.77f);
            var coinIcoImg = coinIco.GetComponent<Image>(); coinIcoImg.preserveAspect = true;
            if (coinSpr != null) { coinIcoImg.sprite = coinSpr; coinIcoImg.color = Color.white; }

            var xpTxtGo = new GameObject("XpTxt", typeof(RectTransform));
            xpTxtGo.transform.SetParent(rewardBg.transform, false); Anch(xpTxtGo, 0.06f, 0.44f, 0.52f, 0.60f);
            var xpT = xpTxtGo.AddComponent<Text>(); xpT.fontSize = 30; xpT.color = new Color(0.2f, 0.15f, 0.1f);
            xpT.alignment = TextAnchor.MiddleRight; xpT.fontStyle = FontStyle.Bold; if (f) xpT.font = f;

            var xpIco = new GameObject("XpIcon", typeof(RectTransform), typeof(Image));
            xpIco.transform.SetParent(rewardBg.transform, false); Anch(xpIco, 0.52f, 0.45f, 0.66f, 0.59f);
            var xpIcoImg = xpIco.GetComponent<Image>(); xpIcoImg.preserveAspect = true;
            if (xpSpr != null) { xpIcoImg.sprite = xpSpr; xpIcoImg.color = Color.white; }

            var card = new GameObject("CardBg", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(root.transform, false); Fill(card);
            var cardImg = card.GetComponent<Image>();
            if (itemSpr != null) { cardImg.sprite = itemSpr; cardImg.color = Color.white; }
            else cardImg.color = new Color(0.95f, 0.9f, 0.8f, 0.95f);

            var recipeImg = new GameObject("RecipeImage", typeof(RectTransform), typeof(Image));
            recipeImg.transform.SetParent(card.transform, false); Anch(recipeImg, 0.2f, 0.22f, 0.8f, 0.78f);
            var riImg = recipeImg.GetComponent<Image>(); riImg.preserveAspect = true;
            riImg.color = Color.white; riImg.enabled = false;

            var hl = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            hl.transform.SetParent(card.transform, false); Fill(hl);
            var hlI = hl.GetComponent<Image>(); hlI.color = new Color(1, 0.85f, 0.3f, 0.35f); hlI.enabled = false;

            var sel = new GameObject("SelectBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            sel.transform.SetParent(root.transform, false); Fill(sel); sel.GetComponent<Image>().color = new Color(1, 1, 1, 0.01f);

            var personGo = new GameObject("PersonIcon", typeof(RectTransform), typeof(Image));
            personGo.transform.SetParent(root.transform, false);
            var piImg = personGo.GetComponent<Image>(); piImg.preserveAspect = true;
            piImg.color = Color.white;
            Anch(personGo, 0.44f, 0.1f, 0.88f, 0.46f);

            var cv2 = root.AddComponent<OrderCardView>();
            SetField(cv2, "recipeImage", riImg);
            SetField(cv2, "rewardBg", rbImg);
            SetField(cv2, "coinText", coinT);
            SetField(cv2, "xpText", xpT);
            SetField(cv2, "personIcon", piImg);
            SetField(cv2, "selectButton", sel.GetComponent<Button>());
            SetField(cv2, "selectionHighlight", hlI);
            if (personIcons != null && personIcons.Length > 0)
            {
                var arr = new Object[personIcons.Length];
                for (int i = 0; i < personIcons.Length; i++) arr[i] = personIcons[i];
                SetFieldArr(cv2, "personIconSprites", arr);
            }
            return cv2;
        }
        static void Anch(GameObject g, float xmin, float ymin, float xmax, float ymax)
        {
            var r = g.GetComponent<RectTransform>();
            r.anchorMin = V2(xmin, ymin); r.anchorMax = V2(xmax, ymax);
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        const float CookPancakePreviewSize = 172f;

        static void MkCookPancakePreview(GameObject root, string side, Vector2 amin, Vector2 amax, Sprite pancakeSideSpr, Font f, out Image pancakeImg)
        {
            var row = new GameObject($"CookRow{side}", typeof(RectTransform));
            row.transform.SetParent(root.transform, false);
            Anch(row, amin.x, amin.y, amax.x, amax.y);

            var lblGo = MkLabel(row.transform, $"Lbl{side}", side, f, 18, new Color(0.95f, 0.92f, 0.85f), 0);
            var lblRt = lblGo.GetComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0f, 0.1f);
            lblRt.anchorMax = new Vector2(0.12f, 0.9f);
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            lblGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            var imgGo = new GameObject("PancakePreview", typeof(RectTransform), typeof(Image));
            imgGo.transform.SetParent(row.transform, false);
            var rt = imgGo.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.68f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(CookPancakePreviewSize, CookPancakePreviewSize);
            rt.anchoredPosition = Vector2.zero;

            pancakeImg = imgGo.GetComponent<Image>();
            pancakeImg.sprite = pancakeSideSpr;
            pancakeImg.type = Image.Type.Simple;
            pancakeImg.preserveAspect = true;
            pancakeImg.color = Color.white;
            pancakeImg.raycastTarget = false;
        }

        static void SetField(Object obj, string prop, Object val)
        {
            var so = new SerializedObject(obj); var p = so.FindProperty(prop);
            if (p != null) { p.objectReferenceValue = val; so.ApplyModifiedPropertiesWithoutUndo(); }
        }
        static void SetFloatField(Object obj, string prop, float val)
        {
            var so = new SerializedObject(obj);
            var p = so.FindProperty(prop);
            if (p != null && p.propertyType == SerializedPropertyType.Float) { p.floatValue = val; so.ApplyModifiedPropertiesWithoutUndo(); }
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
