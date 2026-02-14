# Troubleshooting

## Authentication

### Silent auth fails on first launch

**Symptom**: `AuthenticateAsync()` throws `GamesAuthException` with `SignInRequired`.

**Cause**: PGS v2 silent auth requires the player to have previously signed in to Google Play Games on this device.

**Solution**: Show a manual "Sign In" button when `isRetryable` is true. Calling `AuthenticateAsync()` again shows the Google Play sign-in dialog.

```csharp
catch (GamesAuthException ex)
{
    if (ex.Error.isRetryable)
        ShowSignInButton();
}
```

### Auth works in Editor but fails on device

**Cause**: Missing or incorrect `google-services.json`.

**Solution**:
1. Download `google-services.json` from Firebase Console
2. Place it in `Assets/Plugins/Android/`
3. Verify the `package_name` matches your application ID

### "Google Play Games API is not available" (code -1)

**Cause**: Google Play Services not installed or outdated on the device.

**Solution**: This typically occurs on emulators without Google Play. Test on a physical device with Google Play Store installed.

---

## Achievements

### Achievement ID not found

**Cause**: Achievement ID in code does not match Google Play Console.

**Solution**:
1. Open Play Console > Play Games Services > Achievements
2. Copy the exact achievement ID (starts with `CgkI...`)
3. Verify the ID matches your `games-ids.xml` resource entries

### Achievements not appearing after unlock

**Cause**: Achievement not published in Play Console.

**Solution**: Achievements in Draft state only work for tester accounts. Publish the achievement in Play Console for all users.

### LoadAchievementsAsync returns stale data

**Cause**: Achievements are cached locally for 24 hours.

**Solution**: Pass `forceReload: true` to bypass the cache:

```csharp
var achievements = await GamesServicesManager.Achievements.LoadAchievementsAsync(forceReload: true);
```

---

## Leaderboards

### Score not appearing on leaderboard

**Cause**: Leaderboard in Draft state or score submitted for wrong leaderboard ID.

**Solution**:
1. Verify leaderboard is published in Play Console
2. Check the leaderboard ID matches exactly
3. Scores may take a few minutes to propagate

### LoadTopScoresAsync returns empty list

**Cause**: No scores have been submitted yet, or the time span filter excludes all scores.

**Solution**: Submit a test score first, then load with `LeaderboardTimeSpan.AllTime`.

---

## Cloud Save

### "Cloud save failed" with no specific error

**Cause**: User not authenticated before cloud save operation.

**Solution**: Always authenticate before using cloud save:

```csharp
await GamesServicesManager.Auth.AuthenticateAsync();
await GamesServicesManager.CloudSave.SaveAsync("slot1", data, metadata);
```

### Conflict detected on every save

**Cause**: Multiple devices saving to the same slot without resolving conflicts.

**Solution**: The package handles conflicts automatically with a 60-second timeout (defaults to server version). To customize:

```csharp
GamesServicesManager.CloudSave.OnConflictDetected += async (conflict) =>
{
    // Compare local and server data
    if (ShouldUseLocal(conflict.localData, conflict.serverData))
        await conflict.ResolveAsync(ConflictResolution.UseLocal);
    else
        await conflict.ResolveAsync(ConflictResolution.UseServer);
};
```

### Cover image causes OutOfMemoryError

**Cause**: High-resolution PNG decompresses to large bitmap in Java heap. A 200KB PNG at 3840x2160 decompresses to ~33MB.

**Solution**: Always resize cover images to 640x360 or smaller, regardless of file size. Max file size: 800KB.

### SaveAsync validation warnings

**Cause**: `requireCloudSaveMetadata` is enabled but metadata is incomplete.

**Solution**: Use the `SaveGameMetadata` overload with all fields:

```csharp
var metadata = new SaveGameMetadata
{
    description = "Level 5 - 1500 coins",
    playedTimeMillis = GetTotalPlayTime(),
    coverImage = CaptureScreenshotAsPng(),
    progressValue = 45
};
await GamesServicesManager.CloudSave.SaveAsync("slot1", data, metadata);
```

### Texture2D memory leak from DownloadCoverImageAsync

**Cause**: `Texture2D` uses unmanaged GPU memory that is not garbage collected.

**Solution**: Use `ReleaseCoverImage()` to destroy textures and remove from cache:

```csharp
var texture = await GamesServicesManager.CloudSave.DownloadCoverImageAsync(handle.coverImageUri);
// Use texture...
GamesServicesManager.CloudSave.ReleaseCoverImage(texture);

// Or release all at once when leaving the save list UI:
GamesServicesManager.CloudSave.ReleaseAllCoverImages();
```

---

## Events

### Events not recording in Play Console

**Cause**: Events must be defined in Google Play Console before they can be incremented.

**Solution**:
1. Open Play Console > Play Games Services > Events
2. Create the event and note the event ID
3. Use the exact event ID in `IncrementEventAsync`

### Events batching delay

**Cause**: Events are batched with a 5-second flush interval by design.

**Solution**: This is expected behavior. Events flush automatically on app pause and quit. Do not call at high frequency (per-frame) — use natural game breakpoints.

---

## Player Stats

### Stats return zero values

**Cause**: Google needs sufficient player data to compute stats. New games or players with few sessions return zeros.

**Solution**: Player stats improve over time as Google collects more engagement data. This is a Google-side computation.

---

## Build Errors

### ProGuard stripping callback methods

**Symptom**: JNI callbacks fail silently on release builds.

**Cause**: R8/ProGuard strips methods it considers unused. Callback proxy methods are only called from Java via reflection.

**Solution**: The package includes `proguard-rules.pro` with keep rules for all 6 callback interfaces. Verify the file exists and contains entries for:

- `IAuthCallback`
- `IAchievementCallback`
- `ILeaderboardCallback`
- `ICloudSaveCallback`
- `IStatsCallback`
- `IEventsCallback`

Use the Sidekick Readiness validator (**BizSim > Google Play Games > Sidekick Readiness Check**) to scan ProGuard rules automatically.

### IL2CPP stripping data classes

**Symptom**: `JsonUtility.FromJson<T>()` returns null or empty objects on device.

**Cause**: IL2CPP strips types that appear unused in C# code (data classes only instantiated via JSON deserialization).

**Solution**: All package data classes use `[Preserve]` attribute. If you create custom data classes for cloud save, add `[Preserve]`:

```csharp
using UnityEngine.Scripting;

[Serializable, Preserve]
public class MySaveData
{
    public int level;
    public float score;
}
```

### Gradle dependency conflict

**Cause**: Another plugin includes a different version of `play-services-games-v2`.

**Solution**: Use Gradle dependency resolution in `mainTemplate.gradle`:

```gradle
configurations.all {
    resolutionStrategy {
        force 'com.google.android.gms:play-services-games-v2:21.0.0'
    }
}
```

---

## Editor

### Mock provider not responding

**Cause**: `GamesServicesConfig` not found in Resources folder.

**Solution**:
1. Create config: Assets > Create > BizSim > Google Play Games > Services Config
2. Name it `GamesServicesConfig`
3. Place in any `Resources/` folder

### Sidekick validator shows failures

**Cause**: Config settings do not meet Sidekick tier requirements.

**Solution**: Follow the validator's remediation guidance for each failed check. Common fixes:
- Set `expectedAchievementCount >= 10`
- Enable `requireCloudSaveMetadata`
- Enable `enableEvents` for Tier 2
- Set `sidekickReady = true` when ready

---

## Error Code Reference

All services share a common error code pattern:

| Code | Meaning | All Services |
|------|---------|-------------|
| -1 | API not available | Yes |
| 0 | Unknown error | Yes |
| 1 | User not authenticated | Yes |
| 2 | Network error | Yes |
| 3 | Resource not found | Achievements, Leaderboards, Cloud Save, Events |
| 4 | Conflict timeout | Cloud Save only |
| 5 | Data too large | Cloud Save only |
| 100 | Internal SDK error | Yes |
