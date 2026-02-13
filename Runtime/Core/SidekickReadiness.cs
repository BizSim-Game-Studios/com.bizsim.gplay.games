// Copyright (c) BizSim Game Studios. All rights reserved.

namespace BizSim.GPlay.Games
{
    public enum SidekickTier
    {
        None,
        Tier1,
        Tier2
    }

    public static class SidekickReadiness
    {
        public static SidekickTier Evaluate(GamesServicesConfig config)
        {
            if (config == null || !config.sidekickReady)
                return SidekickTier.None;

            bool hasTier1 = config.enableAuth
                         && config.enableAchievements;

            bool hasTier2 = hasTier1
                         && config.enableCloudSave
                         && config.requireCloudSaveMetadata;

            if (hasTier2) return SidekickTier.Tier2;
            if (hasTier1) return SidekickTier.Tier1;
            return SidekickTier.None;
        }
    }
}
