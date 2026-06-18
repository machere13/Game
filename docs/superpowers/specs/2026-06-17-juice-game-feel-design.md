# Juice / Game Feel — Design

Дата: 2026-06-17
Статус: утверждён, готов к плану реализации
Ветка: `feat/juice` (от `feat/global-map`)

## Контекст

PancakeFlip — прототип про жарку блинов. Геймплейные события уже эмитятся:
`PancakeBehaviour.OnLanded(LandingResult)` (с `rotations`), `GameSession.OnServed`,
`GameSession.OnPancakeStarted`, `GameSession.OnNewLocationUnlocked`, `Wallet.OnChanged`.
Существует базовый отклик: индикатор заряда, попап вращений (`PancakeFlipScoreView` +
`rotationsPopupText`), индикаторы прожарки. Весь UI/сцена генерируются
`PancakeFlipSetup.BuildEverything()`. Аудио-ассетов и арт-ассетов под эффекты нет.

Цель: добавить «сочный» отклик (juice) на ключевые моменты, **умеренной** интенсивности
(заметно, но не утомляет), визуал + простой звук. Подход — **событийный**: отдельный
`JuiceController` слушает существующие события и дёргает переиспользуемый тулкит; геймплейный
код почти не меняется (декаплинг).

## Принципы

- Juice — презентационный слой. Тулкит не знает про геймплей; `JuiceController` — единственный
  мост между событиями и эффектами.
- Никаких внешних зависимостей (без DOTween). Твины — корутины, как в существующем
  `CustomerAnimator`.
- Всё навешивается в `BuildEverything` (остаёмся в текущем паттерне).
- Null-safe: отсутствие клипа/ссылки не должно ронять игру (juice опционален).
- Мобилка и десктоп: эффекты используют существующие UI-якоря (адаптивны автоматически).

## Озвучиваемые моменты (по решению)

Подброс/приземление блина; сдача заказа; покупка/готовка; прогресс (уровень/разблокировка).

## Компоненты

### 1. Тулкит (презентационные хелперы)

- `Juice` (static) — корутинные хелперы поверх `MonoBehaviour`:
  - `PunchScale(Transform, float strength, float duration)` — поп с овершутом (через `JuiceCurves`).
  - `Shake(Transform, float amplitude, float duration)` — затухающее дрожание позиции.
  - `FlashColor(Graphic, Color flash, float duration)` — кратковременная вспышка цвета.
  Запускаются через переданный `MonoBehaviour` (`StartCoroutine`), т.к. static не может сам.
- `FloatingTextSpawner` (MonoBehaviour) — спавнит TMP-текст в заданной экранной точке,
  поднимает и затухает за время; цвет/размер задаются вызовом. Метод
  `Spawn(string text, Vector2 screenPos, Color color, float size)`.
- `CoinFlySpawner` (MonoBehaviour) — спавнит иконку монеты, летит от точки к `RectTransform`
  кошелька, по прилёту — панч кошелька. `Fly(Vector2 fromScreen, int count)`.
- `ScreenShake` (MonoBehaviour на камере) — короткий шейк ортокамеры; `Shake(float amp, float dur)`.
- `ButtonJuice` (MonoBehaviour) — на `Button`: при `onClick` делает `PunchScale` своего
  `RectTransform` и зовёт `Sfx.Click()`. Вешается на «Купить»/«Готовить»/«Сдать»/профиль.
- `Sfx` (MonoBehaviour, singleton-доступ) — `AudioSource` + именованные `AudioClip` поля
  (`flip`, `serve`, `cook`, `levelUp`, `unlock`, `click`). Если клип null — проигрывает
  процедурный блип (`AudioClip.Create`, короткая синусоида с заданной частотой), так что
  звук есть без ассетов. Методы: `PlayFlip(float pitch)`, `PlayServe()`, `PlayCook()`,
  `PlayLevelUp()`, `PlayUnlock()`, `Click()`.

Каждый юнит: одна ответственность, понятный интерфейс, тестируется/проверяется отдельно.

### 2. `JuiceController` (MonoBehaviour) — мост событие→эффект

Ссылки (через инспектор, проставляются в `BuildEverything`): `Camera cam`, `ScreenShake`,
`FloatingTextSpawner`, `CoinFlySpawner`, `Sfx`, `RectTransform walletAnchor`,
`RectTransform profileAnchor`, `PancakeBehaviour pancake`, `Transform pan`, `Graphic panFlashTarget`.

Подписки (в `OnEnable`, отписка в `OnDisable`):
- `pancake.OnLanded(result)` → `PunchScale(pancake)` (сквош), `FlashColor` при хорошем
  приземлении, `FloatingText "x{rotations}!"` у блина, `ScreenShake` с амплитудой от
  `rotations` (кламп), `Sfx.PlayFlip(pitch=f(rotations))`.
- `GameSession.OnPancakeStarted` → `PunchScale(pan)`, `Sfx.PlayCook()`.
- `Wallet.OnChanged` → диффим монеты и опыт относительно запомненных значений:
  - монеты выросли → `CoinFly(fromScreen=centre, count)`, `FloatingText "+{Δ}"` у кошелька,
    `PunchScale(walletAnchor)`; монеты упали (трата) — без эффекта (или лёгкий тик).
  - опыт вырос → `FloatingText "+{Δ} xp"` у профиля.
  Это покрывает и награду за заказ, и опыт за подброс, без изменения логики наград.
- `Wallet.OnLevelUp(newLevel)` → `FlashColor(profile)`, крупный `FloatingText "LEVEL UP"`,
  `ScreenShake`, `Sfx.PlayLevelUp()`.
- `GameSession.OnNewLocationUnlocked` → `FloatingText "Новая локация!"`, `ScreenShake`,
  `Sfx.PlayUnlock()`.

`JuiceController` запоминает последние `Coins`/`TotalXp` в `Awake`/первом тике, чтобы
корректно диффить (первое срабатывание не выдаёт ложный «+»).

### 3. Маленький хук в логике

- `Wallet`: добавить `public event Action<int> OnLevelUp;` и вызывать его в `AddXp`, когда
  `Level` увеличился (после цикла повышения, по разу на каждый новый уровень или один раз с
  итоговым уровнем — один раз с итоговым уровнем достаточно). Изолированное изменение, не
  ломает существующий `OnChanged`.

### 4. Тестируемое ядро

- Новая сборка `IdlePancake.PancakeFlip.JuiceCore` (без UnityEngine) + `JuiceCurves`:
  - `EaseOutBack(float t)` — стандартный ease-out-back.
  - `PunchScale01(float t)` — множитель 0..1..0 с овершутом для пунч-скейла.
- EditMode-тесты (в существующей тест-сборке, добавить ссылку на JuiceCore):
  - `EaseOutBack(0) == 0`, `EaseOutBack(1) == 1` (с допуском).
  - `EaseOutBack` даёт овершут > 1 где-то на (0,1).
  - `PunchScale01(0) ≈ 0`, `PunchScale01(1) ≈ 0`, и пик в середине.
- Хелперы тулкита (`Juice.PunchScale` и т.п.) используют эти кривые.

### 5. Проводка в `BuildEverything`

- Создать `JuiceController`, `FloatingTextSpawner`, `CoinFlySpawner`, `ScreenShake` (на
  камере), `Sfx` (+`AudioSource`); проставить ссылки (камера, `walletGo`/`profileGo`
  как якоря, `pcBh`/`panGo`, спрайт монеты `walletSpr`/`coinIcon`, шрифт `tmpFont`).
- Навесить `ButtonJuice` на кнопки покупки/готовки (в `IngredientsScreenView`-кнопках),
  «Сдать», профиль.
- Всё кодом; UI генерируется как прежде.

## Поток данных

```
gameplay events (pancake/GameSession/Wallet)
        |
        v
   JuiceController  --> Juice / FloatingText / CoinFly / ScreenShake / Sfx
ButtonJuice (на кнопках) --> Juice.PunchScale + Sfx.Click   (напрямую, без контроллера)
```

## Краевые случаи

- Нет `AudioListener`/клипов → `Sfx` генерит блип или молчит, без исключений.
- Быстрая серия событий (несколько монет подряд) → спавнеры переиспользуют/пулят объекты
  либо просто создают и уничтожают по таймеру (для прототипа — создавать/уничтожать ок).
- `Wallet.OnChanged` на трату (покупка) → отрицательная дельта монет, эффект «+» не играем.
- Ресайз/смена ориентации во время эффекта → эффекты короткие, привязаны к экранным якорям;
  допустимая мелочь.

## Проверка (Unity)

- EditMode-тесты `JuiceCurves` зелёные.
- Подброс: блин «пружинит», попап «x{N}!», на крупных вращениях — шейк; слышен блип.
- Сдача: «+монеты» летят в кошелёк, кошелёк пульсирует, «+xp» у профиля.
- Покупка/готовка: кнопки поппят при нажатии, слышен клик.
- Повышение уровня: вспышка профиля + «LEVEL UP» + шейк.
- Разблокировка локации (сдать N): «Новая локация!» + шейк.
- Портрет и ландшафт: эффекты на местах (используют существующие якоря).

## Вне scope

- Реальные звуковые файлы (пока процедурные блипы), частицы-ассеты, специфичный арт.
- Настройки громкости/отключения juice (можно позже).
