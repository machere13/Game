using UnityEngine;

namespace IdlePancake.Prototypes.PancakeFlip
{
    public sealed class GameSession : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] PancakeFlipConfig flipConfig;
        [SerializeField] LevelTableConfig levelTable;
        [SerializeField] RecipeConfig[] startingRecipes;
        [SerializeField] RecipeConfig baseRecipe;
        [SerializeField] IngredientConfig[] allIngredients;
        [SerializeField] IngredientConfig doughIngredient;
        [SerializeField] PanUpgradeConfig[] allUpgrades;

        [Header("Scene refs")]
        [SerializeField] PancakeBehaviour pancake;

        public Wallet Wallet { get; private set; }
        public Inventory Inventory { get; private set; }
        public OrderQueue Orders { get; private set; }
        public PanUpgradeState Upgrades { get; private set; }
        public PancakeFlipConfig FlipConfig => flipConfig;
        public RecipeConfig BaseRecipe => baseRecipe;
        public IngredientConfig[] AllIngredients => allIngredients;
        public IngredientConfig DoughIngredient => doughIngredient;
        public PanUpgradeConfig[] AllUpgrades => allUpgrades;
        public RecipeConfig[] RecipeCatalog => startingRecipes;

        public static GameSession Instance { get; private set; }

        Order _activeOrder;
        public Order ActiveOrder => _activeOrder;

        public event System.Action<Order> OnOrderSelected;
        public event System.Action OnServed;

        void Awake()
        {
            Instance = this;
            Wallet = new Wallet(levelTable);
            Inventory = new Inventory();
            Upgrades = new PanUpgradeState();
            Orders = new OrderQueue(startingRecipes, 3, 3);

            if (pancake != null)
                pancake.OnLanded += OnPancakeLanded;
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

            float cookA = pancake.CookA;
            float cookB = pancake.CookB;
            float minReady = flipConfig != null ? flipConfig.perfectMin : 0.4f;

            if (cookA < minReady || cookB < minReady)
                return false;

            float overcook = flipConfig != null ? flipConfig.overcookedThreshold : 0.85f;
            float coinMult = 1f;
            if (cookA >= overcook) coinMult *= 0.5f;
            if (cookB >= overcook) coinMult *= 0.5f;

            bool hasIngredients = _activeOrder.Recipe == null || Inventory.HasIngredients(_activeOrder.Recipe);
            if (!hasIngredients)
                coinMult *= 0.5f;

            int coins = Mathf.RoundToInt(_activeOrder.RewardCoins * coinMult);
            int xp = hasIngredients ? _activeOrder.RewardXp : Mathf.RoundToInt(_activeOrder.RewardXp * 0.5f);

            Wallet.AddCoins(Mathf.Max(1, coins));
            Wallet.AddXp(Mathf.Max(1, xp));

            if (hasIngredients && _activeOrder.Recipe != null)
                Inventory.Consume(_activeOrder.Recipe);

            Orders.CompleteOrder(_activeOrder);
            _activeOrder = null;

            ResetPancake();
            OnServed?.Invoke();
            return true;
        }

        public bool TryServeBase()
        {
            if (pancake == null || _activeOrder == null || baseRecipe == null) return false;

            float cookA = pancake.CookA;
            float cookB = pancake.CookB;
            float minReady = flipConfig != null ? flipConfig.perfectMin : 0.4f;
            if (cookA < minReady || cookB < minReady) return false;

            bool hasIngredients = Inventory.HasIngredients(baseRecipe);
            float mult = hasIngredients ? 1f : 0.5f;

            if (hasIngredients)
                Inventory.Consume(baseRecipe);

            Wallet.AddCoins(Mathf.Max(1, Mathf.RoundToInt(baseRecipe.rewardCoins * mult)));
            Wallet.AddXp(Mathf.Max(1, Mathf.RoundToInt(baseRecipe.rewardXp * mult)));

            Orders.CompleteOrder(_activeOrder);
            _activeOrder = null;
            ResetPancake();
            OnServed?.Invoke();
            return true;
        }

        public void DismissOrder(Order order)
        {
            if (_activeOrder == order) _activeOrder = null;
            Orders.DismissOrder(order);
        }

        public void BuyIngredient(IngredientConfig ingredient, int amount = 1)
        {
            if (ingredient == null || ingredient.infinite) return;
            int totalCost = ingredient.coinCost * amount;
            if (Wallet.SpendCoins(totalCost))
                Inventory.Add(ingredient, amount);
        }

        public void BuyUpgrade(PanUpgradeConfig upgrade)
        {
            if (upgrade == null || Upgrades.IsOwned(upgrade)) return;
            if (Wallet.Level < upgrade.unlockLevel) return;
            if (Wallet.SpendCoins(upgrade.coinCost))
            {
                Upgrades.Purchase(upgrade);
                ApplyUpgrades();
            }
        }

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
            if (flipConfig == null) return;
            // Upgrades modify config at runtime — prototype approach
        }
    }
}
