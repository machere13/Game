using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IdlePancake.Prototypes.PancakeFlip
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameSession : MonoBehaviour
    {
        public const string PanDataDir = "Assets/Data/PancakeFlip";
        [Header("Config")]
        [SerializeField] PancakeFlipConfig flipConfig;
        [SerializeField] LevelTableConfig levelTable;
        [SerializeField] RecipeConfig[] startingRecipes;
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

        [Header("Scene refs")]
        [SerializeField] PancakeBehaviour pancake;

        public Wallet Wallet { get; private set; }
        public Inventory Inventory { get; private set; }
        public OrderQueue Orders { get; private set; }
        public PanUpgradeState Upgrades { get; private set; }
        public PancakeBuild Build { get; private set; }
        public PancakeFlipConfig FlipConfig => flipConfig;
        public RecipeConfig BaseRecipe => baseRecipe;
        public IngredientConfig[] AllIngredients => allIngredients;
        public IngredientConfig DoughIngredient => doughIngredient;
        public PanStatTrackConfig[] StatTracks => statTracks;
        public PanTierConfig[] PanTiers => panTiers;
        public PanTierConfig EquippedPanTier => Upgrades != null ? Upgrades.EquippedPan : null;
        public RecipeConfig[] RecipeCatalog => startingRecipes;
        public Sprite CoinIcon => coinIcon;
        public Sprite CloseIcon => closeIcon;
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
            PancakeFlipUiFonts.Configure(uiFont);
            AutowirePanAssetsIfEmpty();
            Instance = this;
            Wallet = new Wallet(levelTable);
            Inventory = new Inventory();
            Upgrades = new PanUpgradeState();
            Upgrades.Initialize(defaultPanTier);
            Orders = new OrderQueue(startingRecipes, 3, 3);
            Build = new PancakeBuild();

            if (pancake != null)
                pancake.OnLanded += OnPancakeLanded;
        }

        void Start()
        {
            PancakeFlipUiFonts.ApplyToAllTextsInLoadedScenes();
            if (pancake != null)
                pancake.SetActiveCooking(false);
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
            float coinMult = 1f;
            if (cookA >= overcook) coinMult *= 0.5f;
            if (cookB >= overcook) coinMult *= 0.5f;

            bool matches = MatchesOrderRecipe(_activeOrder.Recipe);
            if (!matches) coinMult *= 0.5f;

            int coins = Mathf.RoundToInt(_activeOrder.RewardCoins * coinMult);
            int xp = matches ? _activeOrder.RewardXp : Mathf.RoundToInt(_activeOrder.RewardXp * 0.5f);

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

        /// <summary>Клик по миске: бесплатно +1 теста, если миска пуста (в инвентаре 0).</summary>
        public void TapDough(IngredientConfig dough)
        {
            if (dough == null) return;
            if (Inventory.GetAmount(dough) > 0) return;
            Inventory.Add(dough, 1);
        }

        void OnPancakeLanded(PancakeBehaviour.LandingResult result)
        {
            int xpPerRot = flipConfig != null ? flipConfig.xpPerRotation : 10;
            int earned = Mathf.Max(1, result.rotations) * xpPerRot;
            Wallet.AddXp(earned);
        }

        void ResetPancake()
        {
            if (pancake != null)
                pancake.ResetCooking();
        }

        void ApplyUpgrades()
        {
            // Множители читаются через Upgrades.GetMultiplier при каждом вызове.
        }

        public float GetEffectivePerfectMin()
        {
            if (flipConfig == null) return 0.4f;
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
            if (flipConfig == null) return 0.7f;
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
            if (flipConfig == null) return 0.85f;
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
        void OnValidate()
        {
            if (EditorApplication.isPlaying) return;
            AutowirePanAssetsIfEmpty();
        }

        [ContextMenu("PancakeFlip/Подставить ассеты сковородки (Data/PancakeFlip)")]
        void ContextAutowirePanAssets()
        {
            AutowirePanAssetsIfEmpty();
            EditorUtility.SetDirty(this);
        }
#endif

        /// <summary>В редакторе подставляет statTracks / panTiers / defaultPanTier из стандартных имён сетапа, если что-то пусто.</summary>
        public void AutowirePanAssetsIfEmpty()
        {
#if UNITY_EDITOR
            const string d = PanDataDir;
            const string art = "Assets/Art/PancakeFlip";
            bool changed = false;

            if (coinIcon == null)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{art}/Wallet.png");
                if (s != null) { coinIcon = s; changed = true; }
            }
            if (closeIcon == null)
            {
                var s = AssetDatabase.LoadAssetAtPath<Sprite>($"{art}/CloseIcon.png");
                if (s != null) { closeIcon = s; changed = true; }
            }

            if (statTracks == null || statTracks.Length != 4 || AnyNull(statTracks))
            {
                var statPaths = new[]
                {
                    $"{d}/StatWidePerfect.asset",
                    $"{d}/StatSlowOver.asset",
                    $"{d}/StatStableSpin.asset",
                    $"{d}/StatEasyFlip.asset",
                };
                var mergedStats = new PanStatTrackConfig[4];
                for (int i = 0; i < 4; i++)
                {
                    var disk = LoadStat(statPaths[i]);
                    var keep = (statTracks != null && i < statTracks.Length) ? statTracks[i] : null;
                    mergedStats[i] = disk != null ? disk : keep;
                }
                if (!AnyNull(mergedStats))
                {
                    statTracks = mergedStats;
                    changed = true;
                }
            }

            if (panTiers == null || panTiers.Length != 3 || AnyNull(panTiers))
            {
                var tierPaths = new[]
                {
                    $"{d}/PanStarter.asset",
                    $"{d}/PanIron.asset",
                    $"{d}/PanPro.asset",
                };
                var mergedTiers = new PanTierConfig[3];
                for (int i = 0; i < 3; i++)
                {
                    var disk = LoadTier(tierPaths[i]);
                    var keep = (panTiers != null && i < panTiers.Length) ? panTiers[i] : null;
                    mergedTiers[i] = disk != null ? disk : keep;
                }
                if (!AnyNull(mergedTiers))
                {
                    panTiers = mergedTiers;
                    changed = true;
                }
            }

            var starter = LoadTier($"{d}/PanStarter.asset");
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

        static PanStatTrackConfig LoadStat(string path) =>
            AssetDatabase.LoadAssetAtPath<PanStatTrackConfig>(path);

        static PanTierConfig LoadTier(string path) =>
            AssetDatabase.LoadAssetAtPath<PanTierConfig>(path);
#endif
    }
}
