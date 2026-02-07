// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    /// <summary>
    /// Facade for all Google Play Games Services.
    /// Automatically initializes and provides access to Auth, Achievements, Leaderboards, SavedGames, and PlayerStats.
    ///
    /// Platform Detection:
    /// - Android (device): Uses JNI bridge to native PGS v2 SDK
    /// - Editor: Auto-loads mock providers from Resources/DefaultGamesConfig
    /// </summary>
    [DefaultExecutionOrder(-999)]
    public class GamesServicesManager : MonoBehaviour
    {
        private static GamesServicesManager _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        private IGamesAuthProvider _authProvider;
        private IGamesAchievementProvider _achievementsProvider;
        private IGamesLeaderboardProvider _leaderboardsProvider;
        private IGamesCloudSaveProvider _cloudSaveProvider;
        private IGamesStatsProvider _statsProvider;

        /// <summary>
        /// Singleton instance (auto-creates on first access).
        /// </summary>
        public static GamesServicesManager Instance
        {
            get
            {
                if (_isQuitting) return null;

                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            Initialize();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Authentication service.
        /// </summary>
        public static IGamesAuthProvider Auth => Instance?._authProvider;

        /// <summary>
        /// Achievements service (unlock, increment, reveal, show UI).
        /// </summary>
        public static IGamesAchievementProvider Achievements => Instance?._achievementsProvider;

        /// <summary>
        /// Leaderboards service (submit scores, show UI, load rankings).
        /// </summary>
        public static IGamesLeaderboardProvider Leaderboards => Instance?._leaderboardsProvider;

        /// <summary>
        /// Cloud Save service (SavedGames / Snapshots API).
        /// </summary>
        public static IGamesCloudSaveProvider CloudSave => Instance?._cloudSaveProvider;

        /// <summary>
        /// Player Stats service (engagement metrics, churn probability, etc.).
        /// </summary>
        public static IGamesStatsProvider Stats => Instance?._statsProvider;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null || _isQuitting) return;

            _isQuitting = false;
            Application.quitting += () => _isQuitting = true;

            var go = new GameObject("[GamesServicesManager]");
            _instance = go.AddComponent<GamesServicesManager>();
            DontDestroyOnLoad(go);

            BizSimGamesLogger.Info("GamesServicesManager initialized");
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
                return;
            }

            _instance = this;
            InitializeServices();
        }

        private void InitializeServices()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
                // Production: JNI bridge to PGS v2 SDK
                BizSimGamesLogger.Info("Platform: Android (JNI Bridge)");
                _authProvider = new GamesAuthController();
                _achievementsProvider = new GamesAchievementController();
                _leaderboardsProvider = new GamesLeaderboardController();
                _cloudSaveProvider = new GamesCloudSaveController();
                _statsProvider = new GamesStatsController();
            #else
                // Editor: Auto-load mock provider
                BizSimGamesLogger.Info("Platform: Editor (Mock Provider)");

                var mockConfig = Resources.Load<GamesServicesMockConfig>("DefaultGamesConfig");
                if (mockConfig != null)
                {
                    _authProvider = new MockAuthProvider(mockConfig);
                    _achievementsProvider = new MockAchievementProvider(mockConfig);
                    _leaderboardsProvider = new MockLeaderboardProvider(mockConfig);
                    _cloudSaveProvider = new MockCloudSaveProvider(mockConfig);
                    _statsProvider = new MockStatsProvider(mockConfig);
                    BizSimGamesLogger.Info("Mock providers loaded from Resources/DefaultGamesConfig");
                }
                else
                {
                    BizSimGamesLogger.Warning("No mock config found at Resources/DefaultGamesConfig - services unavailable in Editor");
                    _authProvider = new MockAuthProvider(null); // Fallback: always fails
                    _achievementsProvider = new MockAchievementProvider(CreateFallbackConfig());
                    _leaderboardsProvider = new MockLeaderboardProvider(CreateFallbackConfig());
                    _cloudSaveProvider = new MockCloudSaveProvider(CreateFallbackConfig());
                    _statsProvider = new MockStatsProvider(CreateFallbackConfig());
                }
            #endif
        }

        private GamesServicesMockConfig CreateFallbackConfig()
        {
            var config = ScriptableObject.CreateInstance<GamesServicesMockConfig>();
            config.mockPlayerId = "editor_player_fallback";
            config.mockDisplayName = "Editor Player";
            return config;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
