// Copyright (c) BizSim Game Studios. All rights reserved.

#if UNITASK_AVAILABLE
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BizSim.GPlay.Games.UniTask
{
    public static class GamesUniTaskExtensions
    {
        public static async UniTask<GamesPlayer> AuthenticateUniTask(
            this IGamesAuthProvider provider, bool silentOnly = false, CancellationToken ct = default)
        {
            return await provider.AuthenticateAsync(silentOnly, ct);
        }

        public static async UniTask UnlockAchievementUniTask(
            this IGamesAchievementProvider provider, string achievementId, CancellationToken ct = default)
        {
            await provider.UnlockAchievementAsync(achievementId, ct);
        }

        public static async UniTask IncrementAchievementUniTask(
            this IGamesAchievementProvider provider, string achievementId, int steps, CancellationToken ct = default)
        {
            await provider.IncrementAchievementAsync(achievementId, steps, ct);
        }

        public static async UniTask<GamesAchievement[]> LoadAchievementsUniTask(
            this IGamesAchievementProvider provider, bool forceReload = false, CancellationToken ct = default)
        {
            return await provider.LoadAchievementsAsync(forceReload, ct);
        }

        public static async UniTask SaveUniTask(
            this IGamesCloudSaveProvider provider, string filename, byte[] data,
            SaveGameMetadata metadata, CancellationToken ct = default)
        {
            await provider.SaveAsync(filename, data, metadata, ct);
        }

        public static async UniTask<byte[]> LoadUniTask(
            this IGamesCloudSaveProvider provider, string filename, CancellationToken ct = default)
        {
            return await provider.LoadAsync(filename, ct);
        }

        public static async UniTask<Texture2D> DownloadCoverImageUniTask(
            this IGamesCloudSaveProvider provider, string coverImageUri, CancellationToken ct = default)
        {
            return await provider.DownloadCoverImageAsync(coverImageUri, ct);
        }

        public static async UniTask IncrementEventUniTask(
            this IGamesEventsProvider provider, string eventId, int steps = 1, CancellationToken ct = default)
        {
            await provider.IncrementEventAsync(eventId, steps, ct);
        }

        public static async UniTask<GamesEvent[]> LoadEventsUniTask(
            this IGamesEventsProvider provider, CancellationToken ct = default)
        {
            return await provider.LoadEventsAsync(ct);
        }

        public static async UniTask<GamesPlayerStats> LoadStatsUniTask(
            this IGamesStatsProvider provider, bool forceReload = false, CancellationToken ct = default)
        {
            return await provider.LoadPlayerStatsAsync(forceReload, ct);
        }
    }
}
#endif
