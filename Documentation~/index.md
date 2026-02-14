# Google Play Games Services Bridge — Documentation

Version: **1.6.0** | Unity: 6000.3+ | Platform: Android

## Table of Contents

| Document | Description |
|----------|-------------|
| [Getting Started](GETTING-STARTED.md) | Installation, Google Play Console setup, first authentication |
| [API Reference](API-REFERENCE.md) | Complete API for all 6 services with signatures, parameters, and events |
| [Architecture](ARCHITECTURE.md) | JNI bridge, mock providers, threading, error handling, batching |
| [Sidekick Guide](SIDEKICK-GUIDE.md) | Sidekick Tier 1 & 2 integration, metadata, validator |
| [Quality Checklist](QUALITY-CHECKLIST.md) | Google Play Games quality requirements checklist |
| [Level Up Program](LEVEL-UP-PROGRAM.md) | Level Up program tiers, benefits, and migration guide |
| [Troubleshooting](TROUBLESHOOTING.md) | Common issues, error codes, platform-specific fixes |

## Quick Links

- **Namespace**: `BizSim.GPlay.Games`
- **Entry Point**: `GamesServicesManager` (singleton MonoBehaviour)
- **Config**: `GamesServicesConfig` ScriptableObject in `Resources/`
- **Editor Tools**: `BizSim > Google Play Games` menu

## Service Overview

| Service | Static Accessor | Interface | Config Toggle |
|---------|----------------|-----------|---------------|
| Auth | `GamesServicesManager.Auth` | `IGamesAuthProvider` | `enableAuth` |
| Achievements | `GamesServicesManager.Achievements` | `IGamesAchievementProvider` | `enableAchievements` |
| Leaderboards | `GamesServicesManager.Leaderboards` | `IGamesLeaderboardProvider` | `enableLeaderboards` |
| Cloud Save | `GamesServicesManager.CloudSave` | `IGamesCloudSaveProvider` | `enableCloudSave` |
| Events | `GamesServicesManager.Events` | `IGamesEventsProvider` | `enableEvents` |
| Player Stats | `GamesServicesManager.Stats` | `IGamesStatsProvider` | `enableStats` |

## Minimum Example

```csharp
using BizSim.GPlay.Games;

public class GameInit : MonoBehaviour
{
    async void Start()
    {
        var player = await GamesServicesManager.Auth.AuthenticateAsync();
        Debug.Log($"Signed in as {player.DisplayName}");

        await GamesServicesManager.Achievements.UnlockAchievementAsync("first_login");
    }
}
```
