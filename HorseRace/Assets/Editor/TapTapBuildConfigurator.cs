#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace HorseRace.Editor
{
    public static class TapTapBuildConfigurator
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";
        private const string PortraitSizeName = "Portrait 1080x1920";

        [InitializeOnLoadMethod]
        private static void QueuePortraitGameView()
        {
            EditorApplication.delayCall += SetPortraitGameView;
        }

        [MenuItem("HorseRace/Apply TapTap Android Settings")]
        public static void ApplyTapTapAndroidSettings()
        {
            PlayerSettings.companyName = "CodexPrototype";
            PlayerSettings.productName = "HorseRace MVP";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.codexprototype.horseracemvp");

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            PlayerSettings.Android.androidIsGame = true;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            SetPortraitGameView();
            Debug.Log("HorseRace MVP TapTap Android settings applied.");
        }

        [MenuItem("HorseRace/Set Game View Portrait 1080x1920")]
        public static void SetPortraitGameView()
        {
            try
            {
                Type gameViewSizeGroupType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizeGroupType");
                object standaloneGroupType = Enum.Parse(gameViewSizeGroupType, "Standalone");
                EnsurePortraitSize(standaloneGroupType, out int standaloneIndex);

                try
                {
                    object androidGroupType = Enum.Parse(gameViewSizeGroupType, "Android");
                    EnsurePortraitSize(androidGroupType, out _);
                }
                catch
                {
                    // Android build support is not required for editor preview sizing.
                }

                Type gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
                EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
                PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                selectedSizeIndex?.SetValue(gameView, standaloneIndex, null);
                gameView.Show();
                gameView.Repaint();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Could not set Game View portrait size automatically: " + exception.Message);
            }
        }

        private static void EnsurePortraitSize(object groupTypeValue, out int sizeIndex)
        {
            object group = GetGameViewSizeGroup(groupTypeValue);
            sizeIndex = FindGameViewSizeIndex(group, PortraitSizeName);
            if (sizeIndex >= 0)
            {
                return;
            }

            Type gameViewSizeType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSize");
            Type gameViewSizeModeType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizeType");
            object fixedResolution = Enum.Parse(gameViewSizeModeType, "FixedResolution");
            object portraitSize = Activator.CreateInstance(gameViewSizeType, fixedResolution, 1080, 1920, PortraitSizeName);
            group.GetType().GetMethod("AddCustomSize")?.Invoke(group, new[] { portraitSize });
            sizeIndex = FindGameViewSizeIndex(group, PortraitSizeName);
        }

        private static object GetGameViewSizeGroup(object groupTypeValue)
        {
            Type gameViewSizesType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameViewSizes");
            Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(gameViewSizesType);
            PropertyInfo instanceProperty = singletonType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            object instance = instanceProperty?.GetValue(null, null);
            return gameViewSizesType.GetMethod("GetGroup")?.Invoke(instance, new[] { groupTypeValue });
        }

        private static int FindGameViewSizeIndex(object group, string displayName)
        {
            MethodInfo totalCountMethod = group.GetType().GetMethod("GetTotalCount");
            MethodInfo getGameViewSizeMethod = group.GetType().GetMethod("GetGameViewSize");
            int totalCount = (int)(totalCountMethod?.Invoke(group, null) ?? 0);

            for (int i = 0; i < totalCount; i++)
            {
                object size = getGameViewSizeMethod?.Invoke(group, new object[] { i });
                string text = size?.GetType().GetProperty("displayText")?.GetValue(size, null) as string;
                if (text == displayName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
#endif

