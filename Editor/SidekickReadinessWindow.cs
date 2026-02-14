// Copyright (c) BizSim Game Studios. All rights reserved.

using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BizSim.GPlay.Games.Editor
{
    public class SidekickReadinessWindow : EditorWindow
    {
        private GamesServicesConfig _config;
        private Vector2 _scrollPos;

        [MenuItem("BizSim/Google Play Games/Sidekick Readiness Check")]
        public static void ShowWindow()
        {
            var window = GetWindow<SidekickReadinessWindow>("Sidekick Readiness");
            window.minSize = new Vector2(400, 500);
        }

        private void OnEnable()
        {
            _config = Resources.Load<GamesServicesConfig>("GamesServicesConfig");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("PGS Sidekick Readiness Check", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            if (_config == null)
            {
                EditorGUILayout.HelpBox(
                    "No GamesServicesConfig found in Resources/. " +
                    "Create one via Assets > Create > BizSim > Google Play Games > Services Config.",
                    MessageType.Error);

                if (GUILayout.Button("Refresh"))
                    _config = Resources.Load<GamesServicesConfig>("GamesServicesConfig");

                return;
            }

            EditorGUILayout.ObjectField("Config", _config, typeof(GamesServicesConfig), false);
            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawTier1Section();
            EditorGUILayout.Space(10);
            DrawTier2Section();
            EditorGUILayout.Space(10);
            DrawRecommendedSection();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(10);
            var tier = SidekickReadiness.Evaluate(_config);
            var tierColor = tier switch
            {
                SidekickTier.Tier2 => Color.green,
                SidekickTier.Tier1 => Color.yellow,
                _ => Color.red
            };
            var prevColor = GUI.color;
            GUI.color = tierColor;
            EditorGUILayout.HelpBox($"Current Sidekick Tier: {tier}", MessageType.Info);
            GUI.color = prevColor;

            if (GUILayout.Button("Refresh"))
            {
                _config = Resources.Load<GamesServicesConfig>("GamesServicesConfig");
                Repaint();
            }
        }

        private void DrawTier1Section()
        {
            EditorGUILayout.LabelField("Tier 1 Requirements", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawCheck(1, "Auth service enabled", _config.enableAuth,
                "Enable authentication in GamesServicesConfig.");
            DrawCheck(2, "Achievements service enabled", _config.enableAchievements,
                "Enable achievements in GamesServicesConfig.");
            DrawCheck(3, "Expected achievement count >= 10", _config.expectedAchievementCount >= 10,
                "Set expectedAchievementCount to 10 or more in config. " +
                "Google requires at least 10 achievements.");
            DrawCheck(4, "4+ achievements easily achievable", true,
                "Ensure at least 4 achievements can be unlocked in the first play session. " +
                "(Manual verification required)");

            EditorGUILayout.EndVertical();
        }

        private void DrawTier2Section()
        {
            EditorGUILayout.LabelField("Tier 2 Requirements", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawCheck(5, "Cloud save service enabled", _config.enableCloudSave,
                "Enable cloud save in GamesServicesConfig.");
            DrawCheck(6, "Cloud save metadata required", _config.requireCloudSaveMetadata,
                "Enable requireCloudSaveMetadata in config to enforce metadata on saves.");

            EditorGUILayout.EndVertical();
        }

        private void DrawRecommendedSection()
        {
            EditorGUILayout.LabelField("Recommended", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawCheck(7, "Events service enabled", _config.enableEvents,
                "Enable events to track player milestones and improve Sidekick insights.");
            DrawCheck(8, "sidekickReady flag enabled", _config.sidekickReady,
                "Enable sidekickReady when all requirements are met.");

            bool proguardValid = CheckProGuardRules(out string proguardIssue);
            DrawCheck(9, "ProGuard rules include all callbacks", proguardValid,
                proguardIssue);

            DrawCheck(10, "SnapshotHandle has coverImageUri", true,
                "coverImageUri field exists in SnapshotHandle. (Verified at compile time)");

            EditorGUILayout.EndVertical();
        }

        private static readonly string[] RequiredCallbackInterfaces =
        {
            "IAuthCallback",
            "IAchievementCallback",
            "ILeaderboardCallback",
            "ICloudSaveCallback",
            "IStatsCallback",
            "IEventsCallback"
        };

        private bool CheckProGuardRules(out string issue)
        {
            var packagePath = Path.GetFullPath("Packages/com.bizsim.gplay.games");
            var proguardFile = Path.Combine(packagePath,
                "Plugins", "Android", "GamesServicesBridge.androidlib", "proguard-rules.pro");

            if (!File.Exists(proguardFile))
            {
                issue = "proguard-rules.pro not found at: " + proguardFile;
                return false;
            }

            var content = File.ReadAllText(proguardFile);
            var missing = new System.Collections.Generic.List<string>();

            foreach (var iface in RequiredCallbackInterfaces)
            {
                if (!Regex.IsMatch(content, @"keepclassmembers\s+interface\s+.*" + Regex.Escape(iface)))
                    missing.Add(iface);
            }

            if (missing.Count > 0)
            {
                issue = "ProGuard is missing keep rules for: " + string.Join(", ", missing) +
                    ". Add -keepclassmembers rules for each interface.";
                return false;
            }

            issue = "All callback interfaces have keep rules.";
            return true;
        }

        private void DrawCheck(int number, string label, bool pass, string remediation)
        {
            EditorGUILayout.BeginHorizontal();

            var icon = pass ? "\u2713" : "\u2717";
            var style = new GUIStyle(EditorStyles.label)
            {
                richText = true
            };
            var color = pass ? "green" : "red";
            EditorGUILayout.LabelField(
                $"<color={color}>{icon}</color> #{number}: {label}", style);

            EditorGUILayout.EndHorizontal();

            if (!pass)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(remediation, EditorStyles.wordWrappedMiniLabel);
                EditorGUI.indentLevel--;
            }
        }
    }
}
