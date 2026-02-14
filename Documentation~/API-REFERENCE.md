# API Reference

All types are in the `BizSim.GPlay.Games` namespace.

---

## GamesServicesManager

Singleton `MonoBehaviour` — auto-creates via `[RuntimeInitializeOnLoadMethod]`.

### Static Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `GamesServicesManager` | Singleton instance |
| `Config` | `GamesServicesConfig` | Active configuration |
| `SidekickStatus` | `SidekickTier` | Current Sidekick compliance tier |
| `Auth` | `IGamesAuthProvider` | Authentication service |
| `Achievements` | `IGamesAchievementProvider` | Achievements service |
| `Leaderboards` | `IGamesLeaderboardProvider` | Leaderboards service |
| `CloudSave` | `IGamesCloudSaveProvider` | Cloud save service |
| `Stats` | `IGamesStatsProvider` | Player stats service |
| `Events` | `IGamesEventsProvider` | Events tracking service |

### Instance Properties (DI-compatible)

For dependency injection frameworks (Zenject, VContainer):

| Property | Type |
|----------|------|
| `AuthProvider` | `IGamesAuthProvider` |
| `AchievementsProvider` | `IGamesAchievementProvider` |
| `LeaderboardsProvider` | `IGamesLeaderboardProvider` |
| `CloudSaveProvider` | `IGamesCloudSaveProvider` |
| `StatsProvider` | `IGamesStatsProvider` |
| `EventsProvider` | `IGamesEventsProvider` |

---

## Authentication — IGamesAuthProvider

### Methods

#### AuthenticateAsync

```csharp
Task<GamesPlayer> AuthenticateAsync(CancellationToken ct = default)
```

Authenticates the player. First call attempts silent sign-in. Subsequent calls show sign-in UI.

**Returns**: `GamesPlayer` with profile data.
**Throws**: `GamesAuthException` on failure.

#### RequestServerSideAccessAsync

```csharp
Task<string> RequestServerSideAccessAsync(
    string serverClientId,
    bool forceRefresh = false,
    CancellationToken ct = default)
```

Requests an OAuth2 server-side access token for backend integration.

**Parameters**:
- `serverClientId` — OAuth 2.0 web client ID from Google Cloud Console
- `forceRefresh` — Force new token even if cached

**Returns**: Server auth code string.

#### RequestServerSideAccessWithScopesAsync

```csharp
Task<GamesAuthResponse> RequestServerSideAccessWithScopesAsync(
    string serverClientId,
    bool forceRefresh,
    List<GamesAuthScope> scopes,
    CancellationToken ct = default)
```

Requests server-side access with additional OAuth scopes.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsAuthenticated` | `bool` | Whether player is currently signed in |
| `CurrentPlayer` | `GamesPlayer` | Current player data (null if not signed in) |

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAuthenticationSuccess` | `Action<GamesPlayer>` | Fired on successful auth |
| `OnAuthenticationFailed` | `Action<GamesAuthError>` | Fired on auth failure |

### Data Types

#### GamesPlayer

| Field | Type | Description |
|-------|------|-------------|
| `PlayerId` | `string` | Unique stable player ID |
| `DisplayName` | `string` | Player display name |
| `BannerImageUri` | `string` | Banner image URI (nullable) |
| `HiResImageUri` | `string` | Avatar image URI (nullable) |

#### GamesAuthError

| Field | Type | Description |
|-------|------|-------------|
| `errorCode` | `int` | Raw error code |
| `errorMessage` | `string` | Human-readable message |
| `isRetryable` | `bool` | Whether retry may succeed |
| `Type` | `AuthErrorType` | Typed error classification |

#### AuthErrorType

| Value | Code | Description |
|-------|------|-------------|
| `UserCancelled` | 1 | User dismissed sign-in dialog |
| `NoConnection` | 2 | Network error |
| `SignInRequired` | 3 | Must show sign-in button |
| `SignInFailed` | 4 | Google Play Services error |
| `Timeout` | -1 | Operation timed out |
| `Unknown` | 0 | Unclassified error |

---

## Achievements — IGamesAchievementProvider

### Methods

#### UnlockAchievementAsync

```csharp
Task UnlockAchievementAsync(string achievementId, CancellationToken ct = default)
```

Unlocks an achievement immediately.

#### IncrementAchievementAsync

```csharp
Task IncrementAchievementAsync(string achievementId, int steps, CancellationToken ct = default)
```

Increments an incremental achievement. Achievement auto-unlocks when `currentSteps >= totalSteps`.

#### RevealAchievementAsync

```csharp
Task RevealAchievementAsync(string achievementId, CancellationToken ct = default)
```

Reveals a hidden achievement (makes it visible to the player).

#### ShowAchievementsUIAsync

```csharp
Task ShowAchievementsUIAsync(CancellationToken ct = default)
```

Shows the native Google Play Games achievements overlay.

#### LoadAchievementsAsync

```csharp
Task<List<GamesAchievement>> LoadAchievementsAsync(bool forceReload = false, CancellationToken ct = default)
```

Loads all achievements. Results are cached locally (24h TTL). Pass `forceReload = true` to bypass cache.

#### UnlockMultipleAsync

```csharp
Task UnlockMultipleAsync(List<string> achievementIds, CancellationToken ct = default)
```

Batch unlocks multiple achievements in one operation.

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnAchievementUnlocked` | `Action<string>` | Achievement ID unlocked |
| `OnAchievementIncremented` | `Action<string, int>` | Achievement ID, new step count |
| `OnAchievementRevealed` | `Action<string>` | Achievement ID revealed |
| `OnAchievementError` | `Action<GamesAchievementError>` | Operation failed |

### Data Types

#### GamesAchievement

| Field | Type | Description |
|-------|------|-------------|
| `achievementId` | `string` | Achievement ID from games-ids.xml |
| `name` | `string` | Localized name |
| `description` | `string` | Localized description |
| `state` | `AchievementState` | Hidden / Revealed / Unlocked |
| `type` | `AchievementType` | Standard / Incremental |
| `currentSteps` | `int` | Current progress (incremental only) |
| `totalSteps` | `int` | Total steps required (incremental only) |
| `xpValue` | `int` | XP awarded on unlock |
| `unlockedTimestamp` | `long` | Unix ms when unlocked (0 if not) |
| `revealedIconUrl` | `string` | Icon URL (revealed state) |
| `unlockedIconUrl` | `string` | Icon URL (unlocked state) |

**Computed properties**: `IsUnlocked`, `IsRevealed`, `ProgressPercentage`

---

## Leaderboards — IGamesLeaderboardProvider

### Methods

#### SubmitScoreAsync

```csharp
Task SubmitScoreAsync(
    string leaderboardId,
    long score,
    string scoreTag = null,
    CancellationToken ct = default)
```

Submits a score to a leaderboard.

**Parameters**:
- `leaderboardId` — Leaderboard ID from games-ids.xml
- `score` — Score value (long)
- `scoreTag` — Optional metadata string (max 64 chars)

#### ShowLeaderboardUIAsync

```csharp
Task ShowLeaderboardUIAsync(string leaderboardId, CancellationToken ct = default)
```

Shows the native leaderboard UI for a specific leaderboard.

#### ShowAllLeaderboardsUIAsync

```csharp
Task ShowAllLeaderboardsUIAsync(CancellationToken ct = default)
```

Shows the native UI with all leaderboards.

#### LoadTopScoresAsync

```csharp
Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(
    string leaderboardId,
    LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
    LeaderboardCollection collection = LeaderboardCollection.Public,
    int maxResults = 25,
    CancellationToken ct = default)
```

Loads top scores from a leaderboard. Max 25 results per call.

#### LoadPlayerCenteredScoresAsync

```csharp
Task<List<GamesLeaderboardEntry>> LoadPlayerCenteredScoresAsync(
    string leaderboardId,
    LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
    LeaderboardCollection collection = LeaderboardCollection.Public,
    int maxResults = 25,
    CancellationToken ct = default)
```

Loads scores centered around the current player's rank.

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnScoreSubmitted` | `Action<string, long>` | Leaderboard ID, score |
| `OnScoresLoaded` | `Action<string, List<GamesLeaderboardEntry>>` | Leaderboard ID, entries |
| `OnLeaderboardError` | `Action<GamesLeaderboardError>` | Operation failed |

### Enums

#### LeaderboardTimeSpan

| Value | Description |
|-------|-------------|
| `Daily` | Today's scores only |
| `Weekly` | This week's scores |
| `AllTime` | All-time scores |

#### LeaderboardCollection

| Value | Description |
|-------|-------------|
| `Public` | All players |
| `Friends` | Google Play friends only |

### Data Types

#### GamesLeaderboardEntry

| Field | Type | Description |
|-------|------|-------------|
| `playerId` | `string` | Player ID |
| `displayName` | `string` | Player display name |
| `score` | `long` | Raw score value |
| `formattedScore` | `string` | Formatted score string |
| `rank` | `long` | Player rank |
| `scoreTag` | `string` | Score metadata tag |
| `timestampMillis` | `long` | Submission timestamp |
| `avatarUrl` | `string` | Player avatar URL |

---

## Cloud Save — IGamesCloudSaveProvider

### Transaction API

Cloud save uses a transaction pattern: **Open → Read/Write → Commit**.

#### OpenSnapshotAsync

```csharp
Task<SnapshotHandle> OpenSnapshotAsync(
    string filename,
    bool createIfNotFound = true,
    CancellationToken ct = default)
```

Opens a snapshot for reading or writing. Returns a handle for subsequent operations.

#### ReadSnapshotAsync

```csharp
Task<byte[]> ReadSnapshotAsync(SnapshotHandle handle, CancellationToken ct = default)
```

Reads data from an open snapshot.

#### CommitSnapshotAsync

```csharp
Task CommitSnapshotAsync(
    SnapshotHandle handle,
    byte[] data,
    string description = null,
    long playedTimeMillis = 0,
    byte[] coverImage = null,
    CancellationToken ct = default)
```

Commits data to cloud storage.

**Google Play Quality Requirement**: `description`, `playedTimeMillis`, and `coverImage` are mandatory for published games (Quality Checklist 6.1).

#### DeleteSnapshotAsync

```csharp
Task DeleteSnapshotAsync(string filename, CancellationToken ct = default)
```

Deletes a snapshot from cloud storage.

#### ShowSavedGamesUIAsync

```csharp
Task<string> ShowSavedGamesUIAsync(
    string title = "Saved Games",
    bool allowAddButton = false,
    bool allowDelete = true,
    int maxSnapshots = 5,
    CancellationToken ct = default)
```

Shows the native saved games UI. Returns selected filename or null if cancelled.

### Convenience Methods

#### SaveAsync (simple)

```csharp
Task SaveAsync(string filename, byte[] data, string description = null, CancellationToken ct = default)
```

One-call save: Open → Write → Commit. Handles conflicts automatically.

**Warning**: Does not include metadata. For Sidekick Tier 2 compliance, use the metadata overload.

#### SaveAsync (with metadata)

```csharp
Task SaveAsync(string filename, byte[] data, SaveGameMetadata metadata, CancellationToken ct = default)
```

One-call save with full metadata. Validates against Sidekick requirements when `requireCloudSaveMetadata` is enabled.

#### LoadAsync

```csharp
Task<byte[]> LoadAsync(string filename, CancellationToken ct = default)
```

One-call load: Open → Read. Returns null if snapshot does not exist.

#### DownloadCoverImageAsync

```csharp
Task<Texture2D> DownloadCoverImageAsync(string coverImageUri, CancellationToken ct = default)
```

Downloads a cover image from Google servers. Results are cached in memory.

**WARNING**: The returned `Texture2D` uses unmanaged GPU memory. Use `ReleaseCoverImage()` or `Object.Destroy(texture)` when done, or GPU memory will leak.

#### ReleaseCoverImage

```csharp
void ReleaseCoverImage(Texture2D texture)
```

Releases a cover image texture from internal cache and destroys the GPU resource. Call this when a cover image is no longer displayed.

#### ReleaseAllCoverImages

```csharp
void ReleaseAllCoverImages()
```

Releases all cached cover image textures. Call this when leaving the saved games list UI.

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnSnapshotOpened` | `Action<SnapshotHandle>` | Snapshot opened successfully |
| `OnSnapshotCommitted` | `Action<string>` | Filename committed |
| `OnConflictDetected` | `Action<SavedGameConflict>` | Conflict needs resolution |
| `OnCloudSaveError` | `Action<GamesCloudSaveError>` | Operation failed |

### Data Types

#### SnapshotHandle

| Field | Type | Description |
|-------|------|-------------|
| `filename` | `string` | Snapshot filename |
| `hasConflict` | `bool` | Whether conflict exists |
| `lastModifiedTimestamp` | `long` | Last modified (Unix ms) |
| `playedTimeMillis` | `long` | Total played time |
| `description` | `string` | Save description |
| `coverImageUri` | `string` | Cover image URI (read-back) |

#### SaveGameMetadata

| Field | Type | Description |
|-------|------|-------------|
| `description` | `string` | Save description text |
| `playedTimeMillis` | `long` | Total played time in ms |
| `coverImage` | `byte[]` | PNG image (max 800KB, 640x360) |
| `progressValue` | `long` | Progress percentage (0-100) |

#### SavedGameConflict

| Field | Type | Description |
|-------|------|-------------|
| `localSnapshot` | `SnapshotHandle` | Local device snapshot |
| `serverSnapshot` | `SnapshotHandle` | Cloud server snapshot |
| `localData` | `byte[]` | Local save data |
| `serverData` | `byte[]` | Server save data |
| `ResolveAsync` | `Func<ConflictResolution, Task>` | Resolution callback |

#### ConflictResolution

| Value | Description |
|-------|-------------|
| `UseLocal` | Keep local data, discard cloud |
| `UseServer` | Keep cloud data, discard local |
| `UseManual` | Custom merge (not implemented, falls back to UseLocal) |

#### CloudSaveErrorType

| Value | Code | Description |
|-------|------|-------------|
| `Unknown` | 0 | Unclassified error |
| `ApiNotAvailable` | -1 | PGS API not available on device |
| `UserNotAuthenticated` | 1 | User not signed in |
| `NetworkError` | 2 | Device offline |
| `SnapshotNotFound` | 3 | Filename does not exist |
| `ConflictTimeout` | 4 | Conflict resolution timed out |
| `DataTooLarge` | 5 | Data exceeds 3MB limit |
| `InternalError` | 100 | Internal SDK error |

---

## Events — IGamesEventsProvider

### Methods

#### IncrementEventAsync

```csharp
Task IncrementEventAsync(string eventId, int steps = 1, CancellationToken ct = default)
```

Increments an event counter. Uses client-side batching with 5-second flush interval. Auto-dispatches to main thread if called from background thread.

Events flush automatically on app pause and quit.

#### LoadEventsAsync

```csharp
Task<GamesEvent[]> LoadEventsAsync(CancellationToken ct = default)
```

Loads all events defined in Google Play Console.

#### LoadEventAsync

```csharp
Task<GamesEvent> LoadEventAsync(string eventId, CancellationToken ct = default)
```

Loads a single event by ID.

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnEventsError` | `Action<GamesEventsError>` | Event operation failed |

### Data Types

#### GamesEvent

| Field | Type | Description |
|-------|------|-------------|
| `eventId` | `string` | Event ID from Play Console |
| `name` | `string` | Event name |
| `description` | `string` | Event description |
| `value` | `long` | Cumulative event count |
| `imageUri` | `string` | Event icon URI |
| `isVisible` | `bool` | Whether event is visible to player |

#### EventsErrorType

| Value | Code | Description |
|-------|------|-------------|
| `Unknown` | 0 | Unclassified error |
| `ApiNotAvailable` | -1 | PGS API not available |
| `UserNotAuthenticated` | 1 | User not signed in |
| `NetworkError` | 2 | Device offline |
| `EventNotFound` | 3 | Event ID not in Play Console |
| `InternalError` | 100 | Internal SDK error |

---

## Player Stats — IGamesStatsProvider

### Methods

#### LoadPlayerStatsAsync

```csharp
Task<GamesPlayerStats> LoadPlayerStatsAsync(bool forceReload = false, CancellationToken ct = default)
```

Loads Google-computed player engagement metrics.

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnStatsLoaded` | `Action<GamesPlayerStats>` | Stats loaded successfully |
| `OnStatsError` | `Action<GamesStatsError>` | Stats operation failed |

### Data Types

#### GamesPlayerStats

| Field | Type | Description |
|-------|------|-------------|
| `avgSessionLengthMinutes` | `float` | Average session duration |
| `daysSinceLastPlayed` | `int` | Days since last session |
| `numberOfPurchases` | `int` | Total IAP purchases |
| `numberOfSessions` | `int` | Total session count |
| `sessionPercentile` | `float` | Session count percentile (0-1) |
| `spendPercentile` | `float` | Spend percentile (0-1) |
| `churnProbability` | `float` | Churn risk prediction (0-1) |
| `highSpenderProbability` | `float` | High spender prediction (0-1) |

#### StatsErrorType

| Value | Code | Description |
|-------|------|-------------|
| `Unknown` | 0 | Unclassified error |
| `ApiNotAvailable` | -1 | PGS API not available |
| `UserNotAuthenticated` | 1 | User not signed in |
| `NetworkError` | 2 | Device offline |
| `InternalError` | 100 | Internal SDK error |

---

## Leaderboard Error Types

#### LeaderboardErrorType

| Value | Code | Description |
|-------|------|-------------|
| `Unknown` | 0 | Unclassified error |
| `ApiNotAvailable` | -1 | PGS API not available |
| `UserNotAuthenticated` | 1 | User not signed in |
| `NetworkError` | 2 | Device offline |
| `LeaderboardNotFound` | 3 | ID not in Play Console |
| `InternalError` | 100 | Internal SDK error |
