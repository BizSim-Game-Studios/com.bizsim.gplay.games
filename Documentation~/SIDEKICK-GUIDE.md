# Google Play Games Sidekick Integration Guide

## What is Sidekick?

Sidekick is Google Play's AI-powered assistant that helps players within your game. It uses your game's Play Games Services data (achievements, cloud saves, events) to provide personalized guidance.

## Sidekick Tiers

### Tier 1: Basic Sidekick
- **Requirements**: Authentication + Achievements (10+ achievements, 4+ easily achievable)
- **Deadline**: July 2026
- **What it provides**: Achievement guidance, progress tracking

### Tier 2: Full Sidekick
- **Requirements**: Tier 1 + Cloud Save with metadata (description, playedTime, coverImage)
- **Deadline**: November 2026
- **What it provides**: Save game awareness, progress visualization, personalized tips

## Package Services → Sidekick Mapping

| Service | Sidekick Feature | Config Toggle |
|---------|-----------------|---------------|
| Auth | Player identity | `enableAuth` |
| Achievements | Progress guidance | `enableAchievements` |
| Cloud Save | Save awareness | `enableCloudSave` |
| Events | Player milestone tracking | `enableEvents` |

## Configuration

1. Create config: **Assets > Create > BizSim > Google Play Games > Services Config**
2. Name it `GamesServicesConfig` and place in `Resources/`
3. Enable required services
4. Set `expectedAchievementCount` to 10+
5. Enable `requireCloudSaveMetadata` for Tier 2
6. Enable `sidekickReady` when ready

## Cloud Save Metadata (Tier 2)

Use `SaveGameMetadata` for structured metadata:

```csharp
var metadata = new SaveGameMetadata
{
    description = "Level 5 - Factory District",
    playedTimeMillis = totalPlayTimeMs,
    coverImage = screenshotPngBytes,  // max 800KB, 640x360 recommended
    progressValue = 45                // 0-100 percentage
};

await GamesServicesManager.CloudSave.SaveAsync("slot1", saveData, metadata);
```

### Cover Image: Write vs Read

| Operation | Format | API |
|-----------|--------|-----|
| **Write** | `byte[]` PNG, max 800KB, max 640x360 | `SaveGameMetadata.coverImage` |
| **Read** | `string` URI (Google Cloud URL) | `SnapshotHandle.coverImageUri` |
| **Display** | `Texture2D` | `DownloadCoverImageAsync(handle.coverImageUri)` |

**WARNING:** A small PNG file (200KB) at high resolution (3840x2160) decompresses to ~33MB Bitmap in Java memory. Always use 640x360 or lower resolution regardless of file size.

## Events API

Track meaningful player milestones:

```csharp
await GamesServicesManager.Events.IncrementEventAsync("vehicle_dismantled", 1);
```

Events use batching (5-second flush interval). Call at natural breakpoints, not per-frame. Pending events flush automatically on app pause/quit.

## Thread Safety

All PGS calls must be on the Unity main thread. JNI crashes on background threads. Use `UnityMainThreadDispatcher.Enqueue()` from async callbacks.

## Editor Validator

**BizSim > Google Play Games > Sidekick Readiness Check** — validates all requirements with pass/fail checklist and remediation guidance.

## Troubleshooting

- **Events not recording**: Ensure events are defined in Google Play Console first
- **Cloud save conflict**: Package handles automatically with 60s timeout, defaults to server version
- **Cover image OOM**: Reduce resolution to 640x360 or lower
- **ProGuard stripping callbacks**: Verify proguard-rules.pro has all `keepclassmembers` entries
