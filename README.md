# Google Play Games Services Bridge

[![Unity 6000.3+](https://img.shields.io/badge/Unity-6000.3%2B-blue.svg)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/Version-1.2.0-orange.svg)](CHANGELOG.md)

**Unity bridge for Google Play Games Services v2**

> **⚠️ Unofficial package.** This is a community-built Unity bridge for [Google Play Games Services v2](https://developers.google.com/games/services). It is **not** an official Google product.

Version: **1.2.0**
Unity: 6000.3+
Platform: Android
License: MIT (package code) — see [Third-Party Licenses](#-third-party-licenses) for SDK terms

---

## Table of Contents

- [Features](#-features)
- [Google Play Compliance](#-critical-google-play-compliance-requirements)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Documentation](#-documentation)
- [Architecture](#-architecture)
- [Troubleshooting](#-troubleshooting)
- [License](#-license)
- [Terms of Service](#-terms-of-service-compliance-notice)

---

## 🚀 Features

- ✅ **Authentication** - Silent + manual sign-in with PGS v2, server-side access tokens
- ✅ **Achievements** - Unlock, increment, reveal, batch operations, local caching
- ✅ **Leaderboards** - Submit scores, load top/player-centered rankings, scoretags
- ✅ **Cloud Save** - Transaction-based saved games with automatic conflict resolution
- ✅ **Player Stats** - Churn prediction, spend probability, engagement metrics

---

## ⚠️ CRITICAL: Google Play Compliance Requirements

**Before publishing your game, you MUST comply with Google Play Games Services policies.**
Failure to comply may result in **app rejection or removal from Google Play Store**.

### 📋 Required Policies

| Policy | Key Requirements | Link |
|--------|------------------|------|
| **Quality Checklist** | • Manual sign-in button if auto-auth fails<br>• 10+ achievements (4+ achievable in 1 hour)<br>• Achievement icons: 512x512 PNG<br>• **Saved games MUST have cover image, description, timestamp** | [Quality Checklist](https://developer.android.com/games/pgs/quality) |
| **Branding Guidelines** | • Use Google Play game controller icon for all UI entry points<br>• **NEVER suppress pop-ups** (welcome back, achievement unlock)<br>• **NEVER include "Google Play Games" in your game TITLE** | [Branding Guidelines](https://developer.android.com/games/pgs/branding) |
| **Data Collection** | • Friends data: **30-DAY MAX retention** (delete or refresh)<br>• Friends data: **ONLY for friends list UI** (NOT analytics/advertising)<br>• Complete Google Play Data Safety form accurately | [Data Collection](https://developer.android.com/games/pgs/data-collection) |
| **Terms of Service** | • **NEVER submit false gameplay data**<br>• **NEVER send multiplayer invites without user approval**<br>• **NEVER use player data for advertising**<br>• **NEVER share friends data with third parties** | [Terms of Service](https://developer.android.com/games/pgs/terms) |

### 🔥 Most Critical Rules

1. **Saved Games Metadata (Quality 6.1)**: MUST include cover image, description, and timestamp
   → Use `CommitSnapshotAsync()` with all 3 parameters (NOT simplified `SaveAsync()`)

2. **Friends Data Retention (Data Collection)**: 30-day maximum
   → If implementing Phase 6 (Friends), add auto-delete after 30 days

3. **Pop-up Suppression (Branding)**: NEVER interrupt Google's welcome/achievement pop-ups
   → Don't hide/dismiss/overlay these notifications

4. **Achievement Icons (Quality 2.4)**: 512x512 PNG on transparent background
   → Configure in Google Play Console, NOT in Unity package

---

## 📦 Installation

### Option 1: Git URL (recommended)

1. In Unity Editor: **Window > Package Manager > + > Add package from git URL...**
2. Enter:
   ```
   https://github.com/BizSim-Game-Studios/com.bizsim.gplay.games.git
   ```

3. Or add directly to `Packages/manifest.json`:
   ```json
   "com.bizsim.gplay.games": "https://github.com/BizSim-Game-Studios/com.bizsim.gplay.games.git"
   ```

### Option 2: Local path

```json
"com.bizsim.gplay.games": "file:../path/to/com.bizsim.gplay.games"
```

### After Installation

1. Get your resources XML from [Google Play Console](https://play.google.com/console)
2. Open the setup window: **BizSim > Google Play > Games Services > Setup**
3. Paste the XML and click "Setup" to configure your Android project

---

## 🎮 Quick Start

### 1. Setup (Editor Window)
```
Unity Menu → BizSim → Google Play → Games Services → Setup
```
1. Get resources XML from Google Play Console
2. Paste XML and parse configuration
3. Click "Setup" to configure Android project

### 2. Authentication
```csharp
using BizSim.GPlay.Games;

void Start()
{
    GamesServicesManager.Initialize();
    GamesServicesManager.Auth.OnAuthenticationSuccess += OnAuthSuccess;
    GamesServicesManager.Auth.OnAuthenticationFailed += OnAuthFailed;
}

async void AuthenticateUser()
{
    try {
        var player = await GamesServicesManager.Auth.AuthenticateAsync();
        Debug.Log($"Welcome {player.DisplayName}!");
    } catch (GamesAuthException ex) {
        Debug.LogError($"Auth failed: {ex.Error.Message}");
    }
}
```

### 3. Achievements
```csharp
// Unlock achievement
await GamesServicesManager.Achievements.UnlockAchievementAsync("achievement_first_win");

// Increment incremental achievement
await GamesServicesManager.Achievements.IncrementAchievementAsync("achievement_100_wins", 1);

// Show achievements UI
await GamesServicesManager.Achievements.ShowAchievementsUIAsync();
```

### 4. Leaderboards
```csharp
// Submit score
await GamesServicesManager.Leaderboards.SubmitScoreAsync("leaderboard_high_score", 12345);

// Show leaderboard UI
await GamesServicesManager.Leaderboards.ShowLeaderboardUIAsync("leaderboard_high_score");
```

### 5. Cloud Save (COMPLIANCE: Use full API with metadata!)
```csharp
// ✅ CORRECT: Full compliance with cover image, description, timestamp
var handle = await GamesServicesManager.CloudSave.OpenSnapshotAsync("slot1", true);
byte[] saveData = SerializeGameState();
byte[] screenshot = CaptureScreenshot(); // REQUIRED by Quality Checklist 6.1

await GamesServicesManager.CloudSave.CommitSnapshotAsync(
    handle,
    saveData,
    description: "Level 5, 1500 coins",  // REQUIRED
    playedTimeMillis: GetPlayTime(),      // REQUIRED
    coverImage: screenshot                // REQUIRED
);

// ❌ WRONG: Simplified API lacks required metadata (non-compliant!)
await GamesServicesManager.CloudSave.SaveAsync("slot1", saveData); // Missing cover image!
```

### 6. Player Stats
```csharp
var stats = await GamesServicesManager.Stats.LoadPlayerStatsAsync();
Debug.Log($"Churn probability: {stats.churnProbability}");
Debug.Log($"High spender probability: {stats.highSpenderProbability}");
```

---

## 📚 Documentation

- **Setup Guide**: Unity Editor → `BizSim/Google Play/Games Services/Setup`
- **API Reference**: Unity Editor → `BizSim/Google Play/Games Services/Documentation`
- **Development Plan**: `docs/development-plans/google-play-games/00-INDEX.md`
- **Official PGS Docs**: https://developers.google.com/games/services

---

## 🛡️ Architecture

- **Platform Abstraction**: Android JNI ↔ Editor Mock providers
- **Async/Await**: Modern C# pattern (no callbacks)
- **Event-Driven**: Success/error events for all services
- **ProGuard-Safe**: AndroidJavaProxy callbacks with keep rules
- **Conflict Resolution**: Automatic with 60s timeout protection
- **Local Caching**: Achievements cached in PlayerPrefs (24h TTL)

---

## 🔧 Troubleshooting

| Issue | Solution |
|-------|----------|
| Authentication fails | Check `google-services.json` is in `Assets/Plugins/Android/` |
| Achievements not unlocking | Verify achievement IDs match Google Play Console exactly |
| Leaderboard not showing | Ensure leaderboard is published in Play Console |
| Cloud save conflict | Handle `OnConflictDetected` event and call `conflict.ResolveAsync()` |
| Build errors (ProGuard) | Package includes ProGuard rules - ensure Gradle build uses them |

---

## 📝 License

This package's C# and Java source code is licensed under the [MIT License](LICENSE.md) — Copyright (c) 2026 BizSim Game Studios.

See [LICENSE.md](LICENSE.md) for the full MIT license text.

---

## 📦 Third-Party Licenses

This package does **not** bundle any Google SDK binaries. Native Android dependencies are resolved at build time via Gradle from the Google Maven repository (`maven.google.com`):

| Dependency | Version | License |
|-----------|---------|---------|
| `com.google.android.gms:play-services-games-v2` | 21.0.0 | [Android SDK License Agreement](https://developer.android.com/studio/terms) |
| `com.google.android.gms:play-services-tasks` | 18.4.1 | [Android SDK License Agreement](https://developer.android.com/studio/terms) |

By installing and using this package, you agree to the [Android Software Development Kit License Agreement](https://developer.android.com/studio/terms) and the [Google APIs Terms of Service](https://developers.google.com/terms).

For full third-party license details, see [NOTICES.md](NOTICES.md).

### Open Source Notices in Your App

Google Play Services libraries contain open source components. Google requires that apps display these notices to end users. See [Include open source notices](https://developers.google.com/android/guides/opensource) for instructions on using the `oss-licenses-plugin` Gradle plugin.

---

## 🚨 Terms of Service Compliance Notice

By using this package, you agree to comply with:
- [Google Play Developer Policies](https://play.google.com/about/developer-content-policy/)
- [Google Play Games Services Terms](https://developer.android.com/games/pgs/terms)
- [Google Controller-Controller Data Protection Terms](https://privacy.google.com/businesses/gdprcontrollerterms/)

**Key Obligations:**
- Submit only authentic gameplay data
- Obtain explicit user approval for multiplayer invites/gifts
- Use player data ONLY for game features (NOT advertising)
- Delete friends data after 30 days OR refresh via new API calls
- Complete Google Play Data Safety section accurately
