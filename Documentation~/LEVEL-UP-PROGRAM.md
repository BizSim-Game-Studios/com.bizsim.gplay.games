# Google Play Level Up Program

## Overview

The Level Up program rewards games that deeply integrate Google Play Games Services with improved visibility, featuring, and promotional opportunities on Google Play Store.

## Tier 1 — July 2026 Deadline

### Requirements
- Authentication (silent + manual fallback)
- 10+ achievements with unique icons
- 4+ achievements easily achievable in first session
- Proper branding and attribution

### Package Features That Satisfy Tier 1
- `GamesServicesManager.Auth` — silent auth with fallback
- `GamesServicesManager.Achievements` — unlock, increment, reveal, load, show UI
- `GamesServicesConfig.enableAuth = true`
- `GamesServicesConfig.enableAchievements = true`
- `GamesServicesConfig.expectedAchievementCount >= 10`

## Tier 2 — November 2026 Deadline

### Requirements
- All Tier 1 requirements
- Cloud Save with full metadata (description, playedTime, coverImage)
- Conflict resolution handling
- Events tracking (recommended)

### Package Features That Satisfy Tier 2
- `GamesServicesManager.CloudSave` — transaction API + convenience methods
- `SaveGameMetadata` — structured metadata with validation
- `ValidateMetadata()` — runtime enforcement when `requireCloudSaveMetadata = true`
- `DownloadCoverImageAsync()` — cover image read-back helper
- `GamesServicesManager.Events` — batched event tracking
- Automatic conflict resolution with 60s timeout

## Migration Guide (v1.0.1 → v1.6.0)

### Step 1: Update Package
Update `manifest.json` to latest git hash.

### Step 2: Create Config
1. Delete old `DefaultGamesConfig` asset (if exists)
2. Create: Assets > Create > BizSim > Google Play Games > Services Config
3. Name: `GamesServicesConfig`, place in `Resources/`

### Step 3: Enable Services
Enable all required service toggles in config.

### Step 4: Update Save Calls
Replace simple `SaveAsync(filename, data, description)` with metadata overload:

```csharp
var metadata = new SaveGameMetadata
{
    description = "Level 5 - Factory District",
    playedTimeMillis = totalPlayTimeMs,
    coverImage = screenshotPngBytes,
    progressValue = 45
};
await GamesServicesManager.CloudSave.SaveAsync("slot1", data, metadata);
```

### Step 5: Validate
Open **BizSim > Google Play Games > Sidekick Readiness Check** and verify all checks pass.
