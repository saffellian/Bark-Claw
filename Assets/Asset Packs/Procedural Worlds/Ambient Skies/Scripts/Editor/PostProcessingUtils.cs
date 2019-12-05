//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

namespace AmbientSkies
{
    /// <summary>
    /// A class manipulate Unity Post Processing v2.
    /// The system has a global overide capability as well;
    /// </summary>
    public static class PostProcessingUtils
    {
#if UNITY_POST_PROCESSING_STACK_V2
        private static PostProcessProfile postProcessProfile;
#endif

        #region Utils

        /// <summary>
        /// Returns true if post processing v2 is installed
        /// </summary>
        /// <returns>True if installed</returns>
        public static bool PostProcessingInstalled()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            return true;
#else
            return false;
#endif
        }

        #region Get/Set From Profile

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromProfileName(AmbientSkyProfiles profiles, string name)
        {
            for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
            {
                if (profiles.m_ppProfiles[idx].name == name)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get current profile index of currently active post processing profile
        /// </summary>
        /// <param name="profile">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromPostProcessing(AmbientSkyProfiles profiles)
        {
            #if UNITY_POST_PROCESSING_STACK_V2
            PostProcessProfile profile = GetGlobalPostProcessingProfile();
            if (profile == null)
            {
                return 0;
            }
            else
            {
                for (int idx = 0; idx < profiles.m_ppProfiles.Count; idx++)
                {
                    if (profiles.m_ppProfiles[idx].assetName == profile.name)
                    {
                        return idx;
                    }
                }
            }
            #endif
            return -1;
        }

        /// <summary>
        /// Get the currently active global post processing profile
        /// </summary>
        /// <returns>Currently active global post processing profile or null if there is none / its not set up properly</returns>
#if UNITY_POST_PROCESSING_STACK_V2
        public static PostProcessProfile GetGlobalPostProcessingProfile()
        {
            //Get global post processing object
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");
            if (postProcessVolumeObj == null)
            {
                return null;
            }

            //Get global post processing volume
            PostProcessVolume postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
            if (postProcessVolume == null)
            {
                return null;
            }

            //Return its profile
            return postProcessVolume.sharedProfile;
    }
#endif

        /// <summary>
        /// Get a profile from the asset name
        /// </summary>
        /// <param name="profiles">List of profiles</param>
        /// <param name="profileAssetName">Asset name we are looking for</param>
        /// <returns>Profile or null if not found</returns>
        public static AmbientPostProcessingProfile GetProfileFromAssetName(AmbientSkyProfiles profiles, string profileAssetName)
        {
            if (profiles == null)
            {
                return null;
            }
            return profiles.m_ppProfiles.Find(x => x.assetName == profileAssetName);
        }

        /// <summary>
        /// Load the selected profile and apply it
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileName(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, string profileName, bool useDefaults)
        {
            AmbientPostProcessingProfile p = profile.m_ppProfiles.Find(x => x.name == profileName);
            if (p == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply post processing profile!");
                return;
            }
            SetPostProcessingProfile(p, profile, useDefaults);
        }

        /// <summary>
        /// Load the selected profile 
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="assetName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromAssetName(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, string assetName, bool useDefaults)
        {
            AmbientPostProcessingProfile p = profile.m_ppProfiles.Find(x => x.assetName == assetName);
            if (p == null)
            {
                Debug.LogWarning("Invalid asset name supplied, can not apply post processing settings!");
                return;
            }
            SetPostProcessingProfile(p, profile, useDefaults);
        }

        /// <summary>
        /// Load the selected profile and apply
        /// </summary>
        /// <param name="profile">The profiles object</param>
        /// <param name="profileIndex">The zero based index to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileIndex(AmbientSkyProfiles profile, AmbientSkyboxProfile skyProfile, int profileIndex, bool useDefaults)
        {
            if (profileIndex < 0 || profileIndex >= profile.m_ppProfiles.Count)
            {
                Debug.LogWarning("Invalid profile index selected, can not apply post processing settings!");
                return;
            }
            if (profile.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                SetHDRPPostProcessingProfile(profile.m_ppProfiles[profileIndex], profile, useDefaults);
            }
            else
            {
                SetPostProcessingProfile(profile.m_ppProfiles[profileIndex], profile, useDefaults);
            }
        }

        #endregion

        #region Post Processing Setup

        /// <summary>
        /// Set the specified post processing profile up.
        /// </summary>
        /// <param name="profile">Profile to set up</param>
        /// <param name="useDefaults">Use defaults or current</param>
        public static void SetPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (profile.usePostProcess)
            {
#if GAIA_PRESENT
                SkyboxUtils.SetGaiaParenting(true);
#endif
                if (skyProfile.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    GameObject postProcessingHDRPObject = GameObject.Find("Post Processing HDRP Volume");
                    if (postProcessingHDRPObject != null)
                    {
                        Object.DestroyImmediate(postProcessingHDRPObject);
                    }
                }

                //Get the FX parent
                GameObject theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
                GameObject mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();

                Camera camera = mainCameraObj.GetComponent<Camera>();
                if (camera != null)
                {
                    if (profile.hDRMode == AmbientSkiesConsts.HDRMode.On)
                    {
                        camera.allowHDR = true;
                    }
                    else
                    {
                        camera.allowHDR = false;
                    }
                }

                //Setup the camera up to support post processing
                PostProcessLayer cameraProcessLayer = mainCameraObj.GetComponent<PostProcessLayer>();
                if (cameraProcessLayer == null)
                {
                    cameraProcessLayer = mainCameraObj.AddComponent<PostProcessLayer>();
                }
                cameraProcessLayer.volumeTrigger = mainCameraObj.transform;
                cameraProcessLayer.volumeLayer = 2;

                switch (profile.antiAliasingMode)
                {
                    case AmbientSkiesConsts.AntiAliasingMode.None:
                        cameraProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.FXAA:
                        cameraProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.SMAA:
                        cameraProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.TAA:
                        cameraProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                        camera.allowMSAA = false;
                        break;
                    case AmbientSkiesConsts.AntiAliasingMode.MSAA:
                        if (camera.renderingPath == RenderingPath.DeferredShading)
                        {
                            if (EditorUtility.DisplayDialog("Warning!", "MSAA is not supported in deferred rendering path. Switching Anti Aliasing to none, please select another option that supported in deferred rendering path or set your rendering path to forward to use MSAA", "Ok"))
                            {
                                cameraProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                                profile.antiAliasingMode = AmbientSkiesConsts.AntiAliasingMode.None;
                            }
                        }
                        else
                        {
                            cameraProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                            camera.allowMSAA = true;
                        }
                        break;
                }

                cameraProcessLayer.fog.excludeSkybox = true;
                cameraProcessLayer.fog.enabled = true;
                cameraProcessLayer.stopNaNPropagation = true;

                //Find or create global post processing volume object
                GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");
                if (postProcessVolumeObj == null)
                {
                    postProcessVolumeObj = new GameObject("Global Post Processing");
                    postProcessVolumeObj.transform.parent = theParent.transform;
                    postProcessVolumeObj.layer = LayerMask.NameToLayer("TransparentFX");
                    postProcessVolumeObj.AddComponent<PostProcessVolume>();
                }

                if (profile.depthOfFieldEnabled)
                {
                    if (profile.depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                    {
                        AutoFocus autoFocus = camera.gameObject.GetComponent<AutoFocus>();
                        if (autoFocus == null)
                        {
                            autoFocus = camera.gameObject.AddComponent<AutoFocus>();
                            autoFocus.m_processingProfile = profile;
                            autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                            autoFocus.m_focusOffset = profile.focusOffset;
                            autoFocus.m_targetLayer = profile.targetLayer;
                            autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                            autoFocus.m_actualFocusDistance = profile.depthOfFieldFocusDistance;
                            profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        }
                        else
                        {
                            autoFocus.m_processingProfile = profile;
                            autoFocus.m_trackingType = profile.depthOfFieldTrackingType;
                            autoFocus.m_focusOffset = profile.focusOffset;
                            autoFocus.m_targetLayer = profile.targetLayer;
                            autoFocus.m_maxFocusDistance = profile.maxFocusDistance;
                            profile.depthOfFieldFocusDistance = autoFocus.m_actualFocusDistance;
                        }
                    }
                    else
                    {
                        AutoFocus autoFocus = camera.gameObject.GetComponent<AutoFocus>();
                        if (autoFocus != null)
                        {
                            Object.DestroyImmediate(autoFocus);
                        }
                    }
                }
                else
                {
                    AutoFocus autoFocus = camera.gameObject.GetComponent<AutoFocus>();
                    if (autoFocus != null)
                    {
                        Object.DestroyImmediate(autoFocus);
                    }
                }

                //Setup the global post processing volume
                PostProcessVolume postProcessVolume = postProcessVolumeObj.GetComponent<PostProcessVolume>();
                postProcessVolume.isGlobal = true;
                postProcessVolume.priority = 0f;
                postProcessVolume.weight = 1f;
                postProcessVolume.blendDistance = 0f;

                if (postProcessVolume != null)
                {
                    postProcessProfile = postProcessVolume.sharedProfile;

                    bool loadProfile = false;
                    if (postProcessVolume.sharedProfile == null)
                    {
                        loadProfile = true;
                    }
                    else
                    {
                        if (postProcessVolume.sharedProfile.name != profile.assetName)
                        {
                            loadProfile = true;
                        }
                    }
                    if (loadProfile)
                    {
                        //Get the profile path
                        string postProcessPath = SkyboxUtils.GetAssetPath(profile.assetName);
                        if (string.IsNullOrEmpty(postProcessPath))
                        {
                            Debug.LogErrorFormat("AmbientSkies:SetPostProcessingProfile() : Unable to load '{0}' profile - Aborting!", profile.assetName);
                            return;
                        }
                        postProcessVolume.sharedProfile = AssetDatabase.LoadAssetAtPath<PostProcessProfile>(postProcessPath);
                        postProcessProfile = postProcessVolume.sharedProfile;
                    }

                    if (postProcessProfile == null)
                    {
                        return;
                    }
                    else
                    {
#if Mewlist_Clouds
                        MassiveCloudsUtils.SetupMassiveCloudsSystem(profile);
#endif
                        EditorUtility.SetDirty(postProcessProfile);

                        //Get the configurable values
                        AmbientOcclusion ao;
                        if (postProcessProfile.TryGetSettings(out ao))
                        {
                            ao.active = profile.aoEnabled;
                            ao.intensity.value = profile.aoAmount;
                            ao.color.value = profile.aoColor;
                            if (!ao.color.overrideState)
                            {
                                ao.color.overrideState = true;
                            }
#if UNITY_POST_PROCESSING_STACK_V2
                            ao.mode.value = profile.ambientOcclusionMode;
#endif
                        }

                        AutoExposure autoExposure;
                        if (postProcessProfile.TryGetSettings(out autoExposure))
                        {
                            autoExposure.active = profile.autoExposureEnabled;
                            autoExposure.keyValue.value = profile.exposureAmount;
                            autoExposure.minLuminance.value = profile.exposureMin;
                            autoExposure.maxLuminance.value = profile.exposureMax;
                        }

                        Bloom bloom;
                        if (postProcessProfile.TryGetSettings(out bloom))
                        {
                            bloom.active = profile.bloomEnabled;
                            bloom.intensity.value = profile.bloomAmount;
                            bloom.threshold.value = profile.bloomThreshold;
                            bloom.dirtTexture.value = profile.lensTexture;
                            bloom.dirtIntensity.value = profile.lensIntensity;
                        }

                        ChromaticAberration chromaticAberration;
                        if (postProcessProfile.TryGetSettings(out chromaticAberration))
                        {
                            chromaticAberration.active = profile.chromaticAberrationEnabled;
                            chromaticAberration.intensity.value = profile.chromaticAberrationIntensity;
                        }

                        ColorGrading colorGrading;
                        if (postProcessProfile.TryGetSettings(out colorGrading))
                        {
                            colorGrading.active = profile.colorGradingEnabled;

                            colorGrading.gradingMode.value = profile.colorGradingMode;
                            colorGrading.gradingMode.overrideState = true;

                            colorGrading.ldrLut.value = profile.colorGradingLut;
                            colorGrading.ldrLut.overrideState = true;

                            colorGrading.externalLut.value = profile.colorGradingLut;
                            colorGrading.externalLut.overrideState = true;

                            colorGrading.colorFilter.value = profile.colorGradingColorFilter;
                            colorGrading.colorFilter.overrideState = true;
                            colorGrading.postExposure.value = profile.colorGradingPostExposure;
                            colorGrading.postExposure.overrideState = true;
                            colorGrading.temperature.value = profile.colorGradingTempature;
                            colorGrading.tint.value = profile.colorGradingTint;
                            colorGrading.saturation.value = profile.colorGradingSaturation;
                            colorGrading.contrast.value = profile.colorGradingContrast;
                        }

                        DepthOfField dof;
                        if (postProcessProfile.TryGetSettings(out dof))
                        {
                            dof.active = profile.depthOfFieldEnabled;
                            if (profile.depthOfFieldMode != AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                            {
                                dof.focusDistance.value = profile.depthOfFieldFocusDistance;
                            }
                            else
                            {
                                profile.depthOfFieldFocusDistance = dof.focusDistance.value;
                            }

                            dof.aperture.value = profile.depthOfFieldAperture;
                            dof.focalLength.value = profile.depthOfFieldFocalLength;
                            dof.kernelSize.value = profile.maxBlurSize;
                        }

                        Grain grain;
                        if (postProcessProfile.TryGetSettings(out grain))
                        {
                            grain.active = profile.grainEnabled;
                            grain.intensity.value = profile.grainIntensity;
                            grain.size.value = profile.grainSize;
                        }

                        LensDistortion lensDistortion = null;
                        if (postProcessProfile.TryGetSettings(out lensDistortion))
                        {
                            if (profile.distortionEnabled && profile.distortionIntensity > 0f)
                            {
                                lensDistortion.active = profile.distortionEnabled;
                                lensDistortion.intensity.value = profile.distortionIntensity;
                                lensDistortion.scale.value = profile.distortionScale;
                            }
                            else
                            {
                                lensDistortion.active = false;
                            }
                        }

                        ScreenSpaceReflections screenSpaceReflections = null;
                        if (postProcessProfile.TryGetSettings(out screenSpaceReflections))
                        {
                            if (profile.screenSpaceReflectionPreset == ScreenSpaceReflectionPreset.Custom)
                            {
                                screenSpaceReflections.maximumIterationCount.overrideState = true;
                                screenSpaceReflections.thickness.overrideState = true;
                                screenSpaceReflections.resolution.overrideState = true;
                            }
                            else
                            {
                                screenSpaceReflections.maximumIterationCount.overrideState = false;
                                screenSpaceReflections.thickness.overrideState = false;
                                screenSpaceReflections.resolution.overrideState = false;
                            }

                            screenSpaceReflections.active = profile.screenSpaceReflectionsEnabled;
                            screenSpaceReflections.maximumIterationCount.value = profile.maximumIterationCount;
                            screenSpaceReflections.thickness.value = profile.thickness;
                            screenSpaceReflections.resolution.value = profile.spaceReflectionResolution;
                            screenSpaceReflections.preset.value = profile.screenSpaceReflectionPreset;
                            screenSpaceReflections.maximumMarchDistance.value = profile.maximumMarchDistance;
                            screenSpaceReflections.distanceFade.value = profile.distanceFade;
                            screenSpaceReflections.vignette.value = profile.screenSpaceVignette;
                        }

                        Vignette vignette = null;
                        if (postProcessProfile.TryGetSettings(out vignette))
                        {
                            if (profile.vignetteEnabled && profile.vignetteIntensity > 0f)
                            {
                                vignette.active = profile.vignetteEnabled;
                                vignette.intensity.value = profile.vignetteIntensity;
                                vignette.smoothness.value = profile.vignetteSmoothness;
                            }
                            else
                            {
                                vignette.active = false;
                            }
                        }

                        MotionBlur motionBlur;
                        if (postProcessProfile.TryGetSettings(out motionBlur))
                        {
                            motionBlur.active = profile.motionBlurEnabled;
                            motionBlur.shutterAngle.value = profile.shutterAngle;
                            motionBlur.sampleCount.value = profile.sampleCount;
                        }

                        if (skyProfile.targetPlatform == AmbientSkiesConsts.PlatformTarget.DekstopAndConsoles)
                        {
                            if (postProcessProfile.TryGetSettings(out bloom))
                            {
                                bloom.fastMode.overrideState = true;
                                bloom.fastMode.value = false;
                            }

                            if (postProcessProfile.TryGetSettings(out chromaticAberration))
                            {
                                chromaticAberration.fastMode.overrideState = true;
                                chromaticAberration.fastMode.value = false;
                            }

                            if (postProcessProfile.TryGetSettings(out ao))
                            {
                                ao.ambientOnly.overrideState = true;
                                ao.ambientOnly.value = true;
                            }
                        }
                        else
                        {
                            if (postProcessProfile.TryGetSettings(out bloom))
                            {
                                bloom.fastMode.overrideState = true;
                                bloom.fastMode.value = true;
                            }

                            if (postProcessProfile.TryGetSettings(out chromaticAberration))
                            {
                                chromaticAberration.fastMode.overrideState = true;
                                chromaticAberration.fastMode.value = true;
                            }
                            if (postProcessProfile.TryGetSettings(out ao))
                            {
                                ao.ambientOnly.overrideState = true;
                                ao.ambientOnly.value = false;
                            }
                        }
                    }
                }

                PostProcessVolume[] postProcessVolumes = Object.FindObjectsOfType<PostProcessVolume>();
                if (postProcessVolumes != null)
                {
                    foreach (PostProcessVolume ppVolume in postProcessVolumes)
                    {
                        UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(ppVolume, false);
                    }
                }

                SkyboxUtils.DestroyParent("Ambient Skies Environment");
            }
            else
            {
                RemovePostProcessing();
                SkyboxUtils.DestroyParent("Ambient Skies Environment");
            }
#endif
        }

        /// <summary>
        /// Sets HDRP post processing 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="skyProfile"></param>
        /// <param name="useDefaults"></param>
        public static void SetHDRPPostProcessingProfile(AmbientPostProcessingProfile profile, AmbientSkyProfiles skyProfile, bool useDefaults)
        {
#if HDPipeline
            if (profile.usePostProcess)
            {
#if !UNITY_2019_1_OR_NEWER
                SetPostProcessingProfile(profile, skyProfile, useDefaults);
#else

                GameObject theParent = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
                GameObject postVolumeObject = GameObject.Find("Post Processing HDRP Volume");
                if (postVolumeObject == null)
                {
                    postVolumeObject = new GameObject("Post Processing HDRP Volume");
                    postVolumeObject.layer = 1;
                    postVolumeObject.transform.SetParent(theParent.transform);                   
                }
                else
                {
                    postVolumeObject.layer = 1;
                    postVolumeObject.transform.SetParent(theParent.transform);
                }

                Volume volume = postVolumeObject.GetComponent<Volume>();
                if (volume == null)
                {
                    volume = postVolumeObject.AddComponent<Volume>();
                    volume.isGlobal = true;
                    volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Default Post Processing Volume Profile"));
                }
                else
                {
                    volume.isGlobal = true;
                    volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Default Post Processing Volume Profile"));
                }
            }
            else
            {
                GameObject postVolumeObject = GameObject.Find("Post Processing HDRP Volume");
                if (postVolumeObject != null)
                {
                    Object.DestroyImmediate(postVolumeObject);
                }
#endif
            }
#if !UNITY_2019_1_OR_NEWER
            else
            {
                RemovePostProcessing();
            }
#endif
#endif
        }

        /// <summary>
        /// Remove post processing from camera and scene
        /// </summary>
        public static void RemovePostProcessing()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            //Remove from camera
            GameObject mainCameraObj = SkyboxUtils.GetOrCreateMainCamera();
            PostProcessLayer cameraProcessLayer = mainCameraObj.GetComponent<PostProcessLayer>();
            if (cameraProcessLayer != null)
            {
                Object.DestroyImmediate(cameraProcessLayer);
            }

            if (mainCameraObj != null)
            {
                Camera camera = mainCameraObj.GetComponent<Camera>();
                if (camera != null && PlayerSettings.colorSpace == ColorSpace.Linear)
                {
                    camera.renderingPath = RenderingPath.Forward;
                }
            }

            //Remove from scene
            GameObject postProcessVolumeObj = GameObject.Find("Global Post Processing");
            if (postProcessVolumeObj != null)
            {
                Object.DestroyImmediate(postProcessVolumeObj);
            }

            AutoFocus autoFocus = Object.FindObjectOfType<AutoFocus>();
            if (autoFocus != null)
            {
                Object.DestroyImmediate(autoFocus);
            }
#endif
        }

        #endregion

        #endregion
    }
}