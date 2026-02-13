// Copyright (c) BizSim Game Studios. All rights reserved.

using UnityEngine;

namespace BizSim.GPlay.Games
{
    [DefaultExecutionOrder(-999)]
    public class GamesServicesManager : MonoBehaviour
    {
        private static GamesServicesManager _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting;

        private GamesServicesConfig _config;
        private IGamesAuthProvider _authProvider;
        private IGamesAchievementProvider _achievementsProvider;
        private IGamesLeaderboardProvider _leaderboardsProvider;
        private IGamesCloudSaveProvider _cloudSaveProvider;
        private IGamesStatsProvider _statsProvider;

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

        public static GamesServicesConfig Config => Instance?._config;
        public static SidekickTier SidekickStatus => SidekickReadiness.Evaluate(Instance?._config);
        public static IGamesAuthProvider Auth => Instance?._authProvider;
        public static IGamesAchievementProvider Achievements => Instance?._achievementsProvider;
        public static IGamesLeaderboardProvider Leaderboards => Instance?._leaderboardsProvider;
        public static IGamesCloudSaveProvider CloudSave => Instance?._cloudSaveProvider;
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
            ResolveConfig();

            #if UNITY_ANDROID && !UNITY_EDITOR
                BizSimGamesLogger.Info("Platform: Android (JNI Bridge)");
                if (_config.enableAuth)
                    _authProvider = new GamesAuthController();
                if (_config.enableAchievements)
                    _achievementsProvider = new GamesAchievementController();
                if (_config.enableLeaderboards)
                    _leaderboardsProvider = new GamesLeaderboardController();
                if (_config.enableCloudSave)
                    _cloudSaveProvider = new GamesCloudSaveController();
                if (_config.enableStats)
                    _statsProvider = new GamesStatsController();
            #else
                BizSimGamesLogger.Info("Platform: Editor (Mock Provider)");
                var mockData = _config.editorMock;
                if (_config.enableAuth)
                    _authProvider = new MockAuthProvider(mockData);
                if (_config.enableAchievements)
                    _achievementsProvider = new MockAchievementProvider(mockData);
                if (_config.enableLeaderboards)
                    _leaderboardsProvider = new MockLeaderboardProvider(mockData);
                if (_config.enableCloudSave)
                    _cloudSaveProvider = new MockCloudSaveProvider(mockData);
                if (_config.enableStats)
                    _statsProvider = new MockStatsProvider(mockData);
            #endif
        }

        private void ResolveConfig()
        {
            _config = Resources.Load<GamesServicesConfig>("GamesServicesConfig");

            #pragma warning disable CS0618
            if (_config == null)
            {
                var legacyMock = Resources.Load<GamesServicesMockConfig>("DefaultGamesConfig");
                if (legacyMock != null)
                {
                    BizSimGamesLogger.Warning("Using legacy DefaultGamesConfig. " +
                        "Create a GamesServicesConfig asset for full Sidekick support.");
                    _config = CreateConfigFromLegacy(legacyMock);
                }
            }
            #pragma warning restore CS0618

            if (_config == null)
            {
                _config = ScriptableObject.CreateInstance<GamesServicesConfig>();
            }

            if (_config.hideFlags == HideFlags.None && !IsPersistedAsset(_config))
                _config.hideFlags = HideFlags.HideAndDontSave;
        }

        #pragma warning disable CS0618
        private GamesServicesConfig CreateConfigFromLegacy(GamesServicesMockConfig legacyMock)
        {
            var config = ScriptableObject.CreateInstance<GamesServicesConfig>();
            config.hideFlags = HideFlags.HideAndDontSave;
            config.editorMock.authSucceeds = legacyMock.authSucceeds;
            config.editorMock.mockPlayerId = legacyMock.mockPlayerId;
            config.editorMock.mockDisplayName = legacyMock.mockDisplayName;
            config.editorMock.mockAuthErrorType = legacyMock.mockAuthErrorType;
            config.editorMock.mockConsentGranted = legacyMock.mockConsentGranted;
            config.editorMock.mockEmail = legacyMock.mockEmail;
            config.editorMock.mockEmailVerified = legacyMock.mockEmailVerified;
            config.editorMock.mockFullName = legacyMock.mockFullName;
            config.editorMock.mockGivenName = legacyMock.mockGivenName;
            config.editorMock.mockFamilyName = legacyMock.mockFamilyName;
            config.editorMock.mockPictureUrl = legacyMock.mockPictureUrl;
            config.editorMock.mockLocale = legacyMock.mockLocale;
            config.editorMock.authDelaySeconds = legacyMock.authDelaySeconds;
            config.editorMock.mockUnlockedCount = legacyMock.mockUnlockedCount;
            config.editorMock.mockScore = legacyMock.mockScore;
            config.editorMock.mockChurnProbability = legacyMock.mockChurnProbability;
            return config;
        }
        #pragma warning restore CS0618

        private static bool IsPersistedAsset(ScriptableObject obj)
        {
            #if UNITY_EDITOR
                return UnityEditor.AssetDatabase.Contains(obj);
            #else
                return false;
            #endif
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
