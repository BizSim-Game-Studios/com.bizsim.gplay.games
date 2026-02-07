// Copyright (c) BizSim Game Studios. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Provider interface for Google Play Games Leaderboards.
    /// Supports score submission, loading, and UI display.
    /// </summary>
    public interface IGamesLeaderboardProvider
    {
        /// <summary>
        /// Submits a score to a leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID from games-ids.xml</param>
        /// <param name="score">Score value to submit</param>
        /// <param name="scoreTag">Optional metadata tag (max 64 chars)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when score is submitted</returns>
        Task SubmitScoreAsync(string leaderboardId, long score, string scoreTag = null, CancellationToken ct = default);

        /// <summary>
        /// Shows the native Google Play Games leaderboard UI for a specific leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID to display</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when UI is closed</returns>
        Task ShowLeaderboardUIAsync(string leaderboardId, CancellationToken ct = default);

        /// <summary>
        /// Shows the native Google Play Games UI with all leaderboards.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when UI is closed</returns>
        Task ShowAllLeaderboardsUIAsync(CancellationToken ct = default);

        /// <summary>
        /// Loads top scores from a leaderboard.
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID</param>
        /// <param name="timeSpan">Time scope (daily, weekly, all-time)</param>
        /// <param name="collection">Collection type (public or friends)</param>
        /// <param name="maxResults">Maximum number of scores to retrieve (1-25)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of top leaderboard entries</returns>
        Task<List<GamesLeaderboardEntry>> LoadTopScoresAsync(
            string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25,
            CancellationToken ct = default);

        /// <summary>
        /// Loads player-centered scores (scores around the current player's rank).
        /// </summary>
        /// <param name="leaderboardId">The leaderboard ID</param>
        /// <param name="timeSpan">Time scope (daily, weekly, all-time)</param>
        /// <param name="collection">Collection type (public or friends)</param>
        /// <param name="maxResults">Maximum number of scores to retrieve (1-25)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of player-centered leaderboard entries</returns>
        Task<List<GamesLeaderboardEntry>> LoadPlayerCenteredScoresAsync(
            string leaderboardId,
            LeaderboardTimeSpan timeSpan = LeaderboardTimeSpan.AllTime,
            LeaderboardCollection collection = LeaderboardCollection.Public,
            int maxResults = 25,
            CancellationToken ct = default);

        /// <summary>
        /// Event fired when a score is successfully submitted.
        /// </summary>
        event Action<string, long> OnScoreSubmitted;

        /// <summary>
        /// Event fired when leaderboard scores are loaded.
        /// </summary>
        event Action<string, List<GamesLeaderboardEntry>> OnScoresLoaded;

        /// <summary>
        /// Event fired when a leaderboard operation fails.
        /// </summary>
        event Action<GamesLeaderboardError> OnLeaderboardError;
    }

    /// <summary>
    /// Leaderboard time scope for score filtering.
    /// </summary>
    public enum LeaderboardTimeSpan
    {
        /// <summary>
        /// Scores from today only.
        /// </summary>
        Daily = 0,

        /// <summary>
        /// Scores from this week.
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// All-time scores.
        /// </summary>
        AllTime = 2
    }

    /// <summary>
    /// Leaderboard collection type.
    /// </summary>
    public enum LeaderboardCollection
    {
        /// <summary>
        /// Public leaderboard (all players).
        /// </summary>
        Public = 0,

        /// <summary>
        /// Friends-only leaderboard (Google Play friends).
        /// PGS v2: COLLECTION_FRIENDS = 3 (replaces deprecated COLLECTION_SOCIAL).
        /// </summary>
        Friends = 3
    }
}
