# Architecture

## Overview

```
┌─────────────────────────────────────────────┐
│              Your Game Code                  │
│                                              │
│  GamesServicesManager.Auth.AuthenticateAsync()│
└──────────────────┬──────────────────────────┘
                   │
        ┌──────────▼──────────┐
        │ GamesServicesManager │  Singleton MonoBehaviour
        │  (Static Accessors)  │  DontDestroyOnLoad
        └──────────┬──────────┘
                   │
        ┌──────────▼──────────┐
        │  Provider Interfaces │  IGamesAuthProvider
        │  (6 services)        │  IGamesAchievementProvider
        │                      │  IGamesLeaderboardProvider
        └──────────┬──────────┘  IGamesCloudSaveProvider
                   │              IGamesStatsProvider
          ┌────────┴────────┐    IGamesEventsProvider
          │                 │
   ┌──────▼──────┐  ┌──────▼──────┐
   │ Android JNI │  │ Editor Mock │
   │ Controllers │  │  Providers  │
   └──────┬──────┘  └─────────────┘
          │
   ┌──────▼──────┐
   │  Java Bridge │  CloudSaveBridge.java
   │  (AAR/Source) │  AuthBridge.java
   └──────┬──────┘  AchievementBridge.java
          │
   ┌──────▼──────┐
   │ Google Play  │  play-services-games-v2:21.0.0
   │ Games SDK    │  play-services-tasks:18.4.1
   └──────────────┘
```

## Platform Abstraction

Each service has a provider interface (`I*Provider`) with two implementations:

| Platform | Implementation | Selection |
|----------|---------------|-----------|
| Android Device | `Games*Controller` | `#if UNITY_ANDROID && !UNITY_EDITOR` |
| Unity Editor | `Mock*Provider` | `#else` |

This enables full development iteration in Editor without a device build. Mock providers use configurable settings from `GamesServicesConfig.editorMock`.

## Initialization

`GamesServicesManager` initializes automatically:

1. `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` creates the singleton
2. `Awake()` calls `InitializeServices()`
3. `ResolveConfig()` loads `GamesServicesConfig` from `Resources/`
4. Each enabled service gets its provider instantiated (Android controller or Editor mock)
5. `DontDestroyOnLoad` prevents destruction on scene transitions

No manual `Initialize()` call is needed. Access any service immediately via static accessors.

## JNI Bridge Pattern

Android controllers communicate with Java through `AndroidJavaObject` and `AndroidJavaProxy`:

```
C# Controller                        Java Bridge
─────────────                         ───────────
_bridge.Call("openSnapshot", ...)  →  CloudSaveBridge.openSnapshot()
                                          │
                                          ↓
                                      Google Play SDK
                                          │
                                          ↓
CallbackProxy.onSnapshotOpened()   ←  Interface callback
        │
        ↓
TaskCompletionSource.TrySetResult()
        │
        ↓
await _openTcs.Task (C# caller)
```

### Callback Proxies

Each service has an `AndroidJavaProxy` subclass that receives Java callbacks:

- `AuthCallbackProxy` implements `IAuthCallback`
- `AchievementCallbackProxy` implements `IAchievementCallback`
- `LeaderboardCallbackProxy` implements `ILeaderboardCallback`
- `CloudSaveCallbackProxy` implements `ICloudSaveCallback`
- `StatsCallbackProxy` implements `IStatsCallback`
- `EventsCallbackProxy` implements `IEventsCallback`

Proxy method names must match Java interface methods exactly — ProGuard/R8 rules prevent stripping.

## Threading Model

Google Play Games SDK callbacks arrive on the **Android main thread** (UI thread), which is the same as Unity's main thread in normal operation. However, some scenarios require explicit thread management:

- `IncrementEventAsync` auto-dispatches to main thread if called from a background thread
- `UnityMainThreadDispatcher.Enqueue()` queues actions for next `Update()` frame
- All JNI calls (`AndroidJavaObject.Call`) must occur on the main thread — JNI crashes on background threads

## Async/Await Pattern

All operations use `TaskCompletionSource<T>` to bridge JNI callbacks to C# async/await:

```csharp
// Controller sets up TCS
_openTcs = new TaskCompletionSource<SnapshotHandle>();
_bridge.Call("openSnapshot", filename);
return await _openTcs.Task;

// Callback proxy resolves TCS
internal void OnSnapshotOpenedFromJava(string json, bool hasConflict)
{
    var handle = JsonUtility.FromJson<SnapshotHandle>(json);
    _openTcs?.TrySetResult(handle);
}
```

`CancellationToken` support: Each async method registers a cancellation callback that calls `TrySetCanceled()` on the TCS.

## Error Handling

Each service defines three error types:

| Type | Purpose |
|------|---------|
| `Games*Error` | Error data class with `errorCode`, `errorMessage`, typed `Type` property |
| `*ErrorType` | Enum mapping error codes to named constants |
| `Games*Exception` | Exception class wrapping the error |

Error flow:
1. Java bridge calls `onError(errorCode, errorMessage, ...)`
2. Callback proxy creates error object and invokes `OnError` event
3. Controller sets exception on all active TCS instances
4. Caller catches `Games*Exception` with access to typed error

Callers can use pattern matching:

```csharp
catch (GamesAuthException ex) when (ex.Error.Type == AuthErrorType.NoConnection)
{
    ShowOfflineMessage();
}
```

## Data Serialization

JNI data crosses the C#↔Java boundary as JSON strings, deserialized via `JsonUtility.FromJson<T>()`.

Requirements for serializable types:
- `[Serializable]` attribute (for `JsonUtility`)
- `[Preserve]` attribute (prevents IL2CPP code stripping on types only referenced via JNI)
- Public fields (not properties) for `JsonUtility` compatibility
- Field names must match JSON keys exactly (case-sensitive)

## Cloud Save Conflict Resolution

When two devices write to the same snapshot, Google detects a conflict:

1. `OpenSnapshotAsync` returns `SnapshotHandle` with `hasConflict = true`
2. `OnConflictDetected` event fires with both local and server data
3. Game calls `conflict.ResolveAsync(ConflictResolution.UseLocal)` or `UseServer`
4. If no resolution within 60 seconds, defaults to `UseServer`
5. Bridge calls `resolveConflict` on Java side
6. Resolved handle returned for commit

## Events Batching

Events use client-side batching to reduce JNI calls:

- `IncrementEventAsync` accumulates increments in a `Dictionary<string, int>`
- Flush triggers: 5-second interval, app pause, app quit
- `FlushPendingIncrements()` sends all accumulated values in one JNI batch call
- This reduces JNI overhead from N calls to 1 call per flush cycle

## Config Resolution

1. `Resources.Load<GamesServicesConfig>("GamesServicesConfig")` — checks all Resources folders
2. If not found, creates a default runtime instance with all services enabled
3. Config is a `ScriptableObject` — editable in Inspector with headers, tooltips, ranges

## ProGuard / R8

Package includes `proguard-rules.pro` with `-keepclassmembers` for all Java callback interfaces. This prevents method name stripping that would break JNI callback routing.

The Sidekick Readiness validator scans the ProGuard file for all 6 callback interfaces at Editor time.
