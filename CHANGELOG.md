# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
- Phase 6 (Friends & Events) not implemented
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
