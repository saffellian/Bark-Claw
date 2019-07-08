//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AmbientSkies.Internal;
using PWCommon1;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.Experimental.Rendering;
using System.Collections;
using UnityEditor.Rendering;
#if Mewlist_Clouds
using Mewlist;
#endif
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif

namespace AmbientSkies
{
    /// <summary>
    /// Main Workflow Editor Window
    /// </summary>
    public class AmbientSkiesEditorWindow : EditorWindow, IPWEditor
    {
        #region Editor Window Variables

        //Render Pipeline selection options
        public AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;      

        //Editor window 
        private Vector2 m_scrollPosition = Vector2.zero;
        private EditorUtils m_editorUtils;
        private TabSet m_mainTabs;

        //Main profiles
        //private List<AmbientSkyProfiles> m_profileList;
        //private int m_profileListIndex;
        private AmbientSkyProfiles m_profiles;
        private int m_selectedSkyboxProfileIndex;
        private AmbientSkyboxProfile m_selectedSkyboxProfile;
        private int m_selectedPostProcessingProfileIndex;
        private AmbientPostProcessingProfile m_selectedPostProcessingProfile;
        private AmbientLightingProfile m_selectedLightingProfile;
        private int m_selectedLightingProfileIndex;

        //private int newProfileListIndex;

        //Main camera variable for shadow distance system
        private Camera mainCam;

        //Profile choices on all 4 tabs
        //private List<string> profileChoices = new List<string>();
        private List<string> skyboxChoices = new List<string>();
        private List<string> ppChoices = new List<string>();
        private List<string> lightmappingChoices = new List<string>();

        //Position checked
        public bool PositionChecked { get; set; }

        private float m_countdown = 0.2f;

        private Texture2D m_skiesIcon;
        private Texture2D m_postProcessingIcon;
        private Texture2D m_lightingIcon;

        private bool m_massiveCloudsPath = false; 

        [SerializeField]
        private int m_createdProfileNumber;

        private bool m_isCompiling;

#if Mewlist_Clouds
        //Massive Clouds System
        private bool massiveCloudsEnabled;
        private MassiveCloudsProfile cloudProfile;
        private bool syncGlobalFogColor = true;
        private Color32 cloudsFogColor = new Color32(200, 200, 230, 255);
        private bool syncBaseFogColor = true;
        private Color32 cloudsBaseFogColor = new Color32(200, 200, 230, 255);
        private bool cloudIsHDRP = false;
#endif

        /// <summary>
        /// Foldout Bools
        /// </summary>
        
        //Global
        private bool m_foldoutGlobalSettings;

        //Skybox
        private bool m_foldoutMainSkySettings;
        private bool m_foldoutTimeOfDaySettings;
        private bool m_foldoutSkySettings;
        private bool m_foldoutFogSettings;
        private bool m_foldoutAmbientSettings;
        private bool m_foldoutSunSettings;
        private bool m_foldoutHorizonSettings;
        private bool m_foldoutShadowSettings;
        private bool m_foldoutHDShadowSettings;
        private bool m_foldoutScreenSpaceReflectionSettings;
        private bool m_foldoutScreenSpaceRefractionSettings;

        //Time Of Day
        private bool m_foldoutTODFogSettings;
        private bool m_foldoutTODHDRPSettings;
        private bool m_foldoutTODKeyBindings;
        private bool m_foldoutTODLightSettings;
        private bool m_foldoutTODRealtimeGI;
        private bool m_foldoutTODSeasons;
        private bool m_foldoutTODTime;
        private bool m_foldoutTODPostProcessing;
        private bool m_foldoutTODSkybox;

        //Post FX
        private bool m_foldoutMainPostProcessing;
        private bool m_foldoutAmbientOcclusion;
        private bool m_foldoutAutoExposure;
        private bool m_foldoutBloom;
        private bool m_foldoutColorGrading;
        private bool m_foldoutDepthOfField;
        private bool m_foldoutGrain;
        private bool m_foldoutLensDistortion;
        private bool m_foldoutScreenSpaceReflections;
        private bool m_foldoutVignette;
        private bool m_foldoutMotionBlur;
        private bool m_foldoutMassiveClouds;
        private bool m_foldoutChromaticAberration;

        //Lighting
        private bool m_foldoutMainLighting;
        private bool m_foldoutRealtimeGI;
        private bool m_foldoutBakedGI;

        #region Sky Tab Values

        //Skies variables
        private bool useSkies;
        //Sky main settings
        private AmbientSkiesConsts.SystemTypes systemtype;
        private AmbientSkiesConsts.VolumeSkyType skyType;
        private int newSkyboxSelection;
        private AmbientSkiesConsts.VolumeFogType fogType;
        private AmbientSkiesConsts.AmbientMode ambientMode;
        //Skybox settings
        private string profileName;
        private AmbientSkiesConsts.DisableAndEnable useTimeOfDay;
        private AmbientSkiesConsts.SkyType skyTypeNonHD;
        private Color skyboxTint;
        private float skyboxExposure;
        private float skyboxRotation;
        public float skyboxPitch;
        private Cubemap customSkybox;
        private bool isProceduralSkybox;
        private float proceduralSunSize;
        private float proceduralSunSizeConvergence;
        private float proceduralAtmosphereThickness;
        private Color32 proceduralGroundColor;
        private Color32 proceduralSkyTint;
        private float proceduralSkyExposure;
        private float proceduralSkyboxRotation;
        private float proceduralSkyboxPitch;
        private bool includeSunInBaking;
        //Time Of Day Settings
        private string seasonString;
        private AmbientSkiesConsts.CurrentSeason currentSeason;
        private AmbientSkiesConsts.HemisphereOrigin hemisphereOrigin;
        private AmbientSkiesTimeOfDayProfile timeOfDayProfile;
        private KeyCode pauseTimeKey;
        private KeyCode incrementUpKey;
        private KeyCode incrementDownKey;
        private KeyCode rotateSunLeftKey;
        private KeyCode rotateSunRightKey;
        private float timeToAddOrRemove;
        private float sunRotationAmount;
        private bool pauseTime;
        private bool syncPostProcessing;
        private bool realtimeGIUpdate;
        private int gIUpdateInterval;
        private float currentTimeOfDay;
        private float dayLengthInSeconds;
        private float nightLengthInSeconds;
        private int dayDate;
        private int monthDate;
        private int yearDate;
        private float timeOfDaySkyboxRotation;
        private AnimationCurve daySunIntensity;
        private Gradient daySunGradientColor;
        private AnimationCurve nightSunIntensity;
        private Gradient nightSunGradientColor;
        private float startFogDistance = 20f;
        private AnimationCurve dayFogDensity;
        private AnimationCurve nightFogDensity;
        private AnimationCurve dayFogDistance;
        private Gradient dayFogColor;
        private AnimationCurve nightFogDistance;
        private Gradient nightFogColor;
        private Gradient dayPostFXColor;
        private Gradient nightPostFXColor;
        private AnimationCurve dayTempature;
        private AnimationCurve nightTempature;
        private AnimationCurve lightAnisotropy;
        private AnimationCurve lightProbeDimmer;
        private AnimationCurve lightDepthExtent;
        private AnimationCurve sunSizeAmount;
        private AnimationCurve skyExposureAmount;
        //Fog settings
        private AmbientSkiesConsts.AutoConfigureType configurationType;
        private Color fogColor;
        private float fogDistance;
        private float nearFogDistance;
        private float fogDensity;
        private Color proceduralFogColor;
        private float proceduralFogDistance;
        private float proceduralNearFogDistance;
        private float proceduralFogDensity;
        //Ambient settings
        private Color32 skyColor;
        private Color32 equatorColor;
        private Color32 groundColor;
        private float skyboxGroundIntensity;
        private float diffuseAmbientIntensity;
        private float specularAmbientIntensity;
        //Sun settings
        public float shadowStrength;
        public float indirectLightMultiplier;
        private Color sunColor;
        private float sunIntensity;
        private Color proceduralSunColor;
        private float proceduralSunIntensity;
        private float shadowDistance;
        //Shadow Settings
        private ShadowmaskMode shadowmaskMode;
        private LightShadows shadowType;
        private ShadowResolution shadowResolution;
        private ShadowProjection shadowProjection;
        //Vsync
        private AmbientSkiesConsts.VSyncMode vSyncMode;
        //Horizon settings
        private bool scaleHorizonObjectWithFog;
        private bool horizonEnabled;
        private float horizonScattering;
        private float horizonFogDensity;
        private float horizonFalloff;
        private float horizonBlend;
        private Vector3 horizonScale;
        private bool followPlayer;
        private float horizonUpdateTime;
        private Vector3 horizonPosition;
        //Gradient Sky
        private Color32 topSkyColor;
        private Color32 middleSkyColor;
        private Color32 bottomSkyColor;
        private float gradientDiffusion;
        //Procedural Sky
        private bool enableSunDisk;
        private float sunSize;
        private float sunConvergence;
        private float atmosphereThickness;
        private Color32 skyTint;
        private float skyExposure;
        private float skyMultiplier;
        //Volumetric Fog
        private float baseFogDistance;
        private float baseFogHeight;
        private float meanFogHeight;
        private float globalAnisotropy;
        private float globalLightProbeDimmer;
        //Exponential Fog
        private float exponentialFogDensity;
        private float exponentialBaseFogHeight;
        private float exponentialHeightAttenuation;
        private float exponentialMaxFogDistance;
        private float exponentialMipFogNear;
        private float exponentialMipFogFar;
        private float exponentialMipFogMax;
        //Linear Fog
        private float linearFogDensity;
        private float linearFogHeightStart;
        private float linearFogHeightEnd;
        private float linearFogMaxDistance;
        private float linearMipFogNear;
        private float linearMipFogFar;
        private float linearMipFogMax;
        //Volumetric Light Controller
        private float depthExtent;
        private float sliceDistribution;
        //Density Fog Volume
        private bool useDensityFogVolume;
        private Color32 singleScatteringAlbedo;
        private float densityVolumeFogDistance;
        private Texture3D fogDensityMaskTexture;
        private Vector3 densityMaskTiling;
        //HD Shadows
        private AmbientSkiesConsts.HDShadowQuality hDShadowQuality;
        private AmbientSkiesConsts.ShadowCascade shadowCascade;
        private float split1;
        private float split2;
        private float split3;
        //Contact Shadows
        private bool enableContactShadows;
        private float contactLength;
        private float contactScaleFactor;
        private float contactMaxDistance;
        private float contactFadeDistance;
        private int contactSampleCount;
        private float contactOpacity;
        //Micro Shadows
        private bool enableMicroShadows;
        private float microShadowOpacity;
        //SS Reflection
        private bool enableSSReflection;
        private float ssrEdgeFade;
        private int ssrNumberOfRays;
        private float ssrObjectThickness;
        private float ssrMinSmoothness;
        private float ssrSmoothnessFade;
        private bool ssrReflectSky;
        //SS Refract
        private bool enableSSRefraction;
        private float ssrWeightDistance;

        #endregion

        #region Post Process Values

        //Use Post FX
        private bool usePostProcess;
        //Selection
        private int newPPSelection;
        //HDR Mode
        private AmbientSkiesConsts.HDRMode hDRMode;
        //Anti Aliasing Mode
        private AmbientSkiesConsts.AntiAliasingMode antiAliasingMode;
        //Target Platform
        private AmbientSkiesConsts.PlatformTarget targetPlatform;
        //AO settings
        private bool aoEnabled;
        private float aoAmount;
        private Color32 aoColor;
        //Exposure settings
        private bool autoExposureEnabled;
        private float exposureAmount;
        private float exposureMin;
        private float exposureMax;
        //Bloom settings
        private bool bloomEnabled;
        private float bloomIntensity;
        private float bloomThreshold;
        private float lensIntensity;
        private Texture2D lensTexture;
        //Chromatic Aberration
        private bool chromaticAberrationEnabled;
        private float chromaticAberrationIntensity;
        //Color Grading settings
        private bool colorGradingEnabled;
        private Texture2D colorGradingLut;
        private float colorGradingPostExposure;
        private Color32 colorGradingColorFilter;
        private int colorGradingTempature;
        private int colorGradingTint;
        private float colorGradingSaturation;
        private float colorGradingContrast;
        //DOF settings
        private AmbientSkiesConsts.DepthOfFieldMode depthOfFieldMode;
        private AmbientSkiesConsts.DOFTrackingType depthOfFieldTrackingType;
        private bool depthOfFieldEnabled;
        private float autoDepthOfFieldFocusDistance;
        private float depthOfFieldFocusDistance;
        private float depthOfFieldAperture;
        private float depthOfFieldFocalLength;
        private float focusOffset;
        private LayerMask targetLayer;
        private float maxFocusDistance;
        private string depthOfFieldDistanceString;
        //Distortion settings
        private bool distortionEnabled;
        private float distortionIntensity;
        private float distortionScale;
        //Grain settings
        private bool grainEnabled;
        private float grainIntensity;
        private float grainSize;
        //SSR settings
        private bool screenSpaceReflectionsEnabled;
        private int maximumIterationCount;
        private float thickness;
#if UNITY_POST_PROCESSING_STACK_V2
        private GradingMode colorGradingMode;
        private PostProcessProfile customPostProcessingProfile;
        private KernelSize maxBlurSize;
        private ScreenSpaceReflectionResolution screenSpaceReflectionResolution;
        private ScreenSpaceReflectionPreset screenSpaceReflectionPreset;
        private AmbientOcclusionMode ambientOcclusionMode;
#endif
        private float maximumMarchDistance;
        private float distanceFade;
        private float screenSpaceVignette;
        //Vignette settings
        private bool vignetteEnabled;
        private float vignetteIntensity;
        private float vignetteSmoothness;
        //Motion Blur settings
        private bool motionBlurEnabled;
        private int motionShutterAngle;
        private int motionSampleCount;

        #endregion

        #region Lighting Values

        //Use Lightmaps
        private bool enableLightmapSettings;
        //Current Lightmap Setting
        private bool autoLightmapGeneration;
        private int newLightmappingSettings;
        //Realtime GI
        private bool realtimeGlobalIllumination;
        private float indirectRelolution;
        private bool useDirectionalMode;
        //Baked GI
        private bool bakedGlobalIllumination;
        private AmbientSkiesConsts.LightmapperMode lightmappingMode;
        private float lightmapResolution;
        private int lightmapPadding;
        private bool useHighResolutionLightmapSize;
        private bool compressLightmaps;
        private bool ambientOcclusion;
        private float maxDistance;
        private float indirectContribution;
        private float directContribution;
        private float lightIndirectIntensity;
        private float lightBoostIntensity;
        private bool finalGather;
        private int finalGatherRayCount;
        private bool finalGatherDenoising;

        #endregion

        #region Creation

        private SerializedObject objectSer { get; set; }
#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessProfile convertPostProfile;
#endif
        private bool focusAsset;
        private bool renamePostProcessProfile;
        private string convertPostProfileName;

        #endregion

        #endregion

        #region Custom Menu Items

        /// <summary>
        /// Creates menu and opens up Ambient skies
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Ambient Skies/Ambient Skies...", false, 40)]
        public static void ShowMenu()
        {
            //Ambient Skies Editor Window
            var mainWindow = GetWindow<AmbientSkiesEditorWindow>(false, "Ambient Skies");
            //Show window
            mainWindow.Show();
        }

        #endregion

        #region Constructors destructors and related delegates

        /// <summary>
        /// Destroys when window is closed
        /// </summary>
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }

            //Apply settings
            if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
            }

            PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
            LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);

            if (!Application.isPlaying)
            {
                SkyboxUtils.MarkActiveSceneAsDirty();
            }

            if (m_massiveCloudsPath)
            {
                //Remove warning
            }

            EditorApplication.update -= EditorUpdate;
            EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
        }

        /// <summary>
        /// Setup when window opens
        /// </summary>
        private void OnEnable()
        {
            #region Load UX

            LoadIcons();

            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }

            // Ambient Skies tabs
            var tabs = new Tab[]
            {
                new Tab("SkyboxesTab", m_skiesIcon, SkyboxesTab),
                new Tab("PostProcessingTab", m_postProcessingIcon, PostProcessingTab),
                new Tab("LightingTab", m_lightingIcon, LightingTab),
            };

            //Assign tabs
            m_mainTabs = new TabSet(m_editorUtils, tabs);

            #endregion

            #region Camera Setup

            //Get main camera
            GameObject mainCameraObject = SkyboxUtils.GetOrCreateMainCamera();
            if (mainCameraObject != null)
            {
                mainCam = mainCameraObject.GetComponent<Camera>();
            }

            #endregion

            #region Load Profile

            /*
            Debug.Log("Profile should be " + m_profileListIndex);

            m_profileList = GetAllSkyProfilesProjectSearch("t:AmbientSkyProfiles");

            //Add global profile names
            profileChoices.Clear();
            foreach (var profile in m_profileList)
            {
                profileChoices.Add(profile.name);
            }

            newProfileListIndex = GetSkyProfile(m_profileList, m_profileList[m_profileListIndex].name);

            Debug.Log("Profile to load is now " + m_profileListIndex + " " + m_profileList[newProfileListIndex].name);

            //Get main Ambient Skies Volume 1 asset
            m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_profileList[newProfileListIndex].name));
            */

            m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath("Ambient Skies Volume 1"));
            if (m_profiles.m_overwriteGlboalSkiesProfile)
            {
                if (m_profiles.m_overwriteSkiesProfile != null)
                {
                    m_profiles = m_profiles.m_overwriteSkiesProfile;
                }
                else
                {
                    Debug.LogError("Warning unable to find an overwrite profile please add one");
                }
            }

            #endregion

#if !UNITY_POST_PROCESSING_STACK_V2 && UNITY_2018_1_OR_NEWER
            AddPostProcessingV2Only();
#else
            #region Apply Settings

            //If ambient skies vol 1 is not loaded
            if (m_profiles == null)
            {
                //Debug and exit
                if (m_profiles.m_showDebug)
                {
                    Debug.LogWarning("Missing Ambient Skies Volume 1.asset. Please make sure it's in your project, closing Ambient Skies window!");
                }

                //Close ambient skies window
                Close();

                return;
            }
            else
            {
                ApplyScriptingDefine();

                //New scene detected, option to save settings LWRP / Built-In
                bool loadingFromNewScene = false;
                if (m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox == null || m_profiles.m_selectedRenderPipeline != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox.name != "Ambient Skies Skybox")
                {
                    if (m_profiles.systemTypes == AmbientSkiesConsts.SystemTypes.AmbientSkies)
                    {
                        if (GameObject.Find("Enviro Sky Manager") != null || GameObject.Find("EnviroSky Standard") != null || GameObject.Find("EnviroSky Lite") != null || GameObject.Find("EnviroSky Lite for Mobiles") != null || GameObject.Find("EnviroSky Standard for VR") != null || RenderSettings.skybox.shader == Shader.Find("Enviro/Skybox") || GameObject.Find("Enviro Sky Manager for GAIA") != null)
                        {
                            if (EditorUtility.DisplayDialog("Enviro Detected!", "Warning Enviro has been detected in your scene, System Type is set to Ambient Skies. Would you like to switch System Type to Third Party?", "Yes", "No"))
                            {
                                systemtype = AmbientSkiesConsts.SystemTypes.ThirdParty;
                                m_profiles.systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                                NewSceneObjectCreation();
                            }
                        }
                    }
                    else if (m_profiles.systemTypes == AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.AddSkyboxIfNull("Default-Sky");
                    }
                    
                    else if (GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)") == null && RenderSettings.skybox.shader != Shader.Find("Enviro/Skybox"))
                    {
                        if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profiles", "Ok"))
                        {
                            SaveBuiltInAndLWRPSettings();
                            loadingFromNewScene = true;
                        }
                    }                   
                }
#if HDPipeline
                //New scene detected, option to save settings HDRP
                else if (m_profiles.m_selectedRenderPipeline ==  AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && FindObjectOfType<Volume>() != null)
                {
                    if (GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)") == null)
                    {
                        if (GameObject.Find("High Definition Environment Volume") == null)
                        {
                            if (EditorUtility.DisplayDialog("New Scene Detected", "This scene isn't using ambient skies content. We have saved your settings to the User Profiles", "Ok"))
                            {
                                SaveHDRPSettings();
                                loadingFromNewScene = true;
                            }
                        }
                    }
                }
#endif
                LoadAndApplySettings(loadingFromNewScene);

                //Add skies profile names
                skyboxChoices.Clear();
                foreach (var profile in m_profiles.m_skyProfiles)
                {
                    skyboxChoices.Add(profile.name);
                }
                //Add post processing profile names
                ppChoices.Clear();
                foreach (var profile in m_profiles.m_ppProfiles)
                {
                    ppChoices.Add(profile.name);
                }
                //Add lightmaps profile names
                lightmappingChoices.Clear();
                foreach (var profile in m_profiles.m_lightingProfiles)
                {
                    lightmappingChoices.Add(profile.name);
                }

                m_profiles.m_version = PWApp.CONF.Version;

                //Apply HD Pipeline Resources
                if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    AmbientSkiesPipelineUtils.ApplyHDPipelineResources(renderPipelineSettings, "Procedural Worlds HDRPRenderPipelineAsset");
                }

                //Make sure Ambient Skies is defined
                AmbientSkiesPipelineUtils.SetAmbientSkiesDefinesStatic();

                //Check for massive clouds plugin is there
                m_massiveCloudsPath = Directory.Exists(SkyboxUtils.GetAssetPath("MassiveClouds"));
                MassiveCloudsUtils.DefineMassiveCouds(m_massiveCloudsPath);

                //Apply settings
                GetAndApplyAllValuesFromAmbientSkies(false);

                SkyboxUtils.MarkActiveSceneAsDirty();

                m_countdown = 2f;

                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                EditorApplication.playModeStateChanged -= HandleOnPlayModeChanged;
                EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
            }

            #endregion
#endif
        }

        /// <summary>
        /// Check if users exits playmode
        /// </summary>
        /// <param name="state"></param>
        private void HandleOnPlayModeChanged(PlayModeStateChange state)
        {
            //Checks state if exiting
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                //Apply Updates
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                m_countdown = 0.2f;
            }
        }

        #endregion

        #region GUI main

        #region GUI Startup Setup

        void OnGUI()
        {
            m_editorUtils.Initialize(); // Do not remove this!
            m_editorUtils.GUIHeader(); //Header

            //Scroll
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, false);
           
            /*

            m_editorUtils.Heading("ProfilesHeader");

            newProfileListIndex = m_profileListIndex;

            if (m_isCompiling)
            {
                GUI.enabled = false;
            }
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("SelectProfilesDropdown", GUILayout.Width(146f));
            newProfileListIndex = EditorGUILayout.Popup(m_profileListIndex, profileChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();

            GUI.enabled = true;

            if (newProfileListIndex != m_profileListIndex)
            {
                m_profileListIndex = newProfileListIndex;
                m_profiles = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(SkyboxUtils.GetAssetPath(m_profileList[m_profileListIndex].name));

                //Apply settings
                GetAndApplyAllValuesFromAmbientSkies(true);

                Debug.Log("Profile Saved is now " + m_profileListIndex + " " + m_profileList[newProfileListIndex].name);
            }

            */

            // Add content here
            m_editorUtils.Heading("PipelineHeader");

            //Select pipeline
            renderPipelineSettings = m_profiles.m_selectedRenderPipeline;
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }
            renderPipelineSettings = (AmbientSkiesConsts.RenderPipelineSettings)m_editorUtils.EnumPopup("RenderPipeline", renderPipelineSettings);

            GUI.enabled = true;
            /*
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("RenderPipeline", GUILayout.Width(146f));
            m_editorUtils.TextNonLocalized(renderPipelineSettings.ToString(), GUILayout.Width(100f), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
            */

            //Render Pipeline setup functions
            if (renderPipelineSettings != m_profiles.m_selectedRenderPipeline)
            {
                if (m_profiles.m_showDebug)
                {
                    Debug.Log("Changing Pipeline");
                }

                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }

                //If application is playing log playing application and return
                if (Application.isPlaying)
                {
                    if (m_profiles.m_showDebug)
                    {
                        Debug.LogWarning("You're trying to switch pipeline while application is running. Please stop application and try again");
                    }
                    
                    renderPipelineSettings = m_profiles.m_selectedRenderPipeline;
                    return;
                }
                else
                {
#if !UNITY_2018_3_OR_NEWER
                    {
                        EditorUtility.DisplayDialog("Pipeline change not supported", "Lightweight and High Definition is only supported in 2018.3 or higher. To use Gaia selected pipeline, please install and upgrade your project to 2018.3.x. Switching back to Built-In Pipeline.", "OK");
                        renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
                    }
#else
                    {
                        if (EditorUtility.DisplayDialog("CHANGE RENDER PIPELINE",
                            "You are about to install a new render pipeline!" +
                            "\nPlease BACKUP your project first!" +
                            "\nAre you sure?",
                            "Yes", "No"))
                        {

                            bool upgradeMaterials = false;
                            if (EditorUtility.DisplayDialog("UPGRADE MATERIALS",
                                "Upgrade materials to the " + renderPipelineSettings.ToString() + " pipeline?" +
                                "\nWARNING: THIS PROCESS CAN NOT BE UNDONE!" +
                                "\nSay NO and change pipeline back if unsure!",
                                "Yes", "No"))
                            {
                                upgradeMaterials = true;
                            }

                            AmbientSkiesPipelineUtilsEditor.ShowAmbientSkiesPipelineUtilsEditor(m_profiles.m_selectedRenderPipeline, renderPipelineSettings, upgradeMaterials, true, this);
                        }
                        else
                        {
                            renderPipelineSettings = m_profiles.m_selectedRenderPipeline;
                        }
                    }
#endif
                }
            }

            m_editorUtils.Tabs(m_mainTabs);

            m_profiles.m_selectedRenderPipeline = renderPipelineSettings;

            //If countdown active
            if (m_countdown > 0)
            {
                //Apply settings
                GetAndApplyAllValuesFromAmbientSkies(false);
            }

            //Update strings
            seasonString = currentSeason.ToString();
            depthOfFieldDistanceString = depthOfFieldFocusDistance.ToString();

            if (EditorApplication.isCompiling)
            {
                m_isCompiling = true;
            }
            else
            {
                m_isCompiling = false;
            }

            if (Application.isPlaying)
            {
                Repaint();
            }

            //End scroll
            GUILayout.EndScrollView();
            m_editorUtils.GUIFooter();
        }

        #endregion

        #region Tabs

        /// <summary>
        /// Display the skyboxes tab
        /// </summary>
        private void SkyboxesTab()
        {
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //See if we can select an ambiet skies skybox
            if (m_selectedSkyboxProfile == null)
            {
                if (m_profiles.m_editSettings)
                {
                    if (RenderSettings.ambientMode == AmbientMode.Skybox && RenderSettings.skybox != null && RenderSettings.skybox.name == "Ambient Skies Skybox")
                    {
                        if (m_editorUtils.Button("CreateSkyboxProfileButton"))
                        {
                            if (EditorUtility.DisplayDialog("ALERT!!",
                                "Would you like to make a profile from this skybox?", "Yes", "Cancel"))
                            {
                                if (SkyboxUtils.CreateProfileFromActiveSkybox(m_profiles))
                                {
                                    m_selectedSkyboxProfileIndex = m_profiles.m_skyProfiles.Count - 1;
                                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                                }
                            }
                        }

                        return;
                    }
                }

                if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (m_editorUtils.Button("SelectSkyboxProfileButton"))
                    {
                        m_selectedSkyboxProfileIndex = 0;
                        m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                    }

                    return;
                }
            }
            else if (m_profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                if (GameObject.Find("High Definition Environment Volume") == null)
                {
                    if (m_editorUtils.Button("SelectSkyboxProfileButton"))
                    {
                        m_selectedSkyboxProfileIndex = 0;
                        m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                        CreateHDRPVolume("High Definition Environment Volume");
                    }

                    return;
                }
            }

            //If skybox profile is there
            if (m_selectedSkyboxProfile != null)
            {
                #region Sky tab variables

                //Global Settings
                systemtype = m_profiles.systemTypes;

                //Target Platform
                targetPlatform = m_profiles.targetPlatform;

                //Skies Settings  
                useSkies = m_selectedSkyboxProfile.useSkies;

                //Main Settings
                skyType = m_profiles.skyType;
                skyTypeNonHD = m_profiles.skyTypeNonHD;
                fogType = m_selectedSkyboxProfile.fogType;
                ambientMode = m_selectedSkyboxProfile.ambientMode;
                useTimeOfDay = m_profiles.useTimeOfDay;

                #region Time Of Day Checks

                //Time Of Day Settings

                bool checkAllTODSettings = true;
                if (m_profiles.timeOfDayProfile != timeOfDayProfile)
                {
                    timeOfDayProfile = m_profiles.timeOfDayProfile;
                    m_profiles.timeOfDayProfile = timeOfDayProfile;

                    checkAllTODSettings = false;

                    /*
                    //Get all settings from new profile
                    currentSeason = m_profiles.timeOfDayProfile.m_environmentSeason;
                    hemisphereOrigin = m_profiles.timeOfDayProfile.m_hemisphereOrigin;
                    pauseTimeKey = m_profiles.timeOfDayProfile.m_pauseTimeKey;
                    incrementUpKey = m_profiles.timeOfDayProfile.m_incrementUpKey;
                    incrementDownKey = m_profiles.timeOfDayProfile.m_incrementDownKey;
                    timeToAddOrRemove = m_profiles.timeOfDayProfile.m_timeToAddOrRemove;
                    rotateSunLeftKey = m_profiles.timeOfDayProfile.m_rotateSunLeftKey;
                    rotateSunRightKey = m_profiles.timeOfDayProfile.m_rotateSunRightKey;
                    sunRotationAmount = m_profiles.timeOfDayProfile.m_sunRotationAmount;
                    pauseTime = m_profiles.timeOfDayProfile.m_pauseTime;
                    currentTimeOfDay = m_profiles.timeOfDayProfile.m_currentTime;
                    timeOfDaySkyboxRotation = m_profiles.timeOfDayProfile.m_sunRotation;
                    daySunIntensity = m_profiles.timeOfDayProfile.m_daySunIntensity;
                    daySunGradientColor = m_profiles.timeOfDayProfile.m_daySunGradientColor;
                    nightSunIntensity = m_profiles.timeOfDayProfile.m_nightSunIntensity;
                    nightSunGradientColor = m_profiles.timeOfDayProfile.m_nightSunGradientColor;
                    lightAnisotropy = m_profiles.timeOfDayProfile.m_lightAnisotropy;
                    lightProbeDimmer = m_profiles.timeOfDayProfile.m_lightProbeDimmer;
                    lightDepthExtent = m_profiles.timeOfDayProfile.m_lightDepthExtent;
                    sunSizeAmount = m_profiles.timeOfDayProfile.m_sunSize;
                    skyExposureAmount = m_profiles.timeOfDayProfile.m_skyExposure;
                    startFogDistance = m_profiles.timeOfDayProfile.m_startFogDistance;
                    dayFogDensity = m_profiles.timeOfDayProfile.m_dayFogDensity;
                    nightFogDensity = m_profiles.timeOfDayProfile.m_nightFogDensity;
                    dayFogDistance = m_profiles.timeOfDayProfile.m_dayFogDistance;
                    dayFogColor = m_profiles.timeOfDayProfile.m_dayFogColor;
                    nightFogDistance = m_profiles.timeOfDayProfile.m_nightFogDistance;
                    nightFogColor = m_profiles.timeOfDayProfile.m_nightFogColor;
                    dayTempature = m_profiles.timeOfDayProfile.m_dayTempature;
                    dayPostFXColor = m_profiles.timeOfDayProfile.m_dayColor;
                    nightTempature = m_profiles.timeOfDayProfile.m_nightTempature;
                    nightPostFXColor = m_profiles.timeOfDayProfile.m_nightColor;
                    syncPostProcessing = m_profiles.timeOfDayProfile.m_syncPostFXToTimeOfDay;
                    realtimeGIUpdate = m_profiles.timeOfDayProfile.m_realtimeGIUpdate;
                    gIUpdateInterval = m_profiles.timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                    dayLengthInSeconds = m_profiles.timeOfDayProfile.m_dayLengthInSeconds;
                    nightLengthInSeconds = m_profiles.timeOfDayProfile.m_nightLengthInSeconds;
                    dayDate = m_profiles.timeOfDayProfile.m_day;
                    monthDate = m_profiles.timeOfDayProfile.m_month;
                    yearDate = m_profiles.timeOfDayProfile.m_year;
                    */
                }
                else
                {
                    timeOfDayProfile = m_profiles.timeOfDayProfile;
                }

                if (checkAllTODSettings)
                {
                    if (currentSeason != m_profiles.timeOfDayProfile.m_environmentSeason)
                    {
                        currentSeason = m_profiles.timeOfDayProfile.m_environmentSeason;
                        m_profiles.currentSeason = m_profiles.timeOfDayProfile.m_environmentSeason;
                    }
                    else
                    {
                        currentSeason = m_profiles.currentSeason;
                    }
                    if (hemisphereOrigin != m_profiles.timeOfDayProfile.m_hemisphereOrigin)
                    {
                        hemisphereOrigin = m_profiles.timeOfDayProfile.m_hemisphereOrigin;
                        m_profiles.hemisphereOrigin = m_profiles.timeOfDayProfile.m_hemisphereOrigin;
                    }
                    else
                    {
                        hemisphereOrigin = m_profiles.hemisphereOrigin;
                    }
                    if (pauseTimeKey != m_profiles.timeOfDayProfile.m_pauseTimeKey)
                    {
                        pauseTimeKey = m_profiles.timeOfDayProfile.m_pauseTimeKey;
                        m_profiles.pauseTimeKey = m_profiles.timeOfDayProfile.m_pauseTimeKey;
                    }
                    else
                    {
                        pauseTimeKey = m_profiles.pauseTimeKey;
                    }
                    if (incrementUpKey != m_profiles.timeOfDayProfile.m_incrementUpKey)
                    {
                        incrementUpKey = m_profiles.timeOfDayProfile.m_incrementUpKey;
                        m_profiles.incrementUpKey = m_profiles.timeOfDayProfile.m_incrementUpKey;
                    }
                    else
                    {
                        incrementUpKey = m_profiles.incrementUpKey;
                    }
                    if (incrementDownKey != m_profiles.timeOfDayProfile.m_incrementDownKey)
                    {
                        incrementDownKey = m_profiles.timeOfDayProfile.m_incrementDownKey;
                        m_profiles.incrementDownKey = m_profiles.timeOfDayProfile.m_incrementDownKey;
                    }
                    else
                    {
                        incrementDownKey = m_profiles.incrementDownKey;
                    }
                    if (timeToAddOrRemove != m_profiles.timeOfDayProfile.m_timeToAddOrRemove)
                    {
                        timeToAddOrRemove = m_profiles.timeOfDayProfile.m_timeToAddOrRemove;
                        m_profiles.timeToAddOrRemove = m_profiles.timeOfDayProfile.m_timeToAddOrRemove;
                    }
                    else
                    {
                        timeToAddOrRemove = m_profiles.timeToAddOrRemove;
                    }
                    if (rotateSunLeftKey != m_profiles.timeOfDayProfile.m_rotateSunLeftKey)
                    {
                        rotateSunLeftKey = m_profiles.timeOfDayProfile.m_rotateSunLeftKey;
                        m_profiles.rotateSunLeftKey = m_profiles.timeOfDayProfile.m_rotateSunLeftKey;
                    }
                    else
                    {
                        rotateSunLeftKey = m_profiles.rotateSunLeftKey;
                    }
                    if (rotateSunRightKey != m_profiles.timeOfDayProfile.m_rotateSunRightKey)
                    {
                        rotateSunRightKey = m_profiles.timeOfDayProfile.m_rotateSunRightKey;
                        m_profiles.rotateSunRightKey = m_profiles.timeOfDayProfile.m_rotateSunRightKey;
                    }
                    else
                    {
                        rotateSunRightKey = m_profiles.rotateSunRightKey;
                    }
                    if (sunRotationAmount != m_profiles.timeOfDayProfile.m_sunRotationAmount)
                    {
                        sunRotationAmount = m_profiles.timeOfDayProfile.m_sunRotationAmount;
                        m_profiles.sunRotationAmount = m_profiles.timeOfDayProfile.m_sunRotationAmount;
                    }
                    else
                    {
                        sunRotationAmount = m_profiles.sunRotationAmount;
                    }
                    if (pauseTime != m_profiles.timeOfDayProfile.m_pauseTime)
                    {
                        pauseTime = m_profiles.timeOfDayProfile.m_pauseTime;
                        m_profiles.pauseTime = m_profiles.timeOfDayProfile.m_pauseTime;
                    }
                    else
                    {
                        pauseTime = m_profiles.pauseTime;
                    }
                    if (currentTimeOfDay != m_profiles.timeOfDayProfile.m_currentTime)
                    {
                        currentTimeOfDay = m_profiles.timeOfDayProfile.m_currentTime;
                        m_profiles.currentTimeOfDay = m_profiles.timeOfDayProfile.m_currentTime;
                    }
                    else
                    {
                        currentTimeOfDay = m_profiles.currentTimeOfDay;
                    }
                    if (timeOfDaySkyboxRotation != m_profiles.timeOfDayProfile.m_sunRotation)
                    {
                        timeOfDaySkyboxRotation = m_profiles.timeOfDayProfile.m_sunRotation;
                        m_profiles.sunRotationAmount = m_profiles.timeOfDayProfile.m_sunRotation;
                    }
                    else
                    {
                        timeOfDaySkyboxRotation = m_profiles.skyboxRotation;
                    }
                    if (daySunIntensity != m_profiles.timeOfDayProfile.m_daySunIntensity)
                    {
                        daySunIntensity = m_profiles.timeOfDayProfile.m_daySunIntensity;
                        m_profiles.daySunIntensity = m_profiles.timeOfDayProfile.m_daySunIntensity;
                    }
                    else
                    {
                        daySunIntensity = m_profiles.daySunIntensity;
                    }
                    if (daySunGradientColor != m_profiles.timeOfDayProfile.m_daySunGradientColor)
                    {
                        daySunGradientColor = m_profiles.timeOfDayProfile.m_daySunGradientColor;
                        m_profiles.daySunGradientColor = m_profiles.timeOfDayProfile.m_daySunGradientColor;
                    }
                    else
                    {
                        daySunGradientColor = m_profiles.daySunGradientColor;
                    }
                    if (nightSunIntensity != m_profiles.timeOfDayProfile.m_nightSunIntensity)
                    {
                        nightSunIntensity = m_profiles.timeOfDayProfile.m_nightSunIntensity;
                        m_profiles.nightSunIntensity = m_profiles.timeOfDayProfile.m_nightSunIntensity;
                    }
                    else
                    {
                        nightSunIntensity = m_profiles.nightSunIntensity;
                    }
                    if (nightSunGradientColor != m_profiles.timeOfDayProfile.m_nightSunGradientColor)
                    {
                        nightSunGradientColor = m_profiles.timeOfDayProfile.m_nightSunGradientColor;
                        m_profiles.nightSunGradientColor = m_profiles.timeOfDayProfile.m_nightSunGradientColor;
                    }
                    else
                    {
                        nightSunGradientColor = m_profiles.nightSunGradientColor;
                    }
                    if (lightAnisotropy != m_profiles.timeOfDayProfile.m_lightAnisotropy)
                    {
                        lightAnisotropy = m_profiles.timeOfDayProfile.m_lightAnisotropy;
                        m_profiles.lightAnisotropy = m_profiles.timeOfDayProfile.m_lightAnisotropy;
                    }
                    else
                    {
                        lightAnisotropy = m_profiles.lightAnisotropy;
                    }
                    if (lightProbeDimmer != m_profiles.timeOfDayProfile.m_lightProbeDimmer)
                    {
                        lightProbeDimmer = m_profiles.timeOfDayProfile.m_lightProbeDimmer;
                        m_profiles.lightProbeDimmer = m_profiles.timeOfDayProfile.m_lightProbeDimmer;
                    }
                    else
                    {
                        lightProbeDimmer = m_profiles.lightProbeDimmer;
                    }
                    if (lightDepthExtent != m_profiles.timeOfDayProfile.m_lightDepthExtent)
                    {
                        lightDepthExtent = m_profiles.timeOfDayProfile.m_lightDepthExtent;
                        m_profiles.lightDepthExtent = m_profiles.timeOfDayProfile.m_lightDepthExtent;
                    }
                    else
                    {
                        lightDepthExtent = m_profiles.lightDepthExtent;
                    }
                    if (sunSizeAmount != m_profiles.timeOfDayProfile.m_sunSize)
                    {
                        sunSizeAmount = m_profiles.timeOfDayProfile.m_sunSize;
                        m_profiles.sunSize = m_profiles.timeOfDayProfile.m_sunSize;
                    }
                    else
                    {
                        sunSizeAmount = m_profiles.sunSize;
                    }
                    if (skyExposureAmount != m_profiles.timeOfDayProfile.m_skyExposure)
                    {
                        skyExposureAmount = m_profiles.timeOfDayProfile.m_skyExposure;
                        m_profiles.skyExposure = m_profiles.timeOfDayProfile.m_skyExposure;
                    }
                    else
                    {
                        skyExposureAmount = m_profiles.skyExposure;
                    }
                    if (startFogDistance != m_profiles.timeOfDayProfile.m_startFogDistance)
                    {
                        startFogDistance = m_profiles.timeOfDayProfile.m_startFogDistance;
                        m_profiles.startFogDistance = m_profiles.timeOfDayProfile.m_startFogDistance;
                    }
                    else
                    {
                        startFogDistance = m_profiles.startFogDistance;
                    }
                    if (dayFogDensity != m_profiles.timeOfDayProfile.m_dayFogDensity)
                    {
                        dayFogDensity = m_profiles.timeOfDayProfile.m_dayFogDensity;
                        m_profiles.dayFogDensity = m_profiles.timeOfDayProfile.m_dayFogDensity;
                    }
                    else
                    {
                        dayFogDensity = m_profiles.dayFogDensity;
                    }
                    if (nightFogDensity != m_profiles.timeOfDayProfile.m_nightFogDensity)
                    {
                        nightFogDensity = m_profiles.timeOfDayProfile.m_nightFogDensity;
                        m_profiles.nightFogDensity = m_profiles.timeOfDayProfile.m_nightFogDensity;
                    }
                    else
                    {
                        nightFogDensity = m_profiles.nightFogDensity;
                    }
                    if (dayFogDistance != m_profiles.timeOfDayProfile.m_dayFogDistance)
                    {
                        dayFogDistance = m_profiles.timeOfDayProfile.m_dayFogDistance;
                        m_profiles.dayFogDistance = m_profiles.timeOfDayProfile.m_dayFogDistance;
                    }
                    else
                    {
                        dayFogDistance = m_profiles.dayFogDistance;
                    }
                    if (dayFogColor != m_profiles.timeOfDayProfile.m_dayFogColor)
                    {
                        dayFogColor = m_profiles.timeOfDayProfile.m_dayFogColor;
                        m_profiles.dayFogColor = m_profiles.timeOfDayProfile.m_dayFogColor;
                    }
                    else
                    {
                        dayFogColor = m_profiles.dayFogColor;
                    }
                    if (nightFogDistance != m_profiles.timeOfDayProfile.m_nightFogDistance)
                    {
                        nightFogDistance = m_profiles.timeOfDayProfile.m_nightFogDistance;
                        m_profiles.nightFogDistance = m_profiles.timeOfDayProfile.m_nightFogDistance;
                    }
                    else
                    {
                        nightFogDistance = m_profiles.nightFogDistance;
                    }
                    if (nightFogColor != m_profiles.timeOfDayProfile.m_nightFogColor)
                    {
                        nightFogColor = m_profiles.timeOfDayProfile.m_nightFogColor;
                        m_profiles.nightFogColor = m_profiles.timeOfDayProfile.m_nightFogColor;
                    }
                    else
                    {
                        nightFogColor = m_profiles.nightFogColor;
                    }
                    if (dayTempature != m_profiles.timeOfDayProfile.m_dayTempature)
                    {
                        dayTempature = m_profiles.timeOfDayProfile.m_dayTempature;
                        m_profiles.dayTempature = m_profiles.timeOfDayProfile.m_dayTempature;
                    }
                    else
                    {
                        dayTempature = m_profiles.dayTempature;
                    }
                    if (dayPostFXColor != m_profiles.timeOfDayProfile.m_dayColor)
                    {
                        dayPostFXColor = m_profiles.timeOfDayProfile.m_dayColor;
                        m_profiles.dayPostFXColor = m_profiles.timeOfDayProfile.m_dayColor;
                    }
                    else
                    {
                        dayPostFXColor = m_profiles.dayPostFXColor;
                    }
                    if (nightTempature != m_profiles.timeOfDayProfile.m_nightTempature)
                    {
                        nightTempature = m_profiles.timeOfDayProfile.m_nightTempature;
                        m_profiles.nightTempature = m_profiles.timeOfDayProfile.m_nightTempature;
                    }
                    else
                    {
                        nightTempature = m_profiles.nightTempature;
                    }
                    if (nightPostFXColor != m_profiles.timeOfDayProfile.m_nightColor)
                    {
                        nightPostFXColor = m_profiles.timeOfDayProfile.m_nightColor;
                        m_profiles.nightPostFXColor = m_profiles.timeOfDayProfile.m_nightColor;
                    }
                    else
                    {
                        nightPostFXColor = m_profiles.nightPostFXColor;
                    }
                    if (syncPostProcessing != m_profiles.timeOfDayProfile.m_syncPostFXToTimeOfDay)
                    {
                        syncPostProcessing = m_profiles.timeOfDayProfile.m_syncPostFXToTimeOfDay;
                        m_profiles.syncPostProcessing = m_profiles.timeOfDayProfile.m_syncPostFXToTimeOfDay;
                    }
                    else
                    {
                        syncPostProcessing = m_profiles.syncPostProcessing;
                    }
                    if (realtimeGIUpdate != m_profiles.timeOfDayProfile.m_realtimeGIUpdate)
                    {
                        realtimeGIUpdate = m_profiles.timeOfDayProfile.m_realtimeGIUpdate;
                        m_profiles.realtimeGIUpdate = m_profiles.timeOfDayProfile.m_realtimeGIUpdate;
                    }
                    else
                    {
                        realtimeGIUpdate = m_profiles.realtimeGIUpdate;
                    }
                    if (gIUpdateInterval != m_profiles.timeOfDayProfile.m_gIUpdateIntervalInSeconds)
                    {
                        gIUpdateInterval = m_profiles.timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                        m_profiles.gIUpdateInterval = m_profiles.timeOfDayProfile.m_gIUpdateIntervalInSeconds;
                    }
                    else
                    {
                        gIUpdateInterval = m_profiles.gIUpdateInterval;
                    }
                    if (dayLengthInSeconds != m_profiles.timeOfDayProfile.m_dayLengthInSeconds)
                    {
                        dayLengthInSeconds = m_profiles.timeOfDayProfile.m_dayLengthInSeconds;
                        m_profiles.dayLengthInSeconds = m_profiles.timeOfDayProfile.m_dayLengthInSeconds;
                    }
                    else
                    {
                        dayLengthInSeconds = m_profiles.dayLengthInSeconds;
                    }
                    if (nightLengthInSeconds != m_profiles.timeOfDayProfile.m_nightLengthInSeconds)
                    {
                        nightLengthInSeconds = m_profiles.timeOfDayProfile.m_nightLengthInSeconds;
                        m_profiles.nightLengthInSeconds = m_profiles.timeOfDayProfile.m_nightLengthInSeconds;
                    }
                    else
                    {
                        nightLengthInSeconds = m_profiles.nightLengthInSeconds;
                    }
                    if (dayDate != m_profiles.timeOfDayProfile.m_day)
                    {
                        dayDate = m_profiles.timeOfDayProfile.m_day;
                        m_profiles.dayDate = m_profiles.timeOfDayProfile.m_day;
                    }
                    else
                    {
                        dayDate = m_profiles.dayDate;
                    }
                    if (monthDate != m_profiles.timeOfDayProfile.m_month)
                    {
                        monthDate = m_profiles.timeOfDayProfile.m_month;
                        m_profiles.monthDate = m_profiles.timeOfDayProfile.m_month;
                    }
                    else
                    {
                        monthDate = m_profiles.monthDate;
                    }
                    if (yearDate != m_profiles.timeOfDayProfile.m_year)
                    {
                        yearDate = m_profiles.timeOfDayProfile.m_year;
                        m_profiles.yearDate = m_profiles.timeOfDayProfile.m_year;
                    }
                    else
                    {
                        yearDate = m_profiles.yearDate;
                    }
                }

                #endregion

                //Skybox Settings
                profileName = m_selectedSkyboxProfile.name;
                newSkyboxSelection = m_selectedSkyboxProfileIndex;
                skyboxTint = m_selectedSkyboxProfile.skyboxTint;
                skyboxExposure = m_selectedSkyboxProfile.skyboxExposure;
                skyboxRotation = m_selectedSkyboxProfile.skyboxRotation;
                skyboxPitch = m_selectedSkyboxProfile.skyboxPitch;
                customSkybox = m_selectedSkyboxProfile.customSkybox;
                proceduralSunSize = m_selectedSkyboxProfile.proceduralSunSize;
                proceduralSunSizeConvergence = m_selectedSkyboxProfile.proceduralSunSizeConvergence;
                proceduralAtmosphereThickness = m_selectedSkyboxProfile.proceduralAtmosphereThickness;
                proceduralGroundColor = m_selectedSkyboxProfile.proceduralGroundColor;
                proceduralSkyTint = m_selectedSkyboxProfile.proceduralSkyTint;
                proceduralSkyExposure = m_selectedSkyboxProfile.proceduralSkyExposure;
                proceduralSkyboxRotation = m_selectedSkyboxProfile.proceduralSkyboxRotation;
                proceduralSkyboxPitch = m_selectedSkyboxProfile.proceduralSkyboxPitch;
                includeSunInBaking = m_selectedSkyboxProfile.includeSunInBaking;
                isProceduralSkybox = m_selectedSkyboxProfile.isProceduralSkybox;

                //Fog Settings
                configurationType = m_profiles.configurationType;
                fogColor = m_selectedSkyboxProfile.fogColor;
                fogDistance = m_selectedSkyboxProfile.fogDistance;
                nearFogDistance = m_selectedSkyboxProfile.nearFogDistance;
                fogDensity = m_selectedSkyboxProfile.fogDensity;
                proceduralFogColor = m_selectedSkyboxProfile.proceduralFogColor;
                proceduralFogDistance = m_selectedSkyboxProfile.proceduralFogDistance;
                proceduralNearFogDistance = m_selectedSkyboxProfile.proceduralNearFogDistance;
                proceduralFogDensity = m_selectedSkyboxProfile.proceduralFogDensity;

                //Ambient Settings
                skyColor = m_selectedSkyboxProfile.skyColor;
                equatorColor = m_selectedSkyboxProfile.equatorColor;
                groundColor = m_selectedSkyboxProfile.groundColor;
                skyboxGroundIntensity = m_selectedSkyboxProfile.skyboxGroundIntensity;

                //Sun Settings
                shadowStrength = m_selectedSkyboxProfile.shadowStrength;
                indirectLightMultiplier = m_selectedSkyboxProfile.indirectLightMultiplier;
                sunColor = m_selectedSkyboxProfile.sunColor;
                sunIntensity = m_selectedSkyboxProfile.sunIntensity;
                proceduralSunColor = m_selectedSkyboxProfile.proceduralSunColor;
                proceduralSunIntensity = m_selectedSkyboxProfile.proceduralSunIntensity;

                //Shadow Settings
                shadowDistance = m_selectedSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedSkyboxProfile.shadowType;
                shadowResolution = m_selectedSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedSkyboxProfile.cascadeCount;

                //VSync Settings
                vSyncMode = m_profiles.vSyncMode;

                //Horizon Settings
                scaleHorizonObjectWithFog = m_selectedSkyboxProfile.scaleHorizonObjectWithFog;
                horizonEnabled = m_selectedSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedSkyboxProfile.horizonBlend;
                horizonScale = m_selectedSkyboxProfile.horizonSize;
                followPlayer = m_selectedSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedSkyboxProfile.horizonPosition;
                enableSunDisk = m_selectedSkyboxProfile.enableSunDisk;

#if HDPipeline
                //HD Pipeline Settings
                //Gradient Sky
                topSkyColor = m_selectedSkyboxProfile.topColor;
                middleSkyColor = m_selectedSkyboxProfile.middleColor;
                bottomSkyColor = m_selectedSkyboxProfile.bottomColor;
                gradientDiffusion = m_selectedSkyboxProfile.gradientDiffusion;

                //Procedural Sky
                sunSize = m_selectedSkyboxProfile.sunSize;
                sunConvergence = m_selectedSkyboxProfile.sunConvergence;
                atmosphereThickness = m_selectedSkyboxProfile.atmosphereThickness;
                skyTint = m_selectedSkyboxProfile.skyTint;
                skyExposure = m_selectedSkyboxProfile.skyExposure;
                skyMultiplier = m_selectedSkyboxProfile.skyMultiplier;

                //Volumetric Fog
                baseFogDistance = m_selectedSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedSkyboxProfile.densityMaskTiling;

                //HD Shadows
                hDShadowQuality = m_selectedSkyboxProfile.shadowQuality;
                split1 = m_selectedSkyboxProfile.cascadeSplit1;
                split2 = m_selectedSkyboxProfile.cascadeSplit2;
                split3 = m_selectedSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedSkyboxProfile.useContactShadows;
                contactLength = m_selectedSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedSkyboxProfile.microShadowOpacity;

                //SS Reflection
                enableSSReflection = m_selectedSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedSkyboxProfile.reflectSky;

                //SS Refract
                enableSSRefraction = m_selectedSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedSkyboxProfile.screenWeightDistance;

                //Ambient Lighting
                diffuseAmbientIntensity = m_selectedSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedSkyboxProfile.indirectSpecularIntensity;
#endif
                #endregion

                EditorGUI.BeginChangeCheck();

                //Skybox GUI
                m_editorUtils.Heading("SkySettingsHeader");

                useSkies = m_editorUtils.ToggleLeft("UseSkies", useSkies);
                EditorGUILayout.Space();

                //If use skies system
                if (useSkies)
                {
                    m_editorUtils.Link("LearnMoreAboutSceneLighting");
                    EditorGUILayout.Space();

                    m_foldoutGlobalSettings = m_editorUtils.Panel("Show Global Settings", GlobalSettingsEnabled, m_foldoutGlobalSettings);

                    if (systemtype == AmbientSkiesConsts.SystemTypes.AmbientSkies)
                    {
                        if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            m_foldoutMainSkySettings = m_editorUtils.Panel("Show Main Settings", MainSkySettingsEnabled, m_foldoutMainSkySettings);
                            if (skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                            {
                                m_foldoutTimeOfDaySettings = m_editorUtils.Panel("Show Time Of Day Settings", TimeOfDaySettingsEnabled, m_foldoutTimeOfDaySettings);
                            }
                            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                            {
                                m_foldoutSkySettings = m_editorUtils.Panel("Show Sky Settings", SkyboxSettingsEnabled, m_foldoutSkySettings);
                                m_foldoutFogSettings = m_editorUtils.Panel("Show Fog Settings", FogSettingsEnabled, m_foldoutFogSettings);
                                m_foldoutAmbientSettings = m_editorUtils.Panel("Show Ambient Settings", AmbientSettingsEnabled, m_foldoutAmbientSettings);
                                m_foldoutSunSettings = m_editorUtils.Panel("Show Sun Settings", SunSettingsEnabled, m_foldoutSunSettings);
                            }
                            m_foldoutHDShadowSettings = m_editorUtils.Panel("Show HD Shadow Settings", HDShadowSettingsEnabled, m_foldoutHDShadowSettings);
                            m_foldoutScreenSpaceReflectionSettings = m_editorUtils.Panel("Show Screen Space Reflection Settings", ScreenSpaceReflectionSettingsEnabled, m_foldoutScreenSpaceReflectionSettings);
                            m_foldoutScreenSpaceRefractionSettings = m_editorUtils.Panel("Show Screen Space Refraction Settings", ScreenSpaceRefractionSettingsEnabled, m_foldoutScreenSpaceRefractionSettings);
                            m_foldoutHorizonSettings = m_editorUtils.Panel("Show Horizon Settings", HorizonSettingsEnabled, m_foldoutHorizonSettings);
                        }
                        else
                        {
                            m_foldoutMainSkySettings = m_editorUtils.Panel("Show Main Settings", MainSkySettingsEnabled, m_foldoutMainSkySettings);
                            if (skyTypeNonHD == AmbientSkiesConsts.SkyType.ProceduralSky)
                            {
                                m_foldoutTimeOfDaySettings = m_editorUtils.Panel("Show Time Of Day Settings", TimeOfDaySettingsEnabled, m_foldoutTimeOfDaySettings);
                            }
                            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                            {
                                m_foldoutSkySettings = m_editorUtils.Panel("Show Sky Settings", SkyboxSettingsEnabled, m_foldoutSkySettings);
                                m_foldoutFogSettings = m_editorUtils.Panel("Show Fog Settings", FogSettingsEnabled, m_foldoutFogSettings);
                                m_foldoutAmbientSettings = m_editorUtils.Panel("Show Ambient Settings", AmbientSettingsEnabled, m_foldoutAmbientSettings);
                                m_foldoutSunSettings = m_editorUtils.Panel("Show Sun Settings", SunSettingsEnabled, m_foldoutSunSettings);
                            }
                            m_foldoutShadowSettings = m_editorUtils.Panel("Show Shadow Settings", ShadowSettingsEnabled, m_foldoutShadowSettings);
                            m_foldoutHorizonSettings = m_editorUtils.Panel("Show Horizon Settings", HorizonSettingsEnabled, m_foldoutHorizonSettings);
                        }
                    }

                    //Save Skybox settings to defaults
                    if (m_profiles.m_editSettings)
                    {
                        if (m_editorUtils.ButtonRight("SaveSkyboxSettingsToDefaultsButton"))
                        {
                            if (EditorUtility.DisplayDialog("WARNING!!",
                                "Are you sure you want to replace the default settings?", "Make My Day!", "Cancel"))
                            {
                                m_profiles.SaveSkyboxSettingsToDefault(m_selectedSkyboxProfileIndex);
                            }
                        }
                    }

                    //Revert Skybox
                    if (m_editorUtils.ButtonRight("RevertSkyboxButton"))
                    {
                        useSkies = m_selectedSkyboxProfile.defaultUseSkies;
                        fogType = m_selectedSkyboxProfile.defaultFogType;
                        fogDensity = m_selectedSkyboxProfile.fogDensity;
                        fogColor = m_selectedSkyboxProfile.defaultFogColor;
                        fogDistance = m_selectedSkyboxProfile.defaultFogDistance;
                        nearFogDistance = m_selectedSkyboxProfile.defaultNearFogDistance;
                        sunColor = m_selectedSkyboxProfile.defaultSunColor;
                        sunIntensity = m_selectedSkyboxProfile.defaultSunIntensity;
                        shadowDistance = m_selectedSkyboxProfile.defaultShadowDistance;
                        skyboxTint = m_selectedSkyboxProfile.defaultSkyboxTint;
                        skyboxExposure = m_selectedSkyboxProfile.defaultSkyboxExposure;
                        skyboxRotation = m_selectedSkyboxProfile.defaultSkyboxRotation;
                        skyboxGroundIntensity = m_selectedSkyboxProfile.defaultSkyboxGroundIntensity;
                        horizonEnabled = m_selectedSkyboxProfile.defaultHorizonSkyEnabled;
                        horizonScattering = m_selectedSkyboxProfile.defaultHorizonScattering;
                        horizonFogDensity = m_selectedSkyboxProfile.defaultFogDensity;
                        horizonFalloff = m_selectedSkyboxProfile.defaultHorizonFalloff;
                        horizonBlend = m_selectedSkyboxProfile.defaultHorizonBlend;
                        horizonScale = m_selectedSkyboxProfile.defaultHorizonSize;
                        followPlayer = m_selectedSkyboxProfile.defaultFollowPlayer;
                        horizonPosition = m_selectedSkyboxProfile.defaultHorizonPosition;
                        horizonUpdateTime = m_selectedSkyboxProfile.defaultHorizonUpdateTime;
                        ambientMode = m_selectedSkyboxProfile.defaultAmbientMode;
                        skyColor = m_selectedSkyboxProfile.defaultSkyColor;
                        equatorColor = m_selectedSkyboxProfile.defaultEquatorColor;
                        groundColor = m_selectedSkyboxProfile.defaultGroundColor;
                        enableSunDisk = m_selectedSkyboxProfile.defaultEnableSunDisk;
#if HDPipeline
                            //HD Pipeline
                            diffuseAmbientIntensity = m_selectedSkyboxProfile.defaultIndirectDiffuseIntensity;
                            specularAmbientIntensity = m_selectedSkyboxProfile.defaultIndirectSpecularIntensity;
                            topSkyColor = m_selectedSkyboxProfile.defaultTopColor;
                            middleSkyColor = m_selectedSkyboxProfile.defaultMiddleColor;
                            bottomSkyColor = m_selectedSkyboxProfile.defaultBottomColor;
                            gradientDiffusion = m_selectedSkyboxProfile.defaultGradientDiffusion;
                            sunSize = m_selectedSkyboxProfile.defaultSunSize;
                            sunConvergence = m_selectedSkyboxProfile.defaultSunConvergence;
                            atmosphereThickness = m_selectedSkyboxProfile.defaultAtmosphereThickness;
                            skyTint = m_selectedSkyboxProfile.defaultSkyTint;
                            groundColor = m_selectedSkyboxProfile.defaultGroundColor;
                            skyExposure = m_selectedSkyboxProfile.defaultSkyExposure;
                            skyMultiplier = m_selectedSkyboxProfile.defaultSkyMultiplier;
                            hDShadowQuality = m_selectedSkyboxProfile.defaultShadowQuality;
                            baseFogDistance = m_selectedSkyboxProfile.defaultVolumetricBaseFogDistance;
                            baseFogHeight = m_selectedSkyboxProfile.defaultVolumetricBaseFogHeight;
                            meanFogHeight = m_selectedSkyboxProfile.defaultVolumetricMeanHeight;
                            exponentialBaseFogHeight = m_selectedSkyboxProfile.defaultExponentialBaseHeight;
                            exponentialHeightAttenuation = m_selectedSkyboxProfile.defaultExponentialHeightAttenuation;
                            exponentialMaxFogDistance = m_selectedSkyboxProfile.defaultExponentialMaxFogDistance;
                            exponentialMipFogNear = m_selectedSkyboxProfile.defaultExponentialMipFogNear;
                            exponentialMipFogFar = m_selectedSkyboxProfile.defaultExponentialMipFogFar;
                            exponentialMipFogMax = m_selectedSkyboxProfile.defaultExponentialMipFogMaxMip;
                            linearFogHeightStart = m_selectedSkyboxProfile.defaultLinearHeightStart;
                            linearFogHeightEnd = m_selectedSkyboxProfile.defaultLinearHeightEnd;
                            linearFogMaxDistance = m_selectedSkyboxProfile.defaultLinearMaxFogDistance;
                            linearMipFogNear = m_selectedSkyboxProfile.defaultLinearMipFogNear;
                            linearMipFogFar = m_selectedSkyboxProfile.defaultLinearMipFogFar;
                            linearMipFogMax = m_selectedSkyboxProfile.defaultLinearMipFogMaxMip;
                            globalAnisotropy = m_selectedSkyboxProfile.defaultVolumetricGlobalAnisotropy;
                            globalLightProbeDimmer = m_selectedSkyboxProfile.defaultVolumetricGlobalLightProbeDimmer;
                            depthExtent = m_selectedSkyboxProfile.defaultVolumetricDistanceRange;
                            sliceDistribution = m_selectedSkyboxProfile.defaultVolumetricSliceDistributionUniformity;
                            shadowCascade = m_selectedSkyboxProfile.defaultCascadeCount;
                            enableContactShadows = m_selectedSkyboxProfile.defaultUseContactShadows;
                            contactLength = m_selectedSkyboxProfile.defaultContactShadowsLength;
                            contactScaleFactor = m_selectedSkyboxProfile.defaultContactShadowsDistanceScaleFactor;
                            contactMaxDistance = m_selectedSkyboxProfile.defaultMaxShadowDistance;
                            contactFadeDistance = m_selectedSkyboxProfile.defaultContactShadowsFadeDistance;
                            contactSampleCount = m_selectedSkyboxProfile.defaultContactShadowsSampleCount;
                            contactOpacity = m_selectedSkyboxProfile.defaultContactShadowsOpacity;
                            enableMicroShadows = m_selectedSkyboxProfile.defaultUseMicroShadowing;
                            microShadowOpacity = m_selectedSkyboxProfile.defaultMicroShadowOpacity;
#endif
                        //Revert Settings
                        m_profiles.RevertSkyboxSettingsToDefault(m_selectedSkyboxProfileIndex);
                    }
                }

#if AMBIENT_SKIES_CREATION && HDPipeline
                //Checks to see if the application is not playing
                if (!Application.isPlaying)
                {
                    //Creates a HD Volume Profile from current settings
                    if (m_editorUtils.ButtonRight("CreationProfileFromSettings"))
                    {
                        //If using HD Pipeline
                        if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                        {
                            //Locates the scenes Volume
                            Volume volume = GameObject.Find("High Definition Environment Volume").GetComponent<Volume>();
                            if (volume != null)
                            {
                                //Load up the new created profile
                                VolumeProfile newProfile = CreateHDVolumeProfileInternal(m_createdProfileNumber);

                                //Get current profile from Volume
                                VolumeProfile profile = volume.sharedProfile;

#if UNITY_2018_3
                                BakingSky bakingSky = volume.GetComponent<BakingSky>();
#elif UNITY_2019_1_OR_NEWER
                                StaticLightingSky bakingSky = volume.GetComponent<StaticLightingSky>();
#endif

                                //Applies all the settings to the new profile from old profile
                                ApplyNewHDRPProfileSettings(newProfile, profile);

                                //Save the asset database
                                AssetDatabase.SaveAssets();

                                m_createdProfileNumber += 1;

                                //Decide which profile to use
                                if (EditorUtility.DisplayDialog("New Profile Created Successfully!", "Your new HD Volume Profile has been created would you like to apply the new profile to the scene?", "Yes", "No"))
                                {
                                    //Apply new profile
                                    volume.sharedProfile = newProfile;
                                    bakingSky.profile = newProfile;
                                }
                                else
                                {
                                    //Apply old profile
                                    volume.sharedProfile = profile;
                                    bakingSky.profile = profile;
                                }
                            }
                            else
                            {
                                //When Volume in the scene is not found
                                Debug.LogError("Scene Volume High Definition Environment Volume could not be found. Ambient Skies create this object in your scene when using HDRP. Make sure you're using HDRP with ambient skies to use this feature");
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Warning!", "This feature is currently only available for High Definition Render Pipeline", "Ok");
                        }
                    }
                }
#endif
                //Apply settings
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_profiles, "Made changes");
                    EditorUtility.SetDirty(m_profiles);

                    //Sets the scene as dirty so changes can be saved
                    SkyboxUtils.MarkActiveSceneAsDirty();

                    //Bool to use skies system
                    m_selectedSkyboxProfile.useSkies = useSkies;

                    //TOD
                    m_profiles.pauseTimeKey = pauseTimeKey;
                    m_profiles.incrementUpKey = incrementUpKey;
                    m_profiles.incrementDownKey = incrementDownKey;
                    m_profiles.timeToAddOrRemove = timeToAddOrRemove;
                    m_profiles.rotateSunLeftKey = rotateSunLeftKey;
                    m_profiles.rotateSunRightKey = rotateSunRightKey;
                    m_profiles.sunRotationAmount = sunRotationAmount;

                    m_profiles.daySunIntensity = daySunIntensity;
                    m_profiles.daySunGradientColor = daySunGradientColor;
                    m_profiles.nightSunIntensity = nightSunIntensity;
                    m_profiles.nightSunGradientColor = nightSunGradientColor;

                    m_profiles.lightAnisotropy = lightAnisotropy;
                    m_profiles.lightProbeDimmer = lightProbeDimmer;
                    m_profiles.lightDepthExtent = lightDepthExtent;
                    m_profiles.sunSize = sunSizeAmount;
                    m_profiles.skyExposure = skyExposureAmount;

                    m_profiles.startFogDistance = startFogDistance;
                    m_profiles.dayFogDensity = dayFogDensity;
                    m_profiles.nightFogDensity = nightFogDensity;
                    m_profiles.dayFogDistance = dayFogDistance;
                    m_profiles.dayFogColor = dayFogColor;
                    m_profiles.nightFogDistance = nightFogDistance;
                    m_profiles.nightFogColor = nightFogColor;

                    m_profiles.syncPostProcessing = syncPostProcessing;
                    m_profiles.dayTempature = dayTempature;
                    m_profiles.dayPostFXColor = dayPostFXColor;
                    m_profiles.nightTempature = nightTempature;
                    m_profiles.nightPostFXColor = nightPostFXColor;

                    m_profiles.realtimeGIUpdate = realtimeGIUpdate;
                    m_profiles.gIUpdateInterval = gIUpdateInterval;

                    m_profiles.currentSeason = currentSeason;
                    m_profiles.hemisphereOrigin = hemisphereOrigin;

                    m_profiles.skyboxRotation = timeOfDaySkyboxRotation;
                    m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;

                    m_profiles.pauseTime = pauseTime;
                    m_profiles.currentTimeOfDay = currentTimeOfDay;
                    m_profiles.nightLengthInSeconds = nightLengthInSeconds;
                    m_profiles.dayLengthInSeconds = dayLengthInSeconds;
                    m_profiles.dayDate = dayDate;
                    m_profiles.monthDate = monthDate;
                    m_profiles.yearDate = yearDate;

                    //Skybox
                    m_selectedSkyboxProfile.fogType = fogType;
                    m_selectedSkyboxProfile.ambientMode = ambientMode;
                    m_profiles.skyTypeNonHD = skyTypeNonHD;

                    m_profiles.systemTypes = systemtype;
                    m_profiles.targetPlatform = targetPlatform;
                    m_profiles.skyType = skyType;
                    m_profiles.skyTypeNonHD = skyTypeNonHD;

                    m_selectedSkyboxProfileIndex = newSkyboxSelection;
                    m_selectedSkyboxProfile.skyboxTint = skyboxTint;
                    m_selectedSkyboxProfile.skyboxExposure = skyboxExposure;
                    m_selectedSkyboxProfile.skyboxRotation = skyboxRotation;
                    m_selectedSkyboxProfile.skyboxPitch = skyboxPitch;
                    m_selectedSkyboxProfile.customSkybox = customSkybox;
                    m_selectedSkyboxProfile.proceduralAtmosphereThickness = proceduralAtmosphereThickness;
                    m_selectedSkyboxProfile.proceduralGroundColor = proceduralGroundColor;
                    m_selectedSkyboxProfile.proceduralSkyTint = proceduralSkyTint;
                    m_selectedSkyboxProfile.proceduralSkyExposure = proceduralSkyExposure;
                    m_selectedSkyboxProfile.proceduralSkyboxPitch = proceduralSkyboxPitch;
                    m_selectedSkyboxProfile.proceduralSkyboxRotation = proceduralSkyboxRotation;
                    m_selectedSkyboxProfile.includeSunInBaking = includeSunInBaking;
                    m_selectedSkyboxProfile.name = profileName;

                    m_selectedSkyboxProfile.proceduralSunColor = proceduralSunColor;
                    m_selectedSkyboxProfile.proceduralSunIntensity = proceduralSunIntensity;
                    m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                    m_selectedSkyboxProfile.isProceduralSkybox = isProceduralSkybox;

                    m_selectedSkyboxProfile.fogColor = fogColor;
                    m_selectedSkyboxProfile.fogDistance = fogDistance;
                    m_selectedSkyboxProfile.fogDensity = fogDensity;
                    m_selectedSkyboxProfile.nearFogDistance = nearFogDistance;
                    m_selectedSkyboxProfile.proceduralFogColor = proceduralFogColor;
                    m_selectedSkyboxProfile.proceduralFogDensity = proceduralFogDensity;
                    m_selectedSkyboxProfile.proceduralFogDistance = proceduralFogDistance;
                    m_selectedSkyboxProfile.proceduralNearFogDistance = proceduralNearFogDistance;
                    m_profiles.configurationType = configurationType;

                    m_selectedSkyboxProfile.skyColor = skyColor;
                    m_selectedSkyboxProfile.equatorColor = equatorColor;
                    m_selectedSkyboxProfile.groundColor = groundColor;
                    m_selectedSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;

                    m_selectedSkyboxProfile.sunColor = sunColor;
                    m_selectedSkyboxProfile.sunIntensity = sunIntensity;
                    m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                    m_selectedSkyboxProfile.proceduralSunIntensity = proceduralSunIntensity;
                    m_selectedSkyboxProfile.proceduralSunColor = proceduralSunColor;
                    m_selectedSkyboxProfile.proceduralSunSize = proceduralSunSize;
                    m_selectedSkyboxProfile.proceduralSunSizeConvergence = proceduralSunSizeConvergence;
                    m_selectedSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                    m_selectedSkyboxProfile.shadowStrength = shadowStrength;

                    m_selectedSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                    m_selectedSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                    m_selectedSkyboxProfile.horizonScattering = horizonScattering;
                    m_selectedSkyboxProfile.horizonFogDensity = horizonFogDensity;
                    m_selectedSkyboxProfile.horizonFalloff = horizonFalloff;
                    m_selectedSkyboxProfile.horizonBlend = horizonBlend;
                    m_selectedSkyboxProfile.horizonSize = horizonScale;
                    m_selectedSkyboxProfile.followPlayer = followPlayer;
                    m_selectedSkyboxProfile.horizonPosition = horizonPosition;

                    m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedSkyboxProfile.cascadeCount = shadowCascade;
                    m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;
                    m_selectedSkyboxProfile.shadowType = shadowType;
                    m_selectedSkyboxProfile.shadowResolution = shadowResolution;
                    m_selectedSkyboxProfile.shadowProjection = shadowProjection;
#if HDPipeline
                    m_profiles.skyType = skyType; 

                    m_selectedSkyboxProfile.sunSize = sunSize;
                    m_selectedSkyboxProfile.sunConvergence = sunConvergence;
                    m_selectedSkyboxProfile.atmosphereThickness = atmosphereThickness;
                    m_selectedSkyboxProfile.skyTint = skyTint;
                    m_selectedSkyboxProfile.groundColor = groundColor;
                    m_selectedSkyboxProfile.skyExposure = skyExposure;
                    m_selectedSkyboxProfile.skyMultiplier = skyMultiplier;
                    m_selectedSkyboxProfile.topColor = topSkyColor;
                    m_selectedSkyboxProfile.middleColor = middleSkyColor;
                    m_selectedSkyboxProfile.bottomColor = bottomSkyColor;
                    m_selectedSkyboxProfile.gradientDiffusion = gradientDiffusion;

                    m_selectedSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                    m_selectedSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                    m_selectedSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                    m_selectedSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                    m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;
                    m_selectedSkyboxProfile.volumetricDistanceRange = depthExtent;
                    m_selectedSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                    m_selectedSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                    m_selectedSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                    m_selectedSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                    m_selectedSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                    m_selectedSkyboxProfile.densityMaskTiling = densityMaskTiling;

                    m_selectedSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                    m_selectedSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                    m_selectedSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                    m_selectedSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                    m_selectedSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                    m_selectedSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                    m_selectedSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                    m_selectedSkyboxProfile.linearFogDensity = linearFogDensity;
                    m_selectedSkyboxProfile.linearHeightStart = linearFogHeightStart;
                    m_selectedSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                    m_selectedSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                    m_selectedSkyboxProfile.linearMipFogNear = linearMipFogNear;
                    m_selectedSkyboxProfile.linearMipFogFar = linearMipFogFar;
                    m_selectedSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                    m_selectedSkyboxProfile.topColor = topSkyColor;
                    m_selectedSkyboxProfile.middleColor = middleSkyColor;
                    m_selectedSkyboxProfile.bottomColor = bottomSkyColor;
                    m_selectedSkyboxProfile.gradientDiffusion = gradientDiffusion;

                    m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                    m_selectedSkyboxProfile.sunSize = sunSize;
                    m_selectedSkyboxProfile.sunConvergence = sunConvergence;
                    m_selectedSkyboxProfile.atmosphereThickness = atmosphereThickness;
                    m_selectedSkyboxProfile.skyTint = skyTint;
                    m_selectedSkyboxProfile.groundColor = groundColor;
                    m_selectedSkyboxProfile.skyExposure = skyExposure;
                    m_selectedSkyboxProfile.skyMultiplier = skyMultiplier;

                    m_selectedSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                    m_selectedSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;

                    m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                    m_selectedSkyboxProfile.shadowQuality = hDShadowQuality;
                    m_selectedSkyboxProfile.cascadeCount = shadowCascade;
                    m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;

                    m_selectedSkyboxProfile.useContactShadows = enableContactShadows;
                    m_selectedSkyboxProfile.contactShadowsLength = contactLength;
                    m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                    m_selectedSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                    m_selectedSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                    m_selectedSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                    m_selectedSkyboxProfile.contactShadowsOpacity = contactOpacity;

                    m_selectedSkyboxProfile.useMicroShadowing = enableMicroShadows;
                    m_selectedSkyboxProfile.microShadowOpacity = microShadowOpacity;

                    m_selectedSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                    m_selectedSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                    m_selectedSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                    m_selectedSkyboxProfile.objectThickness = ssrObjectThickness;
                    m_selectedSkyboxProfile.minSmoothness = ssrMinSmoothness;
                    m_selectedSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                    m_selectedSkyboxProfile.reflectSky = ssrReflectSky;

                    m_selectedSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                    m_selectedSkyboxProfile.screenWeightDistance = ssrWeightDistance;
#endif

                    //Selection changing things - exit immediately to not polute settings
                    if (newSkyboxSelection != m_selectedSkyboxProfileIndex)
                    {
                        m_selectedSkyboxProfileIndex = newSkyboxSelection;
                        m_selectedSkyboxProfile = m_profiles.m_skyProfiles[newSkyboxSelection];
                        if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                        }
                    }

                    //Update skies
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);

                        HorizonUtils.SetHorizonShaderSettings(m_profiles, m_selectedSkyboxProfile);
                    }

                    EditorPrefs.SetString("AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex].name);
                }
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Display the post processing tab
        /// </summary>
        private void PostProcessingTab()
        {
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //See if we can select an ambiet skies skybox
            if (m_selectedPostProcessingProfile == null)
            {
                if (m_editorUtils.Button("SelectPostProcessingProfileButton"))
                {
                    m_selectedPostProcessingProfileIndex = 0;
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                    return;
                }

                return;
            }

            //If profile is there
            if (m_selectedPostProcessingProfile != null)
            {
                #region Post Processing Values
                //Global systems
                systemtype = m_profiles.systemTypes;

                //Enable Post Fx
                usePostProcess = m_selectedPostProcessingProfile.usePostProcess;

                //Selection
                newPPSelection = m_selectedPostProcessingProfile.profileIndex;

                //Custom profile
#if UNITY_POST_PROCESSING_STACK_V2
                customPostProcessingProfile = m_selectedPostProcessingProfile.customPostProcessingProfile;
#endif
                //HDR Mode
                hDRMode = m_selectedPostProcessingProfile.hDRMode;

                //Anti Aliasing Mode
                antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;

                //Target Platform
                targetPlatform = m_profiles.targetPlatform;

                //AO settings
                aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                aoAmount = m_selectedPostProcessingProfile.aoAmount;
                aoColor = m_selectedPostProcessingProfile.aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                ambientOcclusionMode = m_selectedPostProcessingProfile.ambientOcclusionMode;
#endif
                //Exposure settings
                autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                exposureAmount = m_selectedPostProcessingProfile.exposureAmount;
                exposureMin = m_selectedPostProcessingProfile.exposureMin;
                exposureMax = m_selectedPostProcessingProfile.exposureMax;

                //Bloom settings
                bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                bloomIntensity = m_selectedPostProcessingProfile.bloomAmount;
                bloomThreshold = m_selectedPostProcessingProfile.bloomThreshold;
                lensIntensity = m_selectedPostProcessingProfile.lensIntensity;
                lensTexture = m_selectedPostProcessingProfile.lensTexture;

                //Chromatic Aberration
                chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                chromaticAberrationIntensity = m_selectedPostProcessingProfile.chromaticAberrationIntensity;

                //Color Grading settings
                colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
#if UNITY_POST_PROCESSING_STACK_V2
                colorGradingMode = m_selectedPostProcessingProfile.colorGradingMode;
#endif
                colorGradingLut = m_selectedPostProcessingProfile.colorGradingLut;
                colorGradingPostExposure = m_selectedPostProcessingProfile.colorGradingPostExposure;
                colorGradingColorFilter = m_selectedPostProcessingProfile.colorGradingColorFilter;
                colorGradingTempature = m_selectedPostProcessingProfile.colorGradingTempature;
                colorGradingTint = m_selectedPostProcessingProfile.colorGradingTint;
                colorGradingSaturation = m_selectedPostProcessingProfile.colorGradingSaturation;
                colorGradingContrast = m_selectedPostProcessingProfile.colorGradingContrast;

                //DOF settings
                depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                depthOfFieldAperture = m_selectedPostProcessingProfile.depthOfFieldAperture;
                depthOfFieldFocalLength = m_selectedPostProcessingProfile.depthOfFieldFocalLength;
                depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                focusOffset = m_selectedPostProcessingProfile.focusOffset;
                targetLayer = m_selectedPostProcessingProfile.targetLayer;
                maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
#if UNITY_POST_PROCESSING_STACK_V2
                maxBlurSize = m_selectedPostProcessingProfile.maxBlurSize;
#endif
                //Distortion settings
                distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                distortionIntensity = m_selectedPostProcessingProfile.distortionIntensity;
                distortionScale = m_selectedPostProcessingProfile.distortionScale;

                //Grain settings
                grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                grainIntensity = m_selectedPostProcessingProfile.grainIntensity;
                grainSize = m_selectedPostProcessingProfile.grainSize;

                //SSR settings
                screenSpaceReflectionsEnabled = m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled;
                maximumIterationCount = m_selectedPostProcessingProfile.maximumIterationCount;
                thickness = m_selectedPostProcessingProfile.thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                screenSpaceReflectionResolution = m_selectedPostProcessingProfile.spaceReflectionResolution;
                screenSpaceReflectionPreset = m_selectedPostProcessingProfile.screenSpaceReflectionPreset;
#endif
                maximumMarchDistance = m_selectedPostProcessingProfile.maximumMarchDistance;
                distanceFade = m_selectedPostProcessingProfile.distanceFade;
                screenSpaceVignette = m_selectedPostProcessingProfile.screenSpaceVignette;

                //Vignette settings
                vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                vignetteIntensity = m_selectedPostProcessingProfile.vignetteIntensity;
                vignetteSmoothness = m_selectedPostProcessingProfile.vignetteSmoothness;

                //Motion Blur settings
                motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                motionShutterAngle = m_selectedPostProcessingProfile.shutterAngle;
                motionSampleCount = m_selectedPostProcessingProfile.sampleCount;
#if Mewlist_Clouds
                //Massive Cloud Settings
                massiveCloudsEnabled = m_selectedPostProcessingProfile.massiveCloudsEnabled;
                cloudProfile = m_selectedPostProcessingProfile.cloudProfile;
                syncGlobalFogColor = m_selectedPostProcessingProfile.syncGlobalFogColor;
                syncBaseFogColor = m_selectedPostProcessingProfile.syncBaseFogColor;
                cloudsFogColor = m_selectedPostProcessingProfile.cloudsFogColor;
                cloudsBaseFogColor = m_selectedPostProcessingProfile.cloudsBaseFogColor;
                cloudIsHDRP = m_selectedPostProcessingProfile.cloudIsHDRP;
#endif
                #endregion

                EditorGUI.BeginChangeCheck();

                m_editorUtils.Heading("PostProcessingSettingsHeader");

#if UNITY_2018_1_OR_NEWER && !HDPipeline

                usePostProcess = m_editorUtils.ToggleLeft("UsePostProcess", usePostProcess);
                EditorGUILayout.Space();
         
                //If using post processing
                if (usePostProcess)
                {
                    m_editorUtils.Link("LearnMoreAboutPostFX");
                    EditorGUILayout.Space();

                    //m_foldoutGlobalSettings = m_editorUtils.Panel("Show Global Settings", GlobalSettingsEnabled, m_foldoutGlobalSettings);
                    m_foldoutMainPostProcessing = m_editorUtils.Panel("Show Main Settings", MainPostProcessSettingsEnabled, m_foldoutMainPostProcessing);
                    if (m_selectedPostProcessingProfile.name != "User")
                    {
                        m_foldoutAmbientOcclusion = m_editorUtils.Panel("Show Ambient Occlusion Settings", AmbientOcclusionSettingsEnabled, m_foldoutAmbientOcclusion);
                        m_foldoutAutoExposure = m_editorUtils.Panel("Show Auto Exposure Settings", AutoExposureSettingsEnabled, m_foldoutAutoExposure);
                        m_foldoutBloom = m_editorUtils.Panel("Show Bloom Settings", BloomSettingsEnabled, m_foldoutBloom);
                        m_foldoutChromaticAberration = m_editorUtils.Panel("Show Chromatic Aberration Settings", ChromaticAberrationSettingsEnabled, m_foldoutChromaticAberration);
                        m_foldoutColorGrading = m_editorUtils.Panel("Show Color Grading Settings", ColorGradingSettingsEnabled, m_foldoutColorGrading);
                        m_foldoutDepthOfField = m_editorUtils.Panel("Show Depth Of Field Settings", DepthOfFieldSettingsEnabled, m_foldoutDepthOfField);
                        m_foldoutGrain = m_editorUtils.Panel("Show Grain Settings", GrainSettingsEnabled, m_foldoutGrain);
                        m_foldoutLensDistortion = m_editorUtils.Panel("Show Lens Distortion Settings", LensDistortionSettingsEnabled, m_foldoutLensDistortion);
                        if (m_massiveCloudsPath)
                        {
                            m_foldoutMassiveClouds = m_editorUtils.Panel("Show Massive Clouds Settings", MassiveCloudsSettingsEnabled, m_foldoutMassiveClouds);
                        }
                        m_foldoutMotionBlur = m_editorUtils.Panel("Show Motion Blur Settings", MotionBlurSettingsEnabled, m_foldoutMotionBlur);
                        m_foldoutScreenSpaceReflections = m_editorUtils.Panel("Show Screen Space Reflection Settings", ScreenSpaceReflectionsSettingsEnabled, m_foldoutScreenSpaceReflections);
                        m_foldoutVignette = m_editorUtils.Panel("Show Vignette Settings", VignetteSettingsEnabled, m_foldoutVignette);
                    }

                    //Save to defaults
                    if (m_profiles.m_editSettings)
                    {
                        if (m_editorUtils.ButtonRight("SavePostProcessingSettingsToDefaultsButton"))
                        {
                            if (EditorUtility.DisplayDialog("WARNING!!",
                                "Are you sure you want to replace the default settings?", "Make My Day!", "Cancel"))
                            {
                                m_profiles.SavePostProcessingSettingsToDefault(m_selectedPostProcessingProfileIndex);
                            }
                        }
                    }

#if !UNITY_2019_1_OR_NEWER
                    //Revert
                    if (m_editorUtils.ButtonRight("RevertPostProcesssingButton"))
                    {
                        //Update settings to stop them wiping new setttings
                        usePostProcess = m_selectedPostProcessingProfile.defaultUsePostProcess;
                        aoEnabled = m_selectedPostProcessingProfile.defaultAoEnabled;
                        aoAmount = m_selectedPostProcessingProfile.defaultAoAmount;
                        aoColor = m_selectedPostProcessingProfile.defaultAoColor;
                        autoExposureEnabled = m_selectedPostProcessingProfile.defaultAutoExposureEnabled;
                        exposureAmount = m_selectedPostProcessingProfile.defaultExposureAmount;
                        exposureMin = m_selectedPostProcessingProfile.defaultExposureMin;
                        exposureMax = m_selectedPostProcessingProfile.defaultExposureMax;
                        bloomEnabled = m_selectedPostProcessingProfile.defaultBloomEnabled;
                        bloomIntensity = m_selectedPostProcessingProfile.defaultBloomAmount;
                        bloomThreshold = m_selectedPostProcessingProfile.defaultBloomThreshold;
                        colorGradingEnabled = m_selectedPostProcessingProfile.defaultColorGradingEnabled;
                        colorGradingTempature = m_selectedPostProcessingProfile.defaultColorGradingTempature;
                        colorGradingTint = m_selectedPostProcessingProfile.defaultColorGradingTint;
                        colorGradingSaturation = m_selectedPostProcessingProfile.defaultColorGradingSaturation;
                        colorGradingContrast = m_selectedPostProcessingProfile.defaultColorGradingContrast;
                        lensTexture = m_selectedPostProcessingProfile.defaultLensTexture;
                        lensIntensity = m_selectedPostProcessingProfile.defaultLensIntensity;
                        depthOfFieldEnabled = m_selectedPostProcessingProfile.defaultDepthOfFieldEnabled;
                        depthOfFieldFocusDistance = m_selectedPostProcessingProfile.defaultDepthOfFieldFocusDistance;
                        depthOfFieldAperture = m_selectedPostProcessingProfile.defaultDepthOfFieldAperture;
                        depthOfFieldFocalLength = m_selectedPostProcessingProfile.defaultDepthOfFieldFocalLength;
                        focusOffset = m_selectedPostProcessingProfile.defaultFocusOffset;
                        targetLayer = m_selectedPostProcessingProfile.defaultTargetLayer;
                        maxFocusDistance = m_selectedPostProcessingProfile.defaultMaxFocusDistance;
                        distortionEnabled = m_selectedPostProcessingProfile.defaultDistortionEnabled;
                        distortionIntensity = m_selectedPostProcessingProfile.defaultDistortionIntensity;
                        distortionScale = m_selectedPostProcessingProfile.defaultDistortionScale;
                        grainEnabled = m_selectedPostProcessingProfile.defaultGrainEnabled;
                        grainIntensity = m_selectedPostProcessingProfile.defaultGrainIntensity;
                        grainSize = m_selectedPostProcessingProfile.defaultGrainSize;
                        screenSpaceReflectionsEnabled = m_selectedPostProcessingProfile.defaultScreenSpaceReflectionsEnabled;
                        maximumIterationCount = m_selectedPostProcessingProfile.defaultMaximumIterationCount;
                        thickness = m_selectedPostProcessingProfile.defaultThickness;
#if UNITY_POST_PROCESSING_STACK_V2
                        screenSpaceReflectionResolution = m_selectedPostProcessingProfile.defaultSpaceReflectionResolution;
                        screenSpaceReflectionPreset = m_selectedPostProcessingProfile.defaultScreenSpaceReflectionPreset;
#endif
                        maximumMarchDistance = m_selectedPostProcessingProfile.defaultMaximumMarchDistance;
                        distanceFade = m_selectedPostProcessingProfile.defaultDistanceFade;
                        screenSpaceVignette = m_selectedPostProcessingProfile.defaultScreenSpaceVignette;
                        vignetteEnabled = m_selectedPostProcessingProfile.defaultVignetteEnabled;
                        vignetteIntensity = m_selectedPostProcessingProfile.defaultVignetteIntensity;
                        vignetteSmoothness = m_selectedPostProcessingProfile.defaultVignetteSmoothness;
                        motionBlurEnabled = m_selectedPostProcessingProfile.defaultMotionBlurEnabled;
                        motionShutterAngle = m_selectedPostProcessingProfile.defaultShutterAngle;
                        motionSampleCount = m_selectedPostProcessingProfile.defaultSampleCount;

                        m_profiles.RevertPostProcessingSettingsToDefault(m_selectedPostProcessingProfileIndex);
                    }
#endif
                }
#else
                usePostProcess = m_editorUtils.ToggleLeft("UsePostProcess", usePostProcess);
                EditorGUILayout.Space();

                m_editorUtils.Text("PostProcessing2019SetupText");

#if AMBIENT_SKIES_CREATION && HDPipeline
                //Checks to see if the application is not playing
                if (!Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    m_editorUtils.Heading("ConversionTools");
                    convertPostProfile = (PostProcessProfile)m_editorUtils.ObjectField("ConvertPostProcessingProfile", convertPostProfile, typeof(PostProcessProfile), false, GUILayout.Height(16f));
                    focusAsset = m_editorUtils.Toggle("FocusAsset", focusAsset);
                    renamePostProcessProfile = m_editorUtils.Toggle("RenameProfile", renamePostProcessProfile);
                    convertPostProfileName = m_editorUtils.TextField("ProfileNewName", convertPostProfileName);

                    //Creates a HD Volume Profile from current settings
                    if (m_editorUtils.ButtonRight("ConvertToHDRPPostProcessing"))
                    {
                        if (convertPostProfile != null)
                        {
                            VolumeProfile newProfile = CreateHDRPPostProcessingProfile(convertPostProfile, renamePostProcessProfile, convertPostProfileName);
                            if (newProfile != null)
                            {
                                ConvertPostProcessingToHDRP(newProfile, convertPostProfile, renamePostProcessProfile, convertPostProfileName);
                            }

                            //Save the asset database
                            AssetDatabase.SaveAssets();
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Error!", "You're missing a conversion profile, please add one then try again.", "Ok");
                        }
                    }
                }
#endif
#endif
                //Apply settings
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_profiles, "Made changes");
                    EditorUtility.SetDirty(m_profiles);

                    //Sets the scene as dirty so changes can be saved
                    SkyboxUtils.MarkActiveSceneAsDirty();

                    //Apply changes
                    m_selectedPostProcessingProfile.usePostProcess = usePostProcess;

                    //Selection changing things - exit immediately to not polute settings
                    if (newPPSelection != m_selectedPostProcessingProfileIndex)
                    {
                        EditorUtility.SetDirty(m_profiles);

                        m_selectedPostProcessingProfileIndex = newPPSelection;
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                    }

                    m_selectedPostProcessingProfile.antiAliasingMode = antiAliasingMode;
                    m_selectedPostProcessingProfile.hDRMode = hDRMode;

                    m_selectedPostProcessingProfile.aoEnabled = aoEnabled;
                    m_selectedPostProcessingProfile.aoAmount = aoAmount;
                    m_selectedPostProcessingProfile.aoColor = aoColor;

                    m_selectedPostProcessingProfile.autoExposureEnabled = autoExposureEnabled;
                    m_selectedPostProcessingProfile.exposureAmount = exposureAmount;
                    m_selectedPostProcessingProfile.exposureMin = exposureMin;
                    m_selectedPostProcessingProfile.exposureMax = exposureMax;

                    m_selectedPostProcessingProfile.bloomEnabled = bloomEnabled;
                    m_selectedPostProcessingProfile.bloomAmount = bloomIntensity;
                    m_selectedPostProcessingProfile.bloomThreshold = bloomThreshold;
                    m_selectedPostProcessingProfile.lensTexture = lensTexture;
                    m_selectedPostProcessingProfile.lensIntensity = lensIntensity;

                    m_selectedPostProcessingProfile.chromaticAberrationEnabled = chromaticAberrationEnabled;
                    m_selectedPostProcessingProfile.chromaticAberrationIntensity = chromaticAberrationIntensity;

                    m_selectedPostProcessingProfile.colorGradingLut = colorGradingLut;
                    m_selectedPostProcessingProfile.colorGradingColorFilter = colorGradingColorFilter;
                    m_selectedPostProcessingProfile.colorGradingPostExposure = colorGradingPostExposure;
                    m_selectedPostProcessingProfile.colorGradingEnabled = colorGradingEnabled;
                    m_selectedPostProcessingProfile.colorGradingTempature = colorGradingTempature;
                    m_selectedPostProcessingProfile.colorGradingTint = colorGradingTint;
                    m_selectedPostProcessingProfile.colorGradingSaturation = colorGradingSaturation;
                    m_selectedPostProcessingProfile.colorGradingContrast = colorGradingContrast;

                    m_selectedPostProcessingProfile.depthOfFieldEnabled = depthOfFieldEnabled;
                    m_selectedPostProcessingProfile.depthOfFieldMode = depthOfFieldMode;
                    m_selectedPostProcessingProfile.depthOfFieldFocusDistance = depthOfFieldFocusDistance;
                    m_selectedPostProcessingProfile.depthOfFieldTrackingType = depthOfFieldTrackingType;
                    m_selectedPostProcessingProfile.focusOffset = focusOffset;
                    m_selectedPostProcessingProfile.targetLayer = targetLayer;
                    m_selectedPostProcessingProfile.maxFocusDistance = maxFocusDistance;

                    m_selectedPostProcessingProfile.depthOfFieldAperture = depthOfFieldAperture;
                    m_selectedPostProcessingProfile.depthOfFieldFocalLength = depthOfFieldFocalLength;

                    m_selectedPostProcessingProfile.grainEnabled = grainEnabled;
                    m_selectedPostProcessingProfile.grainIntensity = grainIntensity;
                    m_selectedPostProcessingProfile.grainSize = grainSize;

                    m_selectedPostProcessingProfile.distortionEnabled = distortionEnabled;
                    m_selectedPostProcessingProfile.distortionIntensity = distortionIntensity;
                    m_selectedPostProcessingProfile.distortionScale = distortionScale;

                    m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled = screenSpaceReflectionsEnabled;
                    m_selectedPostProcessingProfile.maximumIterationCount = maximumIterationCount;
                    m_selectedPostProcessingProfile.thickness = thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                    m_selectedPostProcessingProfile.maxBlurSize = maxBlurSize;

                    m_selectedPostProcessingProfile.colorGradingMode = colorGradingMode;

                    m_selectedPostProcessingProfile.ambientOcclusionMode = ambientOcclusionMode;

                    m_selectedPostProcessingProfile.customPostProcessingProfile = customPostProcessingProfile;

                    m_selectedPostProcessingProfile.spaceReflectionResolution = screenSpaceReflectionResolution;
                    m_selectedPostProcessingProfile.screenSpaceReflectionPreset = screenSpaceReflectionPreset;
#endif
                    m_selectedPostProcessingProfile.maximumMarchDistance = maximumMarchDistance;
                    m_selectedPostProcessingProfile.distanceFade = distanceFade;
                    m_selectedPostProcessingProfile.screenSpaceVignette = screenSpaceVignette;

                    m_selectedPostProcessingProfile.vignetteEnabled = vignetteEnabled;
                    m_selectedPostProcessingProfile.vignetteIntensity = vignetteIntensity;
                    m_selectedPostProcessingProfile.vignetteSmoothness = vignetteSmoothness;

                    m_selectedPostProcessingProfile.motionBlurEnabled = motionBlurEnabled;
                    m_selectedPostProcessingProfile.shutterAngle = motionShutterAngle;
                    m_selectedPostProcessingProfile.sampleCount = motionSampleCount;

                    //Update post processing
                    PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);

                    EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[newPPSelection].name);
                }
            }

            GUI.enabled = true;
        }

        /// <summary>
        /// Display the lighting tab
        /// </summary>
        private void LightingTab()
        {
            if (m_isCompiling)
            {
                GUI.enabled = false;
            }

            //See if we can select an ambiet skies lightmapping setting
            if (m_selectedLightingProfile == null)
            {
                if (m_editorUtils.Button("SelectLightmappingProfileButton"))
                {
                    m_selectedLightingProfileIndex = 0;
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);
                    return;
                }
            }

            //If profile is there
            if (m_selectedLightingProfile != null)
            {
                EditorGUI.BeginChangeCheck();

                m_editorUtils.Heading("LightmappingSettings");

                #region Lightmaps Variables
                //Global Settings
                systemtype = m_profiles.systemTypes;

                //Target Platform
                targetPlatform = m_profiles.targetPlatform;

                //Local values
                newLightmappingSettings = m_selectedLightingProfile.profileIndex;
                autoLightmapGeneration = m_selectedLightingProfile.autoLightmapGeneration;

                enableLightmapSettings = m_selectedLightingProfile.enableLightmapSettings;

                realtimeGlobalIllumination = m_selectedLightingProfile.realtimeGlobalIllumination;
                bakedGlobalIllumination = m_selectedLightingProfile.bakedGlobalIllumination;
                lightmappingMode = m_selectedLightingProfile.lightmappingMode;
                indirectRelolution = m_selectedLightingProfile.indirectRelolution;
                lightmapResolution = m_selectedLightingProfile.lightmapResolution;
                lightmapPadding = m_selectedLightingProfile.lightmapPadding;
                useHighResolutionLightmapSize = m_selectedLightingProfile.useHighResolutionLightmapSize;
                compressLightmaps = m_selectedLightingProfile.compressLightmaps;
                ambientOcclusion = m_selectedLightingProfile.ambientOcclusion;
                maxDistance = m_selectedLightingProfile.maxDistance;
                indirectContribution = m_selectedLightingProfile.indirectContribution;
                directContribution = m_selectedLightingProfile.directContribution;
                useDirectionalMode = m_selectedLightingProfile.useDirectionalMode;
                lightIndirectIntensity = m_selectedLightingProfile.lightIndirectIntensity;
                lightBoostIntensity = m_selectedLightingProfile.lightBoostIntensity;
                finalGather = m_selectedLightingProfile.finalGather;
                finalGatherRayCount = m_selectedLightingProfile.finalGatherRayCount;
                finalGatherDenoising = m_selectedLightingProfile.finalGatherDenoising;
                #endregion

                enableLightmapSettings = m_editorUtils.ToggleLeft("UseLightmaps", enableLightmapSettings);
                EditorGUILayout.Space();

                if (enableLightmapSettings)
                {
                    m_editorUtils.Link("LearnMoreAboutLightmaps");
                    EditorGUILayout.Space();

                    //m_foldoutGlobalSettings = m_editorUtils.Panel("Show Global Settings", GlobalSettingsEnabled, m_foldoutGlobalSettings);
                    m_foldoutMainLighting = m_editorUtils.Panel("Show Main Settings", MainLightingSettingsEnabled, m_foldoutMainLighting);
                    m_foldoutRealtimeGI = m_editorUtils.Panel("Show Realtime GI Settings", RealtimeGISettingsEnabled, m_foldoutRealtimeGI);
                    m_foldoutBakedGI = m_editorUtils.Panel("Show Baked GI Settings", BakedGISettingsEnabled, m_foldoutBakedGI);

                    EditorGUILayout.Space();
                    m_editorUtils.Heading("LightingSettingsHeader");
                    m_editorUtils.Text("LightingButtonsInfo", GUILayout.MinHeight(100f), GUILayout.MaxHeight(250f), GUILayout.MinWidth(200f), GUILayout.MaxWidth(700f));

                    //Revert
                    if (m_editorUtils.ButtonRight("RevertPostProcesssingButton"))
                    {
                        //Update settings to stop them wiping new setttings
                        realtimeGlobalIllumination = m_selectedLightingProfile.defaultRealtimeGlobalIllumination;
                        bakedGlobalIllumination = m_selectedLightingProfile.defaultBakedGlobalIllumination;
                        lightmappingMode = m_selectedLightingProfile.defaultLightmappingMode;
                        indirectRelolution = m_selectedLightingProfile.defaultIndirectRelolution;
                        lightmapResolution = m_selectedLightingProfile.defaultLightmapResolution;
                        lightmapPadding = m_selectedLightingProfile.defaultLightmapPadding;
                        useHighResolutionLightmapSize = m_selectedLightingProfile.defaultUseHighResolutionLightmapSize;
                        compressLightmaps = m_selectedLightingProfile.defaultCompressLightmaps;
                        ambientOcclusion = m_selectedLightingProfile.defaultAmbientOcclusion;
                        maxDistance = m_selectedLightingProfile.defaultMaxDistance;
                        indirectContribution = m_selectedLightingProfile.defaultIndirectContribution;
                        directContribution = m_selectedLightingProfile.defaultDirectContribution;
                        useDirectionalMode = m_selectedLightingProfile.defaultUseDirectionalMode;
                    }
                }

                //Apply settings
                if (EditorGUI.EndChangeCheck())
                {                   
                    Undo.RecordObject(m_profiles, "Made changes");
                    EditorUtility.SetDirty(m_profiles);

                    //Sets the scene as dirty so changes can be saved
                    SkyboxUtils.MarkActiveSceneAsDirty();

                    //Apply changes
                    m_selectedLightingProfile.enableLightmapSettings = enableLightmapSettings;

                    m_selectedLightingProfile.autoLightmapGeneration = autoLightmapGeneration;

                    m_selectedLightingProfile.realtimeGlobalIllumination = realtimeGlobalIllumination;
                    m_selectedLightingProfile.indirectRelolution = indirectRelolution;
                    m_selectedLightingProfile.useDirectionalMode = useDirectionalMode;
                    m_selectedLightingProfile.lightIndirectIntensity = lightIndirectIntensity;
                    m_selectedLightingProfile.lightBoostIntensity = lightBoostIntensity;

                    m_selectedLightingProfile.bakedGlobalIllumination = bakedGlobalIllumination;
                    m_selectedLightingProfile.lightmappingMode = lightmappingMode;
                    m_selectedLightingProfile.lightmapResolution = lightmapResolution;
                    m_selectedLightingProfile.lightmapPadding = lightmapPadding;
                    m_selectedLightingProfile.useHighResolutionLightmapSize = useHighResolutionLightmapSize;
                    m_selectedLightingProfile.compressLightmaps = compressLightmaps;
                    m_selectedLightingProfile.ambientOcclusion = ambientOcclusion;
                    m_selectedLightingProfile.maxDistance = maxDistance;
                    m_selectedLightingProfile.indirectContribution = indirectContribution;
                    m_selectedLightingProfile.directContribution = directContribution;
                    m_selectedLightingProfile.lightIndirectIntensity = lightIndirectIntensity;
                    m_selectedLightingProfile.lightBoostIntensity = lightBoostIntensity;
                    m_selectedLightingProfile.finalGather = finalGather;
                    m_selectedLightingProfile.finalGatherRayCount = finalGatherRayCount;
                    m_selectedLightingProfile.finalGatherDenoising = finalGatherDenoising;

                    //Selection changing things - exit immediately to not polute settings
                    if (newLightmappingSettings != m_selectedLightingProfileIndex)
                    {
                        m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                        LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);
                        m_selectedLightingProfileIndex = newLightmappingSettings;
                    }

                    //Udpate lightmap settings                   
                    LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);

                    EditorPrefs.SetString("AmbientSkiesLighting_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_lightingProfiles[newLightmappingSettings].name);
                }
            }

            GUI.enabled = true;
        }

        #endregion

        #region Editor Update

        /// <summary>
        /// Editor Update
        /// </summary>
        private void EditorUpdate()
        {
            //Check starts as true before searching.
            //This will loop the update till checks are all true
            bool checkSuccess = true;

            //Countdown timer
            m_countdown -= Time.deltaTime;
            //If timer still active
            if (m_countdown > 0)
            {
                //Update system still enabled
                checkSuccess = false;
            }
            //If timer inactive
            else if (m_countdown < 0)
            {
                //Disable update system
                checkSuccess = true;
                m_countdown = -5f;
            }

            //Makes sure the global post processing is there
            if (GameObject.Find("Global Post Processing") == null)
            {
                //Update post processing
                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                //Failed on check setting to false
                checkSuccess = false;
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                //Checks to make sure the hd volume is there
                if (GameObject.Find("High Definition Environment Volume") == null)
                {
                    //Update skybox
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                    }
                    //Failed on check setting to false
                    checkSuccess = false;
                }
            }
            else
            {
                //Checks to see if using time of day
                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                {
                    if (RenderSettings.skybox.shader == Shader.Find("Skybox/Cubemap"))
                    {
                        //Update skybox
                        if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                        }
                        //Failed on check setting to false
                        checkSuccess = false;
                    }
                }
            }

            //Checks to see if using time of day
            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
            {
                //Looks for time of day prefab
                if (GameObject.Find("AS Time Of Day") == null)
                {
                    //Update skybox
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                    }
                    //Failed on check setting to false
                    checkSuccess = false;
                }

                if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox.shader == Shader.Find("Skybox/Cubemap"))
                {
                    //Update skybox
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                    }
                    //Failed on check setting to false
                    checkSuccess = false;
                }
            }

            //If all checks are success stop update
            if (checkSuccess)
            {
                //Stops the editor update
                EditorApplication.update -= EditorUpdate;
            }
        }

        #endregion

        #region Global Settings

        /// <summary>
        /// Global settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void GlobalSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            systemtype = (AmbientSkiesConsts.SystemTypes)m_editorUtils.EnumPopup("SystemTypes", systemtype, helpEnabled);
            if (systemtype == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                //EditorGUILayout.Space();
                useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                m_editorUtils.Text("DefaultProceduralText");
            }
            /*
            else if (systemtype == AmbientSkiesConsts.SystemTypes.WorldManagerAPI)
            {
                //EditorGUILayout.Space();
                m_editorUtils.Text("WorldManagerAPIText");
            }
            */
            else if (systemtype == AmbientSkiesConsts.SystemTypes.ThirdParty)
            {
                //EditorGUILayout.Space();
                m_editorUtils.Text("ThirdPartyText");
            }
            else
            {
                //Searching for Enviro
                if (GameObject.Find("Enviro Sky Manager") != null || GameObject.Find("EnviroSky Standard") != null || GameObject.Find("EnviroSky Lite") != null || GameObject.Find("EnviroSky Lite for Mobiles") != null || GameObject.Find("EnviroSky Standard for VR") != null || RenderSettings.skybox.shader == Shader.Find("Enviro/Skybox"))
                {
                    if (EditorUtility.DisplayDialog("Enviro Detected!", "Warning Enviro has been detected in your scene, switching System Type to ambient skies will remove Enviro from your scene. Are you sure you want to proceed?", "Yes", "No"))
                    {
                        systemtype = AmbientSkiesConsts.SystemTypes.AmbientSkies;
                        m_profiles.systemTypes = AmbientSkiesConsts.SystemTypes.AmbientSkies;
                    }
                    else
                    {
                        systemtype = AmbientSkiesConsts.SystemTypes.ThirdParty;
                        m_profiles.systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
                        NewSceneObjectCreation();
                    }
                }

                //EditorGUILayout.Space();
                m_editorUtils.Text("AmbientSkiesText");
            }
            EditorGUILayout.Space();

            targetPlatform = (AmbientSkiesConsts.PlatformTarget)m_editorUtils.EnumPopup("TargetPlatform", targetPlatform, helpEnabled);

            //EditorGUILayout.Space();
            m_editorUtils.Text("TargetPlatformText");
            EditorGUILayout.Space();

            vSyncMode = (AmbientSkiesConsts.VSyncMode)m_editorUtils.EnumPopup("VsyncMode", vSyncMode, helpEnabled);

            //EditorGUILayout.Space();
            m_editorUtils.Text("VsyncText");            

            if (EditorGUI.EndChangeCheck())
            {
                m_profiles.targetPlatform = targetPlatform;
                m_profiles.systemTypes = systemtype;
                m_profiles.useTimeOfDay = useTimeOfDay;
                m_profiles.vSyncMode = vSyncMode;
            }
        }

        #endregion

        #region Time Of Day Settings

        /// <summary>
        /// Time of day settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TimeOfDaySettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            useTimeOfDay = (AmbientSkiesConsts.DisableAndEnable)m_editorUtils.EnumPopup("UseTimeOfDay", useTimeOfDay, helpEnabled);

            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable && m_profiles.useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
            {
                if (skyTypeNonHD != AmbientSkiesConsts.SkyType.ProceduralSky && renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (EditorUtility.DisplayDialog("Enabling Time Of Day!", "You're about to enable time of day this will switch your Sky Mode to procedural. Are you sure you want to proceed?", "Yes", "No"))
                    {
                        useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Enable;
                        skyTypeNonHD = AmbientSkiesConsts.SkyType.ProceduralSky;
                        m_profiles.skyTypeNonHD = AmbientSkiesConsts.SkyType.ProceduralSky;
                    }
                    else
                    {
                        useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                    }
                }
                else if (skyType != AmbientSkiesConsts.VolumeSkyType.ProceduralSky && renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (EditorUtility.DisplayDialog("Enabling Time Of Day!", "You're about to enable time of day this will switch your Sky Mode to procedural. Are you sure you want to proceed?", "Yes", "No"))
                    {
                        useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Enable;
                        skyType = AmbientSkiesConsts.VolumeSkyType.ProceduralSky;
                        m_profiles.skyType = AmbientSkiesConsts.VolumeSkyType.ProceduralSky;
                    }
                    else
                    {
                        useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                    }
                }
            }

            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
            {
                EditorGUILayout.Space();
                m_editorUtils.Heading("Profile Settings");

                EditorGUILayout.BeginHorizontal();
                timeOfDayProfile = (AmbientSkiesTimeOfDayProfile)m_editorUtils.ObjectField("TimeOfDayProfile", timeOfDayProfile, typeof(AmbientSkiesTimeOfDayProfile), false, GUILayout.Width(450f));
                if (m_editorUtils.ButtonRight("CreateTimeOfDayProfile", GUILayout.Width(45f), GUILayout.Height(16f)))
                {
                    CreateNewTimeOfDayProfile(timeOfDayProfile);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                m_editorUtils.InlineHelp("TimeOfDayProfile", helpEnabled);

#if UNITY_2018_3_OR_NEWER
                if (fogType != AmbientSkiesConsts.VolumeFogType.None)
                {
                    m_foldoutTODFogSettings = m_editorUtils.Panel("FogSettings", TODFogSettings, m_foldoutTODFogSettings);
                }          
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    m_foldoutTODHDRPSettings = m_editorUtils.Panel("HDRP Settings", TODHDRPSettings, m_foldoutTODHDRPSettings);
                }
                m_foldoutTODKeyBindings = m_editorUtils.Panel("Key Binding Settings", TODKeyBinding, m_foldoutTODKeyBindings);
                m_foldoutTODLightSettings = m_editorUtils.Panel("Light Settings", TODLightSettings, m_foldoutTODLightSettings);
                if (usePostProcess)
                {
                    m_foldoutTODPostProcessing = m_editorUtils.Panel("Post Processing Settings", TODPostProcessing, m_foldoutTODPostProcessing);
                }
                m_foldoutTODRealtimeGI = m_editorUtils.Panel("Global GI Settings", TODRealtimeGI, m_foldoutTODRealtimeGI);
                m_foldoutTODSkybox = m_editorUtils.Panel("Sky Settings", TODSkybox, m_foldoutTODSkybox);
                m_foldoutTODSeasons = m_editorUtils.Panel("Season Settings", TODSeasons, m_foldoutTODSeasons);
                m_foldoutTODTime = m_editorUtils.Panel("Time Of Day Settings", TODTime, m_foldoutTODTime);
#else
                m_editorUtils.Text("TODProfileEditing");
#endif
            }

            if (EditorGUI.EndChangeCheck())
            {              
                EditorUtility.SetDirty(m_profiles);
                EditorUtility.SetDirty(timeOfDayProfile);

                m_profiles.useTimeOfDay = useTimeOfDay;
                m_profiles.timeOfDayProfile = timeOfDayProfile;

                m_profiles.skyType = skyType;
                m_profiles.skyTypeNonHD = skyTypeNonHD;
            }
        }

        /// <summary>
        /// Time of day key bindings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODKeyBinding(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            pauseTimeKey = (KeyCode)m_editorUtils.EnumPopup("PauseTimeKey", pauseTimeKey, helpEnabled);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            incrementUpKey = (KeyCode)m_editorUtils.EnumPopup("IncrementUpKey", incrementUpKey, helpEnabled);
            incrementDownKey = (KeyCode)m_editorUtils.EnumPopup("IncrementDownKey", incrementDownKey, helpEnabled);
            timeToAddOrRemove = m_editorUtils.Slider("TimeToAddOrRemove", timeToAddOrRemove, 0.001f, 0.99f, helpEnabled);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            rotateSunLeftKey = (KeyCode)m_editorUtils.EnumPopup("RotateSunLeftKey", rotateSunLeftKey, helpEnabled);
            rotateSunRightKey = (KeyCode)m_editorUtils.EnumPopup("RotateSunRightKey", rotateSunRightKey, helpEnabled);
            sunRotationAmount = m_editorUtils.Slider("SunRotationAmount", sunRotationAmount, 0f, 359f, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.pauseTimeKey = pauseTimeKey;
                m_profiles.incrementUpKey = incrementUpKey;
                m_profiles.incrementDownKey = incrementDownKey;
                m_profiles.timeToAddOrRemove = timeToAddOrRemove;
                m_profiles.rotateSunLeftKey = rotateSunLeftKey;
                m_profiles.rotateSunRightKey = rotateSunRightKey;
                m_profiles.sunRotationAmount = sunRotationAmount;
            }
        }

        /// <summary>
        /// Time of day light settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODLightSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            daySunIntensity = m_editorUtils.CurveField("DaySunIntensity", daySunIntensity, helpEnabled, GUILayout.Height(16f));
            daySunGradientColor = GradientField("DaySunGradientColor", daySunGradientColor, helpEnabled);
            nightSunIntensity = m_editorUtils.CurveField("NightSunIntensity", nightSunIntensity, helpEnabled, GUILayout.Height(16f));
            nightSunGradientColor = GradientField("NightSunGradientColor", nightSunGradientColor, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.daySunIntensity = daySunIntensity;
                m_profiles.daySunGradientColor = daySunGradientColor;
                m_profiles.nightSunIntensity = nightSunIntensity;
                m_profiles.nightSunGradientColor = nightSunGradientColor;
            }
        }

        /// <summary>
        /// Time of day HDRP settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODHDRPSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            lightAnisotropy = m_editorUtils.CurveField("LightAnisotropy", lightAnisotropy, helpEnabled, GUILayout.Height(16f));
            lightProbeDimmer = m_editorUtils.CurveField("LightProbeDimmer", lightProbeDimmer, helpEnabled, GUILayout.Height(16f));
            lightDepthExtent = m_editorUtils.CurveField("LightDepthExtent", lightDepthExtent, helpEnabled, GUILayout.Height(16f));
            sunSizeAmount = m_editorUtils.CurveField("SunSize", sunSizeAmount, helpEnabled, GUILayout.Height(16f));
            skyExposureAmount = m_editorUtils.CurveField("SkyExposureAmount", skyExposureAmount, helpEnabled, GUILayout.Height(16f));

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.lightAnisotropy = lightAnisotropy;
                m_profiles.lightProbeDimmer = lightProbeDimmer;
                m_profiles.lightDepthExtent = lightDepthExtent;
                m_profiles.sunSize = sunSizeAmount;
                m_profiles.skyExposure = skyExposureAmount;
            }
        }

        /// <summary>
        /// Time of day fog settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODFogSettings(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();


            if (fogType == AmbientSkiesConsts.VolumeFogType.Linear || fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
            {
                startFogDistance = m_editorUtils.Slider("StartFogDistance", startFogDistance, -1000f, 1000f, helpEnabled);
            }
            else if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential || fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
            {
                dayFogDensity = m_editorUtils.CurveField("DayFogDensity", dayFogDensity, helpEnabled, GUILayout.Height(16f));
                nightFogDensity = m_editorUtils.CurveField("NightFogDensity", nightFogDensity, helpEnabled, GUILayout.Height(16f));
            }
            
            dayFogDistance = m_editorUtils.CurveField("DayFogDistance", dayFogDistance, helpEnabled, GUILayout.Height(16f));
            dayFogColor = GradientField("DayFogColor", dayFogColor, helpEnabled);
            nightFogDistance = m_editorUtils.CurveField("NightFogDistance", nightFogDistance, helpEnabled, GUILayout.Height(16f));
            nightFogColor = GradientField("NightFogColor", nightFogColor, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.startFogDistance = startFogDistance;
                m_profiles.dayFogDensity = dayFogDensity;
                m_profiles.nightFogDensity = nightFogDensity;
                m_profiles.dayFogDistance = dayFogDistance;
                m_profiles.dayFogColor = dayFogColor;
                m_profiles.nightFogDistance = nightFogDistance;
                m_profiles.nightFogColor = nightFogColor;
            }
        }

        /// <summary>
        /// Time of day post processing foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODPostProcessing(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            syncPostProcessing = m_editorUtils.Toggle("SyncPostProcessing", syncPostProcessing, helpEnabled);
            dayTempature = m_editorUtils.CurveField("DayTempature", dayTempature, helpEnabled, GUILayout.Height(16f));
            dayPostFXColor = GradientField("DayPostFXColor", dayPostFXColor, helpEnabled);
            nightTempature = m_editorUtils.CurveField("NightTempature", nightTempature, helpEnabled, GUILayout.Height(16f));
            nightPostFXColor = GradientField("NightPostFXColor", nightPostFXColor, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.syncPostProcessing = syncPostProcessing;
                m_profiles.dayTempature = dayTempature;
                m_profiles.dayPostFXColor = dayPostFXColor;
                m_profiles.nightTempature = nightTempature;
                m_profiles.nightPostFXColor = nightPostFXColor;
            }
        }

        /// <summary>
        /// Time of day realtime gi foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODRealtimeGI(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            realtimeGIUpdate = m_editorUtils.Toggle("RealtimeGIUpdate", realtimeGIUpdate, helpEnabled);
            if (realtimeGIUpdate)
            {
                gIUpdateInterval = m_editorUtils.IntField("GIUpdateInterval", gIUpdateInterval, helpEnabled);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.realtimeGIUpdate = realtimeGIUpdate;
                m_profiles.gIUpdateInterval = gIUpdateInterval;
            }
        }

        /// <summary>
        /// Time of day seasons foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODSeasons(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            hemisphereOrigin = (AmbientSkiesConsts.HemisphereOrigin)m_editorUtils.EnumPopup("HemisphereOrigin", hemisphereOrigin, helpEnabled);
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("CurrentSeason", GUILayout.Width(146f));
            m_editorUtils.TextNonLocalized(seasonString, GUILayout.Width(80f), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.currentSeason = currentSeason;
                m_profiles.hemisphereOrigin = hemisphereOrigin;
            }
        }

        /// <summary>
        /// Time of day skybox foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODSkybox(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //m_editorUtils.Heading("Lighting Settings");
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("Fog Settings");
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("Sky Settings");
            enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
            //proceduralSunSize = m_editorUtils.Slider("SunSize", proceduralSunSize, 0f, 1f, helpEnabled);
            //skyTint = m_editorUtils.ColorField("SkyTint", skyTint, helpEnabled);
            //skyExposure = m_editorUtils.Slider("SkyExposure", skyExposure, 0f, 5f, helpEnabled);
            //skyMultiplier = m_editorUtils.Slider("SkyMultiplier", skyMultiplier, 0f, 5f, helpEnabled);
            timeOfDaySkyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", timeOfDaySkyboxRotation, -0.1f, 360.1f, helpEnabled);
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("[Built-In/Lightweight Only] Settings");
            //EditorGUILayout.Space();
            //m_editorUtils.Heading("[High Definition Only] Settings");

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.skyboxRotation = timeOfDaySkyboxRotation;
                m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
            }
        }

        /// <summary>
        /// Time of day time foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void TODTime(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            pauseTime = m_editorUtils.Toggle("PauseTime", pauseTime, helpEnabled);
            currentTimeOfDay = m_editorUtils.Slider("CurrentTimeOfDay", currentTimeOfDay, 0f, 1f, helpEnabled);
            dayLengthInSeconds = m_editorUtils.FloatField("DayLength", dayLengthInSeconds, helpEnabled);
            nightLengthInSeconds = m_editorUtils.FloatField("NightLength", nightLengthInSeconds, helpEnabled);
            dayDate = m_editorUtils.IntSlider("DateDay", dayDate, 1, 31, helpEnabled);
            monthDate = m_editorUtils.IntSlider("DateMonth", monthDate, 1, 12, helpEnabled);
            yearDate = m_editorUtils.IntField("DateYear", yearDate, helpEnabled);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.pauseTime = pauseTime;
                m_profiles.currentTimeOfDay = currentTimeOfDay;
                m_profiles.nightLengthInSeconds = nightLengthInSeconds;
                m_profiles.dayLengthInSeconds = dayLengthInSeconds;
                m_profiles.dayDate = dayDate;
                m_profiles.monthDate = monthDate;
                m_profiles.yearDate = yearDate;
            }
        }

        #endregion

        #region Sky Tab Functions

        /// <summary>
        /// Main settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MainSkySettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                skyType = (AmbientSkiesConsts.VolumeSkyType)m_editorUtils.EnumPopup("SkyType", skyType, helpEnabled);
                fogType = (AmbientSkiesConsts.VolumeFogType)m_editorUtils.EnumPopup("FogType", fogType, helpEnabled);              

                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                {
                    if (skyType != AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                    {
                        if (EditorUtility.DisplayDialog("Warning! Time Of Day Enabled", "You are currently using Time Of Day. Time Of Day is currently only supported in Procedural Sky mode. Would you like to disable Time Of Day and switch Sky Mode?", "Yes", "No"))
                        {
                            useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                            m_profiles.useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                        }
                        else
                        {
                            skyType = AmbientSkiesConsts.VolumeSkyType.ProceduralSky;
                        }
                    }
                }
#endif
            }
            else
            {
                skyTypeNonHD = (AmbientSkiesConsts.SkyType)m_editorUtils.EnumPopup("SkyType", skyTypeNonHD, helpEnabled);
                fogType = (AmbientSkiesConsts.VolumeFogType)m_editorUtils.EnumPopup("FogType", fogType, helpEnabled);
                if (fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                {
                    m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Linear;
                    fogType = AmbientSkiesConsts.VolumeFogType.Linear;

                    Debug.LogWarning("Volumetric Fog only works in High Definition Render Pipeline switching back to Linear Fog Mode");
                }

                ambientMode = (AmbientSkiesConsts.AmbientMode)m_editorUtils.EnumPopup("AmbientMode", ambientMode, helpEnabled);               

                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                {
                    if (skyTypeNonHD != AmbientSkiesConsts.SkyType.ProceduralSky)
                    {
                        if (EditorUtility.DisplayDialog("Warning! Time Of Day Enabled", "You are currently using Time Of Day. Time Of Day is currently only supported in Procedural Sky mode. Would you like to disable Time Of Day and switch Sky Mode?", "Yes", "No"))
                        {
                            useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                            m_profiles.useTimeOfDay = AmbientSkiesConsts.DisableAndEnable.Disable;
                        }
                        else
                        {
                            skyTypeNonHD = AmbientSkiesConsts.SkyType.ProceduralSky;
                            m_profiles.skyTypeNonHD = AmbientSkiesConsts.SkyType.ProceduralSky;
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);
#if HDPipeline
                m_profiles.skyType = skyType; 
#endif
                m_selectedSkyboxProfile.fogType = fogType;
                m_selectedSkyboxProfile.ambientMode = ambientMode;
                m_profiles.skyTypeNonHD = skyTypeNonHD;
            }
        }

        /// <summary>
        /// Skybox settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void SkyboxSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                    if (skyType == AmbientSkiesConsts.VolumeSkyType.Gradient)
                    {
                        topSkyColor = m_editorUtils.ColorField("TopSkyColor", topSkyColor, helpEnabled);
                        middleSkyColor = m_editorUtils.ColorField("MiddleSkyColor", middleSkyColor, helpEnabled);
                        bottomSkyColor = m_editorUtils.ColorField("BottomSkyColor", bottomSkyColor, helpEnabled);
                        gradientDiffusion = m_editorUtils.Slider("GradientDiffusion", gradientDiffusion, 0f, 5f, helpEnabled);
                        skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, -0.1f, 360.1f, helpEnabled);
                        skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, -0.1f, 360.1f, helpEnabled);
                    }
                    else if (skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                    {                        
                        atmosphereThickness = m_editorUtils.Slider("AtmosphereThickness", atmosphereThickness, 0f, 5f, helpEnabled);
                        skyTint = m_editorUtils.ColorField("SkyTint", skyTint, helpEnabled);
                        groundColor = m_editorUtils.ColorField("GroundColor", groundColor, helpEnabled);
                        skyExposure = m_editorUtils.Slider("SkyExposure", skyExposure, 0f, 5f, helpEnabled);
                        skyMultiplier = m_editorUtils.Slider("SkyMultiplier", skyMultiplier, 0f, 5f, helpEnabled);
                        includeSunInBaking = m_editorUtils.Toggle("IncludeSunInBaking", includeSunInBaking, helpEnabled);
                        proceduralSkyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", proceduralSkyboxRotation, -0.1f, 360.1f, helpEnabled);
                        proceduralSkyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", proceduralSkyboxPitch, -0.1f, 360.1f, helpEnabled);
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                        newSkyboxSelection = EditorGUILayout.Popup(m_selectedSkyboxProfileIndex, skyboxChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
                        EditorGUILayout.EndHorizontal();
                        m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                        if (!m_selectedSkyboxProfile.isPWProfile)
                        {
                            customSkybox = (Cubemap)m_editorUtils.ObjectField("CustomSkybox", customSkybox, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                        }
                        skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 10f, helpEnabled);
                        skyMultiplier = m_editorUtils.Slider("SkyMultiplier", skyMultiplier, 0f, 5f, helpEnabled);
                        skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, -0.1f, 360.1f, helpEnabled);
                        skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, -0.1f, 360.1f, helpEnabled);
                    }
#endif
            }
            else
            {
                if (skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                {
                    EditorGUILayout.BeginHorizontal();
                    m_editorUtils.Text("SelectSkyboxDropdown", GUILayout.Width(146f));
                    newSkyboxSelection = EditorGUILayout.Popup(m_selectedSkyboxProfileIndex, skyboxChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
                    EditorGUILayout.EndHorizontal();
                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];

                    if (!m_selectedSkyboxProfile.isPWProfile)
                    {
                        profileName = m_editorUtils.TextField("ProfileNaming", m_selectedSkyboxProfile.name, helpEnabled);
                        isProceduralSkybox = m_editorUtils.Toggle("IsProceduralSkybox", isProceduralSkybox, helpEnabled);
                        if (!isProceduralSkybox)
                        {
                            customSkybox = (Cubemap)m_editorUtils.ObjectField("CustomSkybox", customSkybox, typeof(Cubemap), false, helpEnabled, GUILayout.Height(16f));
                            skyboxTint = m_editorUtils.ColorField("SkyboxTintColorSelector", skyboxTint, helpEnabled);
                            skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 5f, helpEnabled);
                            skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, -0.1f, 360.1f, helpEnabled);
                            skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, -0.1f, 360.1f, helpEnabled);
                        }
                        else
                        {
                            enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
                            proceduralAtmosphereThickness = m_editorUtils.Slider("AtmosphereThickness", proceduralAtmosphereThickness, 0f, 5f, helpEnabled);
                            proceduralGroundColor = m_editorUtils.ColorField("GroundColor", proceduralGroundColor, helpEnabled);
                            proceduralSkyTint = m_editorUtils.ColorField("SkyboxTintColorSelector", proceduralSkyTint, helpEnabled);
                            proceduralSkyExposure = m_editorUtils.Slider("SkyboxExposureSlider", proceduralSkyExposure, 0f, 5f, helpEnabled);
                            proceduralSkyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", proceduralSkyboxRotation, -0.1f, 360.1f, helpEnabled);
                            proceduralSkyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", proceduralSkyboxPitch, -0.1f, 360.1f, helpEnabled);
                        }
                    }
                    else
                    {
                        skyboxTint = m_editorUtils.ColorField("SkyboxTintColorSelector", skyboxTint, helpEnabled);
                        skyboxExposure = m_editorUtils.Slider("SkyboxExposureSlider", skyboxExposure, 0f, 5f, helpEnabled);
                        skyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", skyboxRotation, -0.1f, 360.1f, helpEnabled);
                        skyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", skyboxPitch, -0.1f, 360.1f, helpEnabled);
                    }
                }
                else
                {
                    proceduralAtmosphereThickness = m_editorUtils.Slider("AtmosphereThickness", proceduralAtmosphereThickness, 0f, 5f, helpEnabled);
                    proceduralGroundColor = m_editorUtils.ColorField("GroundColor", proceduralGroundColor, helpEnabled);
                    proceduralSkyTint = m_editorUtils.ColorField("SkyboxTintColorSelector", proceduralSkyTint, helpEnabled);
                    proceduralSkyExposure = m_editorUtils.Slider("SkyboxExposureSlider", proceduralSkyExposure, 0f, 5f, helpEnabled);
                    proceduralSkyboxRotation = m_editorUtils.Slider("SkyboxRotationSlider", proceduralSkyboxRotation, -0.1f, 360.1f, helpEnabled);
                    proceduralSkyboxPitch = m_editorUtils.Slider("SkyboxPitchSlider", proceduralSkyboxPitch, -0.1f, 360.1f, helpEnabled);
                }
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && skyType == AmbientSkiesConsts.VolumeSkyType.HDRISky || renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
            {
                //Prev / Next
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (newSkyboxSelection == 0)
                {
                    GUI.enabled = false;
                }
                if (m_editorUtils.Button("PrevSkyboxButton"))
                {
                    newSkyboxSelection--;
                }
                GUI.enabled = true;
                if (newSkyboxSelection == skyboxChoices.Count - 1)
                {
                    GUI.enabled = false;
                }
                if (m_editorUtils.Button("NextSkyboxButton"))
                {
                    newSkyboxSelection++;
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_profiles.systemTypes = systemtype;
                m_profiles.targetPlatform = targetPlatform;
                m_profiles.skyType = skyType;
                m_profiles.skyTypeNonHD = skyTypeNonHD;

                m_selectedSkyboxProfileIndex = newSkyboxSelection;
                m_selectedSkyboxProfile.skyboxTint = skyboxTint;
                m_selectedSkyboxProfile.skyboxExposure = skyboxExposure;
                m_selectedSkyboxProfile.skyboxRotation = skyboxRotation;
                m_selectedSkyboxProfile.skyboxPitch = skyboxPitch;
                m_selectedSkyboxProfile.customSkybox = customSkybox;
                m_selectedSkyboxProfile.proceduralAtmosphereThickness = proceduralAtmosphereThickness;
                m_selectedSkyboxProfile.proceduralGroundColor = proceduralGroundColor;
                m_selectedSkyboxProfile.proceduralSkyTint = proceduralSkyTint;
                m_selectedSkyboxProfile.proceduralSkyExposure = proceduralSkyExposure;
                m_selectedSkyboxProfile.proceduralSkyboxPitch = proceduralSkyboxPitch;
                m_selectedSkyboxProfile.proceduralSkyboxRotation = proceduralSkyboxRotation;
                m_selectedSkyboxProfile.includeSunInBaking = includeSunInBaking;
                m_selectedSkyboxProfile.name = profileName;

                m_selectedSkyboxProfile.isProceduralSkybox = isProceduralSkybox;
                m_selectedSkyboxProfile.proceduralSunColor = proceduralSunColor;
                m_selectedSkyboxProfile.proceduralSunIntensity = proceduralSunIntensity;
                m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
#if HDPipeline
                m_selectedSkyboxProfile.sunSize = sunSize;
                m_selectedSkyboxProfile.sunConvergence = sunConvergence;
                m_selectedSkyboxProfile.atmosphereThickness = atmosphereThickness;
                m_selectedSkyboxProfile.skyTint = skyTint;
                m_selectedSkyboxProfile.groundColor = groundColor;
                m_selectedSkyboxProfile.skyExposure = skyExposure;
                m_selectedSkyboxProfile.skyMultiplier = skyMultiplier;
                m_selectedSkyboxProfile.topColor = topSkyColor;
                m_selectedSkyboxProfile.middleColor = middleSkyColor;
                m_selectedSkyboxProfile.bottomColor = bottomSkyColor;
                m_selectedSkyboxProfile.gradientDiffusion = gradientDiffusion;
#endif
            }
        }

        /// <summary>
        /// Fog settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void FogSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
#if HDPipeline
                if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                {
                    fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                    exponentialFogDensity = m_editorUtils.Slider("FogDensitySlider", exponentialFogDensity, 0f, 1f, helpEnabled);
                    fogDistance = m_editorUtils.Slider("FogDistanceSlider", fogDistance, 100f, 10000f, helpEnabled);
                    exponentialBaseFogHeight = m_editorUtils.Slider("ExponentialBaseFogHeight", exponentialBaseFogHeight, 0f, 10000f, helpEnabled);
                    exponentialHeightAttenuation = m_editorUtils.Slider("ExponentialFogHeightAttenuation", exponentialHeightAttenuation, 0f, 1f, helpEnabled);
                    exponentialMaxFogDistance = m_editorUtils.Slider("ExponentialMaxFogDistance", exponentialMaxFogDistance, 0f, 20000f, helpEnabled);
                    exponentialMipFogNear = m_editorUtils.Slider("ExponentialMipFogNear", exponentialMipFogNear, 0f, 1000f, helpEnabled);
                    exponentialMipFogFar = m_editorUtils.Slider("ExponentialMipFogFar", exponentialMipFogFar, 0f, 2500f, helpEnabled);
                    exponentialMipFogMax = m_editorUtils.Slider("ExponentialMipFogMax", exponentialMipFogMax, 0f, 1f, helpEnabled);
                }
                else if (fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                {
                    m_editorUtils.Text("NotSupportedInHDRP");
                }
                else if (fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                {
                    fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                    linearFogDensity = m_editorUtils.Slider("FogDensitySlider", linearFogDensity, 0f, 1f, helpEnabled);
                    nearFogDistance = m_editorUtils.Slider("NearFogDistanceSlider", nearFogDistance, 0f, 1000f, helpEnabled);
                    fogDistance = m_editorUtils.Slider("FogDistanceSlider", fogDistance, 100f, 10000f, helpEnabled);
                    linearFogHeightStart = m_editorUtils.Slider("LinearFogHeightStart", linearFogHeightStart, 0f, 250f, helpEnabled);
                    linearFogHeightEnd = m_editorUtils.Slider("LinearFogHeightEnd", linearFogHeightEnd, 0f, 500f, helpEnabled);
                    linearFogMaxDistance = m_editorUtils.Slider("LinearFogMaxDistance", linearFogMaxDistance, 0f, 20000f, helpEnabled);
                    linearMipFogNear = m_editorUtils.Slider("LinearMipFogNear", linearMipFogNear, 0f, 1000f, helpEnabled);
                    linearMipFogFar = m_editorUtils.Slider("LinearMipFogFar", linearMipFogFar, 0f, 2000f, helpEnabled);
                    linearMipFogMax = m_editorUtils.Slider("LinearMipFogMax", linearMipFogMax, 0f, 1f, helpEnabled);
                }
                else if (fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                {
                    if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                    {                     
                        sliceDistribution = m_editorUtils.Slider("VolumetricSliceDistribution", sliceDistribution, 0f, 1f, helpEnabled);
                    }
                    else
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        fogDistance = m_editorUtils.Slider("VolumetricBaseFogDistance", fogDistance, 0f, 10000f, helpEnabled);
                        baseFogDistance = m_editorUtils.Slider("FogDistanceSlider", baseFogDistance, 1f, 10000f, helpEnabled);
                        baseFogHeight = m_editorUtils.Slider("VolumetricBaseFogHeight", baseFogHeight, -250f, 250f);
                        meanFogHeight = m_editorUtils.Slider("VolumetricMeanHeight", meanFogHeight, 1f, 6000f, helpEnabled);
                        globalAnisotropy = m_editorUtils.Slider("VolumetricGlobalAnisotropy", globalAnisotropy, -1f, 1f, helpEnabled);
                        globalLightProbeDimmer = m_editorUtils.Slider("VolumetricGlobalLightProbeDimmer", globalLightProbeDimmer, 0f, 1f, helpEnabled);
                        depthExtent = m_editorUtils.Slider("VolumetricLightDepthDistance", depthExtent, 0.1f, 5000f, helpEnabled);
                        sliceDistribution = m_editorUtils.Slider("VolumetricSliceDistribution", sliceDistribution, 0f, 1f, helpEnabled);
                    }                   

                    EditorGUILayout.Space();
                    useDensityFogVolume = m_editorUtils.ToggleLeft("UseDensityFogVolume", useDensityFogVolume, helpEnabled);
                    if (useDensityFogVolume)
                    {
                        singleScatteringAlbedo = m_editorUtils.ColorField("DensityFogScatteringAlbedo", singleScatteringAlbedo, helpEnabled);
                        densityVolumeFogDistance = m_editorUtils.Slider("DensityVolumeFogDistance", densityVolumeFogDistance, 0f, 1000f, helpEnabled);
                        fogDensityMaskTexture = (Texture3D)m_editorUtils.ObjectField("FogDensityMaskTexture", fogDensityMaskTexture, typeof(Texture3D), false, helpEnabled, GUILayout.Height(16f));
                        densityMaskTiling = m_editorUtils.Vector3Field("DensityFogMaskTiling", densityMaskTiling, helpEnabled);
                    }
                }
#endif
                if (fogType == AmbientSkiesConsts.VolumeFogType.None)
                {
                    m_editorUtils.Text("No fog mode selected. To enable fog select a fog mode in the Main Settings.");
                }
            }
            else
            {
                if (skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                {
                    if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential || fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                        fogDensity = m_editorUtils.Slider("FogDensitySlider", fogDensity, 0f, 0.1f, helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        if (configurationType == AmbientSkiesConsts.AutoConfigureType.Manual)
                        {
                            fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                            configurationType = (AmbientSkiesConsts.AutoConfigureType)m_editorUtils.EnumPopup("ConfigurationType", configurationType, helpEnabled);
                            nearFogDistance = m_editorUtils.Slider("NearFogDistanceSlider", nearFogDistance, -500f, 1000f, helpEnabled);
                            fogDistance = m_editorUtils.Slider("FogDistanceSlider", fogDistance, 100f, 10000f, helpEnabled);
                        }
                        else
                        {
                            fogColor = m_editorUtils.ColorField("FogTintColorSelector", fogColor, helpEnabled);
                            configurationType = (AmbientSkiesConsts.AutoConfigureType)m_editorUtils.EnumPopup("ConfigurationType", configurationType, helpEnabled);
                            m_editorUtils.TextNonLocalized("Your near fog distance is: " + nearFogDistance.ToString());
                            m_editorUtils.TextNonLocalized("Your far fog distance is: " + fogDistance.ToString());

                            //Apply Settings
                            ApplyAutoFogSettings(m_profiles, m_selectedSkyboxProfile);
                        }
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.None)
                    {
                        m_editorUtils.Text("No fog mode selected. To enable fog select a fog mode in the Main Settings.");
                    }
                }
                else
                {                   
                    if (fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                    {
                        proceduralFogColor = m_editorUtils.ColorField("FogTintColorSelector", proceduralFogColor, helpEnabled);
                        proceduralFogDensity = m_editorUtils.Slider("FogDensitySlider", proceduralFogDensity, 0f, 0.1f, helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                    {
                        proceduralFogColor = m_editorUtils.ColorField("FogTintColorSelector", proceduralFogColor, helpEnabled);
                        proceduralFogDensity = m_editorUtils.Slider("FogDensitySlider", proceduralFogDensity, 0f, 0.1f, helpEnabled);
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                    {
                        if (configurationType == AmbientSkiesConsts.AutoConfigureType.Manual)
                        {
                            proceduralFogColor = m_editorUtils.ColorField("FogTintColorSelector", proceduralFogColor, helpEnabled);
                            configurationType = (AmbientSkiesConsts.AutoConfigureType)m_editorUtils.EnumPopup("ConfigurationType", configurationType, helpEnabled);
                            proceduralNearFogDistance = m_editorUtils.Slider("NearFogDistanceSlider", proceduralNearFogDistance, -500f, 1000f, helpEnabled);
                            proceduralFogDistance = m_editorUtils.Slider("FogDistanceSlider", proceduralFogDistance, 100f, 10000f, helpEnabled);
                        }
                        else
                        {
                            proceduralFogColor = m_editorUtils.ColorField("FogTintColorSelector", proceduralFogColor, helpEnabled);
                            configurationType = (AmbientSkiesConsts.AutoConfigureType)m_editorUtils.EnumPopup("ConfigurationType", configurationType, helpEnabled);
                            m_editorUtils.TextNonLocalized("Your near fog distance is: " + proceduralNearFogDistance.ToString());
                            m_editorUtils.TextNonLocalized("Your far fog distance is: " + proceduralFogDistance.ToString());

                            //Apply Settings
                            ApplyAutoFogSettings(m_profiles, m_selectedSkyboxProfile);
                        }
                    }
                    else if (fogType == AmbientSkiesConsts.VolumeFogType.None)
                    {
                        m_editorUtils.Text("No fog mode selected. To enable fog select a fog mode in the Main Settings.");
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.fogColor = fogColor;
                m_selectedSkyboxProfile.fogDistance = fogDistance;
                m_selectedSkyboxProfile.fogDensity = fogDensity;
                m_selectedSkyboxProfile.nearFogDistance = nearFogDistance;
                m_selectedSkyboxProfile.proceduralFogColor = proceduralFogColor;
                m_selectedSkyboxProfile.proceduralFogDensity = proceduralFogDensity;
                m_selectedSkyboxProfile.proceduralFogDistance = proceduralFogDistance;
                m_selectedSkyboxProfile.proceduralNearFogDistance = proceduralNearFogDistance;
                m_profiles.configurationType = configurationType;
#if HDPipeline
                m_selectedSkyboxProfile.volumetricBaseFogDistance = baseFogDistance;
                m_selectedSkyboxProfile.volumetricBaseFogHeight = baseFogHeight;
                m_selectedSkyboxProfile.volumetricMeanHeight = meanFogHeight;
                m_selectedSkyboxProfile.volumetricGlobalAnisotropy = globalAnisotropy;
                m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer = globalLightProbeDimmer;
                m_selectedSkyboxProfile.volumetricDistanceRange = depthExtent;
                m_selectedSkyboxProfile.volumetricSliceDistributionUniformity = sliceDistribution;

                m_selectedSkyboxProfile.useFogDensityVolume = useDensityFogVolume;
                m_selectedSkyboxProfile.singleScatteringAlbedo = singleScatteringAlbedo;
                m_selectedSkyboxProfile.densityVolumeFogDistance = densityVolumeFogDistance;
                m_selectedSkyboxProfile.fogDensityMaskTexture = fogDensityMaskTexture;
                m_selectedSkyboxProfile.densityMaskTiling = densityMaskTiling;

                m_selectedSkyboxProfile.exponentialFogDensity = exponentialFogDensity;
                m_selectedSkyboxProfile.exponentialBaseHeight = exponentialBaseFogHeight;
                m_selectedSkyboxProfile.exponentialHeightAttenuation = exponentialHeightAttenuation;
                m_selectedSkyboxProfile.exponentialMaxFogDistance = exponentialMaxFogDistance;
                m_selectedSkyboxProfile.exponentialMipFogNear = exponentialMipFogNear;
                m_selectedSkyboxProfile.exponentialMipFogFar = exponentialMipFogFar;
                m_selectedSkyboxProfile.exponentialMipFogMaxMip = exponentialMipFogMax;

                m_selectedSkyboxProfile.linearFogDensity = linearFogDensity;
                m_selectedSkyboxProfile.linearHeightStart = linearFogHeightStart;
                m_selectedSkyboxProfile.linearHeightEnd = linearFogHeightEnd;
                m_selectedSkyboxProfile.linearMaxFogDistance = linearFogMaxDistance;
                m_selectedSkyboxProfile.linearMipFogNear = linearMipFogNear;
                m_selectedSkyboxProfile.linearMipFogFar = linearMipFogFar;
                m_selectedSkyboxProfile.linearMipFogMaxMip = linearMipFogMax;

                m_selectedSkyboxProfile.topColor = topSkyColor;
                m_selectedSkyboxProfile.middleColor = middleSkyColor;
                m_selectedSkyboxProfile.bottomColor = bottomSkyColor;
                m_selectedSkyboxProfile.gradientDiffusion = gradientDiffusion;

                m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                m_selectedSkyboxProfile.sunSize = sunSize;
                m_selectedSkyboxProfile.sunConvergence = sunConvergence;
                m_selectedSkyboxProfile.atmosphereThickness = atmosphereThickness;
                m_selectedSkyboxProfile.skyTint = skyTint;
                m_selectedSkyboxProfile.groundColor = groundColor;
                m_selectedSkyboxProfile.skyExposure = skyExposure;
                m_selectedSkyboxProfile.skyMultiplier = skyMultiplier;
#endif
            }
        }

        /// <summary>
        /// Ambient settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void AmbientSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
                   
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                diffuseAmbientIntensity = m_editorUtils.Slider("DiffuseSkyboxGroundIntensitySlider", diffuseAmbientIntensity, 0f,10f, helpEnabled);
                specularAmbientIntensity = m_editorUtils.Slider("SpecularSkyboxGroundIntensitySlider", specularAmbientIntensity, 0f, 10f, helpEnabled);
            }
            else
            {
                if (ambientMode == AmbientSkiesConsts.AmbientMode.Color)
                {
                    skyColor = m_editorUtils.ColorField("SkyColor", skyColor, helpEnabled);
                }
                else if (ambientMode == AmbientSkiesConsts.AmbientMode.Gradient)
                {
                    skyColor = m_editorUtils.ColorField("SkyColor", skyColor, helpEnabled);
                    equatorColor = m_editorUtils.ColorField("EquatorColor", equatorColor, helpEnabled);
                    groundColor = m_editorUtils.ColorField("GroundColor", groundColor, helpEnabled);
                }
                else
                {
                    skyboxGroundIntensity = m_editorUtils.Slider("SkyboxGroundIntensitySlider", skyboxGroundIntensity, 0.01f, 8f, helpEnabled);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.skyColor = skyColor;
                m_selectedSkyboxProfile.equatorColor = equatorColor;
                m_selectedSkyboxProfile.groundColor = groundColor;
                m_selectedSkyboxProfile.skyboxGroundIntensity = skyboxGroundIntensity;
#if HDPipeline
                m_selectedSkyboxProfile.indirectDiffuseIntensity = diffuseAmbientIntensity;
                m_selectedSkyboxProfile.indirectSpecularIntensity = specularAmbientIntensity;
#endif
            }
        }

        /// <summary>
        /// Sun settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void SunSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
            {
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    if (skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                    {
                        enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
                        proceduralSunSize = m_editorUtils.Slider("SunSize", proceduralSunSize, 0f, 1f, helpEnabled);
                        proceduralSunSizeConvergence = m_editorUtils.Slider("SunConvergence", proceduralSunSizeConvergence, 0f, 10f, helpEnabled);
                        proceduralSunColor = m_editorUtils.ColorField("SunTintColorSelector", proceduralSunColor, helpEnabled);
                        proceduralSunIntensity = m_editorUtils.Slider("SunIntensitySlider", proceduralSunIntensity, 0f, 10f, helpEnabled);
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                    }
                    else
                    {
                        sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                        sunIntensity = m_editorUtils.Slider("SunIntensitySlider", sunIntensity, 0f, 10f, helpEnabled);
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                    }
                }
                else
                {
                    if (skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                    {
                        sunColor = m_editorUtils.ColorField("SunTintColorSelector", sunColor, helpEnabled);
                        sunIntensity = m_editorUtils.Slider("SunIntensitySlider", sunIntensity, 0f, 10f, helpEnabled);
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                        shadowStrength = m_editorUtils.Slider("SunShadowStrength", shadowStrength, 0f, 1f, helpEnabled);
                    }
                    else
                    {
                        enableSunDisk = m_editorUtils.Toggle("EnableSunDisk", enableSunDisk, helpEnabled);
                        proceduralSunSize = m_editorUtils.Slider("SunSize", proceduralSunSize, 0f, 1f, helpEnabled);
                        proceduralSunSizeConvergence = m_editorUtils.Slider("SunConvergence", proceduralSunSizeConvergence, 0f, 10f, helpEnabled);
                        proceduralSunColor = m_editorUtils.ColorField("SunTintColorSelector", proceduralSunColor, helpEnabled);
                        proceduralSunIntensity = m_editorUtils.Slider("SunIntensitySlider", proceduralSunIntensity, 0f, 10f, helpEnabled);
                        indirectLightMultiplier = m_editorUtils.Slider("IndirectSunLightMultiplier", indirectLightMultiplier, 0f, 10f, helpEnabled);
                        shadowStrength = m_editorUtils.Slider("SunShadowStrength", shadowStrength, 0f, 1f, helpEnabled);
                    }
                }
            }         

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.sunColor = sunColor;
                m_selectedSkyboxProfile.sunIntensity = sunIntensity;
                m_selectedSkyboxProfile.enableSunDisk = enableSunDisk;
                m_selectedSkyboxProfile.proceduralSunIntensity = proceduralSunIntensity;
                m_selectedSkyboxProfile.proceduralSunColor = proceduralSunColor;
                m_selectedSkyboxProfile.proceduralSunSize = proceduralSunSize;
                m_selectedSkyboxProfile.proceduralSunSizeConvergence = proceduralSunSizeConvergence;
                m_selectedSkyboxProfile.indirectLightMultiplier = indirectLightMultiplier;
                m_selectedSkyboxProfile.shadowStrength = shadowStrength;
            }
        }

        /// <summary>
        /// Horizon settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HorizonSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                horizonEnabled = m_editorUtils.ToggleLeft("EnableHorizonSky", horizonEnabled, helpEnabled);
                if (horizonEnabled)
                {
                    EditorGUI.indentLevel++;
                    followPlayer = m_editorUtils.Toggle("FollowPlayer", followPlayer, helpEnabled);
                    if (!followPlayer)
                    {
                        horizonPosition = m_editorUtils.Vector3Field("HorizonPosition", horizonPosition, helpEnabled);
                    }
                    if (followPlayer)
                    {
                        horizonUpdateTime = m_editorUtils.FloatField("HorizonUpdateTime", horizonUpdateTime, helpEnabled);
                    }
                    scaleHorizonObjectWithFog = m_editorUtils.Toggle("ScaleHorizonObjectWithFog", scaleHorizonObjectWithFog, helpEnabled);
                    if (!scaleHorizonObjectWithFog)
                    {
                        horizonScale = m_editorUtils.Vector3Field("HorizonScale", horizonScale, helpEnabled);
                    }
                    horizonScattering = m_editorUtils.Slider("HorizonSkyScattering", horizonScattering, 0f, 1f, helpEnabled);
                    horizonFogDensity = m_editorUtils.Slider("HorizonSkyFogDensity", horizonFogDensity, 0f, 1f, helpEnabled);
                    horizonFalloff = m_editorUtils.Slider("HorizonSkyFalloff", horizonFalloff, 0f, 1f, helpEnabled);
                    horizonBlend = m_editorUtils.Slider("HorizonSkyBlend", horizonBlend, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }
            else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                m_editorUtils.Text("NotAvailableLW", GUILayout.MinHeight(15f), GUILayout.MaxHeight(30f), GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f));
            }
            else
            {
                m_editorUtils.Text("NotAvailableHD", GUILayout.MinHeight(15f), GUILayout.MaxHeight(30f), GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f));
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.scaleHorizonObjectWithFog = scaleHorizonObjectWithFog;
                m_selectedSkyboxProfile.horizonSkyEnabled = horizonEnabled;
                m_selectedSkyboxProfile.horizonScattering = horizonScattering;
                m_selectedSkyboxProfile.horizonFogDensity = horizonFogDensity;
                m_selectedSkyboxProfile.horizonFalloff = horizonFalloff;
                m_selectedSkyboxProfile.horizonBlend = horizonBlend;
                m_selectedSkyboxProfile.horizonSize = horizonScale;
                m_selectedSkyboxProfile.followPlayer = followPlayer;
                m_selectedSkyboxProfile.horizonPosition = horizonPosition;
            }
        }

        /// <summary>
        /// Normal shadow settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ShadowSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            if (mainCam != null)
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0f, mainCam.farClipPlane, helpEnabled);
            }
            else
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0, 3000, helpEnabled);
            }

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                shadowCascade = (AmbientSkiesConsts.ShadowCascade)m_editorUtils.EnumPopup("ShadowCascade", shadowCascade, helpEnabled);
                shadowmaskMode = (ShadowmaskMode)m_editorUtils.EnumPopup("ShadowmaskMode", shadowmaskMode, helpEnabled);
                shadowType = (LightShadows)m_editorUtils.EnumPopup("ShadowType", shadowType, helpEnabled);
                shadowResolution = (ShadowResolution)m_editorUtils.EnumPopup("ShadowResolution", shadowResolution, helpEnabled);
                shadowProjection = (ShadowProjection)m_editorUtils.EnumPopup("ShadowProjection", shadowProjection, helpEnabled);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                m_selectedSkyboxProfile.cascadeCount = shadowCascade;
                m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;
                m_selectedSkyboxProfile.shadowType = shadowType;
                m_selectedSkyboxProfile.shadowResolution = shadowResolution;
                m_selectedSkyboxProfile.shadowProjection = shadowProjection;
            }
        }

        /// <summary>
        /// HD shadow settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void HDShadowSettingsEnabled(bool helpEnabled)
        {
#if HDPipeline
            EditorGUI.BeginChangeCheck();

            hDShadowQuality = (AmbientSkiesConsts.HDShadowQuality)m_editorUtils.EnumPopup("ShadowQuality", hDShadowQuality, helpEnabled);
            if (mainCam != null)
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0f, mainCam.farClipPlane, helpEnabled);
            }
            else
            {
                shadowDistance = m_editorUtils.Slider("ShadowDistanceSlider", shadowDistance, 0, 3000, helpEnabled);
            }
            shadowmaskMode = (ShadowmaskMode)m_editorUtils.EnumPopup("ShadowmaskMode", shadowmaskMode, helpEnabled);

            shadowCascade = (AmbientSkiesConsts.ShadowCascade)m_editorUtils.EnumPopup("ShadowCascade", shadowCascade, helpEnabled);
            if (shadowCascade == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
            {
                m_editorUtils.Text("Cascade count is set to 1 this will result in no shadow distanced base quality. If you want better shadow quality/resolution set a higher cascade count.");
            }
            else if (shadowCascade == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
            {
                EditorGUI.indentLevel++;
                split1 = m_editorUtils.Slider("CascadeSplit1", split1, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            else if (shadowCascade == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
            {
                EditorGUI.indentLevel++;
                split1 = m_editorUtils.Slider("CascadeSplit1", split1, 0f, 1f, helpEnabled);
                split2 = m_editorUtils.Slider("CascadeSplit2", split2, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                split1 = m_editorUtils.Slider("CascadeSplit1", split1, 0f, 1f, helpEnabled);
                split2 = m_editorUtils.Slider("CascadeSplit2", split2, 0f, 1f, helpEnabled);
                split3 = m_editorUtils.Slider("CascadeSplit3", split3, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();
            enableContactShadows = m_editorUtils.Toggle("EnableContactShadows", enableContactShadows, helpEnabled);
            if (enableContactShadows)
            {
                EditorGUI.indentLevel++;
                contactLength = m_editorUtils.Slider("ContactLength", contactLength, 0f, 1f, helpEnabled);
                contactScaleFactor = m_editorUtils.Slider("ContactScaleFactor", contactScaleFactor, 0f, 1f, helpEnabled);
                contactMaxDistance = m_editorUtils.Slider("ContactMaxDistance", contactMaxDistance, 0f, 400f, helpEnabled);
                contactFadeDistance = m_editorUtils.Slider("ContactFadeDistance", contactFadeDistance, 0f, 200f, helpEnabled);
                contactSampleCount = m_editorUtils.IntSlider("ContactSampleCount", contactSampleCount, 0, 64, helpEnabled);
                contactOpacity = m_editorUtils.Slider("ContactOpacity", contactOpacity, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();
            enableMicroShadows = m_editorUtils.Toggle("EnableMicroShadows", enableMicroShadows, helpEnabled);
            if (enableMicroShadows)
            {
                EditorGUI.indentLevel++;
                microShadowOpacity = m_editorUtils.Slider("MicroShadowOpacity", microShadowOpacity, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.shadowDistance = shadowDistance;
                m_selectedSkyboxProfile.shadowQuality = hDShadowQuality;
                m_selectedSkyboxProfile.cascadeCount = shadowCascade;
                m_selectedSkyboxProfile.shadowmaskMode = shadowmaskMode;

                m_selectedSkyboxProfile.useContactShadows = enableContactShadows;
                m_selectedSkyboxProfile.contactShadowsLength = contactLength;
                m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor = contactScaleFactor;
                m_selectedSkyboxProfile.contactShadowsMaxDistance = contactMaxDistance;
                m_selectedSkyboxProfile.contactShadowsFadeDistance = contactFadeDistance;
                m_selectedSkyboxProfile.contactShadowsSampleCount = contactSampleCount;
                m_selectedSkyboxProfile.contactShadowsOpacity = contactOpacity;

                m_selectedSkyboxProfile.useMicroShadowing = enableMicroShadows;
                m_selectedSkyboxProfile.microShadowOpacity = microShadowOpacity;
            }
#endif
        }

        /// <summary>
        /// Screen space reflection settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ScreenSpaceReflectionSettingsEnabled(bool helpEnabled)
        {
#if HDPipeline
            EditorGUI.BeginChangeCheck();

            enableSSReflection = m_editorUtils.Toggle("EnableScreenSpaceReflections", enableSSReflection, helpEnabled);
            if (enableSSReflection)
            {
                EditorGUI.indentLevel++;
                ssrEdgeFade = m_editorUtils.Slider("ScreenSpaceReflectionEdgeFade", ssrEdgeFade, 0f, 1f, helpEnabled);
                ssrNumberOfRays = m_editorUtils.IntSlider("ScreenSpaceReflectionRayNumber", ssrNumberOfRays, 0, 256, helpEnabled);
                ssrObjectThickness = m_editorUtils.Slider("ScreenSpaceReflectionObjectThickness", ssrObjectThickness, 0f, 1f, helpEnabled);
                ssrMinSmoothness = m_editorUtils.Slider("ScreenSpaceReflectionMinSmoothness", ssrMinSmoothness, 0f, 1f, helpEnabled);
                ssrSmoothnessFade = m_editorUtils.Slider("ScreenSpaceReflectionSmoothnessFade", ssrSmoothnessFade, 0f, 1f, helpEnabled);
                ssrReflectSky = m_editorUtils.Toggle("ScreenSpaceReflectionReflectSky", ssrReflectSky, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.enableScreenSpaceReflections = enableSSReflection;
                m_selectedSkyboxProfile.screenEdgeFadeDistance = ssrEdgeFade;
                m_selectedSkyboxProfile.maxNumberOfRaySteps = ssrNumberOfRays;
                m_selectedSkyboxProfile.objectThickness = ssrObjectThickness;
                m_selectedSkyboxProfile.minSmoothness = ssrMinSmoothness;
                m_selectedSkyboxProfile.smoothnessFadeStart = ssrSmoothnessFade;
                m_selectedSkyboxProfile.reflectSky = ssrReflectSky;
            }
#endif
        }

        /// <summary>
        /// Screen space refraction settings
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ScreenSpaceRefractionSettingsEnabled(bool helpEnabled)
        {
#if HDPipeline
            EditorGUI.BeginChangeCheck();

            enableSSRefraction = m_editorUtils.Toggle("EnableScreenSpaceRefraction", enableSSRefraction, helpEnabled);
            if (enableSSRefraction)
            {
                EditorGUI.indentLevel++;
                ssrWeightDistance = m_editorUtils.Slider("ScreenSpaceRefractionWeightDistance", ssrWeightDistance, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedSkyboxProfile.enableScreenSpaceRefractions = enableSSRefraction;
                m_selectedSkyboxProfile.screenWeightDistance = ssrWeightDistance;
            }
#endif
        }

        #endregion

        #region Post FX Tab Functions

        /// <summary>
        /// Main settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MainPostProcessSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Selection popup
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("SelectPostProcesssingDropdown", GUILayout.Width(146f));
            newPPSelection = EditorGUILayout.Popup(m_selectedPostProcessingProfileIndex, ppChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
            m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[newPPSelection];

            if (m_selectedPostProcessingProfile.name == "User")
            {
#if UNITY_POST_PROCESSING_STACK_V2
                customPostProcessingProfile = (PostProcessProfile)m_editorUtils.ObjectField("CustomPostProcessingProfile", customPostProcessingProfile, typeof(PostProcessProfile), false, helpEnabled, GUILayout.Height(16f));
#endif
            }
            antiAliasingMode = (AmbientSkiesConsts.AntiAliasingMode)m_editorUtils.EnumPopup("AntiAliasingMode", antiAliasingMode, helpEnabled);
            hDRMode = (AmbientSkiesConsts.HDRMode)m_editorUtils.EnumPopup("HDRMode", hDRMode, helpEnabled);

            //Prev / Next
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (newPPSelection == 0)
            {
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("PrevPostProcessingButton"))
            {
                newPPSelection--;
            }
            GUI.enabled = true;
            if (newPPSelection == ppChoices.Count - 1)
            {
                GUI.enabled = false;
            }
            if (m_editorUtils.Button("NextPostProcessingButton"))
            {
                newPPSelection++;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                //Selection changing things - exit immediately to not polute settings
                if (newPPSelection != m_selectedPostProcessingProfileIndex)
                {
                    EditorUtility.SetDirty(m_profiles);

                    m_selectedPostProcessingProfileIndex = newPPSelection;
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    return;
                }

                m_selectedPostProcessingProfile.antiAliasingMode = antiAliasingMode;
                m_selectedPostProcessingProfile.hDRMode = hDRMode;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.customPostProcessingProfile = customPostProcessingProfile;
#endif        
            }
        }

        /// <summary>
        /// Ambient Occlusion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void AmbientOcclusionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //AO
            aoEnabled = m_editorUtils.Toggle("AOEnabledToggle", aoEnabled, helpEnabled);
            if (aoEnabled)
            {
                EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                ambientOcclusionMode = (AmbientOcclusionMode)m_editorUtils.EnumPopup("AmbientOcclusionMode", ambientOcclusionMode, helpEnabled);
#endif
                aoAmount = m_editorUtils.Slider("AOAmount", aoAmount, 0f, 4f, helpEnabled);
                aoColor = m_editorUtils.ColorField("AOColor", aoColor, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.aoEnabled = aoEnabled;
                m_selectedPostProcessingProfile.aoAmount = aoAmount;
                m_selectedPostProcessingProfile.aoColor = aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.ambientOcclusionMode = ambientOcclusionMode;
#endif
            }
        }

        /// <summary>
        /// Auto Exposre settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void AutoExposureSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Auto Exposure
            autoExposureEnabled = m_editorUtils.Toggle("AutoExposureEnabledToggle", autoExposureEnabled, helpEnabled);
            if (autoExposureEnabled)
            {
                EditorGUI.indentLevel++;
                exposureAmount = m_editorUtils.Slider("ExposureAmount", exposureAmount, 0f, 5f, helpEnabled);
                exposureMin = m_editorUtils.Slider("ExposureMin", exposureMin, -9f, 9f, helpEnabled);
                exposureMax = m_editorUtils.Slider("ExposureMax", exposureMax, -9f, 9f, helpEnabled);

                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.autoExposureEnabled = autoExposureEnabled;
                m_selectedPostProcessingProfile.exposureAmount = exposureAmount;
                m_selectedPostProcessingProfile.exposureMin = exposureMin;
                m_selectedPostProcessingProfile.exposureMax = exposureMax;
            }
        }

        /// <summary>
        /// Bloom settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void BloomSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Bloom
            bloomEnabled = m_editorUtils.Toggle("BloomEnabledToggle", bloomEnabled, helpEnabled);
            if (bloomEnabled)
            {
                EditorGUI.indentLevel++;
                bloomIntensity = m_editorUtils.Slider("BloomIntensity", bloomIntensity, 0f, 5f, helpEnabled);
                bloomThreshold = m_editorUtils.Slider("BloomThreshold", bloomThreshold, 0f, 20f, helpEnabled);
                lensIntensity = m_editorUtils.Slider("BloomLensIntensity", lensIntensity, 0f, 20f, helpEnabled);
                lensTexture = (Texture2D)m_editorUtils.ObjectField("LensTexture", lensTexture, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.bloomEnabled = bloomEnabled;
                m_selectedPostProcessingProfile.bloomAmount = bloomIntensity;
                m_selectedPostProcessingProfile.bloomThreshold = bloomThreshold;
                m_selectedPostProcessingProfile.lensTexture = lensTexture;
                m_selectedPostProcessingProfile.lensIntensity = lensIntensity;
            }
        }

        /// <summary>
        /// Chromatic Aberration settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ChromaticAberrationSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            chromaticAberrationEnabled = m_editorUtils.Toggle("ChromaticAberrationEnabled", chromaticAberrationEnabled, helpEnabled);
            if (chromaticAberrationEnabled)
            {
                chromaticAberrationIntensity = m_editorUtils.Slider("ChromaticAberrationIntensity", chromaticAberrationIntensity, 0f, 1f, helpEnabled);
            }           

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.chromaticAberrationEnabled = chromaticAberrationEnabled;
                m_selectedPostProcessingProfile.chromaticAberrationIntensity = chromaticAberrationIntensity;
            }
        }

        /// <summary>
        /// Color Grading settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ColorGradingSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Color grading
            colorGradingEnabled = m_editorUtils.Toggle("ColorGradingEnabledToggle", colorGradingEnabled, helpEnabled);
            if (colorGradingEnabled)
            {
                EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                colorGradingMode = (GradingMode)m_editorUtils.EnumPopup("ColorGradingMode", colorGradingMode, helpEnabled);
                if (colorGradingMode  == GradingMode.HighDefinitionRange)
                {
                    colorGradingPostExposure = m_editorUtils.Slider("ColorGradingPostExposure", colorGradingPostExposure, -5f, 5f, helpEnabled);
                }
                else
                {
                    colorGradingLut = (Texture2D)m_editorUtils.ObjectField("ColorGradingLut", colorGradingLut, typeof(Texture2D), false, helpEnabled, GUILayout.Height(16f));
                }
#endif

                colorGradingColorFilter = m_editorUtils.ColorField("ColorGradingColorFilter", colorGradingColorFilter, helpEnabled);
                if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Disable)
                {
                    colorGradingTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", colorGradingTempature, -100, 100, helpEnabled);
                }
                else if (useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable && !syncPostProcessing)
                {
                    colorGradingTempature = m_editorUtils.IntSlider("ColorGradingTempatureIntSlider", colorGradingTempature, -100, 100, helpEnabled);
                }
                colorGradingTint = m_editorUtils.IntSlider("ColorGradingTintIntSlider", colorGradingTint, -100, 100, helpEnabled);
                colorGradingSaturation = m_editorUtils.Slider("ColorGradingSaturationSlider", colorGradingSaturation, -100f, 100f, helpEnabled);
                colorGradingContrast = m_editorUtils.Slider("ColorGradingContrastSlider", colorGradingContrast, -100f, 100f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.colorGradingMode = colorGradingMode;
#endif

                m_selectedPostProcessingProfile.colorGradingLut = colorGradingLut;
                m_selectedPostProcessingProfile.colorGradingColorFilter = colorGradingColorFilter;
                m_selectedPostProcessingProfile.colorGradingPostExposure = colorGradingPostExposure;
                m_selectedPostProcessingProfile.colorGradingEnabled = colorGradingEnabled;
                m_selectedPostProcessingProfile.colorGradingTempature = colorGradingTempature;
                m_selectedPostProcessingProfile.colorGradingTint = colorGradingTint;
                m_selectedPostProcessingProfile.colorGradingSaturation = colorGradingSaturation;
                m_selectedPostProcessingProfile.colorGradingContrast = colorGradingContrast;
            }
        }

        /// <summary>
        /// Depth Of Field settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void DepthOfFieldSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Depth Of Field
            depthOfFieldEnabled = m_editorUtils.Toggle("DOFEnabledToggle", depthOfFieldEnabled, helpEnabled);
            if (depthOfFieldEnabled)
            {
                EditorGUI.indentLevel++;
                depthOfFieldMode = (AmbientSkiesConsts.DepthOfFieldMode)m_editorUtils.EnumPopup("DepthOfFieldMode", depthOfFieldMode, helpEnabled);
                if (depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    depthOfFieldTrackingType = (AmbientSkiesConsts.DOFTrackingType)m_editorUtils.EnumPopup("DepthOfFieldTrackingType", depthOfFieldTrackingType, helpEnabled);
#if UNITY_POST_PROCESSING_STACK_V2
                    maxBlurSize = (KernelSize)m_editorUtils.EnumPopup("DepthOfFieldMaxBlurSize", maxBlurSize, helpEnabled);
#endif
                    EditorGUILayout.BeginHorizontal();
                    m_editorUtils.Text("DOFFocusDistanceIs", GUILayout.Width(144f));
                    m_editorUtils.TextNonLocalized("" + depthOfFieldDistanceString, GUILayout.Width(80f), GUILayout.Height(16f));
                    EditorGUILayout.EndHorizontal();
                    depthOfFieldAperture = m_editorUtils.Slider("DOFApertureSlider", depthOfFieldAperture, 0.1f, 32f, helpEnabled);
                    depthOfFieldFocalLength = m_editorUtils.Slider("DOFFocalLengthSlider", depthOfFieldFocalLength, 1f, 300f, helpEnabled);
                    focusOffset = m_editorUtils.Slider("DOFFocusOffset", focusOffset, -100f, 100f, helpEnabled);
                    targetLayer = LayerMaskField("DOFTargetLayer", targetLayer.value, helpEnabled);
                    maxFocusDistance = m_editorUtils.Slider("DOFMaxFocusDistance", maxFocusDistance, 0f, 5000f, helpEnabled);
                }
                else
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    maxBlurSize = (KernelSize)m_editorUtils.EnumPopup("DepthOfFieldMaxBlurSize", maxBlurSize, helpEnabled);
#endif
                    depthOfFieldFocusDistance = m_editorUtils.Slider("DOFFocusDistanceSlider", depthOfFieldFocusDistance, 1f, 10000f, helpEnabled);
                    depthOfFieldAperture = m_editorUtils.Slider("DOFApertureSlider", depthOfFieldAperture, 0.1f, 32f, helpEnabled);
                    depthOfFieldFocalLength = m_editorUtils.Slider("DOFFocalLengthSlider", depthOfFieldFocalLength, 1f, 300f, helpEnabled);
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.depthOfFieldEnabled = depthOfFieldEnabled;
                m_selectedPostProcessingProfile.depthOfFieldMode = depthOfFieldMode;
                m_selectedPostProcessingProfile.depthOfFieldFocusDistance = depthOfFieldFocusDistance;
                if (depthOfFieldMode == AmbientSkiesConsts.DepthOfFieldMode.AutoFocus)
                {
                    m_selectedPostProcessingProfile.depthOfFieldTrackingType = depthOfFieldTrackingType;
                    m_selectedPostProcessingProfile.focusOffset = focusOffset;
                    m_selectedPostProcessingProfile.targetLayer = targetLayer;
                    m_selectedPostProcessingProfile.maxFocusDistance = maxFocusDistance;
                }

#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.maxBlurSize = maxBlurSize;
#endif
                m_selectedPostProcessingProfile.depthOfFieldAperture = depthOfFieldAperture;
                m_selectedPostProcessingProfile.depthOfFieldFocalLength = depthOfFieldFocalLength;
            }
        }

        /// <summary>
        /// Grain settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void GrainSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Grain
            grainEnabled = m_editorUtils.Toggle("GrainEnabledToggle", grainEnabled, helpEnabled);
            if (grainEnabled)
            {
                EditorGUI.indentLevel++;
                grainIntensity = m_editorUtils.Slider("GrainIntensity", grainIntensity, 0f, 1f, helpEnabled);
                grainSize = m_editorUtils.Slider("GrainSize", grainSize, 0f, 3f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.grainEnabled = grainEnabled;
                m_selectedPostProcessingProfile.grainIntensity = grainIntensity;
                m_selectedPostProcessingProfile.grainSize = grainSize;
            }
        }

        /// <summary>
        /// Lens Distortion settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void LensDistortionSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Lens Distortion
            distortionEnabled = m_editorUtils.Toggle("DistortionEnabledToggle", distortionEnabled, helpEnabled);
            if (distortionEnabled)
            {
                EditorGUI.indentLevel++;
                distortionIntensity = m_editorUtils.Slider("DistortionIntensitySlider", distortionIntensity, -100f, 100f, helpEnabled);
                distortionScale = m_editorUtils.Slider("DistortionScaleSlider", distortionScale, 0.01f, 5f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.distortionEnabled = distortionEnabled;
                m_selectedPostProcessingProfile.distortionIntensity = distortionIntensity;
                m_selectedPostProcessingProfile.distortionScale = distortionScale;
            }
        }

        /// <summary>
        /// Screen Space Reflections settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void ScreenSpaceReflectionsSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Screen Space Reflection
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                m_editorUtils.Text("Screen Space Reflections is handled in the Skies Tab in High Definition.");
            }
            else
            {
                screenSpaceReflectionsEnabled = m_editorUtils.Toggle("ScreenSpaceReflectionsEnabled", screenSpaceReflectionsEnabled, helpEnabled);
                if (screenSpaceReflectionsEnabled)
                {
                    EditorGUI.indentLevel++;
#if UNITY_POST_PROCESSING_STACK_V2
                    screenSpaceReflectionPreset = (ScreenSpaceReflectionPreset)m_editorUtils.EnumPopup("ScreenSpaceReflectionPreset", screenSpaceReflectionPreset, helpEnabled);
                    if (screenSpaceReflectionPreset == ScreenSpaceReflectionPreset.Custom)
                    {
                        maximumIterationCount = m_editorUtils.IntSlider("MaximumIterationCount", maximumIterationCount, 0, 256, helpEnabled);
                        thickness = m_editorUtils.Slider("Thickness", thickness, 0f, 64f, helpEnabled);
                        screenSpaceReflectionResolution = (ScreenSpaceReflectionResolution)m_editorUtils.EnumPopup("ScreenSpaceReflectionResolution", screenSpaceReflectionResolution, helpEnabled);
                    }
#endif
                    maximumMarchDistance = m_editorUtils.Slider("MaximumMarchDistance", maximumMarchDistance, 0f, 4000f, helpEnabled);
                    distanceFade = m_editorUtils.Slider("DistanceFade", distanceFade, 0f, 1f, helpEnabled);
                    screenSpaceVignette = m_editorUtils.Slider("ScreenSpaceVignette", screenSpaceVignette, 0f, 1f, helpEnabled);
                    EditorGUI.indentLevel--;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled = screenSpaceReflectionsEnabled;
                m_selectedPostProcessingProfile.maximumIterationCount = maximumIterationCount;
                m_selectedPostProcessingProfile.thickness = thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                m_selectedPostProcessingProfile.spaceReflectionResolution = screenSpaceReflectionResolution;
                m_selectedPostProcessingProfile.screenSpaceReflectionPreset = screenSpaceReflectionPreset;
#endif
                m_selectedPostProcessingProfile.maximumMarchDistance = maximumMarchDistance;
                m_selectedPostProcessingProfile.distanceFade = distanceFade;
                m_selectedPostProcessingProfile.screenSpaceVignette = screenSpaceVignette;
            }
        }

        /// <summary>
        /// Vignette settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void VignetteSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Vignette
            vignetteEnabled = m_editorUtils.Toggle("VignetteEnabledToggle", vignetteEnabled, helpEnabled);
            if (vignetteEnabled)
            {
                EditorGUI.indentLevel++;
                vignetteIntensity = m_editorUtils.Slider("VignetteIntensitySlider", vignetteIntensity, 0f, 1f, helpEnabled);
                vignetteSmoothness = m_editorUtils.Slider("VignetteSmoothnessSlider", vignetteSmoothness, 0f, 1f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.vignetteEnabled = vignetteEnabled;
                m_selectedPostProcessingProfile.vignetteIntensity = vignetteIntensity;
                m_selectedPostProcessingProfile.vignetteSmoothness = vignetteSmoothness;
            }
        }

        /// <summary>
        /// Motion Blur settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MotionBlurSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Motion Blur
            motionBlurEnabled = m_editorUtils.Toggle("MotionBlurEnabled", motionBlurEnabled, helpEnabled);
            if (motionBlurEnabled)
            {
                EditorGUI.indentLevel++;
                motionShutterAngle = m_editorUtils.IntSlider("MotionShutterAngle", motionShutterAngle, 0, 360, helpEnabled);
                motionSampleCount = m_editorUtils.IntSlider("MotionSampleCount", motionSampleCount, 0, 32, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedPostProcessingProfile.motionBlurEnabled = motionBlurEnabled;
                m_selectedPostProcessingProfile.shutterAngle = motionShutterAngle;
                m_selectedPostProcessingProfile.sampleCount = motionSampleCount;
            }
        }

        #endregion

        #region Lighting Tab Functions

        /// <summary>
        /// Main Lighting settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MainLightingSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            //Select profile
            autoLightmapGeneration = m_editorUtils.Toggle("AutoLightmapGeneration", autoLightmapGeneration, helpEnabled);
            //newLightmappingSettings = m_editorUtils.Popup("LightmappingSelectionDropDown", m_selectedLightingProfileIndex, lightmappingChoices.ToArray(), helpEnabled);
            EditorGUILayout.BeginHorizontal();
            m_editorUtils.Text("LightmappingSelectionDropDown", GUILayout.Width(146f));
            newLightmappingSettings = EditorGUILayout.Popup(m_selectedLightingProfileIndex, lightmappingChoices.ToArray(), GUILayout.ExpandWidth(true), GUILayout.Height(16f));
            EditorGUILayout.EndHorizontal();
            m_selectedLightingProfile = m_profiles.m_lightingProfiles[newLightmappingSettings];

            //Get graphic Tiers
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);

            //Set linear deferred lighting
            if (PlayerSettings.colorSpace != ColorSpace.Linear || tier1.renderingPath != RenderingPath.DeferredShading || tier2.renderingPath != RenderingPath.DeferredShading || tier3.renderingPath != RenderingPath.DeferredShading)
            if (m_editorUtils.Button("SetLinearDefferedButton"))
            {
                if (EditorUtility.DisplayDialog("Alert!!", "Warning you're about to set Linear Color Space and Deferred Rendering Path. If you're Color Space is not already Linear this will require a reimport, this will also close the Ambient Skies window. Are you sure you want to proceed?", "Yes", "No"))
                {
                    LightingUtils.SetLinearDeferredLighting(this);
                }
            }

            //Add reflection static
            if (m_editorUtils.Button("AddGlobalReflectionProbeButton"))
            {
                LightingUtils.AddGlobalReflectionProbeStatic();
            }

            if (Lightmapping.isRunning)
            {
                //Cancel light bake
                if (m_editorUtils.Button("CancelLightmapBaking"))
                {
                    LightingUtils.CancelLighting();
                }
            }

            if (Lightmapping.isRunning)
            {
                GUI.enabled = false;
            }
            //Bake reflection probe
            if (m_editorUtils.Button("BakeGlobalReflectionProbeButton"))
            {
                LightingUtils.BakeGlobalReflectionProbe(true);
            }
            if (Application.isPlaying)
            {
                GUI.enabled = false;
            }
            //Bake lightmaps
            if (m_editorUtils.Button("BakeGlobalLightingButton"))
            {
                LightingUtils.BakeGlobalLighting();
            }
            //Bake lightmaps
            if (m_editorUtils.Button("ClearBakedLightmaps"))
            {
                LightingUtils.ClearLightmapData();
            }
            GUI.enabled = true;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedLightingProfile.autoLightmapGeneration = autoLightmapGeneration;

                //Selection changing things - exit immediately to not polute settings
                if (newLightmappingSettings != m_selectedLightingProfileIndex)
                {
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    m_selectedLightingProfileIndex = newLightmappingSettings;
                }
            }
        }

        /// <summary>
        /// Realtime GI settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void RealtimeGISettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            realtimeGlobalIllumination = m_editorUtils.Toggle("RealtimeGI", realtimeGlobalIllumination, helpEnabled);
            //Realtime GI
            if (realtimeGlobalIllumination)
            {
                EditorGUI.indentLevel++;
                indirectRelolution = m_editorUtils.Slider("IndirectResolution", indirectRelolution, 0f, 40f, helpEnabled);
                useDirectionalMode = m_editorUtils.Toggle("UseDirectionalLightmapsMode", useDirectionalMode, helpEnabled);
                lightIndirectIntensity = m_editorUtils.Slider("LightIndirectIntensity", lightIndirectIntensity, 0f, 5f, helpEnabled);
                lightBoostIntensity = m_editorUtils.Slider("LightBoostIntensity", lightBoostIntensity, 0f, 10f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedLightingProfile.realtimeGlobalIllumination = realtimeGlobalIllumination;
                m_selectedLightingProfile.indirectRelolution = indirectRelolution;
                m_selectedLightingProfile.useDirectionalMode = useDirectionalMode;
                m_selectedLightingProfile.lightIndirectIntensity = lightIndirectIntensity;
                m_selectedLightingProfile.lightBoostIntensity = lightBoostIntensity;
            }
        }

        /// <summary>
        /// Baked GI settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void BakedGISettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

            bakedGlobalIllumination = m_editorUtils.Toggle("BakedGI", bakedGlobalIllumination, helpEnabled);
            //Baked GI
            if (bakedGlobalIllumination)
            {
                EditorGUI.indentLevel++;
                lightmappingMode = (AmbientSkiesConsts.LightmapperMode)m_editorUtils.EnumPopup("LightmappingMode", lightmappingMode, helpEnabled);
#if !UNITY_2018_3_OR_NEWER
                if (lightmappingMode == AmbientSkiesConsts.LightmapperMode.ProgressiveGPU)
                {
                    EditorUtility.DisplayDialog("Warning Not Supported!", "ProgressiveGPU is not supported in this version of Unity switching back to ProgressiveCPU. Please install Unity 2018.3 or newer to use this feature", "Ok");
                    lightmappingMode = AmbientSkiesConsts.LightmapperMode.ProgressiveCPU;
                }
#endif
                lightmapResolution = m_editorUtils.Slider("LightmapResolution", lightmapResolution, 0f, 250f, helpEnabled);
                lightmapPadding = m_editorUtils.IntSlider("LightmapPadding", lightmapPadding, 0, 100, helpEnabled);
                useHighResolutionLightmapSize = m_editorUtils.Toggle("UseHighQualityLightMapResolution", useHighResolutionLightmapSize, helpEnabled);
                compressLightmaps = m_editorUtils.Toggle("CompressLightmaps", compressLightmaps, helpEnabled);
                ambientOcclusion = m_editorUtils.Toggle("AmbientOcclusion", ambientOcclusion, helpEnabled);
                if (ambientOcclusion)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.indentLevel++;
                    maxDistance = m_editorUtils.Slider("AOMaxDistance", maxDistance, 0f, 25f, helpEnabled);
                    indirectContribution = m_editorUtils.Slider("AOIndirectContribution", indirectContribution, 0f, 10f, helpEnabled);
                    directContribution = m_editorUtils.Slider("AODirectContribution", directContribution, 0f, 10f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                finalGather = m_editorUtils.Toggle("FinalGather", finalGather, helpEnabled);
                if (finalGather)
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.indentLevel++;
                    finalGatherRayCount = m_editorUtils.IntSlider("FinalGatherRayCount", finalGatherRayCount, 0, 4096, helpEnabled);
                    finalGatherDenoising = m_editorUtils.Toggle("FinalGatherDenoising", finalGatherDenoising, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
                lightIndirectIntensity = m_editorUtils.Slider("LightIndirectIntensity", lightIndirectIntensity, 0f, 5f, helpEnabled);
                lightBoostIntensity = m_editorUtils.Slider("LightBoostIntensity", lightBoostIntensity, 0f, 10f, helpEnabled);
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profiles);

                m_selectedLightingProfile.bakedGlobalIllumination = bakedGlobalIllumination;
                m_selectedLightingProfile.lightmappingMode = lightmappingMode;
                m_selectedLightingProfile.lightmapResolution = lightmapResolution;
                m_selectedLightingProfile.lightmapPadding = lightmapPadding;
                m_selectedLightingProfile.useHighResolutionLightmapSize = useHighResolutionLightmapSize;
                m_selectedLightingProfile.compressLightmaps = compressLightmaps;
                m_selectedLightingProfile.ambientOcclusion = ambientOcclusion;
                m_selectedLightingProfile.maxDistance = maxDistance;
                m_selectedLightingProfile.indirectContribution = indirectContribution;
                m_selectedLightingProfile.directContribution = directContribution;
                m_selectedLightingProfile.lightIndirectIntensity = lightIndirectIntensity;
                m_selectedLightingProfile.lightBoostIntensity = lightBoostIntensity;
                m_selectedLightingProfile.finalGather = finalGather;
                m_selectedLightingProfile.finalGatherRayCount = finalGatherRayCount;
                m_selectedLightingProfile.finalGatherDenoising = finalGatherDenoising;
            }
        }

        #endregion

        #region Massive Clouds Tab Function

        /// <summary>
        /// Massive Clouds system settings foldout
        /// </summary>
        /// <param name="helpEnabled"></param>
        private void MassiveCloudsSettingsEnabled(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();

#if Mewlist_Clouds
            massiveCloudsEnabled = m_editorUtils.ToggleLeft("MassiveCloudsEnabled", massiveCloudsEnabled, helpEnabled);
            if (massiveCloudsEnabled)
            {
                EditorGUI.indentLevel++;
                cloudProfile = (MassiveCloudsProfile)m_editorUtils.ObjectField("CloudProfile", cloudProfile, typeof(MassiveCloudsProfile), false, helpEnabled, GUILayout.Height(16f));
                syncGlobalFogColor = m_editorUtils.Toggle("SyncCloudGlobalFogColor", syncGlobalFogColor, helpEnabled);
                if (!syncGlobalFogColor)
                {
                    EditorGUI.indentLevel++;
                    cloudsFogColor = m_editorUtils.ColorField("CloudFogColor", cloudsFogColor, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                syncBaseFogColor = m_editorUtils.Toggle("SyncCloudBaseFogColor", syncBaseFogColor, helpEnabled);
                if (!syncBaseFogColor)
                {
                    EditorGUI.indentLevel++;
                    cloudsBaseFogColor = m_editorUtils.ColorField("CloudBaseFogColor", cloudsBaseFogColor, helpEnabled);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_selectedPostProcessingProfile.massiveCloudsEnabled = massiveCloudsEnabled;
                m_selectedPostProcessingProfile.cloudProfile = cloudProfile;
                m_selectedPostProcessingProfile.syncGlobalFogColor = syncGlobalFogColor;
                m_selectedPostProcessingProfile.cloudsFogColor = cloudsFogColor;
                m_selectedPostProcessingProfile.cloudsBaseFogColor = cloudsBaseFogColor;
                m_selectedPostProcessingProfile.syncBaseFogColor = syncBaseFogColor;
                m_selectedPostProcessingProfile.cloudIsHDRP = cloudIsHDRP;
            }
#endif
        }

        #endregion

        #endregion

        #region HelperMethods

        #region OnEnable

        /// <summary>
        /// Loads icons
        /// </summary>
        public void LoadIcons()
        {
            if (EditorGUIUtility.isProSkin)
            {
                if (m_skiesIcon == null)
                {
                    m_skiesIcon = Resources.Load("Skybox_Pro_icon") as Texture2D;
                }

                if (m_postProcessingIcon == null)
                {
                    m_postProcessingIcon = Resources.Load("Post_Processing_Pro_icon") as Texture2D;
                }

                if (m_lightingIcon == null)
                {
                    m_lightingIcon = Resources.Load("Light_Bake_Pro_icon") as Texture2D;
                }
            }
            else
            {
                if (m_skiesIcon == null)
                {
                    m_skiesIcon = Resources.Load("Skybox_Standard_icon") as Texture2D;
                }

                if (m_postProcessingIcon == null)
                {
                    m_postProcessingIcon = Resources.Load("Post_Processing_Standard_icon") as Texture2D;
                }

                if (m_lightingIcon == null)
                {
                    m_lightingIcon = Resources.Load("Light_Bake_Standard_icon") as Texture2D;
                }
            }
        }

        /// <summary>
        /// Adds post processing if required
        /// </summary>
        public void AddPostProcessingV2Only()
        {
            if (EditorUtility.DisplayDialog("Missing Post Processing V2", "We're about to import post processing v2 from the package manager. This process may take a few minutes and will setup your current scenes environment.", "OK"))
            {
                if (GraphicsSettings.renderPipelineAsset == null)
                {
                    m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
                    renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;
                }
                else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                {
                    m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
                    renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;
                }
                else
                {
                    m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.Lightweight;
                    renderPipelineSettings = AmbientSkiesConsts.RenderPipelineSettings.Lightweight;
                }

                AmbientSkiesPipelineUtilsEditor.ShowAmbientSkiesPipelineUtilsEditor(m_profiles.m_selectedRenderPipeline, renderPipelineSettings, false, false, this);
            }
        }

        /// <summary>
        /// Sets scripting defines for pipeline
        /// </summary>
        public void ApplyScriptingDefine()
        {
            bool isChanged = false;
            if (GraphicsSettings.renderPipelineAsset == null)
            {
                m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.BuiltIn;

                string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                    isChanged = true;
                }
                if (isChanged)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                    return;
                }
            }
            else if (GraphicsSettings.renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
            {
                m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.HighDefinition;

                if (GraphicsSettings.renderPipelineAsset.name != "Procedural Worlds HDRPRenderPipelineAsset")
                {
                    GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath("Procedural Worlds HDRPRenderPipelineAsset"));
                }

                string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (!currBuildSettings.Contains("HDPipeline"))
                {
                    if (string.IsNullOrEmpty(currBuildSettings))
                    {
                        currBuildSettings = "HDPipeline";
                    }
                    else
                    {
                        currBuildSettings += ";HDPipeline";
                    }
                    isChanged = true;
                }
                if (currBuildSettings.Contains("LWPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("LWPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("LWPipeline", "");
                    isChanged = true;
                }
                if (isChanged)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                    return;
                }
            }
            else
            {
                m_profiles.m_selectedRenderPipeline = AmbientSkiesConsts.RenderPipelineSettings.Lightweight;

                if (GraphicsSettings.renderPipelineAsset.name != "Procedural Worlds Lightweight Pipeline Profile Ambient Skies")
                {
                    GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                }

                string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                if (!currBuildSettings.Contains("LWPipeline"))
                {
                    if (string.IsNullOrEmpty(currBuildSettings))
                    {
                        currBuildSettings = "LWPipeline";
                    }
                    else
                    {
                        currBuildSettings += ";LWPipeline";
                    }
                    isChanged = true;
                }
                if (currBuildSettings.Contains("HDPipeline"))
                {
                    currBuildSettings = currBuildSettings.Replace("HDPipeline;", "");
                    currBuildSettings = currBuildSettings.Replace("HDPipeline", "");
                    isChanged = true;
                }
                if (isChanged)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings);
                }
            }
        }

        /// <summary>
        /// Loads the settings based ont he bool input
        /// </summary>
        /// <param name="loadingFromNewScene"></param>
        public void LoadAndApplySettings(bool loadingFromNewScene)
        {
            if (!loadingFromNewScene)
            {
                //Assigns skybox profile
                string key = "AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode();
                if (EditorPrefs.HasKey(key))
                {
                    string profileName = EditorPrefs.GetString(key);
                    int idx = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, profileName);
                    if (idx >= 0 && idx < m_profiles.m_skyProfiles.Count)
                    {
                        m_selectedSkyboxProfileIndex = idx;
                    }
                    else
                    {
                        if (RenderSettings.skybox.name == "Ambient Skies Skybox")
                        {
                            m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, true);
                        }
                        else
                        {
                            m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, "Sky Five Low");
                        }
                    }
                }
                else
                {
                    if (renderPipelineSettings != AmbientSkiesConsts.RenderPipelineSettings.HighDefinition && RenderSettings.skybox.name == "Ambient Skies Skybox")
                    {
                        m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromActiveSkybox(m_profiles, m_selectedSkyboxProfile, true);
                    }
                    else
                    {
                        m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, "Sky Five Low");
                    }
                }

                if (m_selectedSkyboxProfileIndex >= 0)
                {
                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                    }
                }
                else
                {
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Skybox Profile Empty");
                    }

                    m_selectedSkyboxProfile = null;
                }

                //Assigns post processing profile
                string postProcessingName = "AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode();
                if (EditorPrefs.HasKey(postProcessingName))
                {
                    string profileName = EditorPrefs.GetString(postProcessingName);
                    int idx = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, profileName);
                    if (idx >= 0 && idx < m_profiles.m_ppProfiles.Count)
                    {
                        m_selectedPostProcessingProfileIndex = idx;
                    }
                    else
                    {
                        m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromPostProcessing(m_profiles);
                    }
                }
                else
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromPostProcessing(m_profiles);
                }

                if (m_selectedPostProcessingProfileIndex >= 0)
                {
                    m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                    }
                }
                else
                {
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Post Processing Profile Empty");
                    }

                    m_selectedPostProcessingProfile = null;
                }

                //Assigns lighting profile
                string lightingProfileName = EditorPrefs.GetString("AmbientSkiesLighting_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode());
                if (!string.IsNullOrEmpty(lightingProfileName))
                {
                    int idx = LightingUtils.GetProfileIndexFromProfileName(m_profiles, lightingProfileName);
                    if (idx >= 0 && idx < m_profiles.m_lightingProfiles.Count)
                    {
                        m_selectedLightingProfileIndex = idx;
                    }
                    else
                    {
                        m_selectedLightingProfileIndex = LightingUtils.GetProfileIndexFromProfileName(m_profiles, "Default Quality Lighting");
                    }
                }
                else
                {
                    m_selectedLightingProfileIndex = LightingUtils.GetProfileIndexFromProfileName(m_profiles, "Default Quality Lighting");
                }

                if (m_selectedLightingProfileIndex >= 0)
                {
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);
                    }
                }
                else
                {
                    m_selectedLightingProfile = null;
                }
            }
            else
            {
                m_selectedSkyboxProfileIndex = SkyboxUtils.GetFromIsPWProfile(m_profiles, false);
                if (m_selectedSkyboxProfileIndex >= 0)
                {
                    m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                    }
                    EditorPrefs.SetString("AmbientSkiesProfile_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex].name);
                }
                else
                {
                    if (m_profiles.m_showDebug)
                    {
                        Debug.Log("Skybox Profile Empty");
                    }

                    m_selectedSkyboxProfile = null;
                }

#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessVolume processVol = FindObjectOfType<PostProcessVolume>();
                if (processVol != null)
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "User");
                    if (m_selectedPostProcessingProfileIndex >= 0)
                    {
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                        if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                        }
                        EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex].name);
                    }
                    else
                    {
                        if (m_profiles.m_showDebug)
                        {
                            Debug.Log("Post Processing Profile Empty");
                        }

                        m_selectedPostProcessingProfile = null;
                    }
                }
                else
                {
                    m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "Alpine");
                    if (m_selectedPostProcessingProfileIndex >= 0)
                    {
                        m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                        if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                        }
                        EditorPrefs.SetString("AmbientSkiesPostProcessing_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_ppProfiles[newPPSelection].name);
                    }
                }
#endif

                m_selectedLightingProfileIndex = LightingUtils.GetProfileIndexFromProfileName(m_profiles, "Default Quality Lighting");
                if (m_selectedLightingProfileIndex >= 0)
                {
                    m_selectedLightingProfile = m_profiles.m_lightingProfiles[m_selectedLightingProfileIndex];
                    if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                    {
                        LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);
                    }
                    EditorPrefs.SetString("AmbientSkiesLighting_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetHashCode(), m_profiles.m_lightingProfiles[newLightmappingSettings].name);
                }
                else
                {
                    m_selectedLightingProfile = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="currentProfileName"></param>
        /// <returns></returns>
        public static int GetSkyProfile(List<AmbientSkyProfiles> profiles, string currentProfileName)
        {
            for (int idx = 0; idx < profiles.Count; idx++)
            {
                if (profiles[idx].name == currentProfileName)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds all profiles, to find type search t:OBJECT
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeToSearch"></param>
        /// <returns></returns>
        private static List<AmbientSkyProfiles> GetAllSkyProfilesProjectSearch(string typeToSearch)
        {
            string[] skyProfilesGUIDs = AssetDatabase.FindAssets(typeToSearch);
            List<AmbientSkyProfiles> newSkyProfiles = new List<AmbientSkyProfiles>(skyProfilesGUIDs.Length);
            for (int x = 0; x < skyProfilesGUIDs.Length; ++x)
            {
                string path = AssetDatabase.GUIDToAssetPath(skyProfilesGUIDs[x]);
                AmbientSkyProfiles data = AssetDatabase.LoadAssetAtPath<AmbientSkyProfiles>(path);
                if (data == null)
                    continue;
                newSkyProfiles.Add(data);
            }

            return newSkyProfiles;
        }

        /// <summary>
        /// Creates volume object for HDRP
        /// </summary>
        /// <param name="volumeName"></param>
        private static void CreateHDRPVolume(string volumeName)
        {
            //Get parent object
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);

            //Hd Pipeline Volume Setup
            GameObject volumeObject = GameObject.Find(volumeName);
            if (volumeObject == null)
            {
                volumeObject = new GameObject(volumeName);
                volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                volumeObject.transform.SetParent(parentObject.transform);
            }
            else
            {
                volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                volumeObject.transform.SetParent(parentObject.transform);
            }
        }

        /// <summary>
        /// GUI Layout for a gradient field on GUI inspector
        /// </summary>
        /// <param name="key"></param>
        /// <param name="gradient"></param>
        /// <param name="helpEnabled"></param>
        /// <returns></returns>
        private Gradient GradientField(string key, Gradient gradient, bool helpEnabled)
        {
#if UNITY_2018_3_OR_NEWER
            GUIContent label = m_editorUtils.GetContent(key);
            gradient = EditorGUILayout.GradientField(label, gradient);
            m_editorUtils.InlineHelp(key, helpEnabled);
            return gradient;
#else
            return gradient;
#endif
        }

        /// <summary>
        /// Handy layer mask interface
        /// </summary>
        /// <param name="label"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        private LayerMask LayerMaskField(string label, LayerMask layerMask, bool helpEnabled)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                    maskWithoutEmpty |= (1 << i);
            }
            maskWithoutEmpty = m_editorUtils.MaskField(label, maskWithoutEmpty, layers.ToArray(), helpEnabled);
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            layerMask.value = mask;
            return layerMask;
        }

        /// <summary>
        /// Apply auto fog distance
        /// </summary>
        /// <param name="skyProfiles"></param>
        /// <param name="profile"></param>
        private void ApplyAutoFogSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile)
        {
            if (skyProfiles.configurationType == AmbientSkiesConsts.AutoConfigureType.Terrain)
            {
                Terrain terrain = Terrain.activeTerrain;
                if (terrain == null)
                {
                    Debug.LogWarning("No Terrain could be found");
                    return;
                }
                else
                {
                    profile.fogDistance = terrain.terrainData.size.x / 1.4f;
                    fogDistance = terrain.terrainData.size.x / 1.4f;
                    profile.nearFogDistance = 25f / profile.fogDistance;
                    nearFogDistance = 25f / profile.fogDistance;

                    profile.proceduralFogDistance = terrain.terrainData.size.x / 1.4f;
                    proceduralFogDistance = terrain.terrainData.size.x / 1.4f;
                    profile.proceduralNearFogDistance = 25f / profile.fogDistance;
                    proceduralNearFogDistance = 25f / profile.fogDistance;
                }
            }
            else if (skyProfiles.configurationType == AmbientSkiesConsts.AutoConfigureType.Camera)
            {
                Camera mainCamera = FindObjectOfType<Camera>();
                if (mainCamera == null)
                {
                    Debug.LogWarning("No Camera could be found");
                    return;
                }
                else
                {
                    profile.fogDistance = mainCamera.farClipPlane / 1.4f;
                    fogDistance = mainCamera.farClipPlane / 1.4f;
                    profile.nearFogDistance = 25f / profile.fogDistance;
                    nearFogDistance = 25f / profile.fogDistance;

                    profile.proceduralFogDistance = mainCamera.farClipPlane / 1.4f;
                    proceduralFogDistance = mainCamera.farClipPlane / 1.4f;
                    profile.proceduralNearFogDistance = 25f / profile.fogDistance;
                    proceduralNearFogDistance = 25f / profile.fogDistance;
                }
            }
        }

        /// <summary>
        /// Creates a new time of day profile object for you to uses
        /// </summary>
        /// <param name="oldProfile"></param>
        private void CreateNewTimeOfDayProfile(AmbientSkiesTimeOfDayProfile oldProfile)
        {
            AmbientSkiesTimeOfDayProfile asset = ScriptableObject.CreateInstance<AmbientSkiesTimeOfDayProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/Time Of Day Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            if (oldProfile != null)
            {
                EditorUtility.SetDirty(asset);
                EditorUtility.SetDirty(m_profiles);

                asset.m_currentTime = oldProfile.m_currentTime;
                asset.m_day = oldProfile.m_day;
                asset.m_dayColor = oldProfile.m_dayColor;
                asset.m_dayFogColor = oldProfile.m_dayFogColor;
                asset.m_dayFogDensity = oldProfile.m_dayFogDensity;
                asset.m_dayFogDistance = oldProfile.m_dayFogDistance;
                asset.m_dayLengthInSeconds = oldProfile.m_dayLengthInSeconds;
                asset.m_daySunGradientColor = oldProfile.m_daySunGradientColor;
                asset.m_daySunIntensity = oldProfile.m_daySunIntensity;
                asset.m_dayTempature = oldProfile.m_dayTempature;
                asset.m_debugMode = oldProfile.m_debugMode;
                asset.m_environmentSeason = oldProfile.m_environmentSeason;
                asset.m_fogMode = oldProfile.m_fogMode;
                asset.m_gIUpdateIntervalInSeconds = oldProfile.m_gIUpdateIntervalInSeconds;
                asset.m_incrementDownKey = oldProfile.m_incrementDownKey;
                asset.m_incrementUpKey = oldProfile.m_incrementUpKey;
                asset.m_lightAnisotropy = oldProfile.m_lightAnisotropy;
                asset.m_lightDepthExtent = oldProfile.m_lightDepthExtent;
                asset.m_lightProbeDimmer = oldProfile.m_lightProbeDimmer;
                asset.m_month = oldProfile.m_month;
                asset.m_nightColor = oldProfile.m_nightColor;
                asset.m_nightFogColor = oldProfile.m_nightFogColor;
                asset.m_nightFogDensity = oldProfile.m_nightFogDensity;
                asset.m_nightFogDistance = oldProfile.m_nightFogDistance;
                asset.m_nightSunGradientColor = oldProfile.m_nightSunGradientColor;
                asset.m_nightSunIntensity = oldProfile.m_nightSunIntensity;
                asset.m_nightTempature = oldProfile.m_nightTempature;
                asset.m_pauseTime = oldProfile.m_pauseTime;
                asset.m_pauseTimeKey = oldProfile.m_pauseTimeKey;
                asset.m_realtimeGIUpdate = oldProfile.m_realtimeGIUpdate;
                asset.m_renderPipeline = oldProfile.m_renderPipeline;
                asset.m_rotateSunLeftKey = oldProfile.m_rotateSunLeftKey;
                asset.m_rotateSunRightKey = oldProfile.m_rotateSunRightKey;
                asset.m_skyboxMaterial = oldProfile.m_skyboxMaterial;
                asset.m_skyExposure = oldProfile.m_skyExposure;
                asset.m_startFogDistance = oldProfile.m_startFogDistance;
                asset.m_sunRotation = oldProfile.m_sunRotation;
                asset.m_sunRotationAmount = oldProfile.m_sunRotationAmount;
                asset.m_sunSize = oldProfile.m_sunSize;
                asset.m_syncPostFXToTimeOfDay = oldProfile.m_syncPostFXToTimeOfDay;
                asset.m_timeOfDayController = oldProfile.m_timeOfDayController;
                asset.m_timeOfDayHour = oldProfile.m_timeOfDayHour;
                asset.m_timeOfDayMinutes = oldProfile.m_timeOfDayMinutes;
                asset.m_timeOfDaySeconds = oldProfile.m_timeOfDaySeconds;
                asset.m_timeToAddOrRemove = oldProfile.m_timeToAddOrRemove;
                asset.m_year = oldProfile.m_year;

                AssetDatabase.SaveAssets();

                if (EditorUtility.DisplayDialog("New Profile Created!", "Would you like to apply your new profile to Ambient Skies?", "Yes", "No"))
                {
                    timeOfDayProfile = asset;
                    m_profiles.timeOfDayProfile = asset;
                }
            }
        }

        /// <summary>
        /// Gets all variables from Ambient Skies and applies them
        /// </summary>
        /// <param name="getAllSettings"></param>
        private void GetAndApplyAllValuesFromAmbientSkies(bool getAllSettings)
        {
            //If true proceed
            if (getAllSettings)
            {
                //Skybox tab variables
                #region Skybox

                //Global Settings
                systemtype = m_profiles.systemTypes;

                //Target Platform
                targetPlatform = m_profiles.targetPlatform;

                //Skies Settings  
                useSkies = m_selectedSkyboxProfile.useSkies;

                //Main Settings
                skyType = m_profiles.skyType;
                skyTypeNonHD = m_profiles.skyTypeNonHD;
                fogType = m_selectedSkyboxProfile.fogType;
                ambientMode = m_selectedSkyboxProfile.ambientMode;
                useTimeOfDay = m_profiles.useTimeOfDay;

                //Time Of Day Settings
                pauseTimeKey = m_profiles.pauseTimeKey;
                incrementUpKey = m_profiles.incrementUpKey;
                incrementDownKey = m_profiles.incrementDownKey;
                timeToAddOrRemove = m_profiles.timeToAddOrRemove;
                rotateSunLeftKey = m_profiles.rotateSunLeftKey;
                rotateSunRightKey = m_profiles.rotateSunRightKey;
                sunRotationAmount = m_profiles.sunRotationAmount;
                pauseTime = m_profiles.pauseTime;
                currentTimeOfDay = m_profiles.currentTimeOfDay;
                timeOfDaySkyboxRotation = m_profiles.skyboxRotation;

                syncPostProcessing = m_profiles.syncPostProcessing;
                realtimeGIUpdate = m_profiles.realtimeGIUpdate;
                gIUpdateInterval = m_profiles.gIUpdateInterval;
                dayLengthInSeconds = m_profiles.dayLengthInSeconds;
                dayDate = m_profiles.dayDate;
                monthDate = m_profiles.monthDate;
                yearDate = m_profiles.yearDate;

                //Skybox Settings
                newSkyboxSelection = m_selectedSkyboxProfileIndex;
                skyboxTint = m_selectedSkyboxProfile.skyboxTint;
                skyboxExposure = m_selectedSkyboxProfile.skyboxExposure;
                skyboxRotation = m_selectedSkyboxProfile.skyboxRotation;
                skyboxPitch = m_selectedSkyboxProfile.skyboxPitch;
                customSkybox = m_selectedSkyboxProfile.customSkybox;
                isProceduralSkybox = m_selectedSkyboxProfile.isProceduralSkybox;
                proceduralSunSize = m_selectedSkyboxProfile.proceduralSunSize;
                proceduralSunSizeConvergence = m_selectedSkyboxProfile.proceduralSunSizeConvergence;
                proceduralAtmosphereThickness = m_selectedSkyboxProfile.proceduralAtmosphereThickness;
                proceduralGroundColor = m_selectedSkyboxProfile.proceduralGroundColor;
                proceduralSkyTint = m_selectedSkyboxProfile.proceduralSkyTint;
                proceduralSkyExposure = m_selectedSkyboxProfile.proceduralSkyExposure;
                proceduralSkyboxRotation = m_selectedSkyboxProfile.proceduralSkyboxRotation;
                proceduralSkyboxPitch = m_selectedSkyboxProfile.proceduralSkyboxPitch;
                includeSunInBaking = m_selectedSkyboxProfile.includeSunInBaking;

                //Fog Settings
                fogColor = m_selectedSkyboxProfile.fogColor;
                fogDistance = m_selectedSkyboxProfile.fogDistance;
                nearFogDistance = m_selectedSkyboxProfile.nearFogDistance;
                fogDensity = m_selectedSkyboxProfile.fogDensity;
                proceduralFogColor = m_selectedSkyboxProfile.proceduralFogColor;
                proceduralFogDistance = m_selectedSkyboxProfile.proceduralFogDistance;
                proceduralNearFogDistance = m_selectedSkyboxProfile.proceduralNearFogDistance;
                proceduralFogDensity = m_selectedSkyboxProfile.proceduralFogDensity;

                //Ambient Settings
                skyColor = m_selectedSkyboxProfile.skyColor;
                equatorColor = m_selectedSkyboxProfile.equatorColor;
                groundColor = m_selectedSkyboxProfile.groundColor;
                skyboxGroundIntensity = m_selectedSkyboxProfile.skyboxGroundIntensity;

                //Sun Settings
                sunColor = m_selectedSkyboxProfile.sunColor;
                sunIntensity = m_selectedSkyboxProfile.sunIntensity;
                proceduralSunColor = m_selectedSkyboxProfile.proceduralSunColor;
                proceduralSunIntensity = m_selectedSkyboxProfile.proceduralSunIntensity;

                //Shadow Settings
                shadowDistance = m_selectedSkyboxProfile.shadowDistance;
                shadowmaskMode = m_selectedSkyboxProfile.shadowmaskMode;
                shadowType = m_selectedSkyboxProfile.shadowType;
                shadowResolution = m_selectedSkyboxProfile.shadowResolution;
                shadowProjection = m_selectedSkyboxProfile.shadowProjection;
                shadowCascade = m_selectedSkyboxProfile.cascadeCount;

                //VSync Settings
                vSyncMode = m_profiles.vSyncMode;

                //Horizon Settings
                horizonEnabled = m_selectedSkyboxProfile.horizonSkyEnabled;
                horizonScattering = m_selectedSkyboxProfile.horizonScattering;
                horizonFogDensity = m_selectedSkyboxProfile.horizonFogDensity;
                horizonFalloff = m_selectedSkyboxProfile.horizonFalloff;
                horizonBlend = m_selectedSkyboxProfile.horizonBlend;
                horizonScale = m_selectedSkyboxProfile.horizonSize;
                followPlayer = m_selectedSkyboxProfile.followPlayer;
                horizonUpdateTime = m_selectedSkyboxProfile.horizonUpdateTime;
                horizonPosition = m_selectedSkyboxProfile.horizonPosition;
                enableSunDisk = m_selectedSkyboxProfile.enableSunDisk;

#if HDPipeline
                //HD Pipeline Settings
                //Gradient Sky
                topSkyColor = m_selectedSkyboxProfile.topColor;
                middleSkyColor = m_selectedSkyboxProfile.middleColor;
                bottomSkyColor = m_selectedSkyboxProfile.bottomColor;
                gradientDiffusion = m_selectedSkyboxProfile.gradientDiffusion;

                //Procedural Sky
                sunSize = m_selectedSkyboxProfile.sunSize;
                sunConvergence = m_selectedSkyboxProfile.sunConvergence;
                atmosphereThickness = m_selectedSkyboxProfile.atmosphereThickness;
                skyTint = m_selectedSkyboxProfile.skyTint;
                skyExposure = m_selectedSkyboxProfile.skyExposure;
                skyMultiplier = m_selectedSkyboxProfile.skyMultiplier;

                //Volumetric Fog
                baseFogDistance = m_selectedSkyboxProfile.volumetricBaseFogDistance;
                baseFogHeight = m_selectedSkyboxProfile.volumetricBaseFogHeight;
                meanFogHeight = m_selectedSkyboxProfile.volumetricMeanHeight;
                globalAnisotropy = m_selectedSkyboxProfile.volumetricGlobalAnisotropy;
                globalLightProbeDimmer = m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer;

                //Exponential Fog
                exponentialFogDensity = m_selectedSkyboxProfile.exponentialFogDensity;
                exponentialBaseFogHeight = m_selectedSkyboxProfile.exponentialBaseHeight;
                exponentialHeightAttenuation = m_selectedSkyboxProfile.exponentialHeightAttenuation;
                exponentialMaxFogDistance = m_selectedSkyboxProfile.exponentialMaxFogDistance;
                exponentialMipFogNear = m_selectedSkyboxProfile.exponentialMipFogNear;
                exponentialMipFogFar = m_selectedSkyboxProfile.exponentialMipFogFar;
                exponentialMipFogMax = m_selectedSkyboxProfile.exponentialMipFogMaxMip;

                //Linear Fog
                linearFogDensity = m_selectedSkyboxProfile.linearFogDensity;
                linearFogHeightStart = m_selectedSkyboxProfile.linearHeightStart;
                linearFogHeightEnd = m_selectedSkyboxProfile.linearHeightEnd;
                linearFogMaxDistance = m_selectedSkyboxProfile.linearMaxFogDistance;
                linearMipFogNear = m_selectedSkyboxProfile.linearMipFogNear;
                linearMipFogFar = m_selectedSkyboxProfile.linearMipFogFar;
                linearMipFogMax = m_selectedSkyboxProfile.linearMipFogMaxMip;

                //Volumetric Light Controller
                depthExtent = m_selectedSkyboxProfile.volumetricDistanceRange;
                sliceDistribution = m_selectedSkyboxProfile.volumetricSliceDistributionUniformity;

                //Density Fog Volume
                useDensityFogVolume = m_selectedSkyboxProfile.useFogDensityVolume;
                singleScatteringAlbedo = m_selectedSkyboxProfile.singleScatteringAlbedo;
                densityVolumeFogDistance = m_selectedSkyboxProfile.densityVolumeFogDistance;
                fogDensityMaskTexture = m_selectedSkyboxProfile.fogDensityMaskTexture;
                densityMaskTiling = m_selectedSkyboxProfile.densityMaskTiling;

                //HD Shadows
                hDShadowQuality = m_selectedSkyboxProfile.shadowQuality;
                split1 = m_selectedSkyboxProfile.cascadeSplit1;
                split2 = m_selectedSkyboxProfile.cascadeSplit2;
                split3 = m_selectedSkyboxProfile.cascadeSplit3;

                //Contact Shadows
                enableContactShadows = m_selectedSkyboxProfile.useContactShadows;
                contactLength = m_selectedSkyboxProfile.contactShadowsLength;
                contactScaleFactor = m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor;
                contactMaxDistance = m_selectedSkyboxProfile.contactShadowsMaxDistance;
                contactFadeDistance = m_selectedSkyboxProfile.contactShadowsFadeDistance;
                contactSampleCount = m_selectedSkyboxProfile.contactShadowsSampleCount;
                contactOpacity = m_selectedSkyboxProfile.contactShadowsOpacity;

                //Micro Shadows
                enableMicroShadows = m_selectedSkyboxProfile.useMicroShadowing;
                microShadowOpacity = m_selectedSkyboxProfile.microShadowOpacity;

                //SS Reflection
                enableSSReflection = m_selectedSkyboxProfile.enableScreenSpaceReflections;
                ssrEdgeFade = m_selectedSkyboxProfile.screenEdgeFadeDistance;
                ssrNumberOfRays = m_selectedSkyboxProfile.maxNumberOfRaySteps;
                ssrObjectThickness = m_selectedSkyboxProfile.objectThickness;
                ssrMinSmoothness = m_selectedSkyboxProfile.minSmoothness;
                ssrSmoothnessFade = m_selectedSkyboxProfile.smoothnessFadeStart;
                ssrReflectSky = m_selectedSkyboxProfile.reflectSky;

                //SS Refract
                enableSSRefraction = m_selectedSkyboxProfile.enableScreenSpaceRefractions;
                ssrWeightDistance = m_selectedSkyboxProfile.screenWeightDistance;

                //Ambient Lighting
                diffuseAmbientIntensity = m_selectedSkyboxProfile.indirectDiffuseIntensity;
                specularAmbientIntensity = m_selectedSkyboxProfile.indirectSpecularIntensity;
#endif

                #endregion

                //Post FX tab variables
                #region Post FX

                //Global systems
                systemtype = m_profiles.systemTypes;

                //Enable Post Fx
                usePostProcess = m_selectedPostProcessingProfile.usePostProcess;

                //Selection
                newPPSelection = m_selectedPostProcessingProfile.profileIndex;

                //Custom profile
#if UNITY_POST_PROCESSING_STACK_V2
                customPostProcessingProfile = m_selectedPostProcessingProfile.customPostProcessingProfile;
#endif

                //HDR Mode
                hDRMode = m_selectedPostProcessingProfile.hDRMode;

                //Anti Aliasing Mode
                antiAliasingMode = m_selectedPostProcessingProfile.antiAliasingMode;

                //Target Platform
                targetPlatform = m_profiles.targetPlatform;

                //AO settings
                aoEnabled = m_selectedPostProcessingProfile.aoEnabled;
                aoAmount = m_selectedPostProcessingProfile.aoAmount;
                aoColor = m_selectedPostProcessingProfile.aoColor;
#if UNITY_POST_PROCESSING_STACK_V2
                ambientOcclusionMode = m_selectedPostProcessingProfile.ambientOcclusionMode;
#endif

                //Exposure settings
                autoExposureEnabled = m_selectedPostProcessingProfile.autoExposureEnabled;
                exposureAmount = m_selectedPostProcessingProfile.exposureAmount;
                exposureMin = m_selectedPostProcessingProfile.exposureMin;
                exposureMax = m_selectedPostProcessingProfile.exposureMax;

                //Bloom settings
                bloomEnabled = m_selectedPostProcessingProfile.bloomEnabled;
                bloomIntensity = m_selectedPostProcessingProfile.bloomAmount;
                bloomThreshold = m_selectedPostProcessingProfile.bloomThreshold;
                lensIntensity = m_selectedPostProcessingProfile.lensIntensity;
                lensTexture = m_selectedPostProcessingProfile.lensTexture;

                //Chromatic Aberration
                chromaticAberrationEnabled = m_selectedPostProcessingProfile.chromaticAberrationEnabled;
                chromaticAberrationIntensity = m_selectedPostProcessingProfile.chromaticAberrationIntensity;

                //Color Grading settings
                colorGradingEnabled = m_selectedPostProcessingProfile.colorGradingEnabled;
#if UNITY_POST_PROCESSING_STACK_V2
                colorGradingMode = m_selectedPostProcessingProfile.colorGradingMode;
#endif
                colorGradingLut = m_selectedPostProcessingProfile.colorGradingLut;
                colorGradingPostExposure = m_selectedPostProcessingProfile.colorGradingPostExposure;
                colorGradingColorFilter = m_selectedPostProcessingProfile.colorGradingColorFilter;
                colorGradingTempature = m_selectedPostProcessingProfile.colorGradingTempature;
                colorGradingTint = m_selectedPostProcessingProfile.colorGradingTint;
                colorGradingSaturation = m_selectedPostProcessingProfile.colorGradingSaturation;
                colorGradingContrast = m_selectedPostProcessingProfile.colorGradingContrast;

                //DOF settings
                depthOfFieldMode = m_selectedPostProcessingProfile.depthOfFieldMode;
                depthOfFieldEnabled = m_selectedPostProcessingProfile.depthOfFieldEnabled;
                depthOfFieldFocusDistance = m_selectedPostProcessingProfile.depthOfFieldFocusDistance;
                depthOfFieldAperture = m_selectedPostProcessingProfile.depthOfFieldAperture;
                depthOfFieldFocalLength = m_selectedPostProcessingProfile.depthOfFieldFocalLength;
                depthOfFieldTrackingType = m_selectedPostProcessingProfile.depthOfFieldTrackingType;
                focusOffset = m_selectedPostProcessingProfile.focusOffset;
                targetLayer = m_selectedPostProcessingProfile.targetLayer;
                maxFocusDistance = m_selectedPostProcessingProfile.maxFocusDistance;
#if UNITY_POST_PROCESSING_STACK_V2
                maxBlurSize = m_selectedPostProcessingProfile.maxBlurSize;
#endif

                //Distortion settings
                distortionEnabled = m_selectedPostProcessingProfile.distortionEnabled;
                distortionIntensity = m_selectedPostProcessingProfile.distortionIntensity;
                distortionScale = m_selectedPostProcessingProfile.distortionScale;

                //Grain settings
                grainEnabled = m_selectedPostProcessingProfile.grainEnabled;
                grainIntensity = m_selectedPostProcessingProfile.grainIntensity;
                grainSize = m_selectedPostProcessingProfile.grainSize;

                //SSR settings
                screenSpaceReflectionsEnabled = m_selectedPostProcessingProfile.screenSpaceReflectionsEnabled;
                maximumIterationCount = m_selectedPostProcessingProfile.maximumIterationCount;
                thickness = m_selectedPostProcessingProfile.thickness;
#if UNITY_POST_PROCESSING_STACK_V2
                screenSpaceReflectionResolution = m_selectedPostProcessingProfile.spaceReflectionResolution;
                screenSpaceReflectionPreset = m_selectedPostProcessingProfile.screenSpaceReflectionPreset;
#endif
                maximumMarchDistance = m_selectedPostProcessingProfile.maximumMarchDistance;
                distanceFade = m_selectedPostProcessingProfile.distanceFade;
                screenSpaceVignette = m_selectedPostProcessingProfile.screenSpaceVignette;

                //Vignette settings
                vignetteEnabled = m_selectedPostProcessingProfile.vignetteEnabled;
                vignetteIntensity = m_selectedPostProcessingProfile.vignetteIntensity;
                vignetteSmoothness = m_selectedPostProcessingProfile.vignetteSmoothness;

                //Motion Blur settings
                motionBlurEnabled = m_selectedPostProcessingProfile.motionBlurEnabled;
                motionShutterAngle = m_selectedPostProcessingProfile.shutterAngle;
                motionSampleCount = m_selectedPostProcessingProfile.sampleCount;

#if Mewlist_Clouds
                //Massive Cloud Settings
                massiveCloudsEnabled = m_selectedPostProcessingProfile.massiveCloudsEnabled;
                cloudProfile = m_selectedPostProcessingProfile.cloudProfile;
                syncGlobalFogColor = m_selectedPostProcessingProfile.syncGlobalFogColor;
                syncBaseFogColor = m_selectedPostProcessingProfile.syncBaseFogColor;
                cloudsFogColor = m_selectedPostProcessingProfile.cloudsFogColor;
                cloudsBaseFogColor = m_selectedPostProcessingProfile.cloudsBaseFogColor;
                cloudIsHDRP = m_selectedPostProcessingProfile.cloudIsHDRP;
#endif

                #endregion

                //Lightmaps tab variables
                #region Lightmaps

                //Global Settings
                systemtype = m_profiles.systemTypes;

                //Target Platform
                targetPlatform = m_profiles.targetPlatform;

                //Local values
                newLightmappingSettings = m_selectedLightingProfile.profileIndex;
                autoLightmapGeneration = m_selectedLightingProfile.autoLightmapGeneration;

                enableLightmapSettings = m_selectedLightingProfile.enableLightmapSettings;

                realtimeGlobalIllumination = m_selectedLightingProfile.realtimeGlobalIllumination;
                bakedGlobalIllumination = m_selectedLightingProfile.bakedGlobalIllumination;
                lightmappingMode = m_selectedLightingProfile.lightmappingMode;
                indirectRelolution = m_selectedLightingProfile.indirectRelolution;
                lightmapResolution = m_selectedLightingProfile.lightmapResolution;
                lightmapPadding = m_selectedLightingProfile.lightmapPadding;
                useHighResolutionLightmapSize = m_selectedLightingProfile.useHighResolutionLightmapSize;
                compressLightmaps = m_selectedLightingProfile.compressLightmaps;
                ambientOcclusion = m_selectedLightingProfile.ambientOcclusion;
                maxDistance = m_selectedLightingProfile.maxDistance;
                indirectContribution = m_selectedLightingProfile.indirectContribution;
                directContribution = m_selectedLightingProfile.directContribution;
                useDirectionalMode = m_selectedLightingProfile.useDirectionalMode;
                lightIndirectIntensity = m_selectedLightingProfile.lightIndirectIntensity;
                lightBoostIntensity = m_selectedLightingProfile.lightBoostIntensity;
                finalGather = m_selectedLightingProfile.finalGather;
                finalGatherRayCount = m_selectedLightingProfile.finalGatherRayCount;
                finalGatherDenoising = m_selectedLightingProfile.finalGatherDenoising;

                #endregion

                //Apply Settings
                if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                }
                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);
                AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(m_selectedSkyboxProfile, m_profiles, m_selectedSkyboxProfileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile");
            }
            else
            {
                //Apply Settings
                if (m_profiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                {
                    SkyboxUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfileIndex, false);
                }
                PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                LightingUtils.SetFromProfileIndex(m_selectedLightingProfile, m_selectedLightingProfileIndex, false);
                AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(m_selectedSkyboxProfile, m_profiles, m_selectedSkyboxProfileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile");
            }
        }

        /// <summary>
        /// Creates the new scene object
        /// </summary>
        private void NewSceneObjectCreation()
        {
            //Created object to resemble a new scene
            GameObject newSceneObject = GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)");
            //Parent object in the scene
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
            //If the object to resemble a new scene is not there
            if (newSceneObject == null)
            {
                //Create it
                newSceneObject = new GameObject("Ambient Skies New Scene Object (Don't Delete Me)");
                //Parent it
                newSceneObject.transform.SetParent(parentObject.transform);
            }
        }

        /// <summary>
        /// This method saves all the settings for the Built-in and LWRP pipelines
        /// </summary>
        private void SaveBuiltInAndLWRPSettings()
        {
            //Marks the profiles as dirty so settings can be saved
            EditorUtility.SetDirty(m_profiles);

            //Created object to resemble a new scene
            GameObject newSceneObject = GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)");
            //Parent object in the scene
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);
            //If the object to resemble a new scene is not there
            if (newSceneObject == null)
            {
                //Create it
                newSceneObject = new GameObject("Ambient Skies New Scene Object (Don't Delete Me)");
                //Parent it
                newSceneObject.transform.SetParent(parentObject.transform);
            }

            //Finds user profile
            m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, "User");
            //Sets the skybox profile settings to User
            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];

            //Get main sun light
            GameObject mainLight = SkyboxUtils.GetMainDirectionalLight();
            if (mainLight != null)
            {
                //Gets light component of main sun light
                Light lightComponent = mainLight.GetComponent<Light>();
                if (lightComponent != null)
                {
                    //Stores the light rotation
                    Vector3 lightRotation = mainLight.transform.rotation.eulerAngles;

                    //Gets the rotation
                    m_selectedSkyboxProfile.skyboxRotation = lightRotation.y;
                    //Gets the pitch
                    m_selectedSkyboxProfile.skyboxPitch = lightRotation.x;
                    //Gets the procedural rotation
                    m_selectedSkyboxProfile.proceduralSkyboxRotation = lightRotation.y;
                    //Gets the procedural pitch
                    m_selectedSkyboxProfile.proceduralSkyboxPitch = lightRotation.x;

                    //Gets the sun intensity
                    m_selectedSkyboxProfile.sunIntensity = lightComponent.intensity;
                    //Gets the procedural sun intensity
                    m_selectedSkyboxProfile.proceduralSunIntensity = lightComponent.intensity;
                    //Gets the sun color
                    m_selectedSkyboxProfile.sunColor = lightComponent.color;
                    //Gets the procedural sun color
                    m_selectedSkyboxProfile.proceduralSunColor = lightComponent.color;    
                }        
            }

            //Gets fog color
            m_selectedSkyboxProfile.fogColor = RenderSettings.fogColor;
            //Gets procedural fog color
            m_selectedSkyboxProfile.proceduralFogColor = RenderSettings.fogColor;
            //Gets fog density
            m_selectedSkyboxProfile.fogDensity = RenderSettings.fogDensity;
            //Gets procedural fog density
            m_selectedSkyboxProfile.proceduralFogDensity = RenderSettings.fogDensity;
            //Gets fog end distance
            m_selectedSkyboxProfile.fogDistance = RenderSettings.fogEndDistance;
            //Gets procedural fog end distance
            m_selectedSkyboxProfile.proceduralFogDistance = RenderSettings.fogEndDistance;
            //Gets start fog distance
            m_selectedSkyboxProfile.nearFogDistance = RenderSettings.fogStartDistance;
            //Gets procedural start fog distance
            m_selectedSkyboxProfile.proceduralNearFogDistance = RenderSettings.fogStartDistance;
            //If fog is not enabled
            if (!RenderSettings.fog)
            {
                //Gets fog enabled to false
                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.None;
            }
            //If fog mode equals Exponential
            else if (RenderSettings.fogMode == FogMode.Exponential)
            {
                //Gets fog to Exponential
                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Exponential;
            }
            //If fog mode equal Exponential Squared
            else if (RenderSettings.fogMode == FogMode.ExponentialSquared)
            {
                //Get fog to Exponential Squared
                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.ExponentialSquared;
            }
            //If fog mode equals Linear
            else
            {
                //Gets fog to Linear
                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Linear;
            }

            //Gets the skybox ambient intensity
            m_selectedSkyboxProfile.skyboxGroundIntensity = RenderSettings.ambientIntensity;
            //Gets the sky ambeint color
            m_selectedSkyboxProfile.skyColor = RenderSettings.ambientSkyColor;
            //Gets the ground ambient color
            m_selectedSkyboxProfile.groundColor = RenderSettings.ambientGroundColor;
            //Gets the equator ambient color
            m_selectedSkyboxProfile.equatorColor = RenderSettings.ambientEquatorColor;

            //If the ambient mode is Flat
            if (RenderSettings.ambientMode == AmbientMode.Flat)
            {
                //Sets the ambient mode to Color
                m_selectedSkyboxProfile.ambientMode = AmbientSkiesConsts.AmbientMode.Color;
            }
            //If the ambient mode is Trilight
            else if (RenderSettings.ambientMode == AmbientMode.Trilight)
            {
                //Sets the ambient mode to Gradient
                m_selectedSkyboxProfile.ambientMode = AmbientSkiesConsts.AmbientMode.Gradient;
            }
            //If the ambient mode is Skybox
            else if (RenderSettings.ambientMode == AmbientMode.Skybox)
            {
                //Sets the ambient mode to Skybox
                m_selectedSkyboxProfile.ambientMode = AmbientSkiesConsts.AmbientMode.Skybox;
            }

            //If skybox is not empty
            if (RenderSettings.skybox != null)
            {
                //Stores the local material
                Material skyboxMaterial = RenderSettings.skybox;
                //If the skybox shader equals Procedural
                if (skyboxMaterial.shader == Shader.Find("Skybox/Procedural"))
                {
                    //If sun disk is enabled
                    if (skyboxMaterial.GetFloat("_SunDisk") == 2f)
                    {
                        //Get sun disk and set it as true
                        m_selectedSkyboxProfile.enableSunDisk = true;
                    }
                    //If sun disk is not enabled
                    else
                    {
                        //Get sun disk and set it as false
                        m_selectedSkyboxProfile.enableSunDisk = false;
                    }

                    //Sets the custom skybox as procedural sky
                    m_selectedSkyboxProfile.isProceduralSkybox = true;
                    //Gets the sun size
                    m_selectedSkyboxProfile.proceduralSunSize = skyboxMaterial.GetFloat("_SunSize");
                    //Gets the sun size convergence
                    m_selectedSkyboxProfile.proceduralSunSizeConvergence = skyboxMaterial.GetFloat("_SunSizeConvergence");
                    //Gets the atmosphere thickness
                    m_selectedSkyboxProfile.proceduralAtmosphereThickness = skyboxMaterial.GetFloat("_AtmosphereThickness");
                    //Gets the ground color
                    m_selectedSkyboxProfile.proceduralGroundColor = skyboxMaterial.GetColor("_GroundColor");
                    //Gets the skybox tint
                    m_selectedSkyboxProfile.proceduralSkyTint = skyboxMaterial.GetColor("_SkyTint");
                    //Gets the skybox exposure
                    m_selectedSkyboxProfile.proceduralSkyExposure = skyboxMaterial.GetFloat("_Exposure");
                }
                //If the skybox shader equals Cubemap
                else if (skyboxMaterial.shader == Shader.Find("Skybox/Cubemap"))
                {
                    //Sets the custom skybox not as procedural sky
                    m_selectedSkyboxProfile.isProceduralSkybox = false;
                    //Gets Skybox cubemap texture
                    m_selectedSkyboxProfile.customSkybox = skyboxMaterial.GetTexture("_Tex") as Cubemap;
                    //Gets the skybox tint
                    m_selectedSkyboxProfile.skyboxTint = skyboxMaterial.GetColor("_Tint");
                    //Gets the skybox exposure
                    m_selectedSkyboxProfile.skyboxExposure = skyboxMaterial.GetFloat("_Exposure");
                    //Sets the hdri skybox rotation
                    skyboxMaterial.SetFloat("_Rotation", skyboxRotation);
                }
            }

            //Defaults the system type to third party to stop changing settings unless user enabled system
            //m_selectedSkyboxProfile.systemTypes = AmbientSkiesConsts.SystemTypes.ThirdParty;
            //Gets the shadow distance
            m_selectedSkyboxProfile.shadowDistance = QualitySettings.shadowDistance;
            //Gets the shadow mask mode
            m_selectedSkyboxProfile.shadowmaskMode = QualitySettings.shadowmaskMode;
            //Gets the shadow projection
            m_selectedSkyboxProfile.shadowProjection = QualitySettings.shadowProjection;
            //Getws the shadow resolution
            m_selectedSkyboxProfile.shadowResolution = QualitySettings.shadowResolution;

            //If vsync mode is on DontSync
            if (QualitySettings.vSyncCount == 0)
            {
                //Sets the vsync mode to DontSync
                m_profiles.vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;
            }
            //If vsync mode is on EveryVBlank
            else if (QualitySettings.vSyncCount == 1)
            {
                //Sets the vsync mode to EveryVBlank
                m_profiles.vSyncMode = AmbientSkiesConsts.VSyncMode.EveryVBlank;
            }
            //If vsync mode is on EverySecondVBlank
            else
            {
                //Sets the vsync mode to EverySecondVBlank
                m_profiles.vSyncMode = AmbientSkiesConsts.VSyncMode.EverySecondVBlank;
            }

#if UNITY_POST_PROCESSING_STACK_V2
            GameObject postProcessingObject = GameObject.Find("Global Post Processing");
            PostProcessVolume processVol;
            if (postProcessingObject != null)
            {
                processVol = postProcessingObject.GetComponent<PostProcessVolume>();
            }
            else
            {
                processVol = FindObjectOfType<PostProcessVolume>();
            }

            if (processVol != null)
            {
                //Finds user profile
                m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "User");
                //Sets the post fx profile settings to User
                m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];

                PostProcessProfile profileFX = processVol.sharedProfile;
                if (profileFX != null)
                {
                    usePostProcess = true;
                    m_selectedPostProcessingProfile.usePostProcess = true;
                    m_selectedPostProcessingProfile.assetName = profileFX.name;
                    m_selectedPostProcessingProfile.customPostProcessingProfile = profileFX;
                    //Update post processing
                    PostProcessingUtils.SetFromProfileIndex(m_profiles, m_selectedSkyboxProfile, m_selectedPostProcessingProfileIndex, false);
                }
                else
                {
                    m_selectedPostProcessingProfile.usePostProcess = false;
                }

                DestroyImmediate(processVol.gameObject);
            }
            else
            {
                m_selectedPostProcessingProfileIndex = PostProcessingUtils.GetProfileIndexFromProfileName(m_profiles, "Alpine");
                m_selectedPostProcessingProfile = m_profiles.m_ppProfiles[m_selectedPostProcessingProfileIndex];
                m_selectedPostProcessingProfile.usePostProcess = false;
            }
#endif
        }

        /// <summary>
        /// This method saves all the settings for HDRP pipeline
        /// </summary>
        private void SaveHDRPSettings()
        {
#if HDPipeline
            //Marks the profiles as dirty so settings can be saved
            EditorUtility.SetDirty(m_profiles);

            NewSceneObjectCreation();

            //Finds user profile
            m_selectedSkyboxProfileIndex = SkyboxUtils.GetProfileIndexFromProfileName(m_profiles, "User");
            //Sets the skybox profile settings to User
            m_selectedSkyboxProfile = m_profiles.m_skyProfiles[m_selectedSkyboxProfileIndex];

            //Get main sun light
            GameObject mainLight = SkyboxUtils.GetMainDirectionalLight();
            if (mainLight != null)
            {
                //Gets light component of main sun light
                Light lightComponent = mainLight.GetComponent<Light>();
                if (lightComponent != null)
                {
                    //Stores the light rotation
                    Vector3 lightRotation = mainLight.transform.rotation.eulerAngles;

                    //Gets the rotation
                    m_selectedSkyboxProfile.skyboxRotation = lightRotation.y;
                    //Gets the pitch
                    m_selectedSkyboxProfile.skyboxPitch = lightRotation.x;
                    //Gets the procedural rotation
                    m_selectedSkyboxProfile.proceduralSkyboxRotation = lightRotation.y;
                    //Gets the procedural pitch
                    m_selectedSkyboxProfile.proceduralSkyboxPitch = lightRotation.x;

                    //Gets the sun intensity
                    m_selectedSkyboxProfile.sunIntensity = lightComponent.intensity;
                    //Gets the procedural sun intensity
                    m_selectedSkyboxProfile.proceduralSunIntensity = lightComponent.intensity;
                    //Gets the sun color
                    m_selectedSkyboxProfile.sunColor = lightComponent.color;
                    //Gets the procedural sun color
                    m_selectedSkyboxProfile.proceduralSunColor = lightComponent.color;
                }

            }

            Volume volumeObject = FindObjectOfType<Volume>();

            //Finds the volume profine for the environment
            VolumeProfile volumeProfile = volumeObject.sharedProfile;
            if (volumeProfile != null)
            {
                //If the profile has the visual environment added to it
                if (volumeProfile.Has<VisualEnvironment>())
                {
                    //Local visual environment component
                    VisualEnvironment visual;
                    if (volumeProfile.TryGet(out visual))
                    {
                        //If it's = to Gradient
                        if (visual.skyType == 1)
                        {
                            m_profiles.skyType = AmbientSkiesConsts.VolumeSkyType.Gradient;
                        }
                        //If it's = to HDRI sky
                        else if (visual.skyType == 2)
                        {
                            m_profiles.skyType = AmbientSkiesConsts.VolumeSkyType.HDRISky;
                        }
                        //If it's = to Procedural sky
                        else if (visual.skyType == 3)
                        {
                            m_profiles.skyType = AmbientSkiesConsts.VolumeSkyType.ProceduralSky;
                        }

                        //Gets the current fog type
                        switch (visual.fogType.value)
                        {
                            //If fog = none
                            case FogType.None:
                                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.None;
                                break;
                            //If fog = exponential
                            case FogType.Exponential:
                                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Exponential;
                                break;
                            //If fog = linear
                            case FogType.Linear:
                                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Linear;
                                break;
                            //If fog = volumetric
                            case FogType.Volumetric:
                                m_selectedSkyboxProfile.fogType = AmbientSkiesConsts.VolumeFogType.Volumetric;
                                break;
                        }
                    }
                }

                //If the profile has a gradient sky added to it
                if (volumeProfile.Has<GradientSky>())
                {
                    GradientSky gradientSky;
                    if (volumeProfile.TryGet(out gradientSky))
                    {
                        m_selectedSkyboxProfile.topColor = gradientSky.top.value;
                        m_selectedSkyboxProfile.middleColor = gradientSky.middle.value;
                        m_selectedSkyboxProfile.bottomColor = gradientSky.bottom.value;
                        m_selectedSkyboxProfile.gradientDiffusion = gradientSky.gradientDiffusion.value;
                    }
                }

                //If the profile has a procedural sky to it
                if (volumeProfile.Has<ProceduralSky>())
                {
                    ProceduralSky procedural;
                    if (volumeProfile.TryGet(out procedural))
                    {
                        m_selectedSkyboxProfile.enableSunDisk = procedural.enableSunDisk.value;
                        m_selectedSkyboxProfile.proceduralSunSize = procedural.sunSize.value;
                        m_selectedSkyboxProfile.proceduralSunSizeConvergence = procedural.sunSizeConvergence.value;
                        m_selectedSkyboxProfile.proceduralAtmosphereThickness = procedural.atmosphereThickness.value;
                        m_selectedSkyboxProfile.proceduralSkyTint = procedural.skyTint.value;
                        m_selectedSkyboxProfile.proceduralGroundColor = procedural.groundColor.value;
                        m_selectedSkyboxProfile.proceduralSkyExposure = procedural.exposure.value;
                        m_selectedSkyboxProfile.skyMultiplier = procedural.multiplier.value;
                    }              
                }

                //If the profile has a HDRI sky to it
                if (volumeProfile.Has<HDRISky>())
                {
                    HDRISky hDRISky;
                    if (volumeProfile.TryGet(out hDRISky))
                    {
                        m_selectedSkyboxProfile.customSkybox = hDRISky.hdriSky.value;
                        m_selectedSkyboxProfile.skyboxExposure = hDRISky.exposure.value;
                        m_selectedSkyboxProfile.skyMultiplier = hDRISky.multiplier.value;
                        m_selectedSkyboxProfile.skyboxRotation = hDRISky.rotation.value;
                    }
                }

                //If the profile has a expoential fog added to it
                if (volumeProfile.Has<ExponentialFog>())
                {
                    ExponentialFog exponentialFog;
                    if (volumeProfile.TryGet(out exponentialFog))
                    {
                        m_selectedSkyboxProfile.exponentialFogDensity = exponentialFog.density.value;
                        m_selectedSkyboxProfile.proceduralFogDistance = exponentialFog.fogDistance.value;
                        m_selectedSkyboxProfile.exponentialHeightAttenuation = exponentialFog.fogHeightAttenuation.value;
                        m_selectedSkyboxProfile.exponentialBaseHeight = exponentialFog.fogBaseHeight.value;
                        m_selectedSkyboxProfile.exponentialMaxFogDistance = exponentialFog.maxFogDistance.value;
                        m_selectedSkyboxProfile.exponentialMipFogNear = exponentialFog.mipFogNear.value;
                        m_selectedSkyboxProfile.exponentialMipFogFar = exponentialFog.mipFogFar.value;
                        m_selectedSkyboxProfile.exponentialMipFogMaxMip = exponentialFog.mipFogMaxMip.value;
                    }
                }

                //If the profile has linear fog added to it
                if (volumeProfile.Has<LinearFog>())
                {
                    LinearFog linearFog;
                    if (volumeProfile.TryGet(out linearFog))
                    {
                        m_selectedSkyboxProfile.linearFogDensity = linearFog.density.value;
                        m_selectedSkyboxProfile.linearHeightStart = linearFog.fogStart.value;
                        m_selectedSkyboxProfile.linearHeightEnd = linearFog.fogEnd.value;
                        m_selectedSkyboxProfile.linearHeightStart = linearFog.fogHeightStart.value;
                        m_selectedSkyboxProfile.linearHeightEnd = linearFog.fogHeightEnd.value;
                        m_selectedSkyboxProfile.linearMaxFogDistance = linearFog.maxFogDistance.value;
                        m_selectedSkyboxProfile.linearMipFogNear = linearFog.mipFogNear.value;
                        m_selectedSkyboxProfile.linearMipFogFar = linearFog.mipFogFar.value;
                        m_selectedSkyboxProfile.linearMipFogMaxMip = linearFog.mipFogMaxMip.value;
                    }
                }

                //If the profile has volumetric fog added to it
                if (volumeProfile.Has<VolumetricFog>())
                {
                    VolumetricFog volumetricFog;
                    if (volumeProfile.TryGet(out volumetricFog))
                    {
                        m_selectedSkyboxProfile.singleScatteringAlbedo = volumetricFog.albedo.value;
                        m_selectedSkyboxProfile.volumetricBaseFogDistance = volumetricFog.meanFreePath.value;
                        m_selectedSkyboxProfile.volumetricBaseFogHeight = volumetricFog.baseHeight.value;
                        m_selectedSkyboxProfile.volumetricMeanHeight = volumetricFog.meanHeight.value;
                        m_selectedSkyboxProfile.volumetricGlobalAnisotropy = volumetricFog.globalLightProbeDimmer.value;
                        m_selectedSkyboxProfile.volumetricGlobalLightProbeDimmer = volumetricFog.globalLightProbeDimmer.value;
                        m_selectedSkyboxProfile.volumetricMaxFogDistance = volumetricFog.maxFogDistance.value;
                        m_selectedSkyboxProfile.volumetricMipFogNear = volumetricFog.mipFogNear.value;
                        m_selectedSkyboxProfile.volumetricMipFogFar = volumetricFog.mipFogFar.value;
                        m_selectedSkyboxProfile.volumetricMipFogMaxMip = volumetricFog.mipFogMaxMip.value;
                    }
                }

                //If the profile has HD Shadow Settings added to it
                if (volumeProfile.Has<HDShadowSettings>())
                {
                    HDShadowSettings shadowSettings;
                    if (volumeProfile.TryGet(out shadowSettings))
                    {
                        m_selectedSkyboxProfile.shadowDistance = shadowSettings.maxShadowDistance.value;
                        
                        if (shadowSettings.cascadeShadowSplitCount.value == 0)
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount1;
                        }
                        else if (shadowSettings.cascadeShadowSplitCount.value == 1)
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount2;
                        }
                        else if (shadowSettings.cascadeShadowSplitCount.value == 2)
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount3;
                        }
                        else
                        {
                            m_selectedSkyboxProfile.cascadeCount = AmbientSkiesConsts.ShadowCascade.CascadeCount4;
                        }

                        m_selectedSkyboxProfile.cascadeSplit1 = shadowSettings.cascadeShadowSplit0.value;
                        m_selectedSkyboxProfile.cascadeSplit2 = shadowSettings.cascadeShadowSplit1.value;
                        m_selectedSkyboxProfile.cascadeSplit3 = shadowSettings.cascadeShadowSplit2.value;
                    }
                }

                //If the profile has Contact Shadow added to it
                if (volumeProfile.Has<ContactShadows>())
                {
                    ContactShadows contact;
                    if (volumeProfile.TryGet(out contact))
                    {
                        m_selectedSkyboxProfile.useContactShadows = contact.enable.value;
                        m_selectedSkyboxProfile.contactShadowsLength = contact.length.value;
                        m_selectedSkyboxProfile.contactShadowsDistanceScaleFactor = contact.distanceScaleFactor.value;
                        m_selectedSkyboxProfile.contactShadowsMaxDistance = contact.maxDistance.value;
                        m_selectedSkyboxProfile.contactShadowsFadeDistance = contact.fadeDistance.value;
                        m_selectedSkyboxProfile.contactShadowsSampleCount = contact.sampleCount.value;
                        m_selectedSkyboxProfile.contactShadowsOpacity = contact.opacity.value;
                    }
                }

                //If the profile has Micro Shadow added to it
                if (volumeProfile.Has<MicroShadowing>())
                {
                    MicroShadowing micro;
                    if (volumeProfile.TryGet(out micro))
                    {
                        m_selectedSkyboxProfile.useMicroShadowing = micro.enable.value;
                        m_selectedSkyboxProfile.microShadowOpacity = micro.opacity.value;
                    }
                }

                //If the profile has Indirect Lighting Controller added to it
                if (volumeProfile.Has<IndirectLightingController>())
                {
                    IndirectLightingController lightingController;
                    if (volumeProfile.TryGet(out lightingController))
                    {
                        m_selectedSkyboxProfile.indirectDiffuseIntensity = lightingController.indirectDiffuseIntensity.value;
                        m_selectedSkyboxProfile.indirectSpecularIntensity = lightingController.indirectSpecularIntensity.value;
                    }
                }

                //If the profile has Micro Shadow added to it
                if (volumeProfile.Has<VolumetricLightingController>())
                {
                    VolumetricLightingController volumetricLighting;
                    if (volumeProfile.TryGet(out volumetricLighting))
                    {
                        m_selectedSkyboxProfile.volumetricDistanceRange = volumetricLighting.depthExtent.value;
                        m_selectedSkyboxProfile.volumetricSliceDistributionUniformity = volumetricLighting.sliceDistributionUniformity.value;
                    }
                }

                //If the profile has Screen Space Reflection added to it
                if (volumeProfile.Has<ScreenSpaceReflection>())
                {
                    ScreenSpaceReflection screenSpace;
                    if (volumeProfile.TryGet(out screenSpace))
                    {
                        m_selectedSkyboxProfile.enableScreenSpaceReflections = screenSpace.active;
                        m_selectedSkyboxProfile.screenEdgeFadeDistance = screenSpace.screenFadeDistance.value;
                        m_selectedSkyboxProfile.maxNumberOfRaySteps = screenSpace.rayMaxIterations.value;
                        m_selectedSkyboxProfile.objectThickness = screenSpace.depthBufferThickness.value;
                        m_selectedSkyboxProfile.minSmoothness = screenSpace.minSmoothness.value;
                        m_selectedSkyboxProfile.smoothnessFadeStart = screenSpace.smoothnessFadeStart.value;
                        m_selectedSkyboxProfile.reflectSky = screenSpace.reflectSky.value;
                    }
                }

                //If the profile has Screen Space Refraction added to it
                if (volumeProfile.Has<ScreenSpaceRefraction>())
                {
                    ScreenSpaceRefraction spaceRefraction;
                    if (volumeProfile.TryGet(out spaceRefraction))
                    {
                        m_selectedSkyboxProfile.enableScreenSpaceRefractions = spaceRefraction.active;
                        m_selectedSkyboxProfile.screenWeightDistance = spaceRefraction.screenFadeDistance.value;
                    }
                }
            }

            //Gets the shadow mask mode
            m_selectedSkyboxProfile.shadowmaskMode = QualitySettings.shadowmaskMode;

            //If vsync mode is on DontSync
            if (QualitySettings.vSyncCount == 0)
            {
                //Sets the vsync mode to DontSync
                m_profiles.vSyncMode = AmbientSkiesConsts.VSyncMode.DontSync;
            }
            //If vsync mode is on EveryVBlank
            else if (QualitySettings.vSyncCount == 1)
            {
                //Sets the vsync mode to EveryVBlank
                m_profiles.vSyncMode = AmbientSkiesConsts.VSyncMode.EveryVBlank;
            }
            //If vsync mode is on EverySecondVBlank
            else
            {
                //Sets the vsync mode to EverySecondVBlank
                m_profiles.vSyncMode = AmbientSkiesConsts.VSyncMode.EverySecondVBlank;
            }
#endif
        }

#if AMBIENT_SKIES_CREATION && HDPipeline

        /// <summary>
        /// Applies all the settings to the post processing profile in HDRP
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="processProfile"></param>
        /// <param name="renameProfile"></param>
        /// <param name="newProfileName"></param>
        private void ConvertPostProcessingToHDRP(VolumeProfile profile, PostProcessProfile processProfile, bool renameProfile, string newProfileName)
        {
            EditorUtility.SetDirty(profile);
            VolumeComponent target;

            //If the profile is not null
            if (profile != null)
            {
                //AO
                UnityEngine.Experimental.Rendering.HDPipeline.AmbientOcclusion newAO = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.AmbientOcclusion>();
                if (profile.TryGet(out newAO))
                {
                    target = newAO;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.AmbientOcclusion oldAO;
                    if (processProfile.TryGetSettings(out oldAO))
                    {
                        newAO.active = oldAO.active;
                        newAO.noiseFilterTolerance.value = oldAO.noiseFilterTolerance.value;
                        newAO.blurTolerance.value = oldAO.blurTolerance.value;
                        newAO.directLightingStrength.value = oldAO.directLightingStrength.value;
                        newAO.intensity.value = oldAO.intensity.value;
                        newAO.thicknessModifier.value = oldAO.thicknessModifier.value;
                        newAO.upsampleTolerance.value = oldAO.upsampleTolerance.value;
                        newAO.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Exposure
                UnityEngine.Experimental.Rendering.HDPipeline.Exposure newExposure = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.Exposure>();
                if (profile.TryGet(out newExposure))
                {
                    target = newExposure;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.AutoExposure oldExposure;
                    if (processProfile.TryGetSettings(out oldExposure))
                    {
                        newExposure.active = oldExposure.active;
                        if (oldExposure.eyeAdaptation.value == EyeAdaptation.Fixed)
                        {
                            newExposure.adaptationMode.value = AdaptationMode.Fixed;
                        }
                        else
                        {
                            newExposure.adaptationMode.value = AdaptationMode.Progressive;
                        }
                        newExposure.adaptationSpeedDarkToLight.value = oldExposure.speedUp.value;
                        newExposure.adaptationSpeedLightToDark.value = oldExposure.speedDown.value;
                        newExposure.limitMax.value = oldExposure.maxLuminance.value;
                        newExposure.limitMin.value = oldExposure.minLuminance.value;
                        newExposure.mode.value = ExposureMode.Automatic;
                        newExposure.meteringMode.value = MeteringMode.Average;
                        newExposure.luminanceSource.value = LuminanceSource.ColorBuffer;
                        newExposure.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Bloom
                UnityEngine.Experimental.Rendering.HDPipeline.Bloom newBloom = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.Bloom>();
                if (profile.TryGet(out newBloom))
                {
                    target = newBloom;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.Bloom oldBloom;
                    if (processProfile.TryGetSettings(out oldBloom))
                    {
                        newBloom.active = oldBloom.active;
                        newBloom.anamorphic.value = true;
                        newBloom.dirtIntensity.value = oldBloom.dirtIntensity.value;
                        newBloom.dirtTexture.value = oldBloom.dirtTexture.value;
                        newBloom.highQualityFiltering.value = true;
                        newBloom.intensity.value = oldBloom.intensity.value / 10f;
                        newBloom.resolution.value = BloomResolution.Half;
                        newBloom.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Chromatic Aberration
                UnityEngine.Experimental.Rendering.HDPipeline.ChromaticAberration newChromaticAberration = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.ChromaticAberration>();
                if (profile.TryGet(out newChromaticAberration))
                {
                    target = newChromaticAberration;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.ChromaticAberration oldChromaticAberration;
                    if (processProfile.TryGetSettings(out oldChromaticAberration))
                    {
                        newChromaticAberration.active = oldChromaticAberration.active;
                        newChromaticAberration.intensity.value = oldChromaticAberration.intensity.value;
                        newChromaticAberration.maxSamples.value = 8;
                        newChromaticAberration.spectralLut.value = oldChromaticAberration.spectralLut.value;
                        newChromaticAberration.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Color Grading
                UnityEngine.Rendering.PostProcessing.ColorGrading oldColorGrading;
                if (processProfile.TryGetSettings(out oldColorGrading))
                {
                    UnityEngine.Experimental.Rendering.HDPipeline.ChannelMixer newChannelMixer = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.ChannelMixer>();
                    if (profile.TryGet(out newChannelMixer))
                    {
                        target = newChannelMixer;
                        objectSer = new SerializedObject(target);

                        newChannelMixer.active = true;
                        newChannelMixer.redOutBlueIn.value = oldColorGrading.mixerRedOutBlueIn.value;
                        newChannelMixer.redOutGreenIn.value = oldColorGrading.mixerRedOutGreenIn.value;
                        newChannelMixer.redOutRedIn.value = oldColorGrading.mixerRedOutRedIn.value;
                        newChannelMixer.greenOutBlueIn.value = oldColorGrading.mixerGreenOutBlueIn.value;
                        newChannelMixer.greenOutGreenIn.value = oldColorGrading.mixerGreenOutGreenIn.value;
                        newChannelMixer.greenOutRedIn.value = oldColorGrading.mixerGreenOutRedIn.value;
                        newChannelMixer.blueOutBlueIn.value = oldColorGrading.mixerBlueOutBlueIn.value;
                        newChannelMixer.blueOutGreenIn.value = oldColorGrading.mixerBlueOutGreenIn.value;
                        newChannelMixer.blueOutRedIn.value = oldColorGrading.mixerBlueOutRedIn.value;
                        newChannelMixer.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }

                    UnityEngine.Experimental.Rendering.HDPipeline.ColorAdjustments newColorAdjustments = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.ColorAdjustments>();
                    if (profile.TryGet(out newColorAdjustments))
                    {
                        target = newColorAdjustments;
                        objectSer = new SerializedObject(target);

                        newColorAdjustments.active = true;
                        newColorAdjustments.colorFilter.value = oldColorGrading.colorFilter.value;
                        newColorAdjustments.contrast.value = oldColorGrading.contrast.value;
                        newColorAdjustments.hueShift.value = oldColorGrading.hueShift.value;
                        newColorAdjustments.postExposure.value = oldColorGrading.postExposure.value;
                        newColorAdjustments.saturation.value = oldColorGrading.saturation.value;
                        newColorAdjustments.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }

                    UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves newColorCurves = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.ColorCurves>();
                    if (profile.TryGet(out newColorCurves))
                    {
                        target = newColorCurves;
                        objectSer = new SerializedObject(target);

                        newColorCurves.active = oldColorGrading.active;

                        objectSer.ApplyModifiedProperties();
                    }

                    UnityEngine.Experimental.Rendering.HDPipeline.ColorLookup newColorLookup = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.ColorLookup>();
                    if (profile.TryGet(out newColorLookup))
                    {
                        target = newColorLookup;
                        objectSer = new SerializedObject(target);

                        newColorLookup.active = oldColorGrading.active;
                        newColorLookup.contribution.value = oldColorGrading.ldrLutContribution.value;
                        newColorLookup.texture.value = oldColorGrading.ldrLut.value;

                        objectSer.ApplyModifiedProperties();
                    }

                    UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain newLiftGammaGain = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.LiftGammaGain>();
                    if (profile.TryGet(out newLiftGammaGain))
                    {
                        target = newLiftGammaGain;
                        objectSer = new SerializedObject(target);

                        newLiftGammaGain.active = oldColorGrading.active;
                        newLiftGammaGain.gain.value = oldColorGrading.gain.value;
                        newLiftGammaGain.gamma.value = oldColorGrading.gamma.value;
                        newLiftGammaGain.lift.value = oldColorGrading.lift.value;
                        newLiftGammaGain.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }

                    UnityEngine.Experimental.Rendering.HDPipeline.Tonemapping newTonemapping = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.Tonemapping>();
                    if (profile.TryGet(out newTonemapping))
                    {
                        target = newTonemapping;
                        objectSer = new SerializedObject(target);

                        newTonemapping.active = oldColorGrading.active;
                        if (oldColorGrading.tonemapper.value == Tonemapper.ACES)
                        {
                            newTonemapping.mode.value = TonemappingMode.ACES;
                        }
                        else if (oldColorGrading.tonemapper.value == Tonemapper.Custom)
                        {
                            newTonemapping.mode.value = TonemappingMode.Custom;
                        }
                        else if (oldColorGrading.tonemapper.value == Tonemapper.Neutral)
                        {
                            newTonemapping.mode.value = TonemappingMode.Neutral;
                        }
                        else
                        {
                            newTonemapping.mode.value = TonemappingMode.None;
                        }
                        newTonemapping.SetAllOverridesTo(true);
                    }

                    UnityEngine.Experimental.Rendering.HDPipeline.WhiteBalance newWhiteBalance = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.WhiteBalance>();
                    if (profile.TryGet(out newWhiteBalance))
                    {
                        target = newWhiteBalance;
                        objectSer = new SerializedObject(target);

                        newWhiteBalance.active = oldColorGrading.active;
                        newWhiteBalance.temperature.value = oldColorGrading.temperature.value;
                        newWhiteBalance.tint.value = oldColorGrading.tint.value;
                        newWhiteBalance.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Film Grain
                UnityEngine.Experimental.Rendering.HDPipeline.FilmGrain newFilmGrain = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.FilmGrain>();
                if (profile.TryGet(out newFilmGrain))
                {
                    target = newFilmGrain;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.Grain oldGrain;
                    if (processProfile.TryGetSettings(out oldGrain))
                    {
                        newFilmGrain.active = oldGrain.active;
                        newFilmGrain.intensity.value = oldGrain.intensity;
                        newFilmGrain.response.value = 0.8f;
                        newFilmGrain.type.value = FilmGrainLookup.Thin1;
                        newFilmGrain.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Lens Distortion
                UnityEngine.Experimental.Rendering.HDPipeline.LensDistortion newLensDistortion = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.LensDistortion>();
                if (profile.TryGet(out newLensDistortion))
                {
                    target = newLensDistortion;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.LensDistortion oldLensDistortion;
                    if (processProfile.TryGetSettings(out oldLensDistortion))
                    {
                        newLensDistortion.active = oldLensDistortion.active;
                        newLensDistortion.center.value = new Vector2(oldLensDistortion.centerX.value, oldLensDistortion.centerY.value);
                        newLensDistortion.intensity.value = 0f;
                        newLensDistortion.scale.value = oldLensDistortion.scale.value;
                        newLensDistortion.xMultiplier.value = oldLensDistortion.intensityX.value;
                        newLensDistortion.yMultiplier.value = oldLensDistortion.intensityY.value;
                        newLensDistortion.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Motion Blur
                UnityEngine.Experimental.Rendering.HDPipeline.MotionBlur newMotionBlur = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.MotionBlur>();
                if (profile.TryGet(out newMotionBlur))
                {
                    target = newMotionBlur;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.MotionBlur oldMotionBlur;
                    if (processProfile.TryGetSettings(out oldMotionBlur))
                    {
                        newMotionBlur.active = oldMotionBlur.active;
                        newMotionBlur.intensity.value = 0.5f;
                        newMotionBlur.maxVelocity.value = 250f;
                        newMotionBlur.sampleCount.value = 8;
                        newMotionBlur.minVel.value = 2f;
                        newMotionBlur.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Vignette
                UnityEngine.Experimental.Rendering.HDPipeline.Vignette newVignette = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.Vignette>();
                if (profile.TryGet(out newVignette))
                {
                    target = newVignette;
                    objectSer = new SerializedObject(target);

                    UnityEngine.Rendering.PostProcessing.Vignette oldVignette;
                    if (processProfile.TryGetSettings(out oldVignette))
                    {
                        newVignette.active = oldVignette.active;
                        newVignette.center.value = oldVignette.center.value;
                        newVignette.color.value = oldVignette.color.value;
                        newVignette.intensity.value = oldVignette.intensity.value;
                        newVignette.mask.value = oldVignette.mask.value;
                        if (oldVignette.mode.value == UnityEngine.Rendering.PostProcessing.VignetteMode.Classic)
                        {
                            newVignette.mode.value = UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode.Procedural;
                        }
                        else
                        {
                            newVignette.mode.value = UnityEngine.Experimental.Rendering.HDPipeline.VignetteMode.Masked;
                        }
                        newVignette.opacity.value = oldVignette.opacity.value;
                        newVignette.rounded.value = oldVignette.rounded.value;
                        newVignette.roundness.value = oldVignette.roundness.value;
                        newVignette.smoothness.value = oldVignette.smoothness.value;
                        newVignette.SetAllOverridesTo(true);

                        objectSer.ApplyModifiedProperties();
                    }
                }

                //Split Toning
                UnityEngine.Experimental.Rendering.HDPipeline.SplitToning newSplitToning = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.SplitToning>();
                if (profile.TryGet(out newSplitToning))
                {
                    target = newSplitToning;
                    objectSer = new SerializedObject(target);

                    newSplitToning.active = true;
                    newSplitToning.shadows.value = SkyboxUtils.GetColorFromHTML("464646");
                    newSplitToning.highlights.value = SkyboxUtils.GetColorFromHTML("7E7E7E");
                    newSplitToning.balance.value = 5f;
                    newSplitToning.SetAllOverridesTo(true);

                    objectSer.ApplyModifiedProperties();
                }

                //Panini Projection
                UnityEngine.Experimental.Rendering.HDPipeline.PaniniProjection newPaniniProjection = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.PaniniProjection>();
                if (profile.TryGet(out newPaniniProjection))
                {
                    target = newPaniniProjection;
                    objectSer = new SerializedObject(target);

                    newPaniniProjection.active = true;
                    newPaniniProjection.cropToFit.value = 1f;
                    newPaniniProjection.distance.value = 0.02f;
                    newPaniniProjection.SetAllOverridesTo(true);

                    objectSer.ApplyModifiedProperties();
                }

                //Depth Of Field
                UnityEngine.Experimental.Rendering.HDPipeline.DepthOfField newDepthOfField = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.DepthOfField>();
                if (profile.TryGet(out newDepthOfField))
                {
                    target = newDepthOfField;
                    objectSer = new SerializedObject(target);

                    newDepthOfField.active = true;
                    newDepthOfField.focusMode.value = DepthOfFieldMode.Manual;
                    newDepthOfField.nearFocusStart.value = 0f;
                    newDepthOfField.nearFocusEnd.value = 2f;
                    newDepthOfField.farFocusStart.value = 25f;
                    newDepthOfField.farFocusEnd.value = 500f;
                    newDepthOfField.SetAllOverridesTo(true);

                    objectSer.ApplyModifiedProperties();
                }

                //Shadows Midtones Highlights
                UnityEngine.Experimental.Rendering.HDPipeline.ShadowsMidtonesHighlights newShadowsMidtonesHighlights = profile.Add<UnityEngine.Experimental.Rendering.HDPipeline.ShadowsMidtonesHighlights>();
                if (profile.TryGet(out newShadowsMidtonesHighlights))
                {
                    target = newShadowsMidtonesHighlights;
                    objectSer = new SerializedObject(target);

                    newShadowsMidtonesHighlights.active = true;
                    newShadowsMidtonesHighlights.shadows.value = new Vector4(1f, 1f, 1f, -0.2f);
                    newShadowsMidtonesHighlights.midtones.value = new Vector4(1f, 1f, 1f, 0f);
                    newShadowsMidtonesHighlights.highlights.value = new Vector4(0.9f, 0.9f, 1f, -0.1f);
                    newShadowsMidtonesHighlights.shadowsStart.value = 0.1f;
                    newShadowsMidtonesHighlights.shadowsEnd.value = 1f;
                    newShadowsMidtonesHighlights.highlightsStart.value = 0.4f;
                    newShadowsMidtonesHighlights.highlightsEnd.value = 1.3f;
                    newShadowsMidtonesHighlights.SetAllOverridesTo(true);

                    objectSer.ApplyModifiedProperties();
                }

                objectSer.UpdateIfRequiredOrScript();
                objectSer.ApplyModifiedPropertiesWithoutUndo();

                //AssetDatabase.SaveAssets();
            }
        }

        /// <summary>
        /// Creates new post processing profile for HDRP
        /// </summary>
        /// <param name="processProfile"></param>
        /// <param name="renameProfile"></param>
        /// <param name="newProfileName"></param>
        /// <returns></returns>
        private VolumeProfile CreateHDRPPostProcessingProfile(PostProcessProfile processProfile, bool renameProfile, string newProfileName)
        {
            VolumeProfile newProfile;
            if (renameProfile)
            {
                newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

                AssetDatabase.CreateAsset(newProfile, "Assets/" + processProfile.name + newProfileName + ".asset");

                AssetDatabase.SaveAssets();

                if (focusAsset)
                {
                    EditorUtility.FocusProjectWindow();

                    Selection.activeObject = newProfile;
                }
            }
            else
            {
                newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

                AssetDatabase.CreateAsset(newProfile, "Assets/New HDRP Post Processing.asset");

                AssetDatabase.SaveAssets();

                if (focusAsset)
                {
                    EditorUtility.FocusProjectWindow();

                    Selection.activeObject = newProfile;
                }
            }

            return newProfile;
        }

        /// <summary>
        /// Method used to apply settings to a new HD Volume Profile from an older profile
        /// </summary>
        /// <param name="newProfile"></param>
        /// <param name="profile"></param>
        private void ApplyNewHDRPProfileSettings(VolumeProfile newProfile, VolumeProfile profile)
        {
            //Adds the VisualEnvironment component to the new created profile
            newProfile.Add<VisualEnvironment>();

            //The new VisualEnvironment setting
            VisualEnvironment newEnvironment;
            if (newProfile.TryGet(out newEnvironment))
            {
                //The old VisualEnvironment setting
                VisualEnvironment oldEnvironment;
                if (profile.TryGet(out oldEnvironment))
                {
                    newEnvironment.active = oldEnvironment.active;
                    newEnvironment.fogType.value = oldEnvironment.fogType.value;
                    newEnvironment.skyType.value = oldEnvironment.skyType.value;
                    newEnvironment.SetAllOverridesTo(true);
                }
            }

            //Adds the HDRISky component to the new created profile
            newProfile.Add<HDRISky>();

            HDRISky newHDRISky;
            if (newProfile.TryGet(out newHDRISky))
            {
                HDRISky oldHDRISky;
                if (profile.TryGet(out oldHDRISky))
                {
                    newHDRISky.active = oldHDRISky.active;
                    newHDRISky.hdriSky.value = oldHDRISky.hdriSky.value;
                    newHDRISky.skyIntensityMode.value = oldHDRISky.skyIntensityMode.value;
                    newHDRISky.exposure.value = oldHDRISky.exposure.value;
                    newHDRISky.multiplier.value = oldHDRISky.multiplier.value;
                    newHDRISky.rotation.value = oldHDRISky.rotation.value;
                    newHDRISky.updateMode.value = oldHDRISky.updateMode.value;
                    newHDRISky.SetAllOverridesTo(true);
                }
            }

            //Adds the GradientSky component to the new created profile
            newProfile.Add<GradientSky>();

            GradientSky newGradientSky;
            if (newProfile.TryGet(out newGradientSky))
            {
                GradientSky oldGradientSky;
                if (profile.TryGet(out oldGradientSky))
                {
                    newGradientSky.active = oldGradientSky.active;
                    newGradientSky.top.value = oldGradientSky.top.value;
                    newGradientSky.middle.value = oldGradientSky.middle.value;
                    newGradientSky.bottom.value = oldGradientSky.bottom.value;
                    newGradientSky.gradientDiffusion.value = oldGradientSky.gradientDiffusion.value;
                    newGradientSky.updateMode.value = oldGradientSky.updateMode.value;
                    newGradientSky.SetAllOverridesTo(true);
                }
            }

            //Adds the ProceduralSky component to the new created profile
            newProfile.Add<ProceduralSky>();

            ProceduralSky newProcedural;
            if (newProfile.TryGet(out newProcedural))
            {
                ProceduralSky oldProcedural;
                if (profile.TryGet(out oldProcedural))
                {
                    newProcedural.active = oldProcedural.active;
                    newProcedural.sunSize.value = oldProcedural.sunSize.value;
                    newProcedural.sunSizeConvergence.value = oldProcedural.sunSizeConvergence.value;
                    newProcedural.atmosphereThickness.value = oldProcedural.atmosphereThickness.value;
                    newProcedural.skyTint.value = oldProcedural.skyTint.value;
                    newProcedural.groundColor.value = oldProcedural.groundColor.value;
                    newProcedural.exposure.value = oldProcedural.exposure.value;
                    newProcedural.multiplier.value = oldProcedural.multiplier.value;
                    newProcedural.updateMode.value = oldProcedural.updateMode.value;
                    newProcedural.includeSunInBaking.value = oldProcedural.includeSunInBaking.value;
                    newProcedural.SetAllOverridesTo(true);

                }
            }

            //Adds the ExponentialFog component to the new created profile
            newProfile.Add<ExponentialFog>();

            ExponentialFog newExponentialFog;
            if (newProfile.TryGet(out newExponentialFog))
            {
                ExponentialFog oldExponentialFog;
                if (profile.TryGet(out oldExponentialFog))
                {
                    newExponentialFog.active = oldExponentialFog.active;
                    newExponentialFog.density.value = oldExponentialFog.density.value;
                    newExponentialFog.fogDistance.value = oldExponentialFog.fogDistance.value;
                    newExponentialFog.fogBaseHeight.value = oldExponentialFog.fogBaseHeight.value;
                    newExponentialFog.fogHeightAttenuation.value = oldExponentialFog.fogHeightAttenuation.value;
                    newExponentialFog.maxFogDistance.value = oldExponentialFog.maxFogDistance.value;
                    newExponentialFog.colorMode.value = oldExponentialFog.colorMode.value;
                    newExponentialFog.mipFogNear.value = oldExponentialFog.mipFogNear.value;
                    newExponentialFog.mipFogFar.value = oldExponentialFog.mipFogFar.value;
                    newExponentialFog.mipFogMaxMip.value = oldExponentialFog.mipFogMaxMip.value;
                    newExponentialFog.SetAllOverridesTo(true);
                }
            }

            //Adds the LinearFog component to the new created profile
            newProfile.Add<LinearFog>();

            LinearFog newLinearFog;
            if (newProfile.TryGet(out newLinearFog))
            {
                LinearFog oldLinearFog;
                if (profile.TryGet(out oldLinearFog))
                {
                    newLinearFog.active = oldLinearFog.active;
                    newLinearFog.density.value = oldLinearFog.density.value;
                    newLinearFog.fogStart.value = oldLinearFog.fogStart.value;
                    newLinearFog.fogEnd.value = oldLinearFog.fogEnd.value;
                    newLinearFog.fogHeightStart.value = oldLinearFog.fogHeightStart.value;
                    newLinearFog.fogHeightEnd.value = oldLinearFog.fogHeightEnd.value;
                    newLinearFog.maxFogDistance.value = oldLinearFog.maxFogDistance.value;
                    newLinearFog.colorMode.value = oldLinearFog.colorMode.value;
                    newLinearFog.mipFogNear.value = oldLinearFog.mipFogNear.value;
                    newLinearFog.mipFogFar.value = oldLinearFog.mipFogFar.value;
                    newLinearFog.mipFogMaxMip.value = oldLinearFog.mipFogMaxMip.value;
                    newLinearFog.SetAllOverridesTo(true);
                }
            }

            //Adds the VolumetricFog component to the new created profile
            newProfile.Add<VolumetricFog>();

            VolumetricFog newVolumetricFog;
            if (newProfile.TryGet(out newVolumetricFog))
            {
                VolumetricFog oldVolumetricFog;
                if (profile.TryGet(out oldVolumetricFog))
                {
                    newVolumetricFog.active = oldVolumetricFog.active;
                    newVolumetricFog.albedo.value = oldVolumetricFog.albedo.value;
                    newVolumetricFog.meanFreePath.value = oldVolumetricFog.meanFreePath.value;
                    newVolumetricFog.baseHeight.value = oldVolumetricFog.baseHeight.value;
                    newVolumetricFog.meanHeight.value = oldVolumetricFog.meanHeight.value;
                    newVolumetricFog.anisotropy.value = oldVolumetricFog.anisotropy.value;
                    newVolumetricFog.globalLightProbeDimmer.value = oldVolumetricFog.globalLightProbeDimmer.value;
                    newVolumetricFog.maxFogDistance.value = oldVolumetricFog.maxFogDistance.value;
                    newVolumetricFog.enableDistantFog.value = oldVolumetricFog.enableDistantFog.value;
                    newVolumetricFog.colorMode.value = oldVolumetricFog.colorMode.value;
                    newVolumetricFog.mipFogNear.value = oldVolumetricFog.mipFogNear.value;
                    newVolumetricFog.mipFogFar.value = oldVolumetricFog.mipFogFar.value;
                    newVolumetricFog.mipFogMaxMip.value = oldVolumetricFog.mipFogMaxMip.value;
                    newVolumetricFog.SetAllOverridesTo(true);
                }
            }

            //Adds the HDShadowSettings component to the new created profile
            newProfile.Add<HDShadowSettings>();

            HDShadowSettings newHDShadowSettings;
            if (newProfile.TryGet(out newHDShadowSettings))
            {
                HDShadowSettings oldHDShadowSettings;
                if (profile.TryGet(out oldHDShadowSettings))
                {
                    newHDShadowSettings.active = oldHDShadowSettings.active;
                    newHDShadowSettings.maxShadowDistance.value = oldHDShadowSettings.maxShadowDistance.value;
                    newHDShadowSettings.cascadeShadowSplitCount.value = oldHDShadowSettings.cascadeShadowSplitCount.value;
                    newHDShadowSettings.cascadeShadowSplit0.value = oldHDShadowSettings.cascadeShadowSplit0.value;
                    newHDShadowSettings.cascadeShadowSplit1.value = oldHDShadowSettings.cascadeShadowSplit1.value;
                    newHDShadowSettings.cascadeShadowSplit2.value = oldHDShadowSettings.cascadeShadowSplit2.value;
                    newHDShadowSettings.SetAllOverridesTo(true);
                }
            }

            //Adds the ContactShadows component to the new created profile
            newProfile.Add<ContactShadows>();

            ContactShadows newContactShadows;
            if (newProfile.TryGet(out newContactShadows))
            {
                ContactShadows oldContactShadows;
                if (profile.TryGet(out oldContactShadows))
                {
                    newContactShadows.active = oldContactShadows.active;
                    newContactShadows.enable.value = oldContactShadows.enable.value;
                    newContactShadows.length.value = oldContactShadows.length.value;
                    newContactShadows.distanceScaleFactor.value = oldContactShadows.distanceScaleFactor.value;
                    newContactShadows.maxDistance.value = oldContactShadows.maxDistance.value;
                    newContactShadows.fadeDistance.value = oldContactShadows.fadeDistance.value;
                    newContactShadows.sampleCount.value = oldContactShadows.sampleCount.value;
                    newContactShadows.opacity.value = oldContactShadows.opacity.value;
                    newContactShadows.SetAllOverridesTo(true);
                }
            }

            //Adds the MicroShadowing component to the new created profile
            newProfile.Add<MicroShadowing>();

            MicroShadowing newMicroShadows;
            if (newProfile.TryGet(out newMicroShadows))
            {
                MicroShadowing oldMicroShadows;
                if (profile.TryGet(out oldMicroShadows))
                {
                    newMicroShadows.active = oldMicroShadows.active;
                    newMicroShadows.enable.value = oldMicroShadows.enable.value;
                    newMicroShadows.opacity.value = oldMicroShadows.opacity.value;
                    newMicroShadows.SetAllOverridesTo(true);
                }
            }

            //Adds the IndirectLightingController component to the new created profile
            newProfile.Add<IndirectLightingController>();

            IndirectLightingController newIndirectLightingController;
            if (newProfile.TryGet(out newIndirectLightingController))
            {
                IndirectLightingController oldIndirectLightingController;
                if (profile.TryGet(out oldIndirectLightingController))
                {
                    newIndirectLightingController.active = oldIndirectLightingController.active;
                    newIndirectLightingController.indirectDiffuseIntensity.value = oldIndirectLightingController.indirectDiffuseIntensity.value;
                    newIndirectLightingController.indirectSpecularIntensity.value = oldIndirectLightingController.indirectSpecularIntensity.value;
                    newIndirectLightingController.SetAllOverridesTo(true);
                }
            }

            //Adds the VolumetricLightingController component to the new created profile
            newProfile.Add<VolumetricLightingController>();

            VolumetricLightingController newVolumetricLightingController;
            if (newProfile.TryGet(out newVolumetricLightingController))
            {
                VolumetricLightingController oldVolumetricLightingController;
                if (profile.TryGet(out oldVolumetricLightingController))
                {
                    newVolumetricLightingController.active = oldVolumetricLightingController.active;
                    newVolumetricLightingController.depthExtent.value = oldVolumetricLightingController.depthExtent.value;
                    newVolumetricLightingController.sliceDistributionUniformity.value = oldVolumetricLightingController.sliceDistributionUniformity.value;
                    newVolumetricLightingController.SetAllOverridesTo(true);
                }
            }

            //Adds the ScreenSpaceReflection component to the new created profile
            newProfile.Add<ScreenSpaceReflection>();

            ScreenSpaceReflection newScreenSpaceReflection;
            if (newProfile.TryGet(out newScreenSpaceReflection))
            {
                ScreenSpaceReflection oldScreenSpaceReflection;
                if (profile.TryGet(out oldScreenSpaceReflection))
                {
                    newScreenSpaceReflection.active = oldScreenSpaceReflection.active;
                    newScreenSpaceReflection.screenFadeDistance.value = oldScreenSpaceReflection.screenFadeDistance.value;
                    newScreenSpaceReflection.rayMaxIterations.value = oldScreenSpaceReflection.rayMaxIterations.value;
                    newScreenSpaceReflection.depthBufferThickness.value = oldScreenSpaceReflection.depthBufferThickness.value;
                    newScreenSpaceReflection.minSmoothness.value = oldScreenSpaceReflection.minSmoothness.value;
                    newScreenSpaceReflection.smoothnessFadeStart.value = oldScreenSpaceReflection.smoothnessFadeStart.value;
                    newScreenSpaceReflection.reflectSky.value = oldScreenSpaceReflection.reflectSky.value;
                    newScreenSpaceReflection.SetAllOverridesTo(true);
                }
            }

            //Adds the ScreenSpaceRefraction component to the new created profile
            newProfile.Add<ScreenSpaceRefraction>();

            ScreenSpaceRefraction newScreenSpaceRefraction;
            if (newProfile.TryGet(out newScreenSpaceRefraction))
            {
                ScreenSpaceRefraction oldScreenSpaceRefraction;
                if (profile.TryGet(out oldScreenSpaceRefraction))
                {
                    newScreenSpaceRefraction.active = oldScreenSpaceRefraction.active;
                    newScreenSpaceRefraction.screenFadeDistance.value = oldScreenSpaceRefraction.screenFadeDistance.value;
                    newScreenSpaceRefraction.SetAllOverridesTo(true);
                }
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Uses as a return method to create a new volume profile for HDRP
        /// </summary>
        /// <param name="profileNumber"></param>
        /// <returns></returns>
        public VolumeProfile CreateHDVolumeProfileInternal(int profileNumber)
        {
            VolumeProfile volumeProfile0 = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("New Ambient Skies HD Profile 0"));
            if (volumeProfile0 == null)
            {
                profileNumber = 0;
                m_createdProfileNumber = 0;
            }

            VolumeProfile newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

            AssetDatabase.CreateAsset(newProfile, "Assets/New Ambient Skies HD Profile " + profileNumber.ToString() + ".asset");

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = newProfile;

            return newProfile;
        }

        /// <summary>
        /// Uses as an item menu method to create a new volume profile for HDRP
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Ambient Skies/HDRP/Create New HD Volume", false, 40)]
        public static void CreateHDVolumeProfile()
        {
            VolumeProfile newProfile = ScriptableObject.CreateInstance<VolumeProfile>();

            AssetDatabase.CreateAsset(newProfile, "Assets/Procedural Worlds/Ambient Skies/New Ambient Skies HD Profile.asset");

            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = newProfile;
        }
#endif

        #endregion
    }
}