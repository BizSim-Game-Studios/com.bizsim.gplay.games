// Copyright (c) BizSim Game Studios. All rights reserved.

using System;

namespace BizSim.GPlay.Games
{
    [Serializable]
    public class GamesPlayerStats
    {
        public float avgSessionLengthMinutes;
        public int daysSinceLastPlayed;
        public int numberOfPurchases;
        public int numberOfSessions;
        public float sessionPercentile;
        public float spendPercentile;
        public float churnProbability;
        public float highSpenderProbability;
    }
}
