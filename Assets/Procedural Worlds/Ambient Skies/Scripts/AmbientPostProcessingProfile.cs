//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
#if Mewlist_Clouds
using Mewlist;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// Post processing profile for ambient skies
    /// </summary>
    [System.Serializable]
    public class AmbientPostProcessingProfile
    {
        #region Post Processing Default Variables
        //Skybox settings
        public string name;
        public string assetName;
        public int profileIndex;

        //Default settings
        public bool defaultUsePostProcess = true;
        public bool defaultEnableEditMode = false;

        public bool defaultAoEnabled = true;
        public float defaultAoAmount = 1f;
        public Color32 defaultAoColor = new Color32(0, 0, 0, 0);

        public bool defaultAutoExposureEnabled = true;
        public float defaultExposureAmount = 0.85f;
        public float defaultExposureMin = -0.5f;
        public float defaultExposureMax = 0f;

        public bool defaultBloomEnabled = true;
        public float defaultBloomAmount = 2f;
        public float defaultBloomThreshold = 1f;
        public Texture2D defaultLensTexture;
        public float defaultLensIntensity = 1f;

        public bool defaultChromaticAberrationEnabled = true;
        public float defaultChromaticAberrationIntensity = 0.07f;

        public bool defaultColorGradingEnabled = true;
        public Texture2D defaultColorGradingLut;
        public float defaultColorGradingPostExposure = 0.2f;
        public Color32 defaultColorGradingColorFilter = new Color32(255, 255, 255, 255);
        public int defaultColorGradingTempature = 0;
        public int defaultColorGradingTint = 0;
        public float defaultColorGradingSaturation = 5f;
        public float defaultColorGradingContrast = 15f;

        public bool defaultDepthOfFieldEnabled = true;
        public float defaultDepthOfFieldFocusDistance;
        public float defaultDepthOfFieldAperture = 7.5f;
        public float defaultDepthOfFieldFocalLength = 30f;
        public AmbientSkiesConsts.DOFTrackingType defaultDepthOfFieldTrackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;
        public float defaultFocusOffset = 0f;
        public LayerMask defaultTargetLayer;
        public float defaultMaxFocusDistance = 1000f;

        public bool defaultDistortionEnabled = true;
        public float defaultDistortionIntensity = 19f;
        public float defaultDistortionScale = 1.02f;

        public bool defaultGrainEnabled = true;
        public float defaultGrainIntensity = 0.1f;
        public float defaultGrainSize = 0.5f;

        public bool defaultScreenSpaceReflectionsEnabled = true;
        public int defaultMaximumIterationCount = 16;
        public float defaultThickness = 8f;

#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessProfile defaultCustomPostProcessingProfile;
        public GradingMode defaultColorGradingMode = GradingMode.HighDefinitionRange;
        public KernelSize defaultMaxBlurSize = KernelSize.Medium;
        public ScreenSpaceReflectionResolution defaultSpaceReflectionResolution = ScreenSpaceReflectionResolution.Downsampled;
        public ScreenSpaceReflectionPreset defaultScreenSpaceReflectionPreset = ScreenSpaceReflectionPreset.Low;
        public AmbientOcclusionMode defaultAmbientOcclusionMode = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
#endif
        public float defaultMaximumMarchDistance = 250f;
        public float defaultDistanceFade = 0.5f;
        public float defaultScreenSpaceVignette = 0.5f;

        public bool defaultVignetteEnabled = true;
        public float defaultVignetteIntensity = 0.2f;
        public float defaultVignetteSmoothness = 0.7f;

        public bool defaultMotionBlurEnabled = true;
        public int defaultShutterAngle = 142;
        public int defaultSampleCount = 13;

        public AmbientSkiesConsts.AntiAliasingMode defaultAntiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.SMAA;
        public AmbientSkiesConsts.HDRMode defaultHDRMode = AmbientSkiesConsts.HDRMode.On;
        public AmbientSkiesConsts.DepthOfFieldMode defaultDepthOfFieldMode = AmbientSkiesConsts.DepthOfFieldMode.Manual;

        #if Mewlist_Clouds
        //Massive Clouds System Variables
        public bool defaultMassiveCloudsEnabled = false;
        public MassiveCloudsProfile defaultCloudProfile;
        public bool defaultSyncGlobalFogColor = true;
        public Color32 defaultCloudsFogColor = new Color32(200, 200, 230, 255);
        public bool defaultSyncBaseFogColor = true;
        public Color32 defaultCloudsBaseFogColor = new Color32(200, 200, 230, 255);
        public bool defaultCloudIsHDRP = false;
        #endif
        #endregion

        #region Post Processing Current Variables
        //Current settings
        public bool usePostProcess = true;
        public bool enableEditMode = false;

        public bool aoEnabled = true;
        public float aoAmount = 1f;
        public Color32 aoColor = new Color32(0, 0, 0, 0);

        public bool autoExposureEnabled = true;
        public float exposureAmount = 0.85f;
        public float exposureMin = -0.5f;
        public float exposureMax = 0f;

        public bool bloomEnabled = true;
        public float bloomAmount = 2f;
        public float bloomThreshold = 1f;
        public Texture2D lensTexture;
        public float lensIntensity = 1f;

        public bool chromaticAberrationEnabled = true;
        public float chromaticAberrationIntensity = 0.07f;

        public bool colorGradingEnabled = true;
        public Texture2D colorGradingLut;
        public float colorGradingPostExposure = 0.2f;
        public Color32 colorGradingColorFilter = new Color32(255, 255, 255, 255);   
        public int colorGradingTempature = 0;
        public int colorGradingTint = 0;
        public float colorGradingSaturation = 5f;
        public float colorGradingContrast = 15f;

        public bool depthOfFieldEnabled = true;
        public float depthOfFieldFocusDistance = 1f;
        public float depthOfFieldAperture = 7.5f;
        public float depthOfFieldFocalLength = 30f;
        public AmbientSkiesConsts.DOFTrackingType depthOfFieldTrackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;
        public float focusOffset = 0f;
        public LayerMask targetLayer = 1;
        public float maxFocusDistance = 1000f;

        public bool distortionEnabled = true;
        public float distortionIntensity = 19f;
        public float distortionScale = 1.02f;

        public bool grainEnabled = true;
        public float grainIntensity = 0.1f;
        public float grainSize = 0.5f;

        public bool screenSpaceReflectionsEnabled = true;
        public int maximumIterationCount = 16;
        public float thickness = 8f;

#if UNITY_POST_PROCESSING_STACK_V2
        public GradingMode colorGradingMode = GradingMode.HighDefinitionRange;
        public PostProcessProfile customPostProcessingProfile;
        public KernelSize maxBlurSize = KernelSize.Medium;
        public ScreenSpaceReflectionResolution spaceReflectionResolution = ScreenSpaceReflectionResolution.Downsampled;
        public ScreenSpaceReflectionPreset screenSpaceReflectionPreset = ScreenSpaceReflectionPreset.Low;
        public AmbientOcclusionMode ambientOcclusionMode = AmbientOcclusionMode.MultiScaleVolumetricObscurance;
#endif
        public float maximumMarchDistance = 250f;
        public float distanceFade = 0.5f;
        public float screenSpaceVignette = 0.5f;

        public bool vignetteEnabled = true;
        public float vignetteIntensity = 0.2f;
        public float vignetteSmoothness = 0.7f;

        public bool motionBlurEnabled = true;
        public int shutterAngle = 142;
        public int sampleCount = 13;

        public AmbientSkiesConsts.AntiAliasingMode antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.SMAA;
        public AmbientSkiesConsts.HDRMode hDRMode = AmbientSkiesConsts.HDRMode.On;
        public AmbientSkiesConsts.DepthOfFieldMode depthOfFieldMode = AmbientSkiesConsts.DepthOfFieldMode.Manual;

        #if Mewlist_Clouds
        //Massive Clouds System Variables
        public bool massiveCloudsEnabled = false;
        public MassiveCloudsProfile cloudProfile;
        public bool syncGlobalFogColor = true;
        public Color32 cloudsFogColor = new Color32(200, 200, 230, 255);
        public bool syncBaseFogColor = true;
        public Color32 cloudsBaseFogColor = new Color32(200, 200, 230, 255);
        public bool cloudIsHDRP = false;
        #endif
        #endregion

        #region Default Setups
        /// <summary>
        /// Revert current settings back to default settings
        /// </summary>
        public void RevertToDefault()
        {
            usePostProcess = defaultUsePostProcess;
            enableEditMode = defaultEnableEditMode;
            hDRMode = defaultHDRMode;
            depthOfFieldMode = defaultDepthOfFieldMode;

            aoEnabled = defaultAoEnabled;
            aoAmount = defaultAoAmount;
            aoColor = defaultAoColor;

            autoExposureEnabled = defaultAutoExposureEnabled;
            exposureAmount = defaultExposureAmount;
            exposureMin = defaultExposureMin;
            exposureMax = defaultExposureMax;

            bloomEnabled = defaultBloomEnabled;
            bloomAmount = defaultBloomAmount;
            bloomThreshold = defaultBloomThreshold;
            lensTexture = defaultLensTexture;
            lensIntensity = defaultLensIntensity;

            chromaticAberrationEnabled = defaultChromaticAberrationEnabled;
            chromaticAberrationIntensity = defaultChromaticAberrationIntensity;

            colorGradingEnabled = defaultColorGradingEnabled;
            colorGradingLut = defaultColorGradingLut;
            colorGradingColorFilter = defaultColorGradingColorFilter;
            colorGradingPostExposure = defaultColorGradingPostExposure;
            colorGradingTempature = defaultColorGradingTempature;
            colorGradingTint = defaultColorGradingTint;
            colorGradingSaturation = defaultColorGradingSaturation;
            colorGradingContrast = defaultColorGradingContrast;

            depthOfFieldEnabled = defaultDepthOfFieldEnabled;
            depthOfFieldFocusDistance = defaultDepthOfFieldFocusDistance;
            depthOfFieldAperture = defaultDepthOfFieldAperture;
            depthOfFieldFocalLength = defaultDepthOfFieldFocalLength;
            depthOfFieldTrackingType = defaultDepthOfFieldTrackingType;
            focusOffset = defaultFocusOffset;
            targetLayer = defaultTargetLayer;
            maxFocusDistance = defaultMaxFocusDistance;

            distortionEnabled = defaultDistortionEnabled;
            distortionIntensity = defaultDistortionIntensity;
            distortionScale = defaultDistortionScale;

            grainEnabled = defaultGrainEnabled;
            grainIntensity = defaultGrainIntensity;
            grainSize = defaultGrainSize;

            screenSpaceReflectionsEnabled = defaultScreenSpaceReflectionsEnabled;
            maximumIterationCount = defaultMaximumIterationCount;
            thickness = defaultThickness;
#if UNITY_POST_PROCESSING_STACK_V2
            colorGradingMode = defaultColorGradingMode;
            customPostProcessingProfile = defaultCustomPostProcessingProfile;
            maxBlurSize = defaultMaxBlurSize;
            spaceReflectionResolution = defaultSpaceReflectionResolution;
            screenSpaceReflectionPreset = defaultScreenSpaceReflectionPreset;
            ambientOcclusionMode = defaultAmbientOcclusionMode;
#endif
            maximumMarchDistance = defaultMaximumMarchDistance;
            distanceFade = defaultDistanceFade;
            screenSpaceVignette = defaultScreenSpaceVignette;

            vignetteEnabled = defaultVignetteEnabled;
            vignetteIntensity = defaultVignetteIntensity;
            vignetteSmoothness = defaultVignetteSmoothness;

            motionBlurEnabled = defaultMotionBlurEnabled;
            shutterAngle = defaultShutterAngle;
            sampleCount = defaultSampleCount;

            antiAliasingMode = defaultAntiAliasingMode;

#if Mewlist_Clouds
            //Massive Clouds System Variables
            massiveCloudsEnabled = defaultMassiveCloudsEnabled;
            cloudProfile = defaultCloudProfile;
            syncGlobalFogColor = defaultSyncGlobalFogColor;
            cloudsFogColor = defaultCloudsFogColor;
            syncBaseFogColor = defaultSyncBaseFogColor;
            cloudsBaseFogColor = defaultCloudsBaseFogColor;
#endif

        }

        /// <summary>
        /// Save current settings to default settings
        /// </summary>
        public void SaveCurrentToDefault()
        {
            defaultUsePostProcess = usePostProcess;
            defaultEnableEditMode = enableEditMode;
            defaultHDRMode = hDRMode;
            defaultDepthOfFieldMode = depthOfFieldMode;

            defaultAoEnabled = aoEnabled;
            defaultAoAmount = aoAmount;
            defaultAoColor = aoColor;

            defaultAutoExposureEnabled = autoExposureEnabled;
            defaultExposureAmount = exposureAmount;

            defaultBloomEnabled = bloomEnabled;
            defaultBloomAmount = bloomAmount;
            defaultBloomThreshold = bloomThreshold;
            defaultLensTexture = lensTexture;
            defaultLensIntensity = lensIntensity;

            defaultChromaticAberrationEnabled = chromaticAberrationEnabled;
            defaultChromaticAberrationIntensity = chromaticAberrationIntensity;

            defaultColorGradingEnabled = colorGradingEnabled;
            defaultColorGradingLut = colorGradingLut;
            defaultColorGradingColorFilter = colorGradingColorFilter;
            defaultColorGradingPostExposure = colorGradingPostExposure;
            defaultColorGradingTempature = colorGradingTempature;
            defaultColorGradingTint = colorGradingTint;
            defaultColorGradingSaturation = colorGradingSaturation;
            defaultColorGradingContrast = colorGradingContrast;

            defaultDepthOfFieldEnabled = depthOfFieldEnabled;
            defaultDepthOfFieldFocusDistance = depthOfFieldFocusDistance;
            defaultDepthOfFieldAperture = depthOfFieldAperture;
            defaultDepthOfFieldFocalLength = depthOfFieldFocalLength;
            defaultDepthOfFieldTrackingType = depthOfFieldTrackingType;

            defaultDistortionEnabled = distortionEnabled;
            defaultDistortionIntensity = distortionIntensity;
            defaultDistortionScale = distortionScale;

            defaultGrainEnabled = grainEnabled;
            defaultGrainIntensity = grainIntensity;
            defaultGrainSize = grainSize;

            defaultScreenSpaceReflectionsEnabled = screenSpaceReflectionsEnabled;
            defaultMaximumIterationCount = maximumIterationCount;
            defaultThickness = thickness;
#if UNITY_POST_PROCESSING_STACK_V2
            defaultColorGradingMode = colorGradingMode;
            defaultCustomPostProcessingProfile = customPostProcessingProfile;
            defaultMaxBlurSize = maxBlurSize;
            defaultSpaceReflectionResolution = spaceReflectionResolution;
            defaultScreenSpaceReflectionPreset = screenSpaceReflectionPreset;
            defaultAmbientOcclusionMode = ambientOcclusionMode;
#endif
            defaultMaximumMarchDistance = maximumMarchDistance;
            defaultDistanceFade = distanceFade;
            defaultScreenSpaceVignette = screenSpaceVignette;

            defaultVignetteEnabled = vignetteEnabled;
            defaultVignetteIntensity = vignetteIntensity;
            defaultVignetteSmoothness = vignetteSmoothness;

            defaultMotionBlurEnabled = motionBlurEnabled;
            defaultShutterAngle = shutterAngle;
            defaultSampleCount = sampleCount;

            defaultAntiAliasingMode = antiAliasingMode;

#if Mewlist_Clouds
            //Massive Clouds System Variables
            defaultMassiveCloudsEnabled = massiveCloudsEnabled;
            defaultCloudProfile = cloudProfile;
            defaultSyncGlobalFogColor = syncGlobalFogColor;
            defaultCloudsFogColor = cloudsFogColor;
            defaultSyncBaseFogColor = syncBaseFogColor;
            defaultCloudsBaseFogColor = cloudsBaseFogColor;
#endif
        }
        #endregion
    }
}