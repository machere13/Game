using System.Collections.Generic;
using UnityEngine;
using IdlePancake.PancakeFlip.MapCore;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IdlePancake.Prototypes.PancakeFlip
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameSession : MonoBehaviour
    {
        public const string PanDataDir = "Assets/Data/PancakeFlip";

        const float DefaultPerfectMin = 0.4f;
        const float DefaultPerfectMax = 0.7f;
        const float DefaultOvercookThreshold = 0.85f;
        const int DefaultXpPerRotation = 10;
        const float DefaultOvercookPenalty = 0.5f;
        const float DefaultMismatchPenalty = 0.5f;
        const int DefaultMaxVisibleOrders = 3;
        const int DefaultPersonCount = 3;
        [Header("Config")]
        [SerializeField] PancakeFlipConfig flipConfig;
        [SerializeField] LevelTableConfig levelTable;
        [SerializeField] RecipeConfig[] startingRecipes;
        [SerializeField] WorldMapConfig worldMap;
        [SerializeField] RecipeConfig baseRecipe;
        [SerializeField] IngredientConfig[] allIngredients;
        [SerializeField] IngredientConfig doughIngredient;
        [SerializeField] PanStatTrackConfig[] statTracks;
        [SerializeField] PanTierConfig[] panTiers;
        [SerializeField] PanTierConfig defaultPanTier;
        [Header("UI")]
        [SerializeField] Font uiFont;
        [SerializeField] Sprite coinIcon;
        [SerializeField] Sprite closeIcon;
        [SerializeField] Sprite actionButtonSprite;
        [SerializeField] Sprite successButtonSprite;
        [SerializeField] Sprite cancelButtonSprite;
        [SerializeField] Sprite ingredientSpotSprite;
        [SerializeField] Sprite recipeHudSpotSprite;

        [Header("Scene refs")]
        [SerializeField] PancakeBehaviour pancake;
        [SerializeField] CustomerAnimator customerAnimator;
        [SerializeField] SpriteRenderer sceneBackground;
        [SerializeField] SpriteRenderer sceneBottomPanel;
        [SerializeField] StoveView stove;
        public PancakeBehaviour Pancake => pancake;

        public Wallet Wallet { get; private set; }
        public Inventory Inventory { get; private set; }
        public OrderQueue Orders { get; private set; }
        public PanUpgradeState Upgrades { get; private set; }
        public PancakeBuild Build { get; private set; }
        public PancakeFlipConfig FlipConfig => flipConfig;
        public RecipeConfig BaseRecipe => baseRecipe;
        public IngredientConfig[] AllIngredients =>
            MapActive ? _availableIngredients.ToArray() : allIngredients;
        public IngredientConfig DoughIngredient => doughIngredient;
        public PanStatTrackConfig[] StatTracks => statTracks;
        public PanTierConfig[] PanTiers => panTiers;
        public PanTierConfig EquippedPanTier => Upgrades != null ? Upgrades.EquippedPan : null;
        public RecipeConfig[] RecipeCatalog =>
            MapActive ? _book.ToArray() : startingRecipes;

        public MapState Map { get; private set; }
        public WorldMapConfig WorldMap => worldMap;
        public LocationConfig CurrentLocation =>
            (worldMap != null && Map != null && worldMap.locations != null
             && Map.CurrentIndex < worldMap.locations.Length)
                ? worldMap.locations[Map.CurrentIndex] : null;

        public event System.Action OnIngredientsChanged;
        public event System.Action OnNewLocationUnlocked;

        readonly List<RecipeConfig> _book = new List<RecipeConfig>();
        readonly List<IngredientConfig> _availableIngredients = new List<IngredientConfig>();
        readonly HashSet<int> _contentApplied = new HashSet<int>();
        bool MapActive => worldMap != null && worldMap.locations != null && worldMap.locations.Length > 0;
        public Sprite CoinIcon => coinIcon;
        public Sprite CloseIcon => closeIcon;
        public Sprite ActionButtonSprite => actionButtonSprite;
        public Sprite SuccessButtonSprite => successButtonSprite;
        public Sprite CancelButtonSprite => cancelButtonSprite;
        public Sprite IngredientSpotSprite => ingredientSpotSprite;
        public Sprite RecipeHudSpotSprite => recipeHudSpotSprite;
        public bool HasCookingPancake { get; private set; }

        public static GameSession Instance { get; private set; }

        Order _activeOrder;
        public Order ActiveOrder => _activeOrder;
        readonly Dictionary<IngredientConfig, int> _cookingIngredients = new Dictionary<IngredientConfig, int>();

        public event System.Action<Order> OnOrderSelected;
        public event System.Action OnServed;
        public event System.Action OnPancakeStarted;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            PancakeFlipUiFonts.Configure(uiFont);
            AutowirePanAssetsIfEmpty();
            Instance = this;
            Wallet = new Wallet(levelTable);
            Inventory = new Inventory();
            Upgrades = new PanUpgradeState();
            Upgrades.Initialize(defaultPanTier);
            int maxVisible = flipConfig != null ? flipConfig.maxVisibleOrders : DefaultMaxVisibleOrders;
            int persons = flipConfig != null ? flipConfig.personCount : DefaultPersonCount;
            if (MapActive)
            {
                var reqLevels = new int[worldMap.locations.Length];
                for (int i = 0; i < worldMap.locations.Length; i++)
                    reqLevels[i] = worldMap.locations[i] != null ? worldMap.locations[i].requiredLevel : 1;
                Map = new MapState(reqLevels, 0);
                // ВРЕМЕННО: все города открыты для теста (убрать, когда вернём гейт уровня/покупку).
                for (int i = 0; i < Map.LocationCount; i++) Map.MarkOwned(i);

                _book.Clear();
                if (baseRecipe != null) _book.Add(baseRecipe);
                _availableIngredients.Clear();
                if (doughIngredient != null) _availableIngredients.Add(doughIngredient);

                Orders = new OrderQueue(System.Array.Empty<RecipeConfig>(), maxVisible, persons);
            }
            else
            {
                Orders = new OrderQueue(startingRecipes, maxVisible, persons);
            }
            Build = new PancakeBuild();

            if (pancake != null)
                pancake.OnLanded += OnPancakeLanded;
        }

        void Start()
        {
            PancakeFlipUiFonts.ApplyToAllTextsInLoadedScenes();
            if (pancake != null)
                pancake.SetActiveCooking(false);
            if (MapActive)
                TravelTo(0);
        }

        void OnDestroy()
        {
            if (pancake != null)
                pancake.OnLanded -= OnPancakeLanded;
            if (Instance == this) Instance = null;
        }

        public void SelectOrder(Order order)
        {
            _activeOrder = order;
            OnOrderSelected?.Invoke(order);
        }

        public void TravelTo(int index)
        {
            if (!MapActive || Map == null) return;
            if (!Map.CanEnter(index)) return;

            var loc = worldMap.locations[index];
            if (loc == null) return;

            if (_contentApplied.Add(index))
            {
                if (loc.unlockRecipes != null)
                {
                    foreach (var r in loc.unlockRecipes)
                        if (r != null && !_book.Contains(r)) _book.Add(r);
                }
                if (loc.unlockIngredients != null)
                {
                    foreach (var ing in loc.unlockIngredients)
                        if (ing != null && !_availableIngredients.Contains(ing)) _availableIngredients.Add(ing);
                }
                OnIngredientsChanged?.Invoke();
            }

            Orders.SetSource(loc.demandRecipes);
            if (customerAnimator != null)
                customerAnimator.SetPersonSprites(loc.customerSprites);
            if (sceneBackground != null && loc.background != null)
                sceneBackground.sprite = loc.background;
            if (sceneBottomPanel != null)
            {
                if (loc.bottomPanel != null)
                {
                    sceneBottomPanel.sprite = loc.bottomPanel;
                    FitBottomPanelWidth();
                }
                sceneBottomPanel.enabled = loc.bottomPanel != null;
            }
            if (stove != null)
                stove.SetSprites(loc.stoveClosed, loc.stoveOpen);

            Map.SetCurrent(index);
            _activeOrder = null;
        }

        // Прилавок города может иметь свою нативную ширину — растягиваем на всю ширину камеры и прижимаем к низу.
        void FitBottomPanelWidth()
        {
            var cam = Camera.main;
            if (cam == null || sceneBottomPanel == null || sceneBottomPanel.sprite == null) return;
            float ortho = cam.orthographicSize;
            float camW = ortho * 2f * cam.aspect;
            var spr = sceneBottomPanel.sprite;
            if (spr.bounds.size.x <= 0f) return;
            float sc = camW / spr.bounds.size.x;
            var t = sceneBottomPanel.transform;
            t.localScale = Vector3.one * sc;
            float h = spr.bounds.size.y * sc;
            t.position = new Vector3(0f, -ortho + h * 0.5f, t.position.z);
        }

        public bool TryBuyCity(int index)
        {
            if (!MapActive || Map == null) return false;
            if (!Map.CanBuy(index, Wallet.Level)) return false;
            var loc = worldMap.locations[index];
            int cost = loc != null ? loc.cityCost : 0;
            if (cost > 0 && !Wallet.SpendCoins(cost)) return false;
            Map.MarkOwned(index);
            OnNewLocationUnlocked?.Invoke();
            return true;
        }

        public bool TryServe()
        {
            if (pancake == null || _activeOrder == null) return false;
            if (!HasCookingPancake) return false;

            float cookA = pancake.CookA;
            float cookB = pancake.CookB;
            float minReady = GetEffectivePerfectMin();

            if (cookA < minReady || cookB < minReady)
                return false;

            float overcook = GetEffectiveOvercookedThreshold();
            float overcookPenalty = flipConfig != null ? flipConfig.overcookCoinPenalty : DefaultOvercookPenalty;
            float mismatchPenalty = flipConfig != null ? flipConfig.recipeMismatchPenalty : DefaultMismatchPenalty;
            float coinMult = 1f;
            if (cookA >= overcook) coinMult *= overcookPenalty;
            if (cookB >= overcook) coinMult *= overcookPenalty;

            bool matches = MatchesOrderRecipe(_activeOrder.Recipe);
            if (!matches) coinMult *= mismatchPenalty;

            int coins = Mathf.RoundToInt(_activeOrder.RewardCoins * coinMult);
            int xp = matches ? _activeOrder.RewardXp : Mathf.RoundToInt(_activeOrder.RewardXp * mismatchPenalty);

            Wallet.AddCoins(Mathf.Max(1, coins));
            Wallet.AddXp(Mathf.Max(1, xp));

            Orders.CompleteOrder(_activeOrder);
            _activeOrder = null;

            ClearCookingPancake();
            ResetPancake();
            OnServed?.Invoke();
            return true;
        }

        bool MatchesOrderRecipe(RecipeConfig recipe)
        {
            if (recipe == null) return _cookingIngredients.Count == 0;
            if (recipe.ingredients == null) return _cookingIngredients.Count == 0;

            foreach (var slot in recipe.ingredients)
            {
                if (slot.ingredient == null) continue;
                if (slot.ingredient.isDough) continue;
                _cookingIngredients.TryGetValue(slot.ingredient, out int have);
                if (have < slot.amount) return false;
            }

            int builtNonDough = 0;
            foreach (var kv in _cookingIngredients)
            {
                if (kv.Key == null || kv.Key.isDough) continue;
                builtNonDough += kv.Value;
            }
            int recipeNeed = 0;
            foreach (var slot in recipe.ingredients)
            {
                if (slot.ingredient == null || slot.ingredient.isDough) continue;
                recipeNeed += slot.amount;
            }
            return builtNonDough == recipeNeed;
        }

        public bool TryCookFromBuild()
        {
            if (pancake == null) return false;
            if (Build == null || !Build.CanCook) return false;
            if (HasCookingPancake) return false;

            var aggregate = Build.ToAggregate();
            foreach (var kv in aggregate)
            {
                if (kv.Key == null) continue;
                if (Inventory.GetAmount(kv.Key) < kv.Value) return false;
            }

            foreach (var kv in aggregate)
            {
                if (kv.Key == null) continue;
                Inventory.TryRemove(kv.Key, kv.Value);
            }

            _cookingIngredients.Clear();
            foreach (var kv in aggregate)
                _cookingIngredients[kv.Key] = kv.Value;

            Build.Clear();
            HasCookingPancake = true;
            pancake.SetActiveCooking(true);
            pancake.ResetCooking();
            OnPancakeStarted?.Invoke();
            return true;
        }

        void ClearCookingPancake()
        {
            _cookingIngredients.Clear();
            HasCookingPancake = false;
            if (pancake != null)
                pancake.SetActiveCooking(false);
        }

        public bool TryServeBase() => TryServe();

        public void DismissOrder(Order order)
        {
            if (_activeOrder == order) _activeOrder = null;
            Orders.DismissOrder(order);
        }

        public void BuyIngredient(IngredientConfig ingredient, int amount = 1)
        {
            if (ingredient == null || ingredient.infinite) return;
            if (Inventory.IsAtCap(ingredient)) return;
            int totalCost = ingredient.coinCost * amount;
            if (totalCost > 0 && !Wallet.SpendCoins(totalCost)) return;
            if (totalCost == 0)
            {
                Inventory.Add(ingredient, amount);
                return;
            }
            int added = Inventory.Add(ingredient, amount);
            int refund = (amount - added) * ingredient.coinCost;
            if (refund > 0) Wallet.AddCoins(refund);
        }

        public bool TryAddToBuild(IngredientConfig ingredient)
        {
            if (ingredient == null || Build == null) return false;
            if (Build.IsFull) return false;
            if (Inventory.GetAmount(ingredient) <= CountInBuild(ingredient)) return false;
            return Build.TryAdd(ingredient);
        }

        public int CountInBuild(IngredientConfig ingredient)
        {
            if (ingredient == null || Build == null) return 0;
            int n = 0;
            foreach (var s in Build.Slots)
            {
                if (s == ingredient) n++;
            }
            return n;
        }

        public bool TryBuyStatStep(PanStatTrackConfig track) =>
            track != null && Upgrades.TryBuyStatStep(track, Wallet);

        public bool TryBuyPanTier(PanTierConfig tier) =>
            tier != null && Upgrades.TryBuyPan(tier, Wallet);

        public void EquipPanTier(PanTierConfig tier) => Upgrades.EquipPan(tier);

        public void TapDough(IngredientConfig dough)
        {
            if (dough == null) return;
            if (Inventory.GetAmount(dough) > 0) return;
            Inventory.Add(dough, 1);
        }

        void OnPancakeLanded(PancakeBehaviour.LandingResult result)
        {
            int xpPerRot = flipConfig != null ? flipConfig.xpPerRotation : DefaultXpPerRotation;
            int earned = Mathf.Max(1, result.rotations) * xpPerRot;
            Wallet.AddXp(earned);
        }

        void ResetPancake()
        {
            if (pancake != null)
                pancake.ResetCooking();
        }

        public bool IsPancakeCookedEnough()
        {
            if (pancake == null) return false;
            float min = GetEffectivePerfectMin();
            return pancake.CookA >= min && pancake.CookB >= min;
        }

        public float GetEffectivePerfectMin()
        {
            if (flipConfig == null) return DefaultPerfectMin;
            float w = Upgrades.GetMultiplier(PanUpgradeConfig.EffectType.WiderPerfectZone);
            float c = (flipConfig.perfectMin + flipConfig.perfectMax) * 0.5f;
            float half = (flipConfig.perfectMax - flipConfig.perfectMin) * 0.5f * w;
            float lo = Mathf.Clamp01(c - half);
            float hi = Mathf.Clamp01(c + half);
            if (lo > hi) { float t = lo; lo = hi; hi = t; }
            return lo;
        }

        public float GetEffectivePerfectMax()
        {
            if (flipConfig == null) return DefaultPerfectMax;
            float w = Upgrades.GetMultiplier(PanUpgradeConfig.EffectType.WiderPerfectZone);
            float c = (flipConfig.perfectMin + flipConfig.perfectMax) * 0.5f;
            float half = (flipConfig.perfectMax - flipConfig.perfectMin) * 0.5f * w;
            float lo = Mathf.Clamp01(c - half);
            float hi = Mathf.Clamp01(c + half);
            if (lo > hi) { float t = lo; lo = hi; hi = t; }
            return hi;
        }

        public float GetEffectiveOvercookedThreshold()
        {
            if (flipConfig == null) return DefaultOvercookThreshold;
            float m = Upgrades.GetMultiplier(PanUpgradeConfig.EffectType.SlowerOvercook);
            float b = flipConfig.overcookedThreshold;
            return Mathf.Clamp(1f - (1f - b) / Mathf.Max(1.001f, m), b, 0.99f);
        }

        public float ForceFromChargeWithUpgrades(float charge01)
        {
            if (flipConfig == null) return 0f;
            float f = flipConfig.ForceFromCharge(charge01);
            f *= Upgrades.GetMultiplier(PanUpgradeConfig.EffectType.EasierFlip);
            return f;
        }

        public float SpinFromChargeWithUpgrades(float charge01)
        {
            if (flipConfig == null) return 0f;
            float s = flipConfig.SpinFromCharge(charge01);
            float stab = Upgrades.GetMultiplier(PanUpgradeConfig.EffectType.StablerSpin);
            if (stab > 1.001f) s /= stab;
            return s;
        }

#if UNITY_EDITOR
        [ContextMenu("PancakeFlip/Подставить ассеты сковородки (Data/PancakeFlip)")]
        void ContextAutowirePanAssets()
        {
            AutowirePanAssetsIfEmpty();
            EditorUtility.SetDirty(this);
        }
#endif

        public void AutowirePanAssetsIfEmpty()
        {
#if UNITY_EDITOR
            bool changed = false;

            if (coinIcon == null)
            {
                var s = FindSpriteByName("Wallet");
                if (s != null) { coinIcon = s; changed = true; }
            }
            if (closeIcon == null)
            {
                var s = FindSpriteByName("CloseIcon");
                if (s != null) { closeIcon = s; changed = true; }
            }

            if (statTracks == null || statTracks.Length != 4 || AnyNull(statTracks))
            {
                var names = new[] { "StatWidePerfect", "StatSlowOver", "StatStableSpin", "StatEasyFlip" };
                var mergedStats = new PanStatTrackConfig[4];
                for (int i = 0; i < 4; i++)
                {
                    var disk = FindAssetByName<PanStatTrackConfig>(names[i]);
                    var keep = (statTracks != null && i < statTracks.Length) ? statTracks[i] : null;
                    mergedStats[i] = disk != null ? disk : keep;
                }
                if (!AnyNull(mergedStats))
                {
                    statTracks = mergedStats;
                    changed = true;
                }
            }

            if (panTiers == null || panTiers.Length != 4 || AnyNull(panTiers))
            {
                var names = new[] { "PanStarter", "PanIron", "PanPro", "PanElite" };
                var mergedTiers = new PanTierConfig[4];
                for (int i = 0; i < 4; i++)
                {
                    var disk = FindAssetByName<PanTierConfig>(names[i]);
                    var keep = (panTiers != null && i < panTiers.Length) ? panTiers[i] : null;
                    mergedTiers[i] = disk != null ? disk : keep;
                }
                if (!AnyNull(mergedTiers))
                {
                    panTiers = mergedTiers;
                    changed = true;
                }
            }

            var starter = FindAssetByName<PanTierConfig>("PanStarter");
            if (defaultPanTier == null && starter != null)
            {
                defaultPanTier = starter;
                changed = true;
            }

            if (changed && !Application.isPlaying)
                EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        static bool AnyNull<T>(T[] a) where T : Object
        {
            if (a == null) return true;
            foreach (var x in a)
            {
                if (x == null) return true;
            }
            return false;
        }

        static T FindAssetByName<T>(string name) where T : Object
        {
            var guids = AssetDatabase.FindAssets($"{name} t:{typeof(T).Name}", new[] { PanDataDir });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) != name) continue;
                var a = AssetDatabase.LoadAssetAtPath<T>(p);
                if (a != null) return a;
            }
            return null;
        }

        static Sprite FindSpriteByName(string name)
        {
            var guids = AssetDatabase.FindAssets($"{name} t:Sprite", new[] { "Assets/Art/PancakeFlip" });
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                if (System.IO.Path.GetFileNameWithoutExtension(p) != name) continue;
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (s != null) return s;
            }
            return null;
        }

#endif
    }
}
