using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
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

        [MenuItem("PancakeFlip/Подставить ссылки GameSession (сковородки)")]
        public static void WireGameSessionPanData()
        {
            if (Application.isPlaying) { Debug.LogWarning("Останови Play."); return; }
            ForceAllSprites();
            var panSpr = LoadSprite("Pan");
            var frontPanSpr = LoadSprite("FrontPan");
            EnsureDefaultPanProgressionAssets(panSpr, frontPanSpr);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var all = Object.FindObjectsByType<GameSession>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (all.Length == 0)
            {
                Debug.LogWarning("PancakeFlip: в загруженных сценах нет GameSession.");
                return;
            }
            var uiFont = LoadPancakeFlipUiFont();
            foreach (var gs in all)
            {
                gs.AutowirePanAssetsIfEmpty();
                SetField(gs, "uiFont", uiFont);
                EditorUtility.SetDirty(gs);
                if (gs.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(gs.gameObject.scene);
            }
            Debug.Log($"PancakeFlip: подставлены statTracks / panTiers в {all.Length} GameSession. Сохрани сцену (Ctrl+S), если нужно.");
        }

        public static void EnsureDefaultPanProgressionAssets(Sprite panSpr, Sprite frontPanSpr)
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(DataDir);
            CreateStatTrack("StatWidePerfect", "Прожарка: шире норма", PanUpgradeConfig.EffectType.WiderPerfectZone, 1, 25,
                "До 5 уровней: шире зона идеальной прожарки.", panSpr);
            CreateStatTrack("StatSlowOver", "Меньше пережар", PanUpgradeConfig.EffectType.SlowerOvercook, 1, 25,
                "До 5 уровней: выше порог пережарки.", panSpr);
            CreateStatTrack("StatStableSpin", "Стабильное вращение", PanUpgradeConfig.EffectType.StablerSpin, 2, 30,
                "До 5 уровней: спокойнее падение на сковороду.", panSpr);
            CreateStatTrack("StatEasyFlip", "Легче подброс", PanUpgradeConfig.EffectType.EasierFlip, 2, 30,
                "До 5 уровней: сильнее толчок при том же заряде.", panSpr);

            var pan01 = LoadSprite("PanImage01");
            var pan02 = LoadSprite("PanImage02");
            var pan03 = LoadSprite("PanImage03");
            var pan04 = LoadSprite("PanImage04");
            CreatePanTier("PanStarter", "Сковорода из ларька", true, 0, 0, pan01 != null ? pan01 : panSpr,
                "Стартовая. Прокачка ячеек сохраняется при смене сковороды.", 1f, 1f, 1f, 1f);
            CreatePanTier("PanIron", "Чугунная", false, 120, 3, pan02 != null ? pan02 : panSpr,
                "Тяжёлая, ровнее жар. База +5% к каждой характеристике.", 1.05f, 1.05f, 1.05f, 1.05f);
            CreatePanTier("PanPro", "Профи", false, 280, 5, pan03 != null ? pan03 : panSpr,
                "Рабочая сковорода. База +10%.", 1.1f, 1.1f, 1.1f, 1.1f);
            CreatePanTier("PanElite", "Элитная", false, 520, 8, pan04 != null ? pan04 : panSpr,
                "Мастерская. База +15% к каждой характеристике.", 1.15f, 1.15f, 1.15f, 1.15f);
        }

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

            var bootGo = new GameObject("PancakeFlipScreenBootstrap");
            bootGo.AddComponent<PancakeFlipScreenBootstrap>();
            bootGo.AddComponent<ResponsiveLayout>();

            ForceAllSprites();
            // 9-slice бордеры для кнопок-спрайтов, чтобы они не растягивались/не искажались.
            SetSpriteBorder("ActionButton", 40f);
            SetSpriteBorder("SuccessButton", 40f);
            SetSpriteBorder("CancelButton", 40f);
            SetSpriteBorder("RecipesHud", 90f);
            SetSpriteBorder("PanHud", 90f);
            SetSpriteBorder("RecipeHudSpot", 40f);
            SetSpriteBorder("IngredientSpot", 40f);
            // Пер-городские фоны/плиты (Заправка=01, Средний=02, Бостон=03). Открытая плита пока только City01.
            var bgCity01 = LoadSprite("BackgroundCity01");
            var bgCity02 = LoadSprite("BackgroundCity02");
            var bgCity03 = LoadSprite("BackgroundCity03");
            var stoveCity01 = LoadSprite("StoveCity01");
            var stoveCity02 = LoadSprite("StoveCity02");
            var stoveCity03 = LoadSprite("StoveCity03");
            var stoveOpen01 = LoadSprite("StoveOpenCity01");
            var bgSpr = bgCity01;
            var panSpr = LoadSprite("Pan");
            var pancakeSpr = LoadSprite("Pancake");
            var stoveClosedS = stoveCity01;
            var stoveOpenS = stoveOpen01;
            var orderListSpr = LoadSprite("OrderList");
            var orderItemSpr = LoadSprite("OrderItem");
            var profileSpr = LoadSprite("Profile");
            var walletSpr = LoadSprite("Wallet");
            var backPanSpr = LoadSprite("BackPan");
            var frontPanSpr = LoadSprite("FrontPan");
            var bottomPanelSpr = LoadSprite("BottomPanel");
            var bottomPanelCity02 = LoadSprite("BottomPanelCity02");
            var bottomPanelCity03 = LoadSprite("BottomPanelCity03");
            var person1 = LoadSprite("Person1");
            var person2 = LoadSprite("Person2");
            var person3 = LoadSprite("Person3");
            var person4 = LoadSprite("Person4");
            var person1Icon = LoadSprite("Person1Icon");
            var person2Icon = LoadSprite("Person2Icon");
            var person3Icon = LoadSprite("Person3Icon");
            // Посетители по городам: City02 — Средний Сити, City03 — Бостон Сити.
            var personCity02_1 = LoadSprite("Person01City02");
            var personCity02_2 = LoadSprite("Person02City02");
            var personCity02_3 = LoadSprite("Person03City02");
            var personCity03_1 = LoadSprite("Person01City03");
            var personCity03_2 = LoadSprite("Person02City03");
            var personCity03_3 = LoadSprite("Person03City03");
            var rewardInfoSpr = LoadSprite("RewardInfo");
            var commonPancakeSpr = LoadSprite("CommonPancake");
            var cheeseHamPancakeSpr = LoadSprite("CheeseHamPancake");
            var chocoStrawberrySpr = LoadSprite("ChocolateStrawberryPancake");
            var bananaChocoPancakeSpr = LoadSprite("BananaChocolatePancake");
            var mushroomPancakeSpr = LoadSprite("MushroomPancake");
            var meatPancakeSpr = LoadSprite("MeatPancake");
            var jamPeanutPancakeSpr = LoadSprite("JamPeanutPancake");
            var salmonScallopsPancakeSpr = LoadSprite("SalmonScallopsPancake");
            var blackCaviarPancakeSpr = LoadSprite("BlackCaviarPancake");
            var xpIconSpr = LoadSprite("XPIcon");
            var pancakeSideUiSpr = LoadSprite("PancakeSide");
            var receiptBtnSpr = LoadSprite("ReceiptButton");
            var panUpgradeBtnSpr = LoadSprite("PanUpgradeButton");
            var closeIconSpr = LoadSprite("CloseIcon");
            var globalMapSpr = LoadSprite("GlobalMap");
            var globalMapPlateSpr = LoadSprite("GlobalMapPlate");
            var city01Spr = LoadSprite("City01");
            var city02Spr = LoadSprite("City02");
            var city03Spr = LoadSprite("City03");
            var blockedSpr = LoadSprite("Blocked");
            var carSpr = LoadSprite("Car");
            var actionBtnSpr = LoadSprite("ActionButton");
            var successBtnSpr = LoadSprite("SuccessButton");
            var cancelBtnSpr = LoadSprite("CancelButton");
            var recipesHudSpr = LoadSprite("RecipesHud");
            var panHudSpr = LoadSprite("PanHud");
            var ingredientSpotSpr = LoadSprite("IngredientSpot");
            var recipeHudSpotSpr = LoadSprite("RecipeHudSpot");
            var uiFont = LoadPancakeFlipUiFont();
            var tmpFont = GetOrCreateEditorTmpFont(uiFont);

            EnsureFolder("Assets/Data");
            EnsureFolder(DataDir);
            var flipConfig = GetOrCreate<PancakeFlipConfig>(ConfigsFolder, "PancakeFlipConfig");
            var levelTable = GetOrCreate<LevelTableConfig>(ConfigsFolder, "LevelTable");

            var dough = CreateIngredient("Тесто", 0, 0, false, maxStock: 10, isDough: true);
            var salami = CreateIngredient("Салями", 5, 1, false);
            var cheese = CreateIngredient("Сыр", 5, 1, false);
            var banana = CreateIngredient("Банан", 4, 2, false);
            var chocolate = CreateIngredient("Шоколад", 6, 2, false);
            var mushroom = CreateIngredient("Гриб", 4, 3, false);
            var strawberry = CreateIngredient("Клубника", 5, 3, false);

            var baseRecipe = CreateRecipe("Обычный блин", 0, 5, 10,
                new[] { new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 } },
                pancakeSpr != null ? pancakeSpr : commonPancakeSpr);
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
                }, bananaChocoPancakeSpr != null ? bananaChocoPancakeSpr : commonPancakeSpr);
            var mushroomRecipe = CreateRecipe("Грибной блин", 3, 22, 40,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = mushroom, amount = 2 }
                }, mushroomPancakeSpr != null ? mushroomPancakeSpr : commonPancakeSpr);
            var strawberryChocoRecipe = CreateRecipe("Блин с клубникой и шоколадом", 3, 25, 45,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = strawberry, amount = 2 },
                    new RecipeConfig.IngredientSlot { ingredient = chocolate, amount = 1 }
                }, chocoStrawberrySpr);

            // --- Новые ингредиенты по городам ---
            var meat = CreateIngredient("Мясо", 9, 3, false); meat.icon = LoadSprite("Meat"); EditorUtility.SetDirty(meat);
            var jam = CreateIngredient("Джем", 7, 3, false); jam.icon = LoadSprite("Jam"); EditorUtility.SetDirty(jam);
            var peanutButter = CreateIngredient("Арахисовая паста", 8, 3, false); peanutButter.icon = LoadSprite("PeanutButter"); EditorUtility.SetDirty(peanutButter);
            var salmon = CreateIngredient("Лосось", 14, 5, false); salmon.icon = LoadSprite("Salmon"); EditorUtility.SetDirty(salmon);
            var caviar = CreateIngredient("Чёрная икра", 25, 5, false); caviar.icon = LoadSprite("BlackCaviar"); EditorUtility.SetDirty(caviar);
            var scallops = CreateIngredient("Гребешки", 18, 5, false); scallops.icon = LoadSprite("Scallops"); EditorUtility.SetDirty(scallops);

            // --- Новые рецепты со своими картинками блинов ---
            var meatRecipe = CreateRecipe("Сытный блин с мясом", 3, 30, 50,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = meat, amount = 2 }
                }, meatPancakeSpr != null ? meatPancakeSpr : commonPancakeSpr);
            var jamRecipe = CreateRecipe("Блин с джемом", 3, 24, 40,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = jam, amount = 1 }
                }, jamPeanutPancakeSpr != null ? jamPeanutPancakeSpr : commonPancakeSpr);
            var peanutRecipe = CreateRecipe("Блин с арахисовой пастой", 3, 26, 42,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = peanutButter, amount = 1 }
                }, jamPeanutPancakeSpr != null ? jamPeanutPancakeSpr : commonPancakeSpr);
            var salmonRecipe = CreateRecipe("Блин с лососем", 5, 45, 70,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = salmon, amount = 1 }
                }, salmonScallopsPancakeSpr != null ? salmonScallopsPancakeSpr : commonPancakeSpr);
            var caviarRecipe = CreateRecipe("Блин с чёрной икрой", 5, 60, 90,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = caviar, amount = 1 }
                }, blackCaviarPancakeSpr != null ? blackCaviarPancakeSpr : commonPancakeSpr);
            var scallopRecipe = CreateRecipe("Блин с гребешками", 5, 50, 78,
                new[] {
                    new RecipeConfig.IngredientSlot { ingredient = dough, amount = 1 },
                    new RecipeConfig.IngredientSlot { ingredient = scallops, amount = 2 }
                }, salmonScallopsPancakeSpr != null ? salmonScallopsPancakeSpr : commonPancakeSpr);

            // Всё текущее содержимое живёт на Заправке. Другие локации — заглушки под будущий
            // НОВЫЙ контент (свои рецепты/ингредиенты), а не перераспределение заправочного.
            var allRecipes = new[] { baseRecipe, cheeseHamRecipe, bananaChocoRecipe, mushroomRecipe, strawberryChocoRecipe };
            var locStall = CreateLocation("LocStall", "Заправка", 5,
                allRecipes,
                new[] { cheeseHamRecipe, bananaChocoRecipe, mushroomRecipe, strawberryChocoRecipe },
                new[] { salami, cheese, banana, chocolate, mushroom, strawberry },
                new[] { person1, person2, person3, person4 });
            // Спрос локальный: каждый город заказывает только свои рецепты (без заказов прошлого города).
            var locPromenade = CreateLocation("LocPromenade", "Бостон Сити", 8,
                new[] { salmonRecipe, caviarRecipe, scallopRecipe },
                new[] { salmonRecipe, caviarRecipe, scallopRecipe },
                new[] { salmon, caviar, scallops },
                new[] { personCity03_1, personCity03_2, personCity03_3 });
            var locMarket = CreateLocation("LocMarket", "Средний Сити", 10,
                new[] { meatRecipe, jamRecipe, peanutRecipe },
                new[] { meatRecipe, jamRecipe, peanutRecipe },
                new[] { meat, jam, peanutButter },
                new[] { personCity02_1, personCity02_2, personCity02_3 });
            locStall.requiredLevel = 1; locStall.cityCost = 0;
            locStall.mapPosition = new Vector2(0.64f, 0.16f); locStall.mapIcon = city01Spr;
            locStall.background = bgCity01; locStall.stoveClosed = stoveCity01; locStall.stoveOpen = stoveOpen01;
            locStall.bottomPanel = bottomPanelSpr; // у Заправки прилавок отдельным спрайтом
            // Средний Сити (Рынок) открывается вторым; Бостон Сити (Набережная) — третьим.
            locMarket.requiredLevel = 3; locMarket.cityCost = 150;
            locMarket.mapPosition = new Vector2(0.87f, 0.44f); locMarket.mapIcon = city03Spr;
            locMarket.background = bgCity02; locMarket.stoveClosed = stoveCity02; locMarket.stoveOpen = null;
            locMarket.bottomPanel = bottomPanelCity02;
            locPromenade.requiredLevel = 5; locPromenade.cityCost = 300;
            locPromenade.mapPosition = new Vector2(0.55f, 0.47f); locPromenade.mapIcon = city02Spr;
            locPromenade.background = bgCity03; locPromenade.stoveClosed = stoveCity03; locPromenade.stoveOpen = null;
            locPromenade.bottomPanel = bottomPanelCity03;
            EditorUtility.SetDirty(locStall); EditorUtility.SetDirty(locPromenade); EditorUtility.SetDirty(locMarket);

            var worldMap = GetOrCreate<WorldMapConfig>(DataDir, "WorldMap");
            worldMap.locations = new[] { locStall, locPromenade, locMarket };
            EditorUtility.SetDirty(worldMap);
            AssetDatabase.SaveAssets();

            EnsureDefaultPanProgressionAssets(panSpr, frontPanSpr);
            var statWide = FindAssetByName<PanStatTrackConfig>("StatWidePerfect");
            var statOver = FindAssetByName<PanStatTrackConfig>("StatSlowOver");
            var statSpin = FindAssetByName<PanStatTrackConfig>("StatStableSpin");
            var statFlip = FindAssetByName<PanStatTrackConfig>("StatEasyFlip");
            var panStarter = FindAssetByName<PanTierConfig>("PanStarter");
            var panIron = FindAssetByName<PanTierConfig>("PanIron");
            var panPro = FindAssetByName<PanTierConfig>("PanPro");
            var panElite = FindAssetByName<PanTierConfig>("PanElite");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            float camH = ortho * 2f, camW = camH * (9f / 16f);

            if (bgSpr != null)
            {
                var bg = new GameObject("Background");
                var bgSr = bg.AddComponent<SpriteRenderer>(); bgSr.sprite = bgSpr; bgSr.sortingOrder = -100;
                const float bgOverscan = 1.24f;
                float sc = Mathf.Max(camW / bgSpr.bounds.size.x, camH / bgSpr.bounds.size.y) * bgOverscan;
                bg.transform.localScale = Vector3.one * sc;
                bg.transform.position = new Vector3(0, 0.38f, 10);
            }

            if (bottomPanelSpr != null)
            {
                var bpGo = new GameObject("BottomPanel");
                var bpSr = bpGo.AddComponent<SpriteRenderer>(); bpSr.sprite = bottomPanelSpr; bpSr.sortingOrder = -3;
                float bpSc = camW / bottomPanelSpr.bounds.size.x;
                bpGo.transform.localScale = Vector3.one * bpSc;
                float bpH = bottomPanelSpr.bounds.size.y * bpSc;
                bpGo.transform.position = new Vector3(0, -ortho + bpH * 0.5f, 0);
            }

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

            const float worldScale = 0.25f;
            const float panLiftWorld = 0.12f;

            var panGo = new GameObject("Pan");
            panGo.transform.position = new Vector3(0, burnerY + panLiftWorld, 0);
            panGo.transform.localScale = Vector3.one * worldScale;
            panGo.AddComponent<BoxCollider2D>();
            var panBh = panGo.AddComponent<PanBehaviour>();

            var backPanGo = new GameObject("BackPan");
            backPanGo.transform.SetParent(panGo.transform, false);
            var backPanSr = backPanGo.AddComponent<SpriteRenderer>();
            if (backPanSpr != null) { backPanSr.sprite = backPanSpr; backPanSr.color = Color.white; }
            else if (panSpr != null) { backPanSr.sprite = panSpr; backPanSr.color = Color.white; }
            backPanSr.sortingOrder = 0;

            var frontPanGo = new GameObject("FrontPan");
            frontPanGo.transform.SetParent(panGo.transform, false);
            var frontPanSr = frontPanGo.AddComponent<SpriteRenderer>();
            if (frontPanSpr != null) { frontPanSr.sprite = frontPanSpr; frontPanSr.color = Color.white; }
            frontPanSr.sortingOrder = 2;

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
            pcGo.AddComponent<PancakePlasticity>();
            pcBh.SetPanCenter(panBh.PanCenter);
            SetField(pcBh, "config", flipConfig);
            SetField(pcBh, "spriteFaceA", pancakeSpr);
            // Обратная сторона — то же изображение (отзеркаливается в рантайме).
            SetField(pcBh, "spriteFaceB", pancakeSpr);

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            Transform uiRoot = canvas.transform;

            var profileGo = new GameObject("ProfileIcon", typeof(RectTransform), typeof(Image));
            profileGo.transform.SetParent(uiRoot, false);
            var profileR = profileGo.GetComponent<RectTransform>();
            profileR.anchorMin = V2(0.82f, 0.91f); profileR.anchorMax = V2(0.97f, 0.99f);
            profileR.offsetMin = profileR.offsetMax = Vector2.zero;
            AddResponsive(profileGo, V2(0.82f, 0.91f), V2(0.97f, 0.99f), V2(0.885f, 0.85f), V2(0.965f, 0.95f));
            var profileImg = profileGo.GetComponent<Image>();
            if (profileSpr != null) { profileImg.sprite = profileSpr; profileImg.preserveAspect = true; }
            else profileImg.color = new Color(0.8f, 0.3f, 0.3f);
            profileImg.raycastTarget = true;
            var profileBtn = profileGo.AddComponent<Button>();
            profileBtn.targetGraphic = profileImg;
            profileBtn.transition = Selectable.Transition.None;

            var lvlTxt = new GameObject("LevelText", typeof(RectTransform));
            lvlTxt.transform.SetParent(profileGo.transform, false);
            Anch(lvlTxt, -0.2f, -0.42f, 1.2f, 0f);
            var lvlT = lvlTxt.AddComponent<TextMeshProUGUI>();
            lvlT.text = "Level 1";
            lvlT.fontSize = PancakeFlipUiTypography.TopBarCoinsLevel;
            lvlT.alignment = TextAlignmentOptions.Center;
            lvlT.color = Color.white;
            lvlT.fontStyle = FontStyles.Bold;
            lvlT.raycastTarget = false;
            if (tmpFont != null) lvlT.font = tmpFont;

            var walletGo = new GameObject("WalletIcon", typeof(RectTransform), typeof(Image));
            walletGo.transform.SetParent(uiRoot, false);
            var walletR = walletGo.GetComponent<RectTransform>();
            walletR.anchorMin = V2(0.82f, 0.805f); walletR.anchorMax = V2(0.97f, 0.865f);
            walletR.offsetMin = walletR.offsetMax = Vector2.zero;
            AddResponsive(walletGo, V2(0.82f, 0.805f), V2(0.97f, 0.865f), V2(0.885f, 0.74f), V2(0.965f, 0.83f));
            var walletImg = walletGo.GetComponent<Image>();
            if (walletSpr != null) { walletImg.sprite = walletSpr; walletImg.preserveAspect = true; }
            else walletImg.color = new Color(0.2f, 0.7f, 0.2f);
            walletImg.raycastTarget = false;

            var coinTxt = new GameObject("CoinText", typeof(RectTransform));
            coinTxt.transform.SetParent(walletGo.transform, false);
            Anch(coinTxt, -1.5f, 0f, 0f, 1f);
            var coinT = coinTxt.AddComponent<TextMeshProUGUI>();
            coinT.text = "0";
            coinT.fontSize = PancakeFlipUiTypography.TopBarCoinsLevel;
            coinT.alignment = TextAlignmentOptions.MidlineRight;
            coinT.color = Color.white;
            coinT.fontStyle = FontStyles.Bold;
            coinT.raycastTarget = false;
            if (tmpFont != null) coinT.font = tmpFont;

            var tbvGo = new GameObject("TopBarView", typeof(RectTransform));
            tbvGo.transform.SetParent(uiRoot, false);
            var tbv = tbvGo.AddComponent<TopBarView>();
            SetField(tbv, "coinsText", coinT);
            SetField(tbv, "levelText", lvlT);

            var orderPanel = MkPanel(uiRoot, "OrderPanel", V2(0, 0.42f), V2(0.44f, 1.02f), new Color(0, 0, 0, 0));
            AddResponsive(orderPanel, V2(0f, 0.42f), V2(0.44f, 1.02f), V2(0.01f, 0.10f), V2(0.24f, 0.95f));

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

            var cardPrefab = MkOrderCard(cardsContainer.transform, tmpFont, orderItemSpr, rewardInfoSpr,
                new[] { person1Icon, person2Icon, person3Icon }, walletSpr, xpIconSpr);
            cardPrefab.gameObject.SetActive(false);
            var olv = orderPanel.AddComponent<OrderListView>();
            SetField(olv, "cardPrefab", cardPrefab);
            SetField(olv, "container", cardsContainer.transform);
            SetFloatField(olv, "slotHeight", 0.285f);
            SetFloatField(olv, "gap", 0.004f);

            var backProgressSpr = LoadSprite("BackProgress");
            var centerProgressSpr = LoadSprite("CenterProgress");
            var frontProgressSpr = LoadSprite("FrontProgress");

            var chargeGo = MkPanel(uiRoot, "ChargeIndicator", V2(0.25f, 0.84f), V2(0.75f, 0.9f), new Color(0f, 0f, 0f, 0f));
            AddResponsive(chargeGo, V2(0.25f, 0.84f), V2(0.75f, 0.9f), V2(0.38f, 0.88f), V2(0.62f, 0.94f));
            chargeGo.GetComponent<Image>().raycastTarget = false;

            // Back — пустой трек (нижний слой).
            var backGo = new GameObject("Back", typeof(RectTransform), typeof(Image));
            backGo.transform.SetParent(chargeGo.transform, false); Fill(backGo);
            var backImg = backGo.GetComponent<Image>();
            backImg.sprite = backProgressSpr; backImg.type = Image.Type.Simple; backImg.preserveAspect = false;
            backImg.color = Color.white; backImg.raycastTarget = false; backImg.enabled = backProgressSpr != null;

            // Center — заливка, открывается слева направо (средний слой).
            var fillGo2 = new GameObject("Center", typeof(RectTransform), typeof(Image));
            fillGo2.transform.SetParent(chargeGo.transform, false); Fill(fillGo2);
            var fillI = fillGo2.GetComponent<Image>();
            fillI.sprite = centerProgressSpr; fillI.color = Color.white; fillI.raycastTarget = false;
            fillI.type = Image.Type.Filled; fillI.fillMethod = Image.FillMethod.Horizontal;
            fillI.fillOrigin = (int)Image.OriginHorizontal.Left; fillI.fillAmount = 0f;

            // Front — рамка поверх (верхний слой).
            var frontGo = new GameObject("Front", typeof(RectTransform), typeof(Image));
            frontGo.transform.SetParent(chargeGo.transform, false); Fill(frontGo);
            var frontImg = frontGo.GetComponent<Image>();
            frontImg.sprite = frontProgressSpr; frontImg.type = Image.Type.Simple; frontImg.preserveAspect = false;
            frontImg.color = Color.white; frontImg.raycastTarget = false; frontImg.enabled = frontProgressSpr != null;

            var cv = chargeGo.AddComponent<ChargeIndicatorView>(); SetField(cv, "fillImage", fillI);

            var popup = MkLabel(uiRoot, "RotationsPopup", "", tmpFont, PancakeFlipUiTypography.RotationsPopup, new Color(1, 0.95f, 0.7f), 0);
            var popupR = popup.GetComponent<RectTransform>();
            popupR.anchorMin = V2(0.5f, 0.5f); popupR.anchorMax = V2(0.5f, 0.5f);
            popupR.sizeDelta = new Vector2(400, 100);
            popup.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

            var scoreUI = new GameObject("ScoreUI", typeof(RectTransform));
            scoreUI.transform.SetParent(uiRoot, false);
            var sv = scoreUI.AddComponent<PancakeFlipScoreView>();
            SetField(sv, "pancake", pcBh); SetField(sv, "config", flipConfig);
            SetField(sv, "rotationsPopupText", popup.GetComponent<TextMeshProUGUI>());

            var cookRoot = MkPanel(uiRoot, "CookingIndicators", V2(0.74f, 0.34f), V2(0.97f, 0.81f), new Color(0, 0, 0, 0));
            AddResponsive(cookRoot, V2(0.74f, 0.34f), V2(0.97f, 0.81f), V2(0.70f, 0.52f), V2(0.98f, 0.95f));
            cookRoot.GetComponent<Image>().raycastTarget = false;
            MkCookPancakePreview(cookRoot, "A", V2(0, 0.53f), V2(1, 0.778f), pancakeSideUiSpr, tmpFont, out Image imgA);
            MkCookPancakePreview(cookRoot, "B", V2(0, 0.232f), V2(1, 0.498f), pancakeSideUiSpr, tmpFont, out Image imgB);
            var civ = cookRoot.AddComponent<CookingIndicatorView>();
            SetField(civ, "pancake", pcBh); SetField(civ, "config", flipConfig);
            SetField(civ, "pancakeA", imgA); SetField(civ, "pancakeB", imgB);

            var recipeBookScr = MkPanel(uiRoot, "RecipeBookScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.96f, 0.94f, 0.89f, 0.99f));
            StyleButtonSprite(recipeBookScr, recipesHudSpr);
            AddResponsive(recipeBookScr, V2(0.055f, 0.1f), V2(0.945f, 0.92f), V2(0.34f, 0.06f), V2(0.66f, 0.96f));
            recipeBookScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(recipeBookScr);
            var rbSh = recipeBookScr.AddComponent<Shadow>(); rbSh.effectDistance = new Vector2(3, -4); rbSh.effectColor = new Color(0, 0, 0, 0.28f);
            var rbTitle = MkLabel(recipeBookScr.transform, "Title", "Рецепты", tmpFont, PancakeFlipUiTypography.ModalHeaderTitle, Color.white, 0);
            var rbTitleRt = rbTitle.GetComponent<RectTransform>();
            rbTitleRt.anchorMin = V2(0.10f, 0.86f); rbTitleRt.anchorMax = V2(0.90f, 0.94f);
            rbTitleRt.offsetMin = rbTitleRt.offsetMax = Vector2.zero;
            rbTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            MkVerticalScrollArea(recipeBookScr.transform, "Scroll", V2(0.12f, 0.04f), V2(0.88f, 0.84f), out RectTransform rbListContent);
            var rbCloseIcon = MkCloseIcon(recipeBookScr.transform, closeIconSpr);
            var rbsv = recipeBookScr.AddComponent<RecipeBookScreenView>();
            SetField(rbsv, "recipeListContainer", rbListContent);
            SetField(rbsv, "closeButton", rbCloseIcon);

            var ingScr = MkPanel(uiRoot, "IngredientsScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.96f, 0.94f, 0.89f, 0.99f));
            StyleButtonSprite(ingScr, recipesHudSpr);
            AddResponsive(ingScr, V2(0.055f, 0.1f), V2(0.945f, 0.92f), V2(0.34f, 0.06f), V2(0.66f, 0.96f));
            ingScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(ingScr);
            var ingSh = ingScr.AddComponent<Shadow>(); ingSh.effectDistance = new Vector2(3, -4); ingSh.effectColor = new Color(0, 0, 0, 0.28f);
            var ingTitle = MkLabel(ingScr.transform, "Title", "Ингредиенты", tmpFont, PancakeFlipUiTypography.ModalHeaderTitle, Color.white, 0);
            var ingTitleRt = ingTitle.GetComponent<RectTransform>();
            ingTitleRt.anchorMin = V2(0.10f, 0.86f); ingTitleRt.anchorMax = V2(0.90f, 0.94f);
            ingTitleRt.offsetMin = ingTitleRt.offsetMax = Vector2.zero;
            ingTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var iBuilderRow = new GameObject("BuilderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            iBuilderRow.transform.SetParent(ingScr.transform, false);
            var iBuilderRt = iBuilderRow.GetComponent<RectTransform>();
            iBuilderRt.anchorMin = V2(0.03f, 0.75f); iBuilderRt.anchorMax = V2(0.97f, 0.84f);
            iBuilderRt.offsetMin = iBuilderRt.offsetMax = Vector2.zero;
            var iBuilderH = iBuilderRow.GetComponent<HorizontalLayoutGroup>();
            iBuilderH.spacing = 14f;
            iBuilderH.childAlignment = TextAnchor.MiddleCenter;
            iBuilderH.childControlWidth = false;
            iBuilderH.childControlHeight = false;
            iBuilderH.childForceExpandWidth = false;
            iBuilderH.childForceExpandHeight = false;

            MkVerticalScrollArea(ingScr.transform, "Scroll", V2(0.12f, 0.22f), V2(0.88f, 0.74f), out RectTransform iListContent);

            var iCookBtn = MkButton(ingScr.transform, "CookBtn", "Готовить", tmpFont, new Color(0.86f, 0.55f, 0.18f, 1f));
            StyleButtonSprite(iCookBtn, successBtnSpr);
            var iCookRt = iCookBtn.GetComponent<RectTransform>();
            iCookRt.anchorMin = V2(0.18f, 0.06f); iCookRt.anchorMax = V2(0.82f, 0.16f);
            iCookBtn.transform.SetAsLastSibling();
            iCookRt.offsetMin = iCookRt.offsetMax = Vector2.zero;
            var iCookLabel = iCookBtn.GetComponentInChildren<TextMeshProUGUI>();

            var iCloseIcon = MkCloseIcon(ingScr.transform, closeIconSpr);
            var isv = ingScr.AddComponent<IngredientsScreenView>();
            SetField(isv, "ingredientListContainer", iListContent);
            SetField(isv, "builderContainer", iBuilderRow.transform);
            SetField(isv, "cookButton", iCookBtn.GetComponent<Button>());
            SetField(isv, "cookButtonLabel", iCookLabel);
            SetField(isv, "closeIconButton", iCloseIcon);

            var stoveV = Object.FindObjectOfType<StoveView>();
            if (stoveV != null) { SetField(stoveV, "ingredientsScreen", isv); SetField(isv, "stove", stoveV); }

            var upgScr = MkPanel(uiRoot, "UpgradeScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.98f, 0.95f, 0.9f, 0.99f));
            StyleButtonSprite(upgScr, panHudSpr);
            AddResponsive(upgScr, V2(0.055f, 0.1f), V2(0.945f, 0.92f), V2(0.34f, 0.06f), V2(0.66f, 0.96f));
            upgScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(upgScr);
            var upgSh = upgScr.AddComponent<Shadow>(); upgSh.effectDistance = new Vector2(3, -4); upgSh.effectColor = new Color(0, 0, 0, 0.28f);
            var upgTitle = MkLabel(upgScr.transform, "Title", "Сковородки", tmpFont, PancakeFlipUiTypography.ModalHeaderTitle, Color.white, 0);
            var upgTitleRt = upgTitle.GetComponent<RectTransform>();
            upgTitleRt.anchorMin = V2(0.10f, 0.86f); upgTitleRt.anchorMax = V2(0.90f, 0.94f);
            upgTitleRt.offsetMin = upgTitleRt.offsetMax = Vector2.zero;
            upgTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            MkVerticalScrollArea(upgScr.transform, "Scroll", V2(0.12f, 0.04f), V2(0.94f, 0.84f), out RectTransform uListContent);
            var uCloseIcon = MkCloseIcon(upgScr.transform, closeIconSpr);
            var usv = upgScr.AddComponent<PanUpgradeScreenView>();
            SetField(usv, "upgradeListContainer", uListContent);
            SetField(usv, "closeButton", uCloseIcon);
            SetField(usv, "defaultPanIcon", panSpr);

            var mapScr = MkPanel(uiRoot, "MapScreen", V2(0.055f, 0.1f), V2(0.945f, 0.92f), new Color(0.90f, 0.93f, 0.97f, 1f));
            AddResponsive(mapScr, V2(0.055f, 0.1f), V2(0.945f, 0.92f), V2(0.20f, 0.04f), V2(0.80f, 0.97f));
            mapScr.GetComponent<Image>().raycastTarget = true;
            AddModalCanvasLayer(mapScr);

            var mapBg = new GameObject("MapBg", typeof(RectTransform), typeof(Image));
            mapBg.transform.SetParent(mapScr.transform, false);
            var mapBgRt = mapBg.GetComponent<RectTransform>();
            mapBgRt.anchorMin = Vector2.zero; mapBgRt.anchorMax = Vector2.one;
            mapBgRt.offsetMin = mapBgRt.offsetMax = Vector2.zero;
            var mapBgImg = mapBg.GetComponent<Image>();
            if (globalMapSpr != null) { mapBgImg.sprite = globalMapSpr; mapBgImg.preserveAspect = false; }
            else mapBgImg.color = new Color(0.5f, 0.6f, 0.7f, 1f);
            mapBgImg.raycastTarget = true;

            var markersGo = new GameObject("Markers", typeof(RectTransform));
            markersGo.transform.SetParent(mapScr.transform, false);
            var markersRt = markersGo.GetComponent<RectTransform>();
            markersRt.anchorMin = Vector2.zero; markersRt.anchorMax = Vector2.one;
            markersRt.offsetMin = markersRt.offsetMax = Vector2.zero;

            var mapStatus = MkLabel(mapScr.transform, "Status", "", tmpFont, PancakeFlipUiTypography.ModalBody, new Color(0.95f, 0.4f, 0.3f), 0);
            var mapStatusRt = mapStatus.GetComponent<RectTransform>();
            mapStatusRt.anchorMin = V2(0.05f, 0.02f); mapStatusRt.anchorMax = V2(0.95f, 0.09f);
            mapStatusRt.offsetMin = mapStatusRt.offsetMax = Vector2.zero;
            mapStatus.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var mapCloseIcon = MkCloseIcon(mapScr.transform, closeIconSpr);

            var buyModal = MkPanel(mapScr.transform, "BuyModal", V2(0.18f, 0.30f), V2(0.82f, 0.72f), new Color(1f, 1f, 1f, 0f));
            var buyModalImg = buyModal.GetComponent<Image>();
            buyModalImg.raycastTarget = true;
            if (globalMapPlateSpr != null) { buyModalImg.sprite = globalMapPlateSpr; buyModalImg.type = Image.Type.Simple; buyModalImg.preserveAspect = false; buyModalImg.color = Color.white; }
            AddModalCanvasLayer(buyModal);

            var buyTitle = MkLabel(buyModal.transform, "Title", "Город", tmpFont, PancakeFlipUiTypography.ModalHeaderTitle, new Color(0.2f, 0.16f, 0.1f), 0);
            var buyTitleRt = buyTitle.GetComponent<RectTransform>();
            buyTitleRt.anchorMin = V2(0.16f, 0.64f); buyTitleRt.anchorMax = V2(0.84f, 0.80f);
            buyTitleRt.offsetMin = buyTitleRt.offsetMax = Vector2.zero;
            buyTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            // Тело: «Купить N» (+иконка денег) или «Получите N уровень».
            var buyCost = MkLabel(buyModal.transform, "Body", "0", tmpFont, PancakeFlipUiTypography.ModalBody, new Color(0.2f, 0.16f, 0.1f), 0);
            var buyCostRt = buyCost.GetComponent<RectTransform>();
            buyCostRt.anchorMin = V2(0.10f, 0.38f); buyCostRt.anchorMax = V2(0.90f, 0.56f);
            buyCostRt.offsetMin = buyCostRt.offsetMax = Vector2.zero;
            buyCost.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var buyCoin = new GameObject("CostCoin", typeof(RectTransform), typeof(Image));
            buyCoin.transform.SetParent(buyModal.transform, false);
            var buyCoinRt = buyCoin.GetComponent<RectTransform>();
            buyCoinRt.anchorMin = V2(0.66f, 0.40f); buyCoinRt.anchorMax = V2(0.80f, 0.55f);
            buyCoinRt.offsetMin = buyCoinRt.offsetMax = Vector2.zero;
            var buyCoinImg = buyCoin.GetComponent<Image>();
            buyCoinImg.sprite = walletSpr; buyCoinImg.preserveAspect = true; buyCoinImg.raycastTarget = false;
            buyCoinImg.enabled = walletSpr != null;

            var buyConfirm = MkButton(buyModal.transform, "BuyConfirm", "Купить", tmpFont, new Color(0.30f, 0.62f, 0.30f, 1f));
            StyleButtonSprite(buyConfirm, successBtnSpr);
            var buyConfirmRt = buyConfirm.GetComponent<RectTransform>();
            buyConfirmRt.anchorMin = V2(0.34f, 0.20f); buyConfirmRt.anchorMax = V2(0.66f, 0.36f);
            buyConfirmRt.offsetMin = buyConfirmRt.offsetMax = Vector2.zero;
            var buyCloseIcon = MkCloseIcon(buyModal.transform, closeIconSpr);
            buyModal.SetActive(false);

            var mapView = mapScr.AddComponent<MapScreenView>();
            SetField(mapView, "markersContainer", markersRt);
            SetField(mapView, "blockedSprite", blockedSpr);
            SetField(mapView, "carSprite", carSpr);
            SetField(mapView, "statusText", mapStatus.GetComponent<TextMeshProUGUI>());
            SetField(mapView, "closeButton", mapCloseIcon);
            SetField(mapView, "buyModal", buyModal);
            SetField(mapView, "buyTitleText", buyTitle.GetComponent<TextMeshProUGUI>());
            SetField(mapView, "buyCostText", buyCost.GetComponent<TextMeshProUGUI>());
            SetField(mapView, "buyConfirmButton", buyConfirm.GetComponent<Button>());
            SetField(mapView, "buyCloseButton", buyCloseIcon);
            SetField(mapView, "buyCoinIcon", buyCoinImg);

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
            SetFieldArr(sess, "statTracks", new Object[] { statWide, statOver, statSpin, statFlip });
            SetFieldArr(sess, "panTiers", new Object[] { panStarter, panIron, panPro, panElite });
            SetField(sess, "defaultPanTier", panStarter);
            SetField(sess, "uiFont", uiFont);
            SetField(sess, "coinIcon", walletSpr);
            SetField(sess, "closeIcon", closeIconSpr);
            SetField(sess, "actionButtonSprite", actionBtnSpr);
            SetField(sess, "successButtonSprite", successBtnSpr);
            SetField(sess, "cancelButtonSprite", cancelBtnSpr);
            SetField(sess, "ingredientSpotSprite", ingredientSpotSpr);
            SetField(sess, "recipeHudSpotSprite", recipeHudSpotSpr);
            SetField(sess, "worldMap", worldMap);
            SetField(sess, "customerAnimator", custAnim);
            var bgGoRef = GameObject.Find("Background");
            SetField(sess, "sceneBackground", bgGoRef != null ? bgGoRef.GetComponent<SpriteRenderer>() : null);
            var bottomGoRef = GameObject.Find("BottomPanel");
            SetField(sess, "sceneBottomPanel", bottomGoRef != null ? bottomGoRef.GetComponent<SpriteRenderer>() : null);
            SetField(sess, "stove", Object.FindObjectOfType<StoveView>());

            var mscGo = new GameObject("MainScreenController");
            var msc = mscGo.AddComponent<MainScreenController>();
            SetField(msc, "recipeBookScreen", rbsv);
            SetField(msc, "upgradeScreen", usv);
            SetField(msc, "customerAnimator", custAnim);
            SetField(msc, "mapScreen", mapView);

            var kitchenBottom = new GameObject("KitchenBottom", typeof(RectTransform), typeof(KitchenUiFrontLayer));
            kitchenBottom.transform.SetParent(uiRoot, false);
            var kitchenBottomRt = kitchenBottom.GetComponent<RectTransform>();
            kitchenBottomRt.anchorMin = Vector2.zero;
            kitchenBottomRt.anchorMax = Vector2.one;
            kitchenBottomRt.offsetMin = kitchenBottomRt.offsetMax = Vector2.zero;

            var recipesBtnGo = MkKitchenSpriteButton(kitchenBottom.transform, "RecipesBtn", V2(0.05f, 0.04f), V2(0.17f, 0.18f), receiptBtnSpr);
            var upgradesBtnGo = MkKitchenSpriteButton(kitchenBottom.transform, "UpgradesBtn", V2(0.83f, 0.04f), V2(0.95f, 0.18f), panUpgradeBtnSpr);
            AddResponsive(recipesBtnGo, V2(0.05f, 0.04f), V2(0.17f, 0.18f), V2(0.70f, 0.06f), V2(0.82f, 0.20f));
            AddResponsive(upgradesBtnGo, V2(0.83f, 0.04f), V2(0.95f, 0.18f), V2(0.84f, 0.06f), V2(0.96f, 0.20f));

            var kHud = new GameObject("KitchenHUD", typeof(RectTransform));
            kHud.transform.SetParent(uiRoot, false);
            var kHudRt = kHud.GetComponent<RectTransform>();
            kHudRt.anchorMin = kHudRt.anchorMax = Vector2.zero;
            kHudRt.sizeDelta = Vector2.zero;
            var kBar = kHud.AddComponent<KitchenBarController>();
            SetField(kBar, "mainScreen", msc);
            SetField(kBar, "recipesButton", recipesBtnGo.GetComponent<Button>());
            SetField(kBar, "upgradesButton", upgradesBtnGo.GetComponent<Button>());
            SetField(kBar, "mapButton", profileBtn);
            kitchenBottom.transform.SetAsLastSibling();

            // --- Juice setup ---
            var fxCenter = new GameObject("FxCenter", typeof(RectTransform));
            fxCenter.transform.SetParent(canvas.transform, false);
            var fxCenterRt = fxCenter.GetComponent<RectTransform>();
            fxCenterRt.anchorMin = fxCenterRt.anchorMax = new Vector2(0.5f, 0.5f);
            fxCenterRt.pivot = new Vector2(0.5f, 0.5f);
            fxCenterRt.anchoredPosition = new Vector2(0f, 200f);
            fxCenterRt.sizeDelta = Vector2.zero;

            var juiceGo = new GameObject("Juice", typeof(AudioSource));
            juiceGo.transform.SetParent(uiRoot, false);
            var sfx = juiceGo.AddComponent<Sfx>();
            SetField(sfx, "source", juiceGo.GetComponent<AudioSource>());
            var musicSource = juiceGo.AddComponent<AudioSource>();
            musicSource.playOnAwake = false; musicSource.loop = true; musicSource.volume = 0.2f;
            SetField(sfx, "musicSource", musicSource);
            // Звуки из папки Sounds (имена файлов = ключи).
            SetField(sfx, "flip", LoadAudio("Throw"));
            SetField(sfx, "serve", LoadAudio("OrderDone"));
            SetField(sfx, "cook", LoadAudio("Frying"));
            SetField(sfx, "levelUp", LoadAudio("LevelUp"));
            SetField(sfx, "click", LoadAudio("ButtonClick"));
            SetField(sfx, "denied", LoadAudio("Denied"));
            SetField(sfx, "car", LoadAudio("Car"));
            SetField(sfx, "unlock", LoadAudio("Unlock"));
            SetField(sfx, "buy", LoadAudio("Buy"));
            SetField(sfx, "coin", LoadAudio("Coin"));
            // Порядок = порядок городов в worldMap: Заправка(0), Бостон/ресторан(1), Средний/кофейня(2).
            SetFieldArr(sfx, "cityMusic", new Object[] { LoadAudio("MusicCity01"), LoadAudio("MusicCity03"), LoadAudio("MusicCity02") });

            var floatSpawner = juiceGo.AddComponent<FloatingTextSpawner>();

            var coinSpawner = juiceGo.AddComponent<CoinFlySpawner>();
            SetField(coinSpawner, "parentCanvas", (RectTransform)canvas.transform);
            SetField(coinSpawner, "coinSprite", walletSpr);

            var juiceCtrl = juiceGo.AddComponent<JuiceController>();
            SetField(juiceCtrl, "pan", panGo.transform);
            SetField(juiceCtrl, "floatText", floatSpawner);
            SetField(juiceCtrl, "coinFly", coinSpawner);
            SetField(juiceCtrl, "walletAnchor", walletGo.GetComponent<RectTransform>());
            SetField(juiceCtrl, "profileAnchor", profileGo.GetComponent<RectTransform>());
            SetField(juiceCtrl, "centerAnchor", fxCenterRt);
            SetField(juiceCtrl, "pancake", pcBh);
            SetField(juiceCtrl, "profileGraphic", profileGo.GetComponent<Image>());

            // Pop every static button built so far.
            foreach (var b in canvas.GetComponentsInChildren<Button>(true))
                if (b.GetComponent<ButtonJuice>() == null)
                    b.gameObject.AddComponent<ButtonJuice>();
            // --- end Juice setup ---

            // --- Tutorial setup ---
            var tutCanvasGo = new GameObject("TutorialCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var tutCanvas = tutCanvasGo.GetComponent<Canvas>();
            tutCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            tutCanvas.sortingOrder = 200;
            var tutScaler = tutCanvasGo.GetComponent<CanvasScaler>();
            tutScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            tutScaler.referenceResolution = new Vector2(1080, 1920);
            tutScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            tutScaler.matchWidthOrHeight = 1f;

            var tut = tutCanvasGo.AddComponent<TutorialController>();
            SetField(tut, "orderPanelTarget", orderPanel.GetComponent<RectTransform>());
            SetField(tut, "ingredientListTarget", iListContent);
            SetField(tut, "cookButtonTarget", iCookBtn.GetComponent<RectTransform>());
            SetField(tut, "ingredientsScreen", isv);
            // --- end Tutorial setup ---

            var framer = bootGo.AddComponent<WorldSceneFramer>();
            {
                var so = new SerializedObject(framer);
                so.FindProperty("cam").objectReferenceValue = Camera.main;
                var bgGo = GameObject.Find("Background");
                so.FindProperty("background").objectReferenceValue =
                    bgGo != null ? bgGo.GetComponent<SpriteRenderer>() : null;
                var bottomGo = GameObject.Find("BottomPanel");
                so.FindProperty("bottomPanel").objectReferenceValue =
                    bottomGo != null ? bottomGo.GetComponent<SpriteRenderer>() : null;
                var stoveGoRef = GameObject.Find("Stove");
                so.FindProperty("stove").objectReferenceValue =
                    stoveGoRef != null ? stoveGoRef.transform : null;
                so.FindProperty("stoveSr").objectReferenceValue =
                    stoveGoRef != null ? stoveGoRef.GetComponent<SpriteRenderer>() : null;
                var panRef = Object.FindObjectOfType<PanBehaviour>();
                so.FindProperty("pan").objectReferenceValue =
                    panRef != null ? panRef.transform : null;
                var pancakeRef = Object.FindObjectOfType<PancakeBehaviour>();
                so.FindProperty("pancake").objectReferenceValue =
                    pancakeRef != null ? pancakeRef.transform : null;
                so.FindProperty("customer").objectReferenceValue =
                    Object.FindObjectOfType<CustomerAnimator>();
                so.ApplyModifiedPropertiesWithoutUndo();
            }

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

        [MenuItem("PancakeFlip/Сбросить тутор")]
        public static void ResetTutorial()
        {
            UnityEngine.PlayerPrefs.DeleteKey(TutorialController.DoneKey);
            UnityEngine.PlayerPrefs.Save();
            Debug.Log("PancakeFlip: тутор сброшен — покажется при следующем запуске.");
        }

        const string IngredientsFolder = DataDir + "/Ingredients";
        const string RecipesFolder = DataDir + "/Recipes";
        const string StatsFolder = DataDir + "/Stats";
        const string PansFolder = DataDir + "/Pans";
        const string ConfigsFolder = DataDir + "/Configs";

        static IngredientConfig CreateIngredient(string n, int cost, int lvl, bool inf, int maxStock = 0, bool isDough = false)
        {
            var o = GetOrCreate<IngredientConfig>(IngredientsFolder, n);
            o.displayName = n; o.coinCost = cost; o.unlockLevel = lvl; o.infinite = inf;
            SetIntField(o, "maxStock", maxStock);
            SetBoolField(o, "isDough", isDough);
            EditorUtility.SetDirty(o); return o;
        }
        static RecipeConfig CreateRecipe(string n, int lvl, int coins, int xp, RecipeConfig.IngredientSlot[] ing, Sprite icon = null)
        {
            var o = GetOrCreate<RecipeConfig>(RecipesFolder, n);
            o.displayName = n; o.unlockLevel = lvl; o.rewardCoins = coins; o.rewardXp = xp; o.ingredients = ing;
            o.icon = icon;
            EditorUtility.SetDirty(o); return o;
        }
        static LocationConfig CreateLocation(string assetName, string title, int ordersToUnlock,
            RecipeConfig[] demand, RecipeConfig[] unlockRecipes, IngredientConfig[] unlockIngredients, Sprite[] customers)
        {
            var o = GetOrCreate<LocationConfig>(DataDir + "/Locations", assetName);
            o.displayName = title;
            o.ordersToUnlockNext = ordersToUnlock;
            o.demandRecipes = demand;
            o.unlockRecipes = unlockRecipes;
            o.unlockIngredients = unlockIngredients;
            o.customerSprites = customers;
            EditorUtility.SetDirty(o);
            return o;
        }
        static PanStatTrackConfig CreateStatTrack(string assetName, string title, PanUpgradeConfig.EffectType effectType, int unlockLvl, int costBase, string description, Sprite icon)
        {
            var o = GetOrCreate<PanStatTrackConfig>(StatsFolder, assetName);
            o.displayName = title;
            o.effectType = effectType;
            o.unlockLevel = unlockLvl;
            o.coinCostBase = costBase;
            o.description = description;
            if (icon != null) o.icon = icon;
            EditorUtility.SetDirty(o);
            return o;
        }

        static PanTierConfig CreatePanTier(string assetName, string title, bool starter, int cost, int unlockLvl, Sprite icon, string description,
            float wide, float slowCook, float spin, float flip)
        {
            var o = GetOrCreate<PanTierConfig>(PansFolder, assetName);
            o.displayName = title;
            o.isStarter = starter;
            o.coinCost = cost;
            o.unlockLevel = unlockLvl;
            o.description = description;
            if (icon != null) o.icon = icon;
            o.widerPerfectZone = wide;
            o.slowerOvercook = slowCook;
            o.stablerSpin = spin;
            o.easierFlip = flip;
            EditorUtility.SetDirty(o);
            return o;
        }
        static PanUpgradeConfig CreateUpgrade(string n, int cost, int lvl, PanUpgradeConfig.EffectType t, float v, string description, Sprite icon)
        {
            var o = GetOrCreate<PanUpgradeConfig>(StatsFolder, n);
            o.displayName = n; o.coinCost = cost; o.unlockLevel = lvl; o.effectType = t; o.effectValue = v;
            o.description = description;
            if (icon != null) o.icon = icon;
            EditorUtility.SetDirty(o); return o;
        }
        static T GetOrCreate<T>(string folder, string name) where T : ScriptableObject
        {
            string direct = $"{folder}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(direct);
            if (existing != null) return existing;

            var guids = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}", new[] { DataDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) != name) continue;
                var found = AssetDatabase.LoadAssetAtPath<T>(p);
                if (found != null) return found;
            }

            EnsureFolder(folder);
            var o = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(o, direct);
            return o;
        }

        static void ForceAllSprites()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (!p.EndsWith(".png")) continue;
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                if (imp == null) continue;
                bool c = false;
                if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; c = true; }
                if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; c = true; }
                if (c) imp.SaveAndReimport();
            }
            AssetDatabase.Refresh();
        }
        static void SetSpriteBorder(string spriteName, float border)
        {
            var guids = AssetDatabase.FindAssets($"{spriteName} t:Texture2D", new[] { ArtDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) != spriteName) continue;
                var imp = AssetImporter.GetAtPath(p) as TextureImporter;
                if (imp == null) continue;
                var settings = new TextureImporterSettings();
                imp.ReadTextureSettings(settings);
                var b = new Vector4(border, border, border, border);
                if (settings.spriteBorder != b)
                {
                    settings.spriteBorder = b;
                    imp.SetTextureSettings(settings);
                    imp.SaveAndReimport();
                }
                return;
            }
        }
        static AudioClip LoadAudio(string n)
        {
            var guids = AssetDatabase.FindAssets($"{n} t:AudioClip", new[] { ArtDir });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != n) continue;
                var c = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (c != null) return c;
            }
            return null;
        }

        static Sprite LoadSprite(string n)
        {
            var guids = AssetDatabase.FindAssets($"{n} t:Sprite", new[] { ArtDir });
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != n) continue;
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (s != null) return s;
            }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != n) continue;
                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path)) if (o is Sprite sp) return sp;
            }
            return null;
        }

        static string FindAssetFolder<T>(string name, string fallbackFolder) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}", new[] { DataDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) == name)
                    return System.IO.Path.GetDirectoryName(p).Replace('\\', '/');
            }
            return fallbackFolder;
        }

        static T FindAssetByName<T>(string name) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}", new[] { DataDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) != name) continue;
                var a = AssetDatabase.LoadAssetAtPath<T>(p);
                if (a != null) return a;
            }
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

        static void AddResponsive(GameObject go, Vector2 pMin, Vector2 pMax, Vector2 lMin, Vector2 lMax)
        {
            var rr = go.GetComponent<ResponsiveRect>();
            if (rr == null) rr = go.AddComponent<ResponsiveRect>();
            rr.Configure(pMin, pMax, lMin, lMax);
        }

        static void AddModalCanvasLayer(GameObject go)
        {
            var c = go.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 100;
            go.AddComponent<GraphicRaycaster>();
        }
        static TMP_FontAsset _editorTmpFontAsset;

        static TMP_FontAsset GetOrCreateEditorTmpFont(Font source)
        {
            if (_editorTmpFontAsset != null) return _editorTmpFontAsset;
            if (source == null) return null;
            _editorTmpFontAsset = TMP_FontAsset.CreateFontAsset(source, 72, 8, GlyphRenderMode.SDFAA, 4096, 4096);
            if (_editorTmpFontAsset != null)
                _editorTmpFontAsset.name = "PancakeFlip_Editor_Mouse_SDF";
            return _editorTmpFontAsset;
        }

        static GameObject MkLabel(Transform p, string n, string txt, TMP_FontAsset font, int sz, Color c, float w)
        {
            var g = new GameObject(n, typeof(RectTransform));
            g.transform.SetParent(p, false);
            var t = g.AddComponent<TextMeshProUGUI>();
            t.text = txt;
            t.fontSize = sz;
            t.color = c;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = false;
            t.raycastTarget = false;
            if (font != null) t.font = font;
            if (w > 0) { var le = g.AddComponent<LayoutElement>(); le.preferredWidth = w; }
            return g;
        }
        static GameObject MkButton(Transform p, string n, string lbl, TMP_FontAsset font, Color c)
        {
            var g = new GameObject(n, typeof(RectTransform), typeof(Image), typeof(Button));
            g.transform.SetParent(p, false);
            g.GetComponent<Image>().color = c; g.GetComponent<Button>().targetGraphic = g.GetComponent<Image>();
            var t = new GameObject("Text", typeof(RectTransform)); t.transform.SetParent(g.transform, false); Fill(t);
            var tx = t.AddComponent<TextMeshProUGUI>();
            tx.text = lbl;
            tx.fontSize = PancakeFlipUiTypography.PrimaryButtonLabel;
            tx.alignment = TextAlignmentOptions.Center;
            tx.color = Color.white;
            tx.raycastTarget = false;
            if (font != null) tx.font = font;
            return g;
        }
        static void StyleButtonSprite(GameObject btnGo, Sprite sprite)
        {
            if (btnGo == null || sprite == null) return;
            var img = btnGo.GetComponent<Image>();
            if (img == null) return;
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.preserveAspect = false;
        }
        static void Fill(GameObject g)
        {
            var r = g.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }
        static Button MkCloseIcon(Transform p, Sprite spr)
        {
            const float CloseIconSize = 96f;
            var g = new GameObject("CloseIcon", typeof(RectTransform), typeof(Image), typeof(Button));
            g.transform.SetParent(p, false);
            var rt = g.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-52f, -52f); // чуть левее и ниже от угла
            rt.sizeDelta = new Vector2(CloseIconSize, CloseIconSize);
            var img = g.GetComponent<Image>();
            img.raycastTarget = true;
            img.preserveAspect = true;
            var btn = g.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;

            if (spr != null)
            {
                img.sprite = spr;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(1f, 1f, 1f, 0f);
                var xGo = new GameObject("X", typeof(RectTransform));
                xGo.transform.SetParent(g.transform, false);
                var xRt = xGo.GetComponent<RectTransform>();
                xRt.anchorMin = Vector2.zero;
                xRt.anchorMax = Vector2.one;
                xRt.offsetMin = xRt.offsetMax = Vector2.zero;
                var xTmp = xGo.AddComponent<TextMeshProUGUI>();
                var f = PancakeFlipUiFonts.UiTmpFont;
                if (f != null) xTmp.font = f;
                xTmp.text = "×";
                xTmp.color = new Color(0.78f, 0.18f, 0.18f, 1f);
                xTmp.alignment = TextAlignmentOptions.Center;
                xTmp.fontSize = 56f;
                xTmp.fontStyle = FontStyles.Bold;
                xTmp.raycastTarget = false;
            }

            return btn;
        }

        static void MkVerticalScrollArea(Transform parent, string name, Vector2 amin, Vector2 amax, out RectTransform contentOut)
        {
            var scrollGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(parent, false);
            var sRt = scrollGo.GetComponent<RectTransform>();
            sRt.anchorMin = amin; sRt.anchorMax = amax; sRt.offsetMin = sRt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // прозрачно — фон списка не нужен

            var vp = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            vp.transform.SetParent(scrollGo.transform, false);
            var vpRt = vp.GetComponent<RectTransform>();
            vpRt.anchorMin = Vector2.zero; vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = new Vector2(8, 6); vpRt.offsetMax = new Vector2(-8, -6);

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

        static OrderCardView MkOrderCard(Transform p, TMP_FontAsset f, Sprite itemSpr, Sprite rewardSpr,
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
            var coinT = coinTxtGo.AddComponent<TextMeshProUGUI>();
            coinT.fontSize = PancakeFlipUiTypography.OrderCardReward;
            coinT.color = new Color(0.2f, 0.15f, 0.1f);
            coinT.alignment = TextAlignmentOptions.MidlineRight;
            coinT.fontStyle = FontStyles.Bold;
            coinT.raycastTarget = false;
            if (f != null) coinT.font = f;

            var coinIco = new GameObject("CoinIcon", typeof(RectTransform), typeof(Image));
            coinIco.transform.SetParent(rewardBg.transform, false); Anch(coinIco, 0.52f, 0.63f, 0.66f, 0.77f);
            var coinIcoImg = coinIco.GetComponent<Image>(); coinIcoImg.preserveAspect = true;
            if (coinSpr != null) { coinIcoImg.sprite = coinSpr; coinIcoImg.color = Color.white; }

            var xpTxtGo = new GameObject("XpTxt", typeof(RectTransform));
            xpTxtGo.transform.SetParent(rewardBg.transform, false); Anch(xpTxtGo, 0.06f, 0.44f, 0.52f, 0.60f);
            var xpT = xpTxtGo.AddComponent<TextMeshProUGUI>();
            xpT.fontSize = PancakeFlipUiTypography.OrderCardReward;
            xpT.color = new Color(0.2f, 0.15f, 0.1f);
            xpT.alignment = TextAlignmentOptions.MidlineRight;
            xpT.fontStyle = FontStyles.Bold;
            xpT.raycastTarget = false;
            if (f != null) xpT.font = f;

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

        static void MkCookPancakePreview(GameObject root, string side, Vector2 amin, Vector2 amax, Sprite pancakeSideSpr, TMP_FontAsset f, out Image pancakeImg)
        {
            var row = new GameObject($"CookRow{side}", typeof(RectTransform));
            row.transform.SetParent(root.transform, false);
            Anch(row, amin.x, amin.y, amax.x, amax.y);

            var lblGo = MkLabel(row.transform, $"Lbl{side}", side, f, PancakeFlipUiTypography.CookingPreviewSideLabel, new Color(0.95f, 0.92f, 0.85f), 0);
            var lblRt = lblGo.GetComponent<RectTransform>();
            lblRt.anchorMin = new Vector2(0f, 0.1f);
            lblRt.anchorMax = new Vector2(0.12f, 0.9f);
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            lblGo.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.MidlineLeft;

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

        static Font LoadPancakeFlipUiFont()
        {
            var guids = AssetDatabase.FindAssets("MouseMemoirs t:Font", new[] { ArtDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var f = AssetDatabase.LoadAssetAtPath<Font>(p);
                if (f != null) return f;
            }
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        static void SetIntField(Object obj, string prop, int val)
        {
            var so = new SerializedObject(obj);
            var p = so.FindProperty(prop);
            if (p != null && p.propertyType == SerializedPropertyType.Integer) { p.intValue = val; so.ApplyModifiedPropertiesWithoutUndo(); }
        }
        static void SetBoolField(Object obj, string prop, bool val)
        {
            var so = new SerializedObject(obj);
            var p = so.FindProperty(prop);
            if (p != null && p.propertyType == SerializedPropertyType.Boolean) { p.boolValue = val; so.ApplyModifiedPropertiesWithoutUndo(); }
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
