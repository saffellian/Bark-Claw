//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// This class wraps a list of ambient sky profiles
    /// </summary>
    [System.Serializable]
    public class AmbientSkyProfiles : ScriptableObject
    {
        #region Variables

        [Header("Profile Settings")]

        [SerializeField]
        public bool m_editSettings = false;

        [SerializeField]
        public string m_version = "Version 1.0";

        [SerializeField]
        public bool m_showDebug = false;

        [Header("Profile Overwrite")]

        [SerializeField]
        public bool m_overwriteGlboalSkiesProfile = false;

        [SerializeField]
        public AmbientSkyProfiles m_overwriteSkiesProfile;

        [Header("Global Settings")]

        [SerializeField]
        public AmbientSkiesConsts.RenderPipelineSettings m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

        [SerializeField]
        public AmbientSkiesConsts.PlatformTarget targetPlatform = AmbientSkiesConsts.PlatformTarget.DekstopAndConsoles;

        [SerializeField]
        public AmbientSkiesConsts.SystemTypes systemTypes = AmbientSkiesConsts.SystemTypes.AmbientSkies;

        [SerializeField]
        public AmbientSkiesConsts.DisableAndEnable useTimeOfDay;

        [SerializeField]
        public AmbientSkiesConsts.VSyncMode vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;

        [SerializeField]
        public AmbientSkiesConsts.AutoConfigureType configurationType = AmbientSkiesConsts.AutoConfigureType.Terrain;

        [SerializeField]
        public AmbientSkiesConsts.SkyType skyTypeNonHD = AmbientSkiesConsts.SkyType.HDRISky;

        [SerializeField]
        public AmbientSkiesConsts.VolumeSkyType skyType = AmbientSkiesConsts.VolumeSkyType.HDRISky;

        [Header("Time Of Day Settings")]

        [SerializeField]
        public bool syncPostProcessing = true;

        [SerializeField]
        public bool realtimeGIUpdate = false;

        [SerializeField]
        public int gIUpdateInterval = 15;

        [SerializeField]
        public float dayLengthInSeconds = 120f;

        [SerializeField]
        public int dayDate = 18;

        [SerializeField]
        public int monthDate = 5;

        [SerializeField]
        public int yearDate = 2019;

        [SerializeField]
        public AmbientSkiesConsts.CurrentSeason currentSeason;

        [SerializeField]
        public AmbientSkiesConsts.HemisphereOrigin hemisphereOrigin;

        [SerializeField]
        public AmbientSkiesTimeOfDayProfile timeOfDayProfile;

        [SerializeField]
        public KeyCode pauseTimeKey = KeyCode.P;

        [SerializeField]
        public KeyCode incrementUpKey = KeyCode.Q;

        [SerializeField]
        public KeyCode incrementDownKey = KeyCode.E;

        [SerializeField]
        public KeyCode rotateSunLeftKey = KeyCode.I;

        [SerializeField]
        public KeyCode rotateSunRightKey = KeyCode.O;

        [SerializeField]
        public float timeToAddOrRemove = 0.025f;

        [SerializeField]
        public float sunRotationAmount = 15f;

        [SerializeField]
        public bool pauseTime = false;

        [SerializeField]
        public float currentTimeOfDay = 0.5f;

        [SerializeField]
        public float skyboxRotation = 0f;

        [SerializeField]
        public float nightLengthInSeconds = 150f;

        [SerializeField]
        public AnimationCurve daySunIntensity;

        [SerializeField]
        public Gradient daySunGradientColor;

        [SerializeField]
        public AnimationCurve nightSunIntensity;

        [SerializeField]
        public Gradient nightSunGradientColor;

        [SerializeField]
        public float startFogDistance = 20f;

        [SerializeField]
        public AnimationCurve dayFogDensity;

        [SerializeField]
        public AnimationCurve nightFogDensity;

        [SerializeField]
        public AnimationCurve dayFogDistance;

        [SerializeField]
        public Gradient dayFogColor;

        [SerializeField]
        public AnimationCurve nightFogDistance;

        [SerializeField]
        public Gradient nightFogColor;

        [SerializeField]
        public Gradient dayPostFXColor;

        [SerializeField]
        public Gradient nightPostFXColor;

        [SerializeField]
        public AnimationCurve dayTempature;

        [SerializeField]
        public AnimationCurve nightTempature;

        [SerializeField]
        public AnimationCurve lightAnisotropy;

        [SerializeField]
        public AnimationCurve lightProbeDimmer;

        [SerializeField]
        public AnimationCurve lightDepthExtent;

        [SerializeField]
        public AnimationCurve sunSize;

        [SerializeField]
        public AnimationCurve skyExposure;

        [Header("Profiles")]

        [SerializeField]
        public List<AmbientSkyboxProfile> m_skyProfiles = new List<AmbientSkyboxProfile>();

        [SerializeField]
        public List<AmbientPostProcessingProfile> m_ppProfiles = new List<AmbientPostProcessingProfile>();

        [SerializeField]
        public List<AmbientLightingProfile> m_lightingProfiles = new List<AmbientLightingProfile>();

        #endregion

        #region Utils
        /// <summary>
        /// Create sky profile asset
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Ambient Skies/Sky Profiles")]
        public static void CreateSkyProfiles()
        {
            AmbientSkyProfiles asset = ScriptableObject.CreateInstance<AmbientSkyProfiles>();
            #if UNITY_EDITOR
            AssetDatabase.CreateAsset(asset, "Assets/Ambient Skies Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            #endif
        }
#endif

        /// <summary>
        /// Revert back to default settings
        /// </summary>
        /// <param name="profileIdx"></param>
        public void RevertSkyboxSettingsToDefault(int profileIdx)
        {
            if (profileIdx >= 0 && profileIdx < m_skyProfiles.Count)
            {
                m_skyProfiles[profileIdx].RevertToDefault();
            }
        }

        /// <summary>
        /// Copy settings to defaults
        /// </summary>
        /// <param name="profileIdx"></param>
        public void SaveSkyboxSettingsToDefault(int profileIdx)
        {
            if (profileIdx >= 0 && profileIdx < m_skyProfiles.Count)
            {
                m_skyProfiles[profileIdx].SaveCurrentToDefault();
            }
        }

        /// <summary>
        /// Revert back to default settings
        /// </summary>
        /// <param name="profileIdx"></param>
        public void RevertPostProcessingSettingsToDefault(int profileIdx)
        {
            if (profileIdx >= 0 && profileIdx < m_ppProfiles.Count)
            {
                m_ppProfiles[profileIdx].RevertToDefault();
            }
        }

        /// <summary>
        /// Copy settings to defaults
        /// </summary>
        /// <param name="profileIdx"></param>
        public void SavePostProcessingSettingsToDefault(int profileIdx)
        {
            if (profileIdx >= 0 && profileIdx < m_ppProfiles.Count)
            {
                m_ppProfiles[profileIdx].SaveCurrentToDefault();
            }
        }
        #endregion
    }
}