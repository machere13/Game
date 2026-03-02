# Idle Pancake Game Economy Spec

## 1. Economy Goals
- Predictable long-term progression.
- Short feedback loop in first sessions.
- Offline rewards are meaningful but capped.
- Prestige provides clear acceleration without invalidating early game.

## 2. Core Domain Objects
- `Wallet`: current balances by currency type.
- `RecipeRuntimeState`: level, unlocked flag, produced amount buffer.
- `UpgradeRuntimeState`: owned levels for upgrade IDs.
- `PrestigeRuntimeState`: current prestige rank and meta balance.
- `KitchenRuntimeState`: aggregate production multipliers.

## 3. Currencies
- `Coins` (soft currency): main progression resource.
- `PancakeToken` (meta currency): granted by prestige, spent on permanent boosts.

Storage recommendation:
- monetary values: `long` in minor units.
- percentage multipliers: fixed-point integer (`x1000`) or decimal abstraction.

## 4. Formula Contracts
Recommended application/domain interfaces:
- `IEconomyFormulaService`
  - `long CalculateRecipeOutputPerSecond(recipeState, modifiers)`
  - `long CalculateUpgradeCost(upgradeConfig, level)`
  - `long CalculateRecipeUnlockCost(recipeConfig, unlockOrderIndex)`
  - `long CalculatePrestigeReward(totalCoinsEarnedLifetime, currentPrestigeLevel)`
  - `long ApplyOfflineCap(long rawIncome, long maxAllowedIncome)`

- `IProductionService`
  - `ProductionTickResult SimulateTick(deltaSeconds, kitchenState)`

- `IPrestigeService`
  - `PrestigePreview CalculatePreview(playerState)`
  - `PrestigeResult Execute(playerState)`

## 5. Baseline Progression Formulas (MVP)

### 5.1 Recipe Income
- Base production:
  - `rps = baseRps * recipeLevelMultiplier(recipeLevel)`
- Level multiplier:
  - `recipeLevelMultiplier(L) = 1 + (L - 1) * levelStep`
- Final income per second:
  - `finalRps = rps * globalMultiplier * prestigeMultiplier * temporaryBoostMultiplier`

### 5.2 Upgrade Cost Curve
- Geometric:
  - `cost(level) = baseCost * growthRate^(level - 1)`
- Safety limits:
  - clamp to `long.MaxValue` threshold policy before overflow.

### 5.3 Recipe Unlock Curve
- `unlockCost(order) = unlockBase * unlockGrowth^order`

### 5.4 Prestige Reward
- Recommended soft-root curve:
  - `reward = floor(k * sqrt(totalCoinsEarnedLifetime / scale))`
- Final reward includes minimum floor:
  - `finalReward = max(minPrestigeReward, reward - alreadyClaimedEquivalent)`

### 5.5 Offline Reward
- `offlineSeconds = clamp(nowUtc - lastSeenUtc, 0, offlineCapSeconds)`
- `offlineIncome = finalRps * offlineSeconds * offlineEfficiency`
- `offlineEfficiency` for MVP: `0.7 .. 1.0` (configurable).

## 6. Use Cases (Application Layer)

### `GameTickUseCase`
Input:
- `deltaSeconds`
- current snapshot

Steps:
1. Compute per-recipe output through `IProductionService`.
2. Accumulate into uncollected buffer.
3. Emit `IncomeAccumulated`.
4. Enqueue save debounce signal.

### `CollectIncomeUseCase`
Input:
- player collect intent

Steps:
1. Move buffer to `Wallet.Coins`.
2. Reset buffer.
3. Emit `IncomeCollected`.

### `BuyUpgradeUseCase`
Input:
- `upgradeId`

Steps:
1. Load upgrade config and current level.
2. Compute cost via `IEconomyFormulaService`.
3. Validate wallet amount.
4. Deduct cost, increment upgrade level, apply modifier.
5. Emit `UpgradePurchased`.

### `UnlockRecipeUseCase`
Input:
- `recipeId`

Steps:
1. Validate prerequisites and unlock order.
2. Compute unlock cost.
3. Deduct wallet, set unlocked state.
4. Emit `RecipeUnlocked`.

### `ApplyOfflineProgressUseCase`
Input:
- `nowUtc`, `lastSeenUtc`

Steps:
1. Compute elapsed with cap.
2. Simulate offline income with efficiency coefficient.
3. Add result to uncollected or directly wallet (project choice, keep consistent).
4. Update `lastSeenUtc`.
5. Emit `OfflineProgressApplied`.

### `PrestigeResetUseCase`
Input:
- explicit player confirmation

Steps:
1. Compute reward preview and validate threshold.
2. Grant `PancakeToken`.
3. Reset run-specific state (coins, recipe unlocks, non-meta upgrades).
4. Keep meta progression and prestige upgrades.
5. Reinitialize run defaults.
6. Emit `PrestigePerformed`.

## 7. Feature Contracts by Module
- `CoreLoop`
  - `ITickSource`, `IGameTickUseCase`
- `Economy`
  - `IEconomyFormulaService`, `IWalletService`
- `Recipes`
  - `IRecipeUnlockUseCase`, `IRecipeProgressService`
- `Upgrades`
  - `IBuyUpgradeUseCase`, `IUpgradeEffectResolver`
- `OfflineProgress`
  - `IApplyOfflineProgressUseCase`
- `Prestige`
  - `IPrestigePreviewUseCase`, `IPrestigeResetUseCase`

## 8. Balancing Parameters (Configurable)
- `levelStep`
- `growthRate`
- `unlockGrowth`
- `offlineCapSeconds`
- `offlineEfficiency`
- `prestigeScale`
- `prestigeK`
- `minPrestigeReward`

Keep these in balance `ScriptableObject` assets, never hardcoded in domain logic.
