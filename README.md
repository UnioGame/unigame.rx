# UniGame.Rx

A comprehensive reactive module based on R3 and ObservableCollections for Unity, providing powerful tools for reactive programming, state management, and asynchronous operations.

## Installation

To work with the module, you need to install the following dependencies:

```json
{
  "dependencies": {
    "com.cysharp.unitask" : "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.unigame.unicore": "https://github.com/UnioGame/unigame.core.git",
    "com.cysharp.r3": "https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity",
    "com.github-glitchenzo.nugetforunity": "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity"
  }
}
```

### Additional packages via NuGetForUnity:

- **ObservableCollections** - for reactive collections
- **ObservableCollections.R3** - R3 integration (if needed)

Follow the instructions on the packages' home pages:
- [R3](https://github.com/Cysharp/R3)
- [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity)
- [ObservableCollections](https://github.com/Cysharp/ObservableCollections)

# Core Components

## 🔄 ReactiveValue<T>

The main reactive container for values with subscription support and automatic notifications.

### Basic Usage

```csharp
// Creating reactive values
var reactiveInt = new ReactiveValue<int>();
var reactiveString = new ReactiveValue<string>("initial value");

// Subscribing to changes
reactiveInt.Subscribe(value => Debug.Log($"New value: {value}"))
    .AddTo(lifeTime);

// Changing value
reactiveInt.Value = 42; // Automatically notifies subscribers

// Force notification
reactiveInt.ForceNotify();

// Set value without equality check
reactiveInt.SetValueForce(100);
```

### State Checking

```csharp
var reactive = new ReactiveValue<string>();

// Check if has value
if (reactive.HasValue)
{
    Debug.Log($"Value: {reactive.Value}");
}

// Check if has subscribers
if (reactive.HasObservers)
{
    Debug.Log("Has active subscribers");
}

// Check completion status
if (reactive.IsCompleted || reactive.IsDisposed)
{
    Debug.Log("ReactiveValue is no longer active");
}
```

### Specialized Types

```csharp
// Ready-to-use typed versions
var intValue = new IntReactiveValue();
var floatValue = new FloatReactiveValue();
var boolValue = new BoolReactiveValue(true); // With initial value
var stringValue = new StringReactiveValue();
var doubleValue = new DoubleReactiveValue();
var byteValue = new ByteReactiveValue();
```

## 🔄 Reactive Extensions

Powerful extensions for working with reactive values and data binding.

### Asynchronous Operations

```csharp
// Awaiting first value
var firstValue = await reactiveValue.FirstAsync(lifeTime);

// Awaiting first value with predicate
var firstValidValue = await reactiveValue
    .Where(x => x > 0)
    .FirstAsync(lifeTime);

// Awaiting with timeout and logging
var result = await reactiveValue.AwaitFirstAsyncNoException(lifeTime);
if (!result.IsCanceled)
{
    Debug.Log($"Received: {result.Result}");
}
```

### Data Binding

```csharp
public class MyComponent : MonoBehaviour, ILifeTimeContext
{
    private LifeTime _lifeTime = new();
    public ILifeTime LifeTime => _lifeTime;

    [SerializeField] private GameObject targetObject;
    [SerializeField] private Text uiText;

    private ReactiveValue<bool> isVisible = new();
    private ReactiveValue<string> displayText = new();

    void Start()
    {
        // Bind to GameObject activity
        this.Bind(isVisible, targetObject);
        
        // Inverted binding
        this.BindNot(isVisible, someOtherObject);
        
        // Bind to UI text
        this.Bind(displayText, text => uiText.text = text);
        
        // Bind with async action
        this.Bind(displayText, async text => {
            await SomeAsyncOperation(text);
        });
        
        // Conditional binding
        this.BindIf(displayText, 
            predicate: () => Application.isPlaying,
            target: text => Debug.Log(text));
    }

    void OnDestroy() => _lifeTime.Release();
}
```

#### Choose the Binding Entry Point

Use the receiver to express who owns the subscription:

| Entry point | Use when | Typical overloads |
|-------------|----------|-------------------|
| `this.Bind(...)` | A view, presenter, or MonoBehaviour already implements `ILifeTimeContext` | `Bind(source, Action<T>)`, `Bind(source, Func<T, UniTask>)`, `Bind(source, GameObject)` |
| `lifeTime.Bind(...)` | A service, test, or pure runtime object only has `ILifeTime` | `Bind(source, Action<T>)`, `Bind(source, Func<UniTask>)` |
| `Bind(data, source, ...)` | The callback needs extra data without capturing outer variables | `Bind(data, source, Action<TValue, TData>)` |
| `BindData(...)` | The callback needs sender + extra data + current value without closure allocations | `BindData(source, Action<BindData<TSender, TValue>>)` |

#### Binding Families

`ReactiveBindingExtensions` is intentionally broad. In practice the overloads fall into a few predictable groups:

| Family | Typical targets | What it solves |
|--------|------------------|----------------|
| Direct action bindings | `Action<T>`, `Action`, `Action<TSender, T>` | Subscribe and execute sync callbacks with lifetime ownership |
| Reactive target bindings | `ReactiveValue<T>`, `ReactiveProperty<T>`, `ISubject<T>` | Forward values into another reactive container |
| Async bindings | `Func<T, UniTask>`, `Func<UniTask>` | Fire async work per event with automatic cancellation on lifetime end |
| Bool/UI helpers | `GameObject`, `IEnumerable<GameObject>`, `BindNot(...)` | Toggle active state and avoid repetitive `SetActive` plumbing |
| Command bindings | `ReactiveCommand<T>`, `ReactiveCommand<Unit>` | Route observable values into command execution with `CanExecute()` checks |
| Conditional & timer bindings | `BindIf`, `BindWhere`, `BindIntervalUpdate` | Add guards and periodic updates without manual timer setup |
| Cleanup bindings | `BindDispose`, `BindCleanUp`, `BindRestart` | Tie cleanup and restart logic to the same lifetime contract |
| Reflection bindings | `MethodInfo` | Bind a stream to a method dynamically when static wiring is not practical |

#### Common Binding Patterns

##### 1. Bind a value stream to a callback

```csharp
private readonly ReactiveValue<int> _coins = new();

void Start()
{
    this.Bind(_coins, value => coinsText.text = value.ToString());
}
```

##### 2. Forward one reactive source into another

```csharp
private readonly ReactiveValue<int> _source = new();
private readonly ReactiveValue<int> _target = new();

void Start()
{
    this.Bind(_source, _target);
}
```

##### 3. Bind bool state to visibility

```csharp
private Observable<bool> _visibilityStream;

void Start()
{
    this.Bind(_visibilityStream, contentRoot);
    this.BindNot(_visibilityStream, loadingOverlay);
}
```

##### 4. Run async logic per event with automatic cancellation

```csharp
void Start()
{
    this.Bind(saveRequests, async request =>
    {
        await SaveProfile(request);
    });
}
```

##### 5. Execute commands from a stream

```csharp
private readonly ReactiveCommand<int> _selectLevelCommand = new();

void Start()
{
    this.Bind(selectedLevel, _selectLevelCommand, LifeTime);
}
```

##### 6. Register cleanup with the same fluent style

```csharp
void Start()
{
    this.BindCleanUp(() => Disconnect());
    this.BindDispose(_subscription);
}
```

#### Allocation-Aware Binding Patterns

If a callback needs external context, prefer overloads that thread data through the observable chain instead of capturing outer variables in a lambda.

The simplest form is `Bind(data, source, Action<TValue, TData>)`:

```csharp
private Observable<int> _scoreStream;

void Start()
{
    this.Bind(scoreLabel, _scoreStream, static (value, label) =>
    {
        label.text = value.ToString();
    });
}
```

That pattern keeps the callback explicit and avoids allocating a closure around `scoreLabel`.

Use `BindData(...)` when the callback also needs the sender or when a single context struct is easier to pass around.

##### `BindData<TSender, TValue>`

```csharp
private Observable<int> _scoreStream;

void Start()
{
    this.BindData(_scoreStream, static context =>
    {
        context.Source.name = $"Score:{context.Value}";
    });
}
```

This overload packs `Source` and the emitted `Value` into a small struct.

##### `BindData<TSender, TData>` plus emitted value

```csharp
private Observable<int> _scoreStream;

void Start()
{
    this.BindData(scoreLabel, _scoreStream, static (value, context) =>
    {
        context.Value.text = value.ToString();
    });
}
```

This overload is useful when you need both the emitted value and some external data. Note that in `BindData<TSender, TData>` the field is named `Value`, but it stores the external data payload, not the emitted observable value.

##### `BindData<TSender, TData, TValue>` full context

```csharp
private Observable<int> _scoreStream;

void Start()
{
    this.BindData(scoreLabel, _scoreStream, static context =>
    {
        context.Data.text = $"{context.Source.name}: {context.Value}";
    });
}
```

This is the most self-descriptive shape when the callback needs all three pieces of information:

- `Source`: the lifetime owner that created the binding
- `Data`: external context passed into the bind call
- `Value`: the emitted observable value

Use these overloads when subscriptions are created frequently or when UI code tends to capture multiple external references.

#### Reflection Binding

`Bind(source, MethodInfo)` exists for dynamic binding scenarios and supports methods with either no arguments or a single argument matching the observable value type.

```csharp
var method = GetType().GetMethod(nameof(HandleReward), BindingFlags.Instance | BindingFlags.NonPublic);
this.Bind(rewardStream, method);
```

Internally this path uses `ArrayPool<object>` for parameter packing, but it is still less explicit and slower than direct typed overloads. Prefer typed `Bind(...)` methods whenever the target method is known at compile time.

#### Subscription Helpers Around Bindings

`ReactiveBindingExtensions` works best together with helpers from `RxExtension` and `RxLifetimeExtension`.

##### Reuse observers with pooling

```csharp
var observer = this.CreateRecycleObserver<int>(value => Debug.Log(value));
numberStream.Subscribe(observer).AddTo(LifeTime);
```

Use `CreateRecycleObserver()` when the same observer shape is created often and you want to reuse pooled observer instances.

##### Add side effects without breaking the stream

```csharp
healthStream
    .WhenTrue(_ => damageFlash.Play())
    .Subscribe()
    .AddTo(LifeTime);
```

`When`, `WhenTrue`, and `WhenFalse` are good for readable side effects while keeping the observable chain intact.

##### Bind classic events to lifetime

```csharp
public event Action Closed;

void Start()
{
    LifeTime.BindEvent(Closed, OnClosed);
}
```

`BindEvent(...)` gives regular C# events the same lifetime discipline as reactive subscriptions.

### Interval Updates

```csharp
// Periodic action execution
this.BindIntervalUpdate(
    interval: TimeSpan.FromSeconds(1),
    target: () => Debug.Log("Every second"));

// With execution condition
this.BindIntervalUpdate(
    interval: TimeSpan.FromSeconds(0.1f),
    target: () => UpdateUI(),
    predicate: () => gameObject.activeInHierarchy);

// With data source
this.BindIntervalUpdate(
    interval: TimeSpan.FromMilliseconds(100),
    source: () => player.Health,
    target: health => healthBar.value = health,
    predicate: () => player != null);
```

## 📨 MessageBroker

Message exchange system for loosely coupled architecture.

### Basic Usage

```csharp
// Global broker
var broker = MessageBroker.Default;

// Publishing messages
broker.Publish(new PlayerDiedEvent { PlayerId = 1 });
broker.Publish("Simple string message");

// Subscribing to messages
broker.Receive<PlayerDiedEvent>()
    .Subscribe(evt => Debug.Log($"Player {evt.PlayerId} died"))
    .AddTo(lifeTime);

// Subscribing to string messages
broker.Receive<string>()
    .Subscribe(message => Debug.Log($"Received: {message}"))
    .AddTo(lifeTime);
```

### Creating Custom Broker

```csharp
public class GameEventBroker : MonoBehaviour, IMessageBroker
{
    private MessageBroker _broker = new();

    public void Publish<T>(T message) => _broker.Publish(message);
    public Observable<T> Receive<T>() => _broker.Receive<T>();
    public void Dispose() => _broker.Dispose();
}
```

## 🗃️ TypeData

Type-safe data storage with reactive capabilities.

### Basic Operations

```csharp
var typeData = new TypeData();

// Publishing data
typeData.Publish(new PlayerData { Name = "John", Level = 5 });
typeData.Publish(42); // int value
typeData.Publish("Hello World"); // string value

// Getting data
var playerData = typeData.Get<PlayerData>();
var intValue = typeData.Get<int>();

// Checking data availability
if (typeData.Contains<PlayerData>())
{
    Debug.Log("Player data is available");
}

// Subscribing to changes
typeData.Receive<PlayerData>()
    .Subscribe(data => Debug.Log($"Player: {data.Name}, Level: {data.Level}"))
    .AddTo(lifeTime);

// Removing data
typeData.Remove<PlayerData>();
```

### Force Publishing

```csharp
// Normal publishing (only if value changed)
typeData.Publish(playerData);

// Force publishing (always notifies subscribers)
typeData.PublishForce(playerData);
```

## 🎭 AsyncState<TData, TResult>

Powerful asynchronous state management system with lifecycle support.

### Creating States

```csharp
public class LoadPlayerDataState : AsyncState<int, PlayerData>
{
    protected override async UniTask<PlayerData> OnExecute(int playerId, ILifeTime executionLifeTime)
    {
        // Main state logic
        var playerData = await LoadPlayerFromServer(playerId);
        return playerData;
    }

    protected override async UniTask OnComplete(PlayerData result, int playerId, ILifeTime lifeTime)
    {
        // Actions after successful execution
        Debug.Log($"Loaded player: {result.Name}");
    }

    protected override async UniTask OnExit(int playerId)
    {
        // Resource cleanup
        Debug.Log("State completed");
    }
}
```

### Using States

```csharp
var loadState = new LoadPlayerDataState();

// Execute state
var playerData = await loadState.ExecuteAsync(playerId: 123);

// Check activity
if (loadState.IsActive)
{
    Debug.Log("State is currently executing");
}

// Force termination
await loadState.ExitAsync();
```

### States with Rollback

```csharp
public class TransactionState : AsyncState<TransactionData, bool>, IAsyncRollback<TransactionData>
{
    protected override async UniTask<bool> OnExecute(TransactionData data, ILifeTime executionLifeTime)
    {
        // Execute transaction
        return await ProcessTransaction(data);
    }

    public async UniTask Rollback(TransactionData data)
    {
        // Rollback transaction on error
        await RollbackTransaction(data);
    }
}
```

## 🎬 AsyncScenario<TCommand, TData>

Sequential command execution with rollback support.

### Creating Scenarios

```csharp
public class GameStartScenario : AsyncScenario<IAsyncCommand<GameData, AsyncStatus>, GameData>
{
    public GameStartScenario()
    {
        commands.Add(new LoadConfigCommand());
        commands.Add(new InitializeServicesCommand());
        commands.Add(new LoadPlayerDataCommand());
        commands.Add(new ShowMainMenuCommand());
    }
}
```

### Using Scenarios

```csharp
var gameData = new GameData();
var scenario = new GameStartScenario();

// Execute scenario
var result = await scenario.ExecuteAsync(gameData);

if (result != AsyncStatus.Succeeded)
{
    // Rollback on error
    await scenario.Rollback(context);
}
```

## 🔄 RxState<TData, TResult>

Reactive state with Observable properties.

### Creating RxState

```csharp
public class PlayerHealthState : RxState<Player, float>
{
    protected override async UniTask<StateResult<float>> OnExecuteAsync(Player player, ILifeTime executionLifeTime)
    {
        // Monitor player health
        var currentHealth = player.Health;
        
        return new StateResult<float>
        {
            success = true,
            result = currentHealth
        };
    }

    protected override async UniTask OnCompleteAsync(Player player, float health, ILifeTime lifeTime)
    {
        if (health <= 0)
        {
            await HandlePlayerDeath(player);
        }
    }
}
```

### Using RxState

```csharp
var healthState = new PlayerHealthState();

// Subscribe to context changes
healthState.Context
    .Subscribe(player => Debug.Log($"Monitoring player: {player.Name}"))
    .AddTo(lifeTime);

// Subscribe to results
healthState.Result
    .Subscribe(health => UpdateHealthUI(health))
    .AddTo(lifeTime);

// Execute state
var finalHealth = await healthState.ExecuteAsync(player);
```

## ⚡ Async Extensions

Extensions for working with asynchronous operations.

### UniTask Extensions

```csharp
// Check completion
if (myTask.IsCompleted())
{
    Debug.Log("Task completed");
}

// Auto despawn next frame
someGameObject.DespawnNextFrame(destroy: false);

// Wait for specific number of frames
await this.AwaitTiming(PlayerLoopTiming.Update, awaitAmount: 5);

// Timeout with logging
await myTask.AttachTimeoutLogAsync("Operation timed out", 5000f, cancellationToken);

// Create shared instance
var sharedAsset = await LoadAssetAsync().ToSharedInstanceAsync(lifeTime);
```

### Semaphore Extensions

```csharp
// Safe execution with semaphore
await semaphore.WaitAsyncWithAction(cancellationToken, () => {
    // Critical section
    ProcessCriticalOperation();
});

// Async execution with semaphore
await semaphore.WaitAsyncWithAsyncAction(cancellationToken, async () => {
    await ProcessAsyncOperation();
});
```

## 🎮 UI Extensions

Specialized extensions for working with UI.

### Binding to UI Components

```csharp
public class HealthBarView : MonoBehaviour, ILifeTimeContext
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private GameObject warningIcon;

    private ReactiveValue<float> health = new();
    private ReactiveValue<bool> isLowHealth = new();

    void Start()
    {
        // Bind to slider
        this.Bind(health, value => healthSlider.value = value / 100f);
        
        // Bind to text
        this.Bind(health, value => healthText.text = $"{value:F0}/100");
        
        // Bind to warning icon
        this.Bind(isLowHealth, warningIcon);
        
        // Computed value
        health.Subscribe(value => isLowHealth.Value = value < 25f)
            .AddTo(LifeTime);
    }
}
```

## 🔧 Utility Classes

### ProgressReporter

Operation progress tracking system.

```csharp
var progressReporter = new ProgressReporter();
progressReporter.minProgress = 0f;
progressReporter.maxProgress = 100f;

// Add reporters
progressReporter.AddReporter(new Progress<float>(value => 
    progressBar.value = value / 100f));

// Create progress nodes
var loadingProgress = progressReporter.NewProgress();
var processingProgress = progressReporter.NewProgress();

// Update progress
loadingProgress.Report(50f);
processingProgress.Report(75f);

// Wait for completion
await progressReporter.WaitForComplete();

// Cleanup
progressReporter.Dispose();
```

### AnimatorStateTrigger

Reactive events for Animator states.

```csharp
public class AnimatorStateTrigger : StateMachineBehaviour
{
    [SerializeField] public string StateName = "Attack";

    void Start()
    {
        // Subscribe to state enter
        ObserveStateEnter
            .Subscribe(stateName => Debug.Log($"Entered state: {stateName}"))
            .AddTo(lifeTime);

        // Subscribe to state exit
        ObserveStateExit
            .Subscribe(stateName => Debug.Log($"Exited state: {stateName}"))
            .AddTo(lifeTime);
    }
}
```

### Animator Async Extensions

```csharp
// Wait for state completion
await animator.WaitStateEndAsync(Animator.StringToHash("AttackState"));

// Wait for current state completion
await animator.WaitForEndAsync(Animator.StringToHash("JumpState"), layer: 0);
```

## 🏗️ Advanced Patterns

### State Composition

```csharp
// Proxy state with delegates
var proxyState = new AsyncStateProxy<GameData, bool>(
    onExecute: async (data, lifeTime) => {
        return await ProcessGameData(data);
    },
    onComplete: async (result, data, lifeTime) => {
        if (result) await ShowSuccessMessage();
    },
    onExit: async (data) => {
        await CleanupResources();
    }
);
```

### Creating Reusable Observers

```csharp
// Create reusable observer
var observer = this.CreateRecycleObserver<string>(
    onNext: message => Debug.Log(message),
    onComplete: () => Debug.Log("Completed"),
    onError: error => Debug.LogError(error.Message)
);

// Use with different Observables
observable1.Subscribe(observer).AddTo(lifeTime);
observable2.Subscribe(observer).AddTo(lifeTime);

// Automatic cleanup
observer.MakeDespawn();
```

### Conditional Operations

```csharp
// Conditional execution
observable
    .When(x => x > 0, 
          actionIfTrue: x => Debug.Log($"Positive: {x}"),
          actionIfFalse: x => Debug.Log($"Non-positive: {x}"));

// React to true/false
boolObservable
    .WhenTrue(value => Debug.Log("Value is true"))
    .WhenFalse(value => Debug.Log("Value is false"));
```

## 📋 Best Practices

1. **Always bind to a lifetime**: Prefer `this.Bind(...)` or `lifeTime.Bind(...)` over raw subscriptions in gameplay code.
2. **Use the narrowest overload**: Choose a typed overload before reaching for reflection-based binding.
3. **Thread data explicitly**: Prefer `Bind(data, source, ...)` or `BindData(...)` when a callback needs external context.
4. **Keep async callbacks cancellation-aware**: Let the provided lifetime handle `UniTask` cancellation instead of building parallel cancellation logic.
5. **Reserve reflection for dynamic cases**: `MethodInfo` binding is flexible, but typed callbacks are clearer and cheaper.

## 🎯 Performance Tips

- Prefer overloads that pass external data explicitly: `Bind(data, source, ...)` and `BindData(...)` avoid common closure captures.
- Use `static` lambdas or method groups with binding callbacks whenever possible.
- Apply `CreateRecycleObserver()` when identical observer shapes are created frequently.
- Use specialized types (`IntReactiveValue`, `BoolReactiveValue`) when hot paths benefit from clearer intent and better runtime behavior.
- Prefer `AwaitFirstAsyncNoException()` for cancellation-heavy flows where exceptions would only add noise.
- Use `SetValueForce()` only when you really need to bypass equality checks.
- Keep `Bind(source, MethodInfo)` for tooling or dynamic composition; direct typed overloads are the faster path.

## 🔗 Integration Examples

### With Unity UI

```csharp
public class ShopView : MonoBehaviour, ILifeTimeContext
{
    private ReactiveValue<int> coins = new();
    private ReactiveValue<bool> canPurchase = new();

    void Start()
    {
        // Bind to UI
        this.Bind(coins, value => coinsText.text = value.ToString());
        this.Bind(canPurchase, purchaseButton.gameObject);
        
        // Purchase logic
        purchaseButton.onClick.AsObservable()
            .Where(_ => canPurchase.Value)
            .Subscribe(_ => PurchaseItem())
            .AddTo(LifeTime);
    }
}
```

### With Game Logic

```csharp
public class GameManager : MonoBehaviour, ILifeTimeContext
{
    private MessageBroker _eventBus = new();
    private TypeData _gameData = new();

    void Start()
    {
        // Subscribe to game events
        _eventBus.Receive<PlayerLevelUpEvent>()
            .Subscribe(HandleLevelUp)
            .AddTo(LifeTime);

        // Reactive calculations
        _gameData.Receive<PlayerStats>()
            .Subscribe(stats => UpdatePlayerPower(stats))
            .AddTo(LifeTime);
    }
}
```

## 🏭 Factory & Controller Patterns

### IAsyncFactory

Factory interfaces for asynchronous object creation.

```csharp
// Simple factory
public class PlayerFactory : IAsyncFactory<Player>
{
    public async UniTask<Player> Create()
    {
        var playerData = await LoadPlayerData();
        return new Player(playerData);
    }
}

// Factory with parameters
public class WeaponFactory : IAsyncFactory<WeaponType, Weapon>
{
    public async UniTask<Weapon> Create(WeaponType weaponType)
    {
        var weaponConfig = await LoadWeaponConfig(weaponType);
        return InstantiateWeapon(weaponConfig);
    }
}

// Prototype for cloning
public class EnemyPrototype : IAsyncPrototype<Enemy>
{
    public async UniTask<Enemy> Create()
    {
        return await CloneEnemyFromTemplate();
    }
}
```

### IAsyncController

Controller with reactive initialization state.

```csharp
public class GameController : IAsyncController
{
    private LifeTime _lifeTime = new();
    private ReactiveProperty<bool> _isInitialized = new();

    public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized;
    public ILifeTime LifeTime => _lifeTime;

    public async UniTask Initialize()
    {
        if (_isInitialized.Value) return;

        await LoadGameSystems();
        await InitializeServices();
        
        _isInitialized.Value = true;
    }

    public void Dispose()
    {
        _lifeTime.Release();
        _isInitialized.Dispose();
    }
}

// Usage
var controller = new GameController();

// Subscribe to initialization state
controller.IsInitialized
    .Subscribe(isReady => {
        if (isReady) StartGame();
    })
    .AddTo(lifeTime);

await controller.Initialize();
```

## ⚙️ Task Execution & Jobs

### IUniTaskExecutor

Executor for managing asynchronous tasks.

```csharp
public class TaskExecutor : IUniTaskExecutor
{
    private CancellationTokenSource _cancellationSource = new();

    public async UniTask Execute(UniTask actionTask)
    {
        try
        {
            await actionTask.AttachExternalCancellation(_cancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Task was cancelled");
        }
    }

    public void Stop()
    {
        _cancellationSource.Cancel();
        _cancellationSource.Dispose();
        _cancellationSource = new CancellationTokenSource();
    }
}

// Usage
var executor = new TaskExecutor();

// Execute task
await executor.Execute(SomeLongRunningTask());

// Stop all tasks
executor.Stop();
```

### IUpdatableJob

Interface for updatable jobs with update type specification.

```csharp
public class HealthRegenerationJob : IUpdatableJob
{
    public PlayerLoopTiming UpdateType => PlayerLoopTiming.Update;
    
    private Player _player;
    private float _regenRate;

    public HealthRegenerationJob(Player player, float regenRate)
    {
        _player = player;
        _regenRate = regenRate;
    }

    public void Update()
    {
        if (_player.Health < _player.MaxHealth)
        {
            _player.Health += _regenRate * Time.deltaTime;
        }
    }
}

// Job management system
public class JobSystem : MonoBehaviour
{
    private List<IUpdatableJob> _updateJobs = new();
    private List<IUpdatableJob> _fixedUpdateJobs = new();
    private List<IUpdatableJob> _lateUpdateJobs = new();

    public void RegisterJob(IUpdatableJob job)
    {
        switch (job.UpdateType)
        {
            case PlayerLoopTiming.Update:
                _updateJobs.Add(job);
                break;
            case PlayerLoopTiming.FixedUpdate:
                _fixedUpdateJobs.Add(job);
                break;
            case PlayerLoopTiming.LastPostLateUpdate:
                _lateUpdateJobs.Add(job);
                break;
        }
    }

    void Update()
    {
        foreach (var job in _updateJobs)
            job.Update();
    }

    void FixedUpdate()
    {
        foreach (var job in _fixedUpdateJobs)
            job.Update();
    }

    void LateUpdate()
    {
        foreach (var job in _lateUpdateJobs)
            job.Update();
    }
}
```

## 🔌 Channels & Communication

### IObservableChannel

Channel for event publishing and subscription.

```csharp
public class EventChannel<T> : IObservableChannel<T>, IDisposable
{
    private Subject<T> _subject = new();

    public void Publish(T message)
    {
        _subject.OnNext(message);
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _subject.Subscribe(observer);
    }

    public void Dispose()
    {
        _subject.Dispose();
    }
}

// Specialized channels
public class PlayerEventChannel : EventChannel<PlayerEvent> { }
public class GameStateChannel : EventChannel<GameState> { }

// Usage
var playerEvents = new PlayerEventChannel();

// Subscription
playerEvents.Subscribe(evt => HandlePlayerEvent(evt))
    .AddTo(lifeTime);

// Publishing
playerEvents.Publish(new PlayerLevelUpEvent { NewLevel = 5 });
```

## 🎯 Context & State Management

### IAsyncContextState

States working with context.

```csharp
public class LoadGameState : IAsyncContextState<GameData>
{
    public async UniTask<GameData> ExecuteAsync(IContext context)
    {
        // Get data from context
        var saveData = context.Get<SaveData>();
        var settings = context.Get<GameSettings>();

        // Load game data
        var gameData = await LoadGameData(saveData, settings);

        // Save result to context
        context.Publish(gameData);

        return gameData;
    }
}

// Context state with status
public class InitializationState : IAsyncContextStateStatus
{
    public async UniTask<AsyncStatus> ExecuteAsync(IContext context)
    {
        try
        {
            await InitializeGame(context);
            return AsyncStatus.Succeeded;
        }
        catch
        {
            return AsyncStatus.Failed;
        }
    }
}
```

## 🔄 Reactive Status & Services

### IReactiveStatus

Interface for services with reactive readiness status.

```csharp
public class NetworkService : IReactiveStatus, IDisposable
{
    private ReactiveProperty<bool> _isReady = new();
    
    public ReadOnlyReactiveProperty<bool> IsReady => _isReady;

    public async UniTask Initialize()
    {
        _isReady.Value = false;
        
        await ConnectToServer();
        await AuthenticateUser();
        
        _isReady.Value = true;
    }

    public void Disconnect()
    {
        _isReady.Value = false;
        // Disconnect logic
    }

    public void Dispose()
    {
        _isReady.Dispose();
    }
}

// Usage
var networkService = new NetworkService();

// Subscribe to readiness
networkService.IsReady
    .Subscribe(isReady => {
        if (isReady) EnableOnlineFeatures();
        else DisableOnlineFeatures();
    })
    .AddTo(lifeTime);

await networkService.Initialize();
```

## 📝 Command Patterns

Extended commands for various usage scenarios.

### Basic Commands

```csharp
// Simple command
public class SaveGameCommand : IAsyncCommand
{
    public async UniTask ExecuteAsync()
    {
        await SaveCurrentGameState();
    }
}

// Command with result
public class LoadPlayerCommand : IAsyncCommand<PlayerData>
{
    public async UniTask<PlayerData> ExecuteAsync()
    {
        return await LoadPlayerFromFile();
    }
}

// Command with parameter and result
public class ProcessPurchaseCommand : IAsyncCommand<PurchaseRequest, PurchaseResult>
{
    public async UniTask<PurchaseResult> ExecuteAsync(PurchaseRequest request)
    {
        return await ProcessPurchase(request);
    }
}
```

### Commands with Completion

```csharp
public class ComplexOperationCommand : IAsyncCommand<OperationData, bool>, 
                                      IAsyncCompletion<bool, OperationData>
{
    public async UniTask<bool> ExecuteAsync(OperationData data)
    {
        // Main logic
        return await PerformOperation(data);
    }

    public async UniTask CompleteAsync(bool result, OperationData data, ILifeTime lifeTime)
    {
        // Actions after completion
        if (result)
        {
            await SendSuccessNotification(data);
        }
        else
        {
            await HandleOperationFailure(data);
        }
    }
}
```

## 🎯 Advanced Integration Patterns

### Service-Oriented Architecture

```csharp
public class GameServiceContainer : IContext, IDisposable
{
    private TypeData _services = new();
    private MessageBroker _eventBus = new();
    private LifeTime _lifeTime = new();

    public ILifeTime LifeTime => _lifeTime;

    // Service registration
    public void RegisterService<T>(T service) where T : class
    {
        _services.Publish(service);
        
        // Automatic initialization
        if (service is IAsyncController controller)
        {
            controller.Initialize().Forget();
        }
    }

    // Service retrieval
    public T GetService<T>() where T : class
    {
        return _services.Get<T>();
    }

    // Reactive service retrieval
    public Observable<T> ReceiveService<T>() where T : class
    {
        return _services.Receive<T>();
    }

    public void Dispose()
    {
        _services.Dispose();
        _eventBus.Dispose();
        _lifeTime.Release();
    }
}
```

### Reactive Game Loop

```csharp
public class ReactiveGameLoop : MonoBehaviour
{
    private Subject<float> _updateStream = new();
    private Subject<float> _fixedUpdateStream = new();
    private LifeTime _lifeTime = new();

    public Observable<float> UpdateStream => _updateStream;
    public Observable<float> FixedUpdateStream => _fixedUpdateStream;

    void Update()
    {
        _updateStream.OnNext(Time.deltaTime);
    }

    void FixedUpdate()
    {
        _fixedUpdateStream.OnNext(Time.fixedDeltaTime);
    }

    void OnDestroy()
    {
        _lifeTime.Release();
        _updateStream.Dispose();
        _fixedUpdateStream.Dispose();
    }

    // Subscribe to game loop
    public void SubscribeToUpdate(Action<float> action)
    {
        _updateStream.Subscribe(action).AddTo(_lifeTime);
    }
}
```

UniGame.Rx provides a powerful and flexible foundation for reactive programming in Unity, combining modern development best practices with performance and ease of use.
