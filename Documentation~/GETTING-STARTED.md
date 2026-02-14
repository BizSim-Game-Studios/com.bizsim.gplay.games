# Getting Started

## Prerequisites

- Unity 6000.3 or later
- Android Build Support module installed
- Google Play Console project with Play Games Services enabled
- `google-services.json` from Firebase Console (required for Android build)

## Step 1: Install the Package

### Option A: Git URL (recommended)

In Unity Editor: **Window > Package Manager > + > Add package from git URL**

```
https://github.com/BizSim-Game-Studios/com.bizsim.gplay.games.git
```

Or add directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.bizsim.gplay.games": "https://github.com/BizSim-Game-Studios/com.bizsim.gplay.games.git"
  }
}
```

### Option B: Local Path

```json
{
  "dependencies": {
    "com.bizsim.gplay.games": "file:../path/to/com.bizsim.gplay.games"
  }
}
```

## Step 2: Google Play Console Setup

1. Open [Google Play Console](https://play.google.com/console) and select your game
2. Navigate to **Grow > Play Games Services > Setup and management > Configuration**
3. Click **Get resources** and copy the XML content

The XML looks like this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <string name="app_id">123456789012</string>
    <string name="package_name">com.example.game</string>
    <string name="achievement_first_login">CgkI...</string>
    <string name="leaderboard_high_score">CgkI...</string>
</resources>
```

## Step 3: Run Setup Window

1. Open **BizSim > Google Play > Games Services > Setup** in Unity Editor
2. Paste the resources XML
3. Click **Parse** to extract IDs
4. Click **Setup** to configure your Android project

This automatically:
- Creates `res/values/games-ids.xml` in your Android plugin directory
- Sets up the required `AndroidManifest.xml` metadata entries
- Configures Gradle dependencies

## Step 4: Create Configuration

1. In Project window: **Assets > Create > BizSim > Google Play Games > Services Config**
2. Name it `GamesServicesConfig`
3. Place it in a `Resources/` folder (e.g., `Assets/Resources/GamesServicesConfig.asset`)

### Config Fields

| Field | Default | Description |
|-------|---------|-------------|
| `enableAuth` | `true` | Enable authentication service |
| `enableAchievements` | `true` | Enable achievements service |
| `enableLeaderboards` | `true` | Enable leaderboards service |
| `enableCloudSave` | `true` | Enable cloud save service |
| `enableEvents` | `false` | Enable events tracking service |
| `enableStats` | `true` | Enable player stats service |
| `sidekickReady` | `false` | Mark game as Sidekick-ready |
| `expectedAchievementCount` | `10` | Minimum achievements for quality check |
| `requireCloudSaveMetadata` | `true` | Enforce metadata on cloud saves |
| `conflictTimeoutSeconds` | `60` | Seconds before auto-resolving cloud save conflicts (0 = immediate) |
| `debugMode` | `false` | Enable verbose logging in release builds |

### Editor Mock Settings

The config includes mock settings for Editor testing. These are ignored on device builds:

| Field | Description |
|-------|-------------|
| `authSucceeds` | Whether mock auth succeeds or fails |
| `mockPlayerId` | Simulated player ID |
| `mockDisplayName` | Simulated display name |
| `authDelaySeconds` | Simulated network delay (0-5s) |
| `mockAuthErrorType` | Error type when auth fails |

## Step 5: Place google-services.json

Copy `google-services.json` from Firebase Console to:

```
Assets/Plugins/Android/google-services.json
```

This file is required for Google Play Services authentication on Android.

## Step 6: First Authentication

```csharp
using BizSim.GPlay.Games;
using UnityEngine;

public class GameInit : MonoBehaviour
{
    async void Start()
    {
        try
        {
            var player = await GamesServicesManager.Auth.AuthenticateAsync();
            Debug.Log($"Welcome {player.DisplayName} ({player.PlayerId})");
        }
        catch (GamesAuthException ex)
        {
            Debug.LogError($"Auth failed: {ex.Error.Type} - {ex.Error.Message}");

            if (ex.Error.isRetryable)
                ShowSignInButton();
        }
    }

    void ShowSignInButton()
    {
        // Show manual sign-in button per Google Play quality requirements
    }
}
```

### Authentication Flow

1. `GamesServicesManager` auto-initializes via `[RuntimeInitializeOnLoadMethod]`
2. First `AuthenticateAsync()` call attempts **silent sign-in** (no UI)
3. If silent auth fails, subsequent calls show the **Google Play sign-in dialog**
4. On success, `GamesPlayer` is returned with player profile data
5. `IsAuthenticated` becomes `true` and `CurrentPlayer` is set

## Step 7: Validate Setup

Open **BizSim > Google Play Games > Sidekick Readiness Check** to verify:

- Config exists and is in Resources/
- Required services are enabled
- Achievement count meets minimum
- Cloud save metadata enforcement is active
- ProGuard rules include all callback interfaces

## Next Steps

- Read the [API Reference](API-REFERENCE.md) for all service methods
- Follow the [Sidekick Guide](SIDEKICK-GUIDE.md) for Tier 1 & 2 compliance
- Review the [Quality Checklist](QUALITY-CHECKLIST.md) before publishing
