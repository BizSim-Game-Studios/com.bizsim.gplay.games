# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.15.5] - 2026-03-14

### Fixed
- **AGP 8+ namespace error** — setup wizard now generates `AndroidManifest.xml` and `project.properties` in `GooglePlayGamesManifest.androidlib/`. Without these files, AGP 8+ fails with "Namespace not specified" during Gradle build.
- **GPGSIds output path** — changed `GPGS_IDS_PATH` from `"Assets/GPGSIds.cs"` to `"Assets/Scripts/Config/GPGSIds.cs"` to match project convention; added `Directory.CreateDirectory()` guard before file write to prevent errors when target folder doesn't exist

---

## [1.15.1] - 2026-02-19

### Fixed
- **Fatal crash in CloudSaveBridge.showSavedGamesUI** — `getSelectSnapshotIntent()` can return null Intent on some devices/GMS versions (especially when no snapshots exist); `savedGamesLauncher.launch(null)` triggers NullPointerException in Kotlin-compiled `ActivityResultContracts.StartActivityForResult`. Added null check; returns null result via `onSavedGamesUIResult` (same as user cancellation) instead of crashing.

---

## [1.15.0] - 2026-02-18

### Changed
- **Gradle modernization** — `compileSdk 36`, `targetSdk 36`, modern DSL (`compileSdkVersion` → `compileSdk`), removed `versionCode`/`versionName` from library module
- **LeaderboardBridge** — complete rewrite with `ActivityResultLauncher`; `onLeaderboardUIClosed()` now fires when UI actually closes, not when it opens; added `shutdown()` for launcher cleanup
- **AchievementBridge** — migrated `showAchievementsUI()` from `startActivityForResult` to `ActivityResultLauncher`; fixed `onAchievementsUIClosed()` callback timing; added `addOnFailureListener` to `loadAchievementSteps`; added `shutdown()`
- **CloudSaveBridge** — `getParcelableExtra` now uses typed API 33+ overload with `Build.VERSION.SDK_INT` check; I/O executor thread now daemon (`setDaemon(true)`) to prevent zombie processes
- Removed redundant `package` attribute from `AndroidManifest.xml` (AGP 8+ uses `namespace` in build.gradle)
- Changed `androidx.activity:activity` from `compileOnly 1.8.0` to `implementation 1.12.4` (fixes Gradle consistent resolution conflict with PGS v2 transitive deps)
- Removed explicit `play-services-tasks` dependency (transitive via PGS v2)
- Removed all Javadoc comments and inline comments across all bridge classes

---

## [1.14.2] - 2026-02-18

### Fixed
- **Cloud save UI result never received** — replaced deprecated `startActivityForResult` with `ActivityResultLauncher` in `CloudSaveBridge`; Unity 6 GameActivity does not forward `onActivityResult` to custom Java code, causing `ShowSavedGamesUIAsync` to hang indefinitely

### Changed
- `CloudSaveBridge` constructor now registers an `ActivityResultLauncher` via `ComponentActivity.getActivityResultRegistry()`
- `showSavedGamesUI()` uses `savedGamesLauncher.launch(intent)` instead of `activity.startActivityForResult(intent, RC_SAVED_GAMES)`
- Removed static `pendingUICallback` field and static `handleActivityResult()` method — replaced with instance-bound `handleSavedGamesResult(ActivityResult)` callback
- `shutdown()` now calls `savedGamesLauncher.unregister()` for cleanup
- Added `compileOnly 'androidx.activity:activity:1.8.0'` dependency (already in runtime via Unity 6 GameActivity)

---

## [1.14.1] - 2026-02-16

### Fixed
- **Cloud save ANR on conflict detection** — moved blocking I/O (`readFully`, `writeBytes`, `decodeCoverImageSafe`) from Android main thread to single-threaded `ioExecutor` in `CloudSaveBridge`; JNI callbacks posted back to main thread via `postToMainThread` for AndroidJavaProxy safety
- **Cloud save ANR on snapshot read** — `readSnapshot()` `readFully` call offloaded to background thread
- **Cloud save ANR on snapshot commit** — `commitSnapshot()` `writeBytes` and bitmap decode offloaded to background thread

### Added
- `CloudSaveBridge.shutdown()` method — cleanly terminates the I/O executor thread; called from `GamesCloudSaveController.OnDispose()`
- `CloudSaveBridge.postToMainThread()` helper — guards `runOnUiThread` against destroyed Activity state (`isFinishing`/`isDestroyed` check)
- `lastConflict` field made `volatile` for cross-thread visibility safety

---

## [1.14.0] - 2026-02-14

### Fixed
- **Achievement concurrent operation race condition** — replaced shared `TaskCompletionSource` fields with per-achievementId dictionaries for `IncrementAchievementAsync`, `UnlockAchievementAsync`, and `RevealAchievementAsync`; concurrent calls for different achievement IDs no longer cancel each other via `TcsGuard.Replace`, eliminating spurious "A task was canceled" errors

---

## [1.13.0] - 2026-02-13

### Fixed
- Added `PlayGamesSdk.initialize(activity)` call in `AuthBridge` constructor before `PlayGames.getGamesSignInClient()` — PGS v2 requires explicit SDK initialization, missing call caused `DEVELOPER_ERROR` on all Play Games operations

---

## [1.12.0] - 2026-02-13

### Added
- `GamesServicesConfig.webClientId` field — stores Web Application OAuth 2.0 Client ID for `RequestServerSideAccessAsync`, eliminating hardcoded client IDs in game code
- Setup Wizard now persists `webClientId` to `GamesServicesConfig` asset on completion
- Setup Wizard loads existing `webClientId` from config when re-opened

---

## [1.11.0] - 2026-02-13

### Added
- **`GamesException` base class** — all 7 package exception types now inherit from `GamesException` instead of `System.Exception`; enables `catch (GamesException)` for blanket error handling across all services
- `GamesException.ErrorCode` property — shared integer error code accessible from any package exception without casting

### Changed
- `GamesAuthException`, `GamesAchievementException`, `GamesLeaderboardException`, `GamesCloudSaveException`, `GamesStatsException`, `GamesEventsException` — now inherit from `GamesException` (backward-compatible; existing `catch` blocks still work)
- `GamesNativeBridgeException` — inherits from `GamesException` with `ErrorCode = GamesErrorCodes.BridgeNotInitialized`
- `UnityMainThreadDispatcher` — replaced `lock` + `Queue<Action>` with `ConcurrentQueue<Action>` for lock-free thread safety; removed `_lock` field
- `JsonArrayParser.Parse` — added null/empty/`[]`/`{}` guard clause to prevent `JsonUtility` parse errors from malformed JNI data
- ProGuard rules — added `-assumenosideeffects` to strip `Log.v()` and `Log.d()` calls in consumer release builds (R8); `Log.i`/`Log.w`/`Log.e` preserved

### Removed
- Unused `using System.Linq` from `MockLeaderboardProvider`

---

## [1.10.0] - 2026-02-13

### Added
- **`JniConstants` static class** — centralizes all 13 Java class/interface name strings (bridge + callback); eliminates Shotgun Surgery risk when Java packages change
- **`GamesNativeBridgeException`** — specific exception type for JNI bridge initialization failures, wraps inner Java exception with `JavaClassName` property
- `GamesServicesConfig.jniTimeoutSeconds` — configurable JNI operation timeout (5-120 seconds, default 30) via ScriptableObject Inspector; replaces hardcoded 30-second constant

### Changed
- All 5 JNI controllers (`Achievement`, `Leaderboard`, `CloudSave`, `Stats`, `Events`) — `JavaClassName` override now uses `JniConstants.*Bridge` instead of hardcoded strings
- All 6 callback proxies (`Auth`, `Achievement`, `Leaderboard`, `CloudSave`, `Stats`, `Events`) — `base(...)` constructor uses `JniConstants.*Callback` instead of hardcoded strings
- `GamesAuthController` — uses `JniConstants.AuthBridge` and `JniConstants.UnityPlayer`
- `JniBridgeBase` — uses `JniConstants.UnityPlayer`; throws `GamesNativeBridgeException` instead of rethrowing generic `Exception`; logs full stack trace instead of `ex.Message`
- `JniTaskExtensions` — reads timeout from `GamesServicesManager.Config.jniTimeoutSeconds` with 30-second fallback

---

## [1.9.0] - 2026-02-13

### Changed
- `UnityMainThreadDispatcher` — exception logging now includes full stack trace (`{e}` instead of `{e.Message}`)
- `MockEventsProvider.IncrementEventAsync` — added `await Task.Delay(50, ct)` for consistent async behavior with other mock providers

### Fixed
- `MockCloudSaveProvider.OpenSnapshotAsync` — now throws `GamesCloudSaveException` when `createIfNotFound=false` and snapshot doesn't exist (matches real controller behavior)
- `MockCloudSaveProvider.LoadAsync` — correctly returns null for missing snapshots instead of throwing

---

## [1.8.0] - 2026-02-13

### Added
- **`JniBridgeBase` abstract class** — encapsulates JNI initialization (get Activity → create bridge → set callback → IDisposable) shared by 5 controllers; reduces ~25 lines per controller
- **`GamesErrorCodes` static class** — centralized error code constants (`BridgeNotInitialized=-100`, `ApiNotAvailable=-1`, `Unknown=0`, `UserCancelled=1`, `NetworkError=2`, `NotAuthenticated=3`, `InternalError=100`)
- **`JsonArrayParser`** — shared `Parse<TWrapper, T>()` helper for `JsonUtility` array deserialization via `{"items":...}` wrapping pattern

### Changed
- `GamesAchievementController`, `GamesLeaderboardController`, `GamesCloudSaveController`, `GamesStatsController`, `GamesEventsController` — now inherit from `JniBridgeBase` instead of managing JNI bridge directly
- Mock providers — replaced magic number error codes with `GamesErrorCodes` constants

---

## [1.7.0] - 2026-02-13

### Added
- **`TcsGuard`** — concurrent `TaskCompletionSource` protection; `Replace(ref field)` cancels previous pending TCS before creating new one, preventing silent overwrites
- **`JniTaskExtensions`** — `WithJniTimeout()` extension method for TCS-based async operations; prevents indefinite hangs when Java callbacks never arrive (30-second default)

### Fixed
- **Thread safety** — singleton instance fields (`GamesServicesManager._instance`, `UnityMainThreadDispatcher._instance`) now marked `volatile` for correct double-checked locking under C# memory model
- **Memory leaks** — all 6 controllers implement `IDisposable`; `GamesServicesManager.OnDestroy()` calls `Dispose()` on all providers, releasing JNI bridges and cancelling pending operations
- **TCS overwrites** — all controller async methods now use `TcsGuard.Replace()` instead of raw `new TaskCompletionSource<T>()`
- **Achievement cache** — `IsAchievementUnlockedInCache()` now checks memory dict before PlayerPrefs (was reversed)
- **Mock provider gaps** — `MockLeaderboardProvider.SubmitScoreAsync` now persists scores with best-score semantics and rank recalculation; `MockAchievementProvider` unlock/increment/reveal correctly throws for unknown achievement IDs

---

## [1.6.0] - 2026-02-13

### Added
- **Typed Exceptions** — service-specific exception classes for granular error handling:
  - `GamesCloudSaveException` with `GamesCloudSaveError` (wraps `CloudSaveErrorType`)
  - `GamesAchievementException` with `GamesAchievementError` (wraps `AchievementErrorType`)
  - `GamesLeaderboardException` with `GamesLeaderboardError` (wraps `LeaderboardErrorType`)
  - `GamesStatsException` with `GamesStatsError` (wraps `StatsErrorType`)
  - `GamesEventsException` with `GamesEventsError` (event-specific error info)
- **UniTask Optional Support** — `BizSim.GPlay.Games.UniTask` assembly with extension methods for all provider interfaces; compiled only when `com.cysharp.unitask` package is installed (via `UNITASK_AVAILABLE` define constraint)
- `GamesEventsError.ToString()` — human-readable error formatting

### Fixed
- **Saved Games UI** — `showSavedGamesUI` no longer fires `onSavedGamesUIResult(null)` immediately on Intent launch; static `CloudSaveBridge.handleActivityResult()` method properly captures user selection via `onActivityResult` with `EXTRA_SNAPSHOT_METADATA` / `EXTRA_SNAPSHOT_NEW` handling

### Changed
- All controllers now throw typed exceptions instead of generic `Exception`:
  - `GamesCloudSaveController` → `GamesCloudSaveException`
  - `GamesAchievementController` → `GamesAchievementException`
  - `GamesLeaderboardController` → `GamesLeaderboardException`
  - `GamesStatsController` → `GamesStatsException`
  - `GamesEventsController` → `GamesEventsException`
  - `MockAchievementProvider` → `GamesAchievementException`

---

## [1.5.0] - 2026-02-13

### Added
- **SaveGameMetadata model** — structured metadata class with description, playedTimeMillis, coverImage (PNG, max 800KB, 640x360 recommended), and progressValue (0-100 percentage for Sidekick display)
- `SaveAsync(filename, data, metadata)` overload — convenience method with full metadata support and runtime validation
- `ValidateMetadata()` — runtime enforcement when `requireCloudSaveMetadata` is true: Warning for missing description/playedTime/coverImage, Error for coverImage > 800KB
- `DownloadCoverImageAsync(coverImageUri)` — downloads cover image from Google Cloud URI to `Texture2D`; mock returns 1x1 placeholder
- Metadata validation in `CommitSnapshotAsync` — warns when `requireCloudSaveMetadata` is enabled but description or playedTime is missing
- **Sidekick Readiness Validator** — Editor window at BizSim > Google Play Games > Sidekick Readiness Check with 10-check pass/fail checklist and remediation guidance
- Documentation: `SIDEKICK-GUIDE.md` — comprehensive Sidekick integration guide with cover image write/read/display patterns
- Documentation: `QUALITY-CHECKLIST.md` — full PGS Quality Checklist with achievement, cloud save, leaderboard, and events best practices
- Documentation: `LEVEL-UP-PROGRAM.md` — Level Up program overview, tier deadlines, and migration guide from v1.0.1

### Changed
- `IGamesCloudSaveProvider` — added `SaveAsync` metadata overload and `DownloadCoverImageAsync` method
- `MockCloudSaveProvider` — supports metadata overload and placeholder cover image download

---

## [1.4.0] - 2026-02-13

### Added
- **Events API** — full implementation of Google Play Games Events with batching support
- `IGamesEventsProvider` interface — `IncrementEventAsync`, `LoadEventsAsync`, `LoadEventAsync`
- `GamesEventsController` — JNI bridge with Dictionary-based batching (5-second flush interval), swap-and-clear pattern for thread safety
- `GamesEvent` model — serializable event data (eventId, name, description, value, imageUri, isVisible)
- `GamesEventsError` model — event-specific error reporting
- `EventsCallbackProxy` — AndroidJavaProxy for Java `IEventsCallback` interface
- `MockEventsProvider` — in-memory event counter for Editor testing
- Java `EventsBridge` — Android bridge for `PlayGames.getEventsClient()` with increment, load all, and load single event support
- `GamesServicesManager.Events` static accessor for events provider
- `OnApplicationPause`/`OnApplicationQuit` lifecycle hooks — flush pending event increments on app pause and quit to prevent data loss
- `UnityMainThreadDispatcher.IsMainThread` property — thread safety check for JNI-sensitive operations

### Changed
- `GamesServicesManager.InitializeServices()` now conditionally initializes events provider based on `enableEvents` config toggle
- `GamesServicesManager.ResolveConfig()` simplified — removed legacy `GamesServicesMockConfig` fallback (was deprecated in v1.3.0)

### Removed
- `GamesServicesMockConfig` — deleted entirely (deprecated since v1.3.0). Use `GamesServicesConfig` instead.
- `GamesServicesMockConfigEditor` — custom inspector for removed config class
- Legacy 3-tier config resolution — `CreateConfigFromLegacy()` bridge method removed

---

## [1.3.0] - 2026-02-13

### Added
- **Unified Configuration System** — `GamesServicesConfig` ScriptableObject replaces dual-config confusion with single asset containing service toggles, Sidekick settings, quality checklist options, and editor mock data
- `SidekickReadiness` static evaluator — `SidekickReadiness.Evaluate(config)` returns `SidekickTier` (None, Tier1, Tier2) based on enabled services and metadata requirements
- `GamesServicesManager.Config` static accessor — provides current config to all services
- `GamesServicesManager.SidekickStatus` static accessor — returns current Sidekick tier
- **3-tier config resolution** — (1) new `GamesServicesConfig` from Resources, (2) legacy `DefaultGamesConfig` auto-migration, (3) hardcoded defaults with all services enabled
- Domain Reload safety — runtime-created ScriptableObjects use `HideFlags.HideAndDontSave`

### Changed
- Mock providers now accept `GamesServicesConfig.MockSettings` instead of `GamesServicesMockConfig`
- `GamesServicesManager.InitializeServices()` uses conditional service initialization based on config toggles
- Service provider fields are only initialized when their corresponding `enable*` toggle is true

### Deprecated
- `GamesServicesMockConfig` — marked `[Obsolete]`, will be removed in v1.4.0. Use `GamesServicesConfig` instead.

---

## [1.2.1] - 2026-02-13

### Fixed
- **SimpleJson parser replaced with JsonUtility** — custom JSON parser split on `","` which broke when achievement descriptions contained commas; now uses `JsonUtility.FromJson` with wrapper array pattern for reliable parsing
- **Cloud save conflict race condition** — extracted `HandleConflictWithTimeout` method with guaranteed safe ordering: wait for resolution → renew `_openTcs` → call Java → await new handle; prevents stale handle usage and TCS race condition after conflict resolution
- **Cover image forwarding to Java bridge** — `CommitSnapshotAsync` now passes `coverImage` byte array to Java `commitSnapshot`; Java decodes via `BitmapFactory.decodeByteArray` with `OutOfMemoryError` catch, sets `setCoverImage` on `SnapshotMetadataChange.Builder`
- **Cover image URI read-back** — `serializeSnapshot` in Java now includes `coverImageUri` from `SnapshotMetadata.getCoverImageUri()`; `SnapshotHandle.coverImageUri` field added for C# access

### Removed
- `SimpleJson` internal JSON parser class (replaced by `JsonUtility`)
- `DictionaryExtensions` helper class (no longer needed)

---

## [1.2.0] - 2026-02-13

### Added
- **Auth Scopes API**: New `RequestServerSideAccessWithScopesAsync()` method for requesting additional OAuth scopes (EMAIL, PROFILE, OPEN_ID) via PGS v2 21.0.0's `requestServerSideAccess(clientId, forceRefresh, List<AuthScope>)` overload
- `GamesAuthScope` enum — maps to Java `AuthScope.EMAIL`, `AuthScope.PROFILE`, `AuthScope.OPEN_ID`
- `GamesAuthResponse` model — wraps `AuthCode`, `GrantedScopes`, and optional `IdTokenClaims` from the scoped auth flow
- `GamesIdTokenClaims` model — represents decoded JWT ID Token payload (sub, email, emailVerified, name, givenName, familyName, picture, locale) matching Google's `GoogleIdToken.Payload` structure
- Java `AuthBridge.requestServerSideAccessWithScopes()` — parses scope JSON, calls PGS v2 scoped overload, returns granted scopes
- `IAuthCallback.onServerSideAccessWithScopesSuccess/Failure` — new JNI callback methods for scoped auth flow
- Mock provider support — scope-dependent claims simulation: EMAIL scope populates email fields, PROFILE scope populates name/picture fields, consent decline returns empty scopes with null claims
- `mockConsentGranted` toggle in `GamesServicesMockConfig` — simulates Google consent screen accept/decline behavior

### Changed
- ProGuard rules updated for new `IAuthCallback` scoped methods and `AuthResponse`/`AuthScope` classes
- `consumer-rules.pro` updated to mirror ProGuard changes

---

## [1.1.0] - 2026-02-13

### Changed
- Bumped `play-services-games-v2` from 20.1.1 to **21.0.0** — adds additional auth scope support for `requestServerSideAccess`, removes deprecated Google Sign-In and Google Drive dependencies
- Bumped `play-services-tasks` from 18.0.2 to **18.4.1** — background thread callback handling, new `checkApiAvailability` with Executor parameter
- Bumped `compileSdkVersion` and `targetSdkVersion` from 34 to **35** (Google Play Store requirement August 2025+)
- Updated `GradleDependencyInjector` to inject PGS v2 21.0.0
- Updated README version badge and third-party license table

---

## [1.0.1] - 2026-02-09

### Fixed
- Added `consumer-rules.pro` for R8/ProGuard compatibility — consumer keep rules now propagate to the app's minification pass, preventing `ClassNotFoundException` for JNI bridge classes
- Added `consumerProguardFiles` directive to `build.gradle`

### Added
- `.gitattributes` for consistent line endings across platforms

---

## [1.0.0] - 2026-02-01

### 🎉 First Stable Release - Complete Feature Set

**Major milestone:** Full implementation of Google Play Games Services v2 SDK wrapper with all core features (Phase 1-5).

### Added

#### Core Services (Phase 1-5)
- **Authentication (Phase 1)**
  - Silent authentication with automatic sign-in flow
  - Manual authentication fallback (user-initiated)
  - Server-side access token retrieval for backend validation
  - `GamesPlayer` model (player ID, display name, avatar URLs)
  - Type-safe error handling (`GamesAuthError`, `AuthErrorType`)

- **Achievements (Phase 2)**
  - Unlock standard achievements
  - Increment incremental achievements with progress tracking
  - Reveal hidden achievements
  - Batch unlock multiple achievements (`UnlockMultipleAsync`)
  - Load achievements with force reload option
  - Show native Google Play achievements UI
  - Local caching (PlayerPrefs + memory cache, 24h TTL)

- **Leaderboards (Phase 3)**
  - Submit scores with optional scoretag metadata
  - Load top scores (daily, weekly, all-time)
  - Load player-centered scores (±10 rankings around player)
  - Show native Google Play leaderboard UI (single/all)
  - Leaderboard filtering (time span, public/friends collection)

- **Cloud Save (Phase 4)**
  - Transaction-based API (Open → Read/Write → Commit)
  - Automatic conflict resolution with 60-second timeout
  - Snapshot metadata support (cover image, description, played time)
  - Snapshot deletion
  - Native saved games UI (view/delete)
  - Convenience methods (`SaveAsync`, `LoadAsync`)

- **Player Stats (Phase 5)**
  - Churn probability prediction
  - High spender probability calculation
  - Engagement metrics (avg session length, days since last played)
  - Purchase/session tracking
  - Percentile rankings (session, spend)

#### Infrastructure
- `GamesServicesManager` facade for unified API access
- Platform abstraction (Android JNI ↔ Editor Mock providers)
- Modern async/await pattern throughout
- Event-driven architecture (success/error events for all services)
- ProGuard-safe AndroidJavaProxy callbacks with keep rules
- Gradle dependency auto-injection (`play-services-games-v2:20.1.1`)
- Unity 6+ GameActivity compatibility
- Mock providers for Editor testing
- `GamesServicesMockConfig` ScriptableObject with dropdown error types
- `UnityMainThreadDispatcher` for thread-safe Java callbacks
- Conditional logging (`BizSimGamesLogger`) - zero overhead in Release

#### Editor Tools
- **Setup Window**: 6-step wizard for Android configuration
  - Google Play Console integration guide
  - XML resources parser (app ID, achievement/leaderboard IDs)
  - Automatic Android manifest configuration
- **Documentation Window**: Complete API reference + compliance section
  - Phase 1-5 API docs with method signatures
  - Quick start examples
  - **Google Play Policy Compliance** (CRITICAL section)
    - Quality Checklist requirements
    - Branding guidelines
    - Data Collection policies
    - Terms of Service obligations
- **About Window**: Package info with implementation status

#### Google Play Policy Compliance
- Cloud save metadata requirement enforcement (Quality 6.1)
- Branding guidelines documentation
- 30-day friends retention warning (future Phase 6)
- Terms of Service compliance notices
- Comprehensive README with policy table
- API documentation warnings for critical requirements

### Changed
- Mock config: AuthErrorType dropdown (user-friendly enum vs int codes)
- CreateAssetMenu: Default filename → `"DefaultGamesConfig"` for auto-load
- README: Complete rewrite for v1.0.0 with compliance warnings
- About window: Phase 2-5 marked as complete

### Fixed
- **AndroidManifest.xml**: Removed unnecessary permissions
  - Removed: ACCESS_FINE_LOCATION, ACCESS_COARSE_LOCATION, BLUETOOTH_CONNECT
  - Resolved Google Play "Device and Network Abuse policy" violation
- Mock config naming consistency (`DefaultGamesConfig.asset`)
- .NET Standard 2.1 compatibility (manual timeout via `Task.WhenAny`)
- ProGuard rules for all callback interfaces

### Migration from Deprecated Plugin
- **Controller_GooglePlay.cs** (805 → 460 lines)
  - Callback-based → async/await
  - Event subscription pattern
- **Controller_GooglePlaySaveGame.cs** (620 → 280 lines)
  - Transaction-based cloud save
  - Automatic conflict resolution
- **UI_C_StockMarketCompanyCard.cs**
  - Async leaderboard UI calls

### Technical Details
- **Total Package Size**: 51 files (~4,800 lines)
- **Architecture**: Service interfaces + JNI bridge + Mock providers
- **Threading**: UnityMainThreadDispatcher (Java → Unity main thread)
- **Error Handling**: Type-safe enums (AuthErrorType, AchievementErrorType, etc.)
- **Caching**: Achievements (PlayerPrefs + memory, 24h TTL)
- **Conflict Resolution**: SavedGames (60s timeout, UseLocal fallback)
- **Gradle**: Auto-inject PGS v2 via mainTemplate/settingsTemplate

### Known Limitations
- Phase 6 (Friends) not implemented
- Android-only (iOS not supported)
- Achievement icons configured in Google Play Console (not Unity)

### Developer Notes
- ⚠️ **CRITICAL**: Use `CommitSnapshotAsync` with metadata (cover image, description, timestamp) for Google Play compliance
- ⚠️ **CRITICAL**: If implementing Phase 6 (Friends), enforce 30-day data retention
- Mock providers enable Editor testing without Android device
- ProGuard rules included - ensure Gradle applies them

---

## [0.1.0] - 2026-01-30

### Added
- Phase 1: Authentication implementation
- Basic project structure
- Android JNI bridge foundation
- Editor mock provider foundation
- Setup window (basic version)

### Notes
- Internal development release
- Not production-ready

---

**Legend:**
- `Added`: New features
- `Changed`: Changes to existing functionality
- `Deprecated`: Soon-to-be removed features
- `Removed`: Removed features
- `Fixed`: Bug fixes
- `Security`: Security vulnerability fixes
