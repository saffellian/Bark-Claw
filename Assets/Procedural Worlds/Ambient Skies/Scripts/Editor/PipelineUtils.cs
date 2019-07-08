//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.Rendering;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#if LWPipeline && UNITY_2018_3
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#endif
#if CTS_PRESENT
using CTS;
#endif
#if GAIA_PRESENT
using Gaia;
#endif

namespace AmbientSkies
{
    public static class AmbientSkiesPipelineUtils
    {
        #region Utils

        #region Pipeline Setup

        /// <summary>
        /// Apply Resources to HD pipeline
        /// </summary>
        /// <param name="renderPipelineSettings"></param>
        /// <param name="renderPipelineAsset"></param>
        public static void ApplyHDPipelineResources(AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, string renderPipelineAsset)
        {
            //If current pipeline is HDRP
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                RenderPipelineAsset hdRenderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath(renderPipelineAsset));
                if (hdRenderPipelineAsset != null)
                {
#if HDPipeline
                    HDRenderPipelineAsset pipelineSettings = AssetDatabase.LoadAssetAtPath<HDRenderPipelineAsset>(SkyboxUtils.GetAssetPath(renderPipelineAsset));
                    if (pipelineSettings != null)
                    {
                        if (pipelineSettings.renderPipelineResources == null)
                        {
                            pipelineSettings.renderPipelineResources = AssetDatabase.LoadAssetAtPath<RenderPipelineResources>(SkyboxUtils.GetAssetPath("HDRenderPipelineResources"));
                        }
                        if (pipelineSettings.renderPipelineEditorResources == null)
                        {
                            pipelineSettings.renderPipelineEditorResources = AssetDatabase.LoadAssetAtPath<HDRenderPipelineEditorResources>(SkyboxUtils.GetAssetPath("HDRenderPipelineEditorResources"));
                        }
#if UNITY_2018_3
                        pipelineSettings.diffusionProfileSettings = AssetDatabase.LoadAssetAtPath<DiffusionProfileSettings>(SkyboxUtils.GetAssetPath("Procedural Worlds Diffusion Profile Settings"));
#elif UNITY_2019_1_OR_NEWER
                        if (string.IsNullOrEmpty(SkyboxUtils.GetAssetPath("Procedural Worlds Diffusion Profile Settings_Foliage")))
                        {
                            pipelineSettings.diffusionProfileSettingsList = new DiffusionProfileSettings[2];
                            pipelineSettings.diffusionProfileSettingsList[0] = AssetDatabase.LoadAssetAtPath<DiffusionProfileSettings>(SkyboxUtils.GetAssetPath("Procedural Worlds Diffusion Profile Settings_Foliage"));
                        }

                        HDRenderPipelineEditorResources editorResources = AssetDatabase.LoadAssetAtPath<HDRenderPipelineEditorResources>(SkyboxUtils.GetAssetPath("HDRenderPipelineEditorResources"));
                        if (editorResources != null)
                        {
                            if (editorResources.defaultPostProcessingProfile == null)
                            {
                                editorResources.defaultPostProcessingProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Ambient Skies HD Volume Profile"));
                            }
                            if (editorResources.defaultRenderSettingsProfile == null)
                            {
                                editorResources.defaultRenderSettingsProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Ambient Skies HD Volume Profile"));
                            }
                        }
#endif
                    }
#endif
                }
            }
        }

        /// <summary>
        /// Configures projects for the pipeline selected
        /// </summary>
        /// <param name="renderPipelineSettings"></param>
        /// <param name="renderPipelineAsset"></param>
        /// <param name="waterShader"></param>
        /// <param name="terrainShader"></param>
        public static void SetupPipeline(AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, string renderPipelineAsset, string waterShader, string terrainMaterial, string terrainShader, bool finalizeEnvironment)
        {
            //Terrain object
            Terrain[] terrains = GetActiveTerrain();

            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn)
            {
                GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath(renderPipelineAsset));
                if (finalizeEnvironment)
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                }
                SkyboxUtils.DestroyParent("Ambient Skies Environment");

                GameObject HDRP2019PostFX = GameObject.Find("Post Processing HDRP Volume");
                if (HDRP2019PostFX != null)
                {
                    Object.DestroyImmediate(HDRP2019PostFX);
                }

#if GAIA_PRESENT
                GaiaSettings[] gaiaSettings = Resources.FindObjectsOfTypeAll<GaiaSettings>();
                if (gaiaSettings != null)
                {
                    foreach (GaiaSettings settings in gaiaSettings)
                    {
                        settings.m_currentRenderer = GaiaConstants.EnvironmentRenderer.BuiltIn;
                    }
                }
#endif

#if !CTS_PRESENT
                if (terrains != null)
                {
                    foreach (Terrain activeTerrains in terrains)
                    {
                        activeTerrains.materialType = Terrain.MaterialType.BuiltInStandard;
                        activeTerrains.materialTemplate = null;
                    }
                }
#else
                if (terrains != null)
                {
                    CompleteTerrainShader[] ctsShader = Object.FindObjectsOfType<CompleteTerrainShader>();
                    if (ctsShader != null)
                    {
                        foreach (CompleteTerrainShader ctsSettings in ctsShader)
                        {
                            CTSTerrainManager.Instance.BroadcastProfileUpdate(ctsSettings.Profile);
                        }
                    }
                }
#endif
#if HDPipeline
                HDAdditionalLightData hDAdditionalLightData = Object.FindObjectOfType<HDAdditionalLightData>();
                if (hDAdditionalLightData != null)
                {
                    Object.DestroyImmediate(hDAdditionalLightData);
                }

                AdditionalShadowData additionalShadowData = Object.FindObjectOfType<AdditionalShadowData>();
                if (additionalShadowData != null)
                {
                    Object.DestroyImmediate(additionalShadowData);
                }

                GameObject densityFogVolume = GameObject.Find("Density Fog Volume");
                if (densityFogVolume != null)
                {
                    Object.DestroyImmediate(densityFogVolume);
                }

                HDAdditionalCameraData hDAdditionalCameraData = Object.FindObjectOfType<HDAdditionalCameraData>();
                if (hDAdditionalCameraData != null)
                {
                    Object.DestroyImmediate(hDAdditionalCameraData);
                }
#endif

#if LWPipeline && UNITY_2018_3_OR_NEWER
                LWRPAdditionalCameraData lWRPAdditionalCameraData = Object.FindObjectOfType<LWRPAdditionalCameraData>();
                if (lWRPAdditionalCameraData != null)
                {
                    Object.DestroyImmediate(lWRPAdditionalCameraData);
                }

                LWRPAdditionalLightData lWRPAdditionalLightData = Object.FindObjectOfType<LWRPAdditionalLightData>();
                if (lWRPAdditionalLightData != null)
                {
                    Object.DestroyImmediate(lWRPAdditionalLightData);
                }
#endif
            }
            else if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
            {
                GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath(renderPipelineAsset));
                if (finalizeEnvironment)
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                }
                    
                GameObject HDRP2019PostFX = GameObject.Find("Post Processing HDRP Volume");
                if (HDRP2019PostFX != null)
                {
                    Object.DestroyImmediate(HDRP2019PostFX);
                }

#if GAIA_PRESENT
                GaiaSettings[] gaiaSettings = Resources.FindObjectsOfTypeAll<GaiaSettings>();
                if (gaiaSettings != null)
                {
                    foreach (GaiaSettings settings in gaiaSettings)
                    {
                        settings.m_currentRenderer = GaiaConstants.EnvironmentRenderer.LightWeight2018x;
                    }
                }
#endif

#if !CTS_PRESENT
                if (terrains != null)
                {
                    foreach (Terrain activeTerrains in terrains)
                    {
                        activeTerrains.materialType = Terrain.MaterialType.Custom;
                        activeTerrains.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath(terrainMaterial));
                    }

                    Material material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath(terrainMaterial));
                    if (material != null)
                    {
                        material.shader = Shader.Find(terrainShader);
                        material.enableInstancing = true;
                    }
                }
#else
                if (terrains != null)
                {
                    CompleteTerrainShader[] ctsShader = Object.FindObjectsOfType<CompleteTerrainShader>();
                    if (ctsShader != null)
                    {
                        foreach (CompleteTerrainShader ctsSettings in ctsShader)
                        {
                            CTSTerrainManager.Instance.BroadcastProfileUpdate(ctsSettings.Profile);
                        }
                    }
                }
#endif

#if LWPipeline && UNITY_2018_3_OR_NEWER
                Camera camera = Object.FindObjectOfType<Camera>();
                if (camera != null)
                {
                    camera.gameObject.AddComponent<LWRPAdditionalCameraData>();
                }
                GameObject light = SkyboxUtils.GetMainDirectionalLight();
                if (light != null)
                {
                    light.AddComponent<LWRPAdditionalLightData>();
                }
#endif

#if HDPipeline
                HDAdditionalLightData hDAdditionalLightData = Object.FindObjectOfType<HDAdditionalLightData>();
                if (hDAdditionalLightData != null)
                {
                    Object.DestroyImmediate(hDAdditionalLightData);
                }

                AdditionalShadowData additionalShadowData = Object.FindObjectOfType<AdditionalShadowData>();
                if (additionalShadowData != null)
                {
                    Object.DestroyImmediate(additionalShadowData);
                }

                GameObject densityFogVolume = GameObject.Find("Density Fog Volume");
                if (densityFogVolume != null)
                {
                    Object.DestroyImmediate(densityFogVolume);
                }

                HDAdditionalCameraData hDAdditionalCameraData = Object.FindObjectOfType<HDAdditionalCameraData>();
                if (hDAdditionalCameraData != null)
                {
                    Object.DestroyImmediate(hDAdditionalCameraData);
                }
#endif
                SkyboxUtils.DestroyParent("Ambient Skies Environment");
            }
            else
            {
                ApplyHDPipelineResources(renderPipelineSettings, renderPipelineAsset);

                GraphicsSettings.renderPipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(SkyboxUtils.GetAssetPath(renderPipelineAsset));

#if GAIA_PRESENT
                GaiaSettings[] gaiaSettings = Resources.FindObjectsOfTypeAll<GaiaSettings>();
                if (gaiaSettings != null)
                {
                    foreach (GaiaSettings settings in gaiaSettings)
                    {
                        settings.m_currentRenderer = GaiaConstants.EnvironmentRenderer.HighDefinition2018x;
                    }
                }
#endif

#if !CTS_PRESENT
                if (terrains != null)
                {
                    foreach (Terrain activeTerrains in terrains)
                    {
                        activeTerrains.materialType = Terrain.MaterialType.Custom;
                        activeTerrains.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath(terrainMaterial));
                    }

                    Material material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath(terrainMaterial));
                    if (material != null)
                    {
                        material.shader = Shader.Find(terrainShader);
                        material.enableInstancing = true;
#if UNITY_2018_3_OR_NEWER
                        material.SetFloat("_EnableHeightBlend", 1f);
#endif
                    }
                }
#else
                if (terrains != null)
                {
                    CompleteTerrainShader[] ctsShader = Object.FindObjectsOfType<CompleteTerrainShader>();
                    if (ctsShader != null)
                    {
                        foreach (CompleteTerrainShader ctsSettings in ctsShader)
                        {
                            CTSTerrainManager.Instance.BroadcastProfileUpdate(ctsSettings.Profile);
                        }
                    }
                }
#endif

#if LWPipeline && UNITY_2018_3_OR_NEWER
                LWRPAdditionalCameraData lWRPAdditionalCameraData = Object.FindObjectOfType<LWRPAdditionalCameraData>();
                if (lWRPAdditionalCameraData != null)
                {
                    Object.DestroyImmediate(lWRPAdditionalCameraData);
                }
                LWRPAdditionalLightData lWRPAdditionalLightData = Object.FindObjectOfType<LWRPAdditionalLightData>();
                if (lWRPAdditionalLightData != null)
                {
                    Object.DestroyImmediate(lWRPAdditionalLightData);
                }
#endif

#if HDPipeline
                Camera camera = Object.FindObjectOfType<Camera>();
                if (camera != null)
                {
                    if (camera.gameObject.GetComponent<HDAdditionalCameraData>() == null)
                    {
                        HDAdditionalCameraData hdCamData = camera.gameObject.AddComponent<HDAdditionalCameraData>();
#if UNITY_2019_1_OR_NEWER
                        hdCamData.volumeLayerMask = 2;
#endif
                    }   
                    else
                    {
                        HDAdditionalCameraData hdCamData = camera.gameObject.GetComponent<HDAdditionalCameraData>();
#if UNITY_2019_1_OR_NEWER
                        hdCamData.volumeLayerMask = 2;
#endif
                    }
                }

                GameObject light = SkyboxUtils.GetMainDirectionalLight();
                if (light != null)
                {
                    if (light.GetComponent<AdditionalShadowData>() == null)
                    {
                        light.AddComponent<AdditionalShadowData>();
                    }

                    if (light.GetComponent<HDAdditionalLightData>() == null)
                    {
                        light.AddComponent<HDAdditionalLightData>();
                    }
                }
#endif
                SkyboxUtils.DestroyParent("Ambient Skies Environment");

                if (PlayerSettings.colorSpace == ColorSpace.Gamma)
                {
                    if (EditorUtility.DisplayDialog("Incorrect Color Space!", "High Difinition requires Linear Color Space. Would you like to change your color space to Linear?", "Yes", "Cancel"))
                    {
                        SetLinearDeferredLighting();
                    }
                }
            }
        }

        #endregion

        #region Volume and Profile Updates

        /// <summary>
        /// Applies and creates the volume settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="renderPipelineSettings"></param>
        /// <param name="volumeName"></param>
        public static void SetupHDEnvironmentalVolume(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles, int profileIdx, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings, string volumeName, string hdVolumeProfile)
        {
            //Get parent object
            GameObject parentObject = SkyboxUtils.GetOrCreateParentObject("Ambient Skies Environment", false);

            //Apply only if system type is Ambient Skies
            if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.AmbientSkies)
            {
                //High Definition
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
                    //Locates the old volume profine that unity creates
                    GameObject oldVolumeObject = GameObject.Find("Volume Settings");
                    if (oldVolumeObject != null)
                    {
                        //Destroys the object
                        Object.DestroyImmediate(oldVolumeObject);
                    }

                    //Find the old post processing object that unity creates
                    GameObject oldPostProcessing = GameObject.Find("Post-process Volume");
                    if (oldPostProcessing != null)
                    {
                        //Destoys the old post processing object
                        Object.DestroyImmediate(oldPostProcessing);
                    }

                    if (profile.useSkies)
                    {
#if HDPipeline
                        //Starts the fix probes function
                        FixHDReflectionProbes();

                        //Set Sun rotation
                        SkyboxUtils.SetSkyboxRotation(skyProfiles, profile, renderPipelineSettings);

                        //Get the main directional light
                        GameObject lightObj = SkyboxUtils.GetMainDirectionalLight();
                        if (lightObj != null)
                        {
                            HDAdditionalLightData lightData = lightObj.GetComponent<HDAdditionalLightData>();
                            if (lightData != null)
                            {
                                if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Volumetric)
                                {
                                    lightData.useVolumetric = true;
                                }
                                else
                                {
                                    lightData.useVolumetric = false;
                                }
                                lightData.lightUnit = LightUnit.Lux;
                                if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                                {
                                    lightData.intensity = profile.proceduralSunIntensity * 3.14f;
                                }
                                else
                                {
                                    lightData.intensity = profile.sunIntensity * 3.14f;
                                }
                            }

                            AdditionalShadowData[] shadowData = Object.FindObjectsOfType<AdditionalShadowData>();
                            if (shadowData != null)
                            {
                                foreach (AdditionalShadowData data in shadowData)
                                {
                                    data.contactShadows = profile.useContactShadows;
                                    switch (profile.shadowQuality)
                                    {
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution64:
                                            data.shadowResolution = 64;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution128:
                                            data.shadowResolution = 128;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution256:
                                            data.shadowResolution = 256;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution512:
                                            data.shadowResolution = 512;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution1024:
                                            data.shadowResolution = 1024;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution2048:
                                            data.shadowResolution = 2048;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution4096:
                                            data.shadowResolution = 4096;
                                            break;
                                        case AmbientSkiesConsts.HDShadowQuality.Resolution8192:
                                            data.shadowResolution = 8192;
                                            break;
                                    }
                                }
                            }
                        }

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

                        Volume volumeSettings;

                        HDAdditionalCameraData hdCamData = Object.FindObjectOfType<HDAdditionalCameraData>();
                        if (hdCamData != null)
                        {
#if UNITY_2019_1_OR_NEWER
                            hdCamData.volumeLayerMask = 2;
#endif
                        }

                        if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.AmbientSkies)
                        {
                            volumeSettings = volumeObject.GetComponent<Volume>();
                            if (volumeSettings == null)
                            {
                                volumeSettings = volumeObject.AddComponent<Volume>();
                                volumeSettings.isGlobal = profile.isGlobal;
                                volumeSettings.blendDistance = profile.blendDistance;
                                volumeSettings.weight = profile.weight;
                                volumeSettings.priority = profile.priority;
                            }
                            else
                            {
                                volumeSettings.isGlobal = profile.isGlobal;
                                volumeSettings.blendDistance = profile.blendDistance;
                                volumeSettings.weight = profile.weight;
                                volumeSettings.priority = profile.priority;
                            }

                            Volume[] volumes = Object.FindObjectsOfType<Volume>();
                            foreach (Volume volume in volumes)
                            {
                                if (volume.gameObject.name != "High Definition Environment Volume" && volume.gameObject.name != "High Definition Post Processing Environment Volume")
                                {
                                    Object.DestroyImmediate(volume.gameObject);
                                }
                            }

                            if (skyProfiles.useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                            {
                                if (!string.IsNullOrEmpty(hdVolumeProfile))
                                {
                                    volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                    VolumeProfile volumeProfile = volumeSettings.sharedProfile;

                                    if (volumeProfile != null)
                                    {
                                        EditorUtility.SetDirty(volumeProfile);

                                        //Visual Enviro
                                        VisualEnvironment visualEnvironmentSettings;
                                        if (volumeProfile.TryGet(out visualEnvironmentSettings))
                                        {
                                            visualEnvironmentSettings.skyType.value = 2;

                                            ProceduralSky proceduralSky;
                                            if (volumeProfile.TryGet(out proceduralSky))
                                            {
                                                proceduralSky.active = true;
                                                proceduralSky.enableSunDisk.value = profile.enableSunDisk;
                                                proceduralSky.sunSize.value = profile.proceduralSunSize;
                                                proceduralSky.sunSizeConvergence.value = profile.sunConvergence;
                                                proceduralSky.atmosphereThickness.value = profile.atmosphereThickness;
                                                proceduralSky.skyTint.value = profile.skyTint;
                                                proceduralSky.groundColor.value = profile.groundColor;
                                                proceduralSky.multiplier.value = profile.skyMultiplier;
                                            }

                                            HDRISky hDRISky;
                                            if (volumeProfile.TryGet(out hDRISky))
                                            {
                                                hDRISky.active = false;
                                            }

                                            GradientSky gradientSkyV;
                                            if (volumeProfile.TryGet(out gradientSkyV))
                                            {
                                                gradientSkyV.active = false;
                                            }

                                            if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.None;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = false;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = false;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = false;
                                                }

                                                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                                                if (densityVolumeObject1 != null)
                                                {
                                                    Object.DestroyImmediate(densityVolumeObject1);
                                                }
                                            }
                                            else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential || profile.fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.Exponential;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = true;
                                                    exponentialFog.density.value = profile.exponentialFogDensity;
                                                    exponentialFog.fogDistance.value = profile.fogDistance;
                                                    exponentialFog.fogBaseHeight.value = profile.exponentialBaseHeight;
                                                    exponentialFog.fogHeightAttenuation.value = profile.exponentialHeightAttenuation;
                                                    exponentialFog.maxFogDistance.value = profile.exponentialMaxFogDistance;
                                                    exponentialFog.mipFogNear.value = profile.exponentialMipFogNear;
                                                    exponentialFog.mipFogFar.value = profile.exponentialMipFogFar;
                                                    exponentialFog.mipFogMaxMip.value = profile.exponentialMipFogMaxMip;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = false;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = false;
                                                }

                                                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                                                if (densityVolumeObject1 != null)
                                                {
                                                    Object.DestroyImmediate(densityVolumeObject1);
                                                }
                                            }
                                            else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.Linear;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = false;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = true;
                                                    linearFog.density.value = profile.linearFogDensity;
                                                    linearFog.fogStart.value = profile.nearFogDistance;
                                                    linearFog.fogEnd.value = profile.fogDistance;
                                                    linearFog.fogHeightStart.value = profile.linearHeightStart;
                                                    linearFog.fogHeightEnd.value = profile.linearHeightEnd;
                                                    linearFog.maxFogDistance.value = profile.linearMaxFogDistance;
                                                    linearFog.mipFogNear.value = profile.linearMipFogNear;
                                                    linearFog.mipFogFar.value = profile.linearMipFogFar;
                                                    linearFog.mipFogMaxMip.value = profile.linearMipFogMaxMip;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = false;
                                                }

                                                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                                                if (densityVolumeObject1 != null)
                                                {
                                                    Object.DestroyImmediate(densityVolumeObject1);
                                                }
                                            }
                                            else
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = false;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = false;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = true;
                                                    volumetricFog.albedo.value = profile.fogColor;
                                                    volumetricFog.color.value = profile.fogColor;
                                                    volumetricFog.meanFreePath.value = profile.volumetricBaseFogDistance;
                                                    volumetricFog.baseHeight.value = profile.volumetricBaseFogHeight;
                                                    volumetricFog.meanHeight.value = profile.volumetricMeanHeight;
                                                    volumetricFog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                                                    volumetricFog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                                                    volumetricFog.maxFogDistance.value = profile.fogDistance;
                                                    volumetricFog.enableDistantFog.value = profile.volumetricEnableDistanceFog;
                                                    volumetricFog.colorMode.value = profile.volumetricFogColorMode;
                                                    volumetricFog.mipFogNear.value = profile.nearFogDistance;
                                                    volumetricFog.mipFogFar.value = profile.fogDistance;
                                                    volumetricFog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                                                }

                                                if (profile.useFogDensityVolume)
                                                {
                                                    Terrain terrain = Terrain.activeTerrain;
                                                    GameObject densityFogVolume = GameObject.Find("Density Volume");
                                                    if (densityFogVolume == null)
                                                    {
                                                        densityFogVolume = new GameObject("Density Volume");
                                                        densityFogVolume.transform.SetParent(parentObject.transform);

                                                        DensityVolume density = densityFogVolume.AddComponent<DensityVolume>();
                                                        density.parameters.albedo = profile.singleScatteringAlbedo;
                                                        density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                                        density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                                        density.parameters.textureTiling = profile.densityMaskTiling;

                                                        if (terrain != null)
                                                        {
                                                            density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                                        }
                                                        else
                                                        {
                                                            density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DensityVolume density = densityFogVolume.GetComponent<DensityVolume>();
                                                        if (density != null)
                                                        {
                                                            density.parameters.albedo = profile.singleScatteringAlbedo;
                                                            density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                                            density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                                            density.parameters.textureTiling = profile.densityMaskTiling;
                                                        }

                                                        if (terrain != null)
                                                        {
                                                            density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                                        }
                                                        else
                                                        {
                                                            density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    GameObject densityFogVolume = GameObject.Find("Density Volume");
                                                    if (densityFogVolume != null)
                                                    {
                                                        Object.DestroyImmediate(densityFogVolume);
                                                    }
                                                }
                                            }
                                        }

                                        //HD Shadows
                                        HDShadowSettings hDShadowSettings;
                                        if (volumeProfile.TryGet(out hDShadowSettings))
                                        {
                                            hDShadowSettings.maxShadowDistance.value = profile.shadowDistance;
                                            if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 1;
                                            }
                                            else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 2;
                                            }
                                            else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 3;
                                            }
                                            else
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 4;
                                            }

                                            hDShadowSettings.cascadeShadowSplit0.value = profile.cascadeSplit1;
                                            hDShadowSettings.cascadeShadowSplit1.value = profile.cascadeSplit2;
                                            hDShadowSettings.cascadeShadowSplit2.value = profile.cascadeSplit3;
                                        }

                                        //Contact Shadows
                                        ContactShadows contactShadowsSettings;
                                        if (volumeProfile.TryGet(out contactShadowsSettings))
                                        {
                                            contactShadowsSettings.active = profile.useContactShadows;
                                            contactShadowsSettings.length.value = profile.contactShadowsLength;
                                            contactShadowsSettings.distanceScaleFactor.value = profile.contactShadowsDistanceScaleFactor;
                                            contactShadowsSettings.maxDistance.value = profile.contactShadowsMaxDistance;
                                            contactShadowsSettings.fadeDistance.value = profile.contactShadowsFadeDistance;
                                            contactShadowsSettings.sampleCount.value = profile.contactShadowsSampleCount;
                                            contactShadowsSettings.opacity.value = profile.contactShadowsOpacity;
                                        }

#if UNITY_2018_3_OR_NEWER
                                        //Micro Shadows
                                        MicroShadowing microShadowingSettings;
                                        if (volumeProfile.TryGet(out microShadowingSettings))
                                        {
                                            microShadowingSettings.active = profile.useMicroShadowing;
                                            microShadowingSettings.opacity.value = profile.microShadowOpacity;
                                        }
#endif

                                        //Volumetric Light
                                        VolumetricLightingController volumetricLightingControllerSettings;
                                        if (volumeProfile.TryGet(out volumetricLightingControllerSettings))
                                        {
                                            volumetricLightingControllerSettings.depthExtent.value = profile.volumetricDistanceRange;
                                            volumetricLightingControllerSettings.sliceDistributionUniformity.value = profile.volumetricSliceDistributionUniformity;
                                        }

#if UNITY_2018_3_OR_NEWER
                                        //Indirect Lighting
                                        IndirectLightingController indirectLightingControllerSettings;
                                        if (volumeProfile.TryGet(out indirectLightingControllerSettings))
                                        {
                                            indirectLightingControllerSettings.indirectDiffuseIntensity.value = profile.indirectDiffuseIntensity;
                                            indirectLightingControllerSettings.indirectSpecularIntensity.value = profile.indirectSpecularIntensity;
                                        }
#endif

                                        //Screen Space Reflection
                                        ScreenSpaceReflection screenSpaceReflectionSettings;
                                        if (volumeProfile.TryGet(out screenSpaceReflectionSettings))
                                        {
                                            screenSpaceReflectionSettings.active = profile.enableScreenSpaceReflections;
                                            screenSpaceReflectionSettings.screenFadeDistance.value = profile.screenEdgeFadeDistance;
                                            screenSpaceReflectionSettings.rayMaxIterations.value = profile.maxNumberOfRaySteps;
                                            screenSpaceReflectionSettings.depthBufferThickness.value = profile.objectThickness;
                                            screenSpaceReflectionSettings.minSmoothness.value = profile.minSmoothness;
                                            screenSpaceReflectionSettings.smoothnessFadeStart.value = profile.smoothnessFadeStart;
                                            screenSpaceReflectionSettings.reflectSky.value = profile.reflectSky;
                                        }

                                        //Screen Space Refraction
                                        ScreenSpaceRefraction screenSpaceRefractionSettings;
                                        if (volumeProfile.TryGet(out screenSpaceRefractionSettings))
                                        {
                                            screenSpaceRefractionSettings.active = profile.enableScreenSpaceRefractions;
                                            screenSpaceRefractionSettings.screenFadeDistance.value = profile.screenWeightDistance;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(hdVolumeProfile))
                                {
                                    volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                                    VolumeProfile volumeProfile = volumeSettings.sharedProfile;

                                    if (volumeProfile != null)
                                    {
                                        EditorUtility.SetDirty(volumeProfile);

                                        //Visual Enviro
                                        VisualEnvironment visualEnvironmentSettings;
                                        if (volumeProfile.TryGet(out visualEnvironmentSettings))
                                        {
                                            if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.HDRISky)
                                            {
                                                visualEnvironmentSettings.skyType.value = 1;

                                                HDRISky hDRISky;
                                                if (volumeProfile.TryGet(out hDRISky))
                                                {
                                                    hDRISky.active = true;
                                                    if (!profile.isPWProfile)
                                                    {
                                                        hDRISky.hdriSky.value = profile.customSkybox;
                                                    }
                                                    else
                                                    {
                                                        hDRISky.hdriSky.value = profile.hDRISkybox;
                                                    }
                                                    hDRISky.skyIntensityMode.value = profile.hDRISkyIntensityMode;
                                                    hDRISky.exposure.value = profile.skyboxExposure;
                                                    hDRISky.multiplier.value = profile.skyMultiplier;
                                                    hDRISky.rotation.value = profile.skyboxRotation;

                                                    if (profile.hDRIUpdateMode == AmbientSkiesConsts.EnvironementSkyUpdateMode.OnChanged)
                                                    {
                                                        hDRISky.updateMode.value = EnvironementUpdateMode.OnChanged;
                                                    }
                                                    else if (profile.hDRIUpdateMode == AmbientSkiesConsts.EnvironementSkyUpdateMode.OnDemand)
                                                    {
                                                        hDRISky.updateMode.value = EnvironementUpdateMode.OnDemand;
                                                    }
                                                    else
                                                    {
                                                        hDRISky.updateMode.value = EnvironementUpdateMode.Realtime;
                                                    }
                                                }

                                                GradientSky gradientSkyV;
                                                if (volumeProfile.TryGet(out gradientSkyV))
                                                {
                                                    gradientSkyV.active = false;
                                                }

                                                ProceduralSky proceduralSkyV;
                                                if (volumeProfile.TryGet(out proceduralSkyV))
                                                {
                                                    proceduralSkyV.active = false;
                                                }
                                            }
                                            else if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                                            {
                                                visualEnvironmentSettings.skyType.value = 2;

                                                HDRISky hDRISky;
                                                if (volumeProfile.TryGet(out hDRISky))
                                                {
                                                    hDRISky.active = false;
                                                }

                                                GradientSky gradientSkyV;
                                                if (volumeProfile.TryGet(out gradientSkyV))
                                                {
                                                    gradientSkyV.active = false;
                                                }

                                                ProceduralSky proceduralSky;
                                                if (volumeProfile.TryGet(out proceduralSky))
                                                {
                                                    proceduralSky.active = true;
                                                    proceduralSky.enableSunDisk.value = profile.enableSunDisk;
                                                    proceduralSky.sunSize.value = profile.sunSize;
                                                    proceduralSky.sunSizeConvergence.value = profile.sunConvergence;
                                                    proceduralSky.atmosphereThickness.value = profile.atmosphereThickness;
                                                    proceduralSky.skyTint.value = profile.skyTint;
                                                    proceduralSky.groundColor.value = profile.groundColor;
                                                    proceduralSky.exposure.value = profile.skyExposure;
                                                    proceduralSky.multiplier.value = profile.skyMultiplier;
                                                    proceduralSky.includeSunInBaking.value = profile.includeSunInBaking;
                                                }
                                            }
                                            else
                                            {
                                                visualEnvironmentSettings.skyType.value = 3;

                                                HDRISky hDRISky;
                                                if (volumeProfile.TryGet(out hDRISky))
                                                {
                                                    hDRISky.active = false;
                                                }

                                                GradientSky gradientSky;
                                                if (volumeProfile.TryGet(out gradientSky))
                                                {
                                                    gradientSky.active = true;
                                                    gradientSky.top.value = profile.topColor;
                                                    gradientSky.middle.value = profile.middleColor;
                                                    gradientSky.bottom.value = profile.bottomColor;
                                                    gradientSky.gradientDiffusion.value = profile.gradientDiffusion;
                                                }

                                                ProceduralSky proceduralSkyV;
                                                if (volumeProfile.TryGet(out proceduralSkyV))
                                                {
                                                    proceduralSkyV.active = false;
                                                }
                                            }

                                            if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.None;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = false;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = false;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = false;
                                                }

                                                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                                                if (densityVolumeObject1 != null)
                                                {
                                                    Object.DestroyImmediate(densityVolumeObject1);
                                                }
                                            }
                                            else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential || profile.fogType == AmbientSkiesConsts.VolumeFogType.ExponentialSquared)
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.Exponential;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = true;
                                                    exponentialFog.density.value = profile.exponentialFogDensity;
                                                    exponentialFog.fogDistance.value = profile.fogDistance;
                                                    exponentialFog.fogBaseHeight.value = profile.exponentialBaseHeight;
                                                    exponentialFog.fogHeightAttenuation.value = profile.exponentialHeightAttenuation;
                                                    exponentialFog.maxFogDistance.value = profile.exponentialMaxFogDistance;
                                                    exponentialFog.mipFogNear.value = profile.exponentialMipFogNear;
                                                    exponentialFog.mipFogFar.value = profile.exponentialMipFogFar;
                                                    exponentialFog.mipFogMaxMip.value = profile.exponentialMipFogMaxMip;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = false;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = false;
                                                }

                                                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                                                if (densityVolumeObject1 != null)
                                                {
                                                    Object.DestroyImmediate(densityVolumeObject1);
                                                }
                                            }
                                            else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.Linear;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = false;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = true;
                                                    linearFog.density.value = profile.linearFogDensity;
                                                    linearFog.fogStart.value = profile.nearFogDistance;
                                                    linearFog.fogEnd.value = profile.fogDistance;
                                                    linearFog.fogHeightStart.value = profile.linearHeightStart;
                                                    linearFog.fogHeightEnd.value = profile.linearHeightEnd;
                                                    linearFog.maxFogDistance.value = profile.linearMaxFogDistance;
                                                    linearFog.mipFogNear.value = profile.linearMipFogNear;
                                                    linearFog.mipFogFar.value = profile.linearMipFogFar;
                                                    linearFog.mipFogMaxMip.value = profile.linearMipFogMaxMip;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = false;
                                                }

                                                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                                                if (densityVolumeObject1 != null)
                                                {
                                                    Object.DestroyImmediate(densityVolumeObject1);
                                                }
                                            }
                                            else
                                            {
                                                visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                                                ExponentialFog exponentialFog;
                                                if (volumeProfile.TryGet(out exponentialFog))
                                                {
                                                    exponentialFog.active = false;
                                                }

                                                LinearFog linearFog;
                                                if (volumeProfile.TryGet(out linearFog))
                                                {
                                                    linearFog.active = false;
                                                }

                                                VolumetricFog volumetricFog;
                                                if (volumeProfile.TryGet(out volumetricFog))
                                                {
                                                    volumetricFog.active = true;
                                                    volumetricFog.albedo.value = profile.fogColor;
                                                    volumetricFog.color.value = profile.fogColor;
                                                    volumetricFog.meanFreePath.value = profile.volumetricBaseFogDistance;
                                                    volumetricFog.baseHeight.value = profile.volumetricBaseFogHeight;
                                                    volumetricFog.meanHeight.value = profile.volumetricMeanHeight;
                                                    volumetricFog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                                                    volumetricFog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                                                    volumetricFog.maxFogDistance.value = profile.fogDistance;
                                                    volumetricFog.enableDistantFog.value = profile.volumetricEnableDistanceFog;
                                                    volumetricFog.colorMode.value = profile.volumetricFogColorMode;
                                                    volumetricFog.mipFogNear.value = profile.nearFogDistance;
                                                    volumetricFog.mipFogFar.value = profile.fogDistance;
                                                    volumetricFog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                                                }

                                                if (profile.useFogDensityVolume)
                                                {
                                                    Terrain terrain = Terrain.activeTerrain;
                                                    GameObject densityFogVolume = GameObject.Find("Density Volume");
                                                    if (densityFogVolume == null)
                                                    {
                                                        densityFogVolume = new GameObject("Density Volume");
                                                        densityFogVolume.transform.SetParent(parentObject.transform);

                                                        DensityVolume density = densityFogVolume.AddComponent<DensityVolume>();
                                                        density.parameters.albedo = profile.singleScatteringAlbedo;
                                                        density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                                        density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                                        density.parameters.textureTiling = profile.densityMaskTiling;

                                                        if (terrain != null)
                                                        {
                                                            density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                                        }
                                                        else
                                                        {
                                                            density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        DensityVolume density = densityFogVolume.GetComponent<DensityVolume>();
                                                        if (density != null)
                                                        {
                                                            density.parameters.albedo = profile.singleScatteringAlbedo;
                                                            density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                                            density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                                            density.parameters.textureTiling = profile.densityMaskTiling;
                                                        }

                                                        if (terrain != null)
                                                        {
                                                            density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                                        }
                                                        else
                                                        {
                                                            density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    GameObject densityFogVolume = GameObject.Find("Density Volume");
                                                    if (densityFogVolume != null)
                                                    {
                                                        Object.DestroyImmediate(densityFogVolume);
                                                    }
                                                }
                                            }
                                        }

                                        //HD Shadows
                                        HDShadowSettings hDShadowSettings;
                                        if (volumeProfile.TryGet(out hDShadowSettings))
                                        {
                                            hDShadowSettings.maxShadowDistance.value = profile.shadowDistance;
                                            if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 1;
                                            }
                                            else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 2;
                                            }
                                            else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 3;
                                            }
                                            else
                                            {
                                                hDShadowSettings.cascadeShadowSplitCount.value = 4;
                                            }

                                            hDShadowSettings.cascadeShadowSplit0.value = profile.cascadeSplit1;
                                            hDShadowSettings.cascadeShadowSplit1.value = profile.cascadeSplit2;
                                            hDShadowSettings.cascadeShadowSplit2.value = profile.cascadeSplit3;
                                        }

                                        //Contact Shadows
                                        ContactShadows contactShadowsSettings;
                                        if (volumeProfile.TryGet(out contactShadowsSettings))
                                        {
                                            contactShadowsSettings.active = profile.useContactShadows;
                                            contactShadowsSettings.length.value = profile.contactShadowsLength;
                                            contactShadowsSettings.distanceScaleFactor.value = profile.contactShadowsDistanceScaleFactor;
                                            contactShadowsSettings.maxDistance.value = profile.contactShadowsMaxDistance;
                                            contactShadowsSettings.fadeDistance.value = profile.contactShadowsFadeDistance;
                                            contactShadowsSettings.sampleCount.value = profile.contactShadowsSampleCount;
                                            contactShadowsSettings.opacity.value = profile.contactShadowsOpacity;
                                        }

#if UNITY_2018_3_OR_NEWER
                                        //Micro Shadows
                                        MicroShadowing microShadowingSettings;
                                        if (volumeProfile.TryGet(out microShadowingSettings))
                                        {
                                            microShadowingSettings.active = profile.useMicroShadowing;
                                            microShadowingSettings.opacity.value = profile.microShadowOpacity;
                                        }
#endif

                                        //Volumetric Light
                                        VolumetricLightingController volumetricLightingControllerSettings;
                                        if (volumeProfile.TryGet(out volumetricLightingControllerSettings))
                                        {
                                            volumetricLightingControllerSettings.depthExtent.value = profile.volumetricDistanceRange;
                                            volumetricLightingControllerSettings.sliceDistributionUniformity.value = profile.volumetricSliceDistributionUniformity;
                                        }

#if UNITY_2018_3_OR_NEWER
                                        //Indirect Lighting
                                        IndirectLightingController indirectLightingControllerSettings;
                                        if (volumeProfile.TryGet(out indirectLightingControllerSettings))
                                        {
                                            indirectLightingControllerSettings.indirectDiffuseIntensity.value = profile.indirectDiffuseIntensity;
                                            indirectLightingControllerSettings.indirectSpecularIntensity.value = profile.indirectSpecularIntensity;
                                        }
#endif

                                        //Screen Space Reflection
                                        ScreenSpaceReflection screenSpaceReflectionSettings;
                                        if (volumeProfile.TryGet(out screenSpaceReflectionSettings))
                                        {
                                            screenSpaceReflectionSettings.active = profile.enableScreenSpaceReflections;
                                            screenSpaceReflectionSettings.screenFadeDistance.value = profile.screenEdgeFadeDistance;
                                            screenSpaceReflectionSettings.rayMaxIterations.value = profile.maxNumberOfRaySteps;
                                            screenSpaceReflectionSettings.depthBufferThickness.value = profile.objectThickness;
                                            screenSpaceReflectionSettings.minSmoothness.value = profile.minSmoothness;
                                            screenSpaceReflectionSettings.smoothnessFadeStart.value = profile.smoothnessFadeStart;
                                            screenSpaceReflectionSettings.reflectSky.value = profile.reflectSky;
                                        }

                                        //Screen Space Refraction
                                        ScreenSpaceRefraction screenSpaceRefractionSettings;
                                        if (volumeProfile.TryGet(out screenSpaceRefractionSettings))
                                        {
                                            screenSpaceRefractionSettings.active = profile.enableScreenSpaceRefractions;
                                            screenSpaceRefractionSettings.screenFadeDistance.value = profile.screenWeightDistance;
                                        }
                                    }
                                }
                            }
                        }
                        else if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            volumeSettings = volumeObject.GetComponent<Volume>();
                            if (volumeSettings != null)
                            {
                                Object.DestroyImmediate(volumeSettings);
                            }
                        }
                        else
                        {
                            volumeSettings = volumeObject.GetComponent<Volume>();
                            volumeSettings.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));
                            VolumeProfile volumeProfile = volumeSettings.sharedProfile;

                            if (volumeProfile != null)
                            {
                                EditorUtility.SetDirty(volumeProfile);

                                //Visual Enviro
                                VisualEnvironment visualEnvironmentSettings;
                                if (volumeProfile.TryGet(out visualEnvironmentSettings))
                                {
                                    ProceduralSky proceduralSky;
                                    if (volumeProfile.TryGet(out proceduralSky))
                                    {
                                        proceduralSky.active = true;
                                        proceduralSky.enableSunDisk.value = profile.enableSunDisk;
                                        proceduralSky.sunSize.value = profile.proceduralSunSize;
                                        proceduralSky.sunSizeConvergence.value = profile.sunConvergence;
                                        proceduralSky.atmosphereThickness.value = profile.atmosphereThickness;
                                        proceduralSky.skyTint.value = profile.skyTint;
                                        proceduralSky.groundColor.value = profile.groundColor;
                                        proceduralSky.exposure.value = profile.skyExposure;
                                        proceduralSky.multiplier.value = profile.skyMultiplier;
                                    }

                                    if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                                    {
                                        visualEnvironmentSettings.fogType.value = FogType.None;

                                        ExponentialFog exponentialFog;
                                        if (volumeProfile.TryGet(out exponentialFog))
                                        {
                                            exponentialFog.active = false;
                                        }

                                        LinearFog linearFog;
                                        if (volumeProfile.TryGet(out linearFog))
                                        {
                                            linearFog.active = false;
                                        }

                                        VolumetricFog volumetricFog;
                                        if (volumeProfile.TryGet(out volumetricFog))
                                        {
                                            volumetricFog.active = false;
                                        }
                                    }
                                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                                    {
                                        visualEnvironmentSettings.fogType.value = FogType.Exponential;

                                        ExponentialFog exponentialFog;
                                        if (volumeProfile.TryGet(out exponentialFog))
                                        {
                                            exponentialFog.active = true;
                                            exponentialFog.density.value = profile.exponentialFogDensity;
                                            exponentialFog.fogDistance.value = profile.fogDistance;
                                            exponentialFog.fogBaseHeight.value = profile.exponentialBaseHeight;
                                            exponentialFog.fogHeightAttenuation.value = profile.exponentialHeightAttenuation;
                                            exponentialFog.maxFogDistance.value = profile.exponentialMaxFogDistance;
                                            exponentialFog.mipFogNear.value = profile.exponentialMipFogNear;
                                            exponentialFog.mipFogFar.value = profile.exponentialMipFogFar;
                                            exponentialFog.mipFogMaxMip.value = profile.exponentialMipFogMaxMip;
                                        }

                                        LinearFog linearFog;
                                        if (volumeProfile.TryGet(out linearFog))
                                        {
                                            linearFog.active = false;
                                        }

                                        VolumetricFog volumetricFog;
                                        if (volumeProfile.TryGet(out volumetricFog))
                                        {
                                            volumetricFog.active = false;
                                        }
                                    }
                                    else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                                    {
                                        visualEnvironmentSettings.fogType.value = FogType.Linear;

                                        ExponentialFog exponentialFog;
                                        if (volumeProfile.TryGet(out exponentialFog))
                                        {
                                            exponentialFog.active = false;
                                        }

                                        LinearFog linearFog;
                                        if (volumeProfile.TryGet(out linearFog))
                                        {
                                            linearFog.active = true;
                                            linearFog.density.value = profile.linearFogDensity;
                                            linearFog.fogStart.value = profile.nearFogDistance;
                                            linearFog.fogEnd.value = profile.fogDistance;
                                            linearFog.fogHeightStart.value = profile.linearHeightStart;
                                            linearFog.fogHeightEnd.value = profile.linearHeightEnd;
                                            linearFog.maxFogDistance.value = profile.linearMaxFogDistance;
                                            linearFog.mipFogNear.value = profile.linearMipFogNear;
                                            linearFog.mipFogFar.value = profile.linearMipFogFar;
                                            linearFog.mipFogMaxMip.value = profile.linearMipFogMaxMip;
                                        }

                                        VolumetricFog volumetricFog;
                                        if (volumeProfile.TryGet(out volumetricFog))
                                        {
                                            volumetricFog.active = false;
                                        }
                                    }
                                    else
                                    {
                                        visualEnvironmentSettings.fogType.value = FogType.Volumetric;

                                        ExponentialFog exponentialFog;
                                        if (volumeProfile.TryGet(out exponentialFog))
                                        {
                                            exponentialFog.active = false;
                                        }

                                        LinearFog linearFog;
                                        if (volumeProfile.TryGet(out linearFog))
                                        {
                                            linearFog.active = false;
                                        }

                                        VolumetricFog volumetricFog;
                                        if (volumeProfile.TryGet(out volumetricFog))
                                        {
                                            volumetricFog.active = true;
                                            volumetricFog.albedo.value = profile.fogColor;
                                            volumetricFog.color.value = profile.fogColor;
                                            volumetricFog.meanFreePath.value = profile.volumetricBaseFogDistance;
                                            volumetricFog.baseHeight.value = profile.volumetricBaseFogHeight;
                                            volumetricFog.meanHeight.value = profile.volumetricMeanHeight;
                                            volumetricFog.anisotropy.value = profile.volumetricGlobalAnisotropy;
                                            volumetricFog.globalLightProbeDimmer.value = profile.volumetricGlobalLightProbeDimmer;
                                            volumetricFog.maxFogDistance.value = profile.fogDistance;
                                            volumetricFog.enableDistantFog.value = profile.volumetricEnableDistanceFog;
                                            volumetricFog.colorMode.value = profile.volumetricFogColorMode;
                                            volumetricFog.mipFogNear.value = profile.nearFogDistance;
                                            volumetricFog.mipFogFar.value = profile.fogDistance;
                                            volumetricFog.mipFogMaxMip.value = profile.volumetricMipFogMaxMip;
                                        }

                                        if (profile.useFogDensityVolume)
                                        {
                                            Terrain terrain = Terrain.activeTerrain;
                                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                                            if (densityFogVolume == null)
                                            {
                                                densityFogVolume = new GameObject("Density Volume");
                                                densityFogVolume.transform.SetParent(parentObject.transform);

                                                DensityVolume density = densityFogVolume.AddComponent<DensityVolume>();
                                                density.parameters.albedo = profile.singleScatteringAlbedo;
                                                density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                                density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                                density.parameters.textureTiling = profile.densityMaskTiling;

                                                if (terrain != null)
                                                {
                                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                                }
                                                else
                                                {
                                                    density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                                }
                                            }
                                            else
                                            {
                                                DensityVolume density = densityFogVolume.GetComponent<DensityVolume>();
                                                if (density != null)
                                                {
                                                    density.parameters.albedo = profile.singleScatteringAlbedo;
                                                    density.parameters.meanFreePath = profile.densityVolumeFogDistance;
                                                    density.parameters.volumeMask = profile.fogDensityMaskTexture;
                                                    density.parameters.textureTiling = profile.densityMaskTiling;
                                                }

                                                if (terrain != null)
                                                {
                                                    density.parameters.size = new Vector3(terrain.terrainData.size.x, terrain.terrainData.size.y / 2f, terrain.terrainData.size.z);
                                                }
                                                else
                                                {
                                                    density.parameters.size = new Vector3(2000f, 400f, 2000f);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            GameObject densityFogVolume = GameObject.Find("Density Volume");
                                            if (densityFogVolume != null)
                                            {
                                                Object.DestroyImmediate(densityFogVolume);
                                            }
                                        }
                                    }
                                }

                                //HD Shadows
                                HDShadowSettings hDShadowSettings;
                                if (volumeProfile.TryGet(out hDShadowSettings))
                                {
                                    hDShadowSettings.maxShadowDistance.value = profile.shadowDistance;
                                    if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount1)
                                    {
                                        hDShadowSettings.cascadeShadowSplitCount.value = 1;
                                    }
                                    else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount2)
                                    {
                                        hDShadowSettings.cascadeShadowSplitCount.value = 2;
                                    }
                                    else if (profile.cascadeCount == AmbientSkiesConsts.ShadowCascade.CascadeCount3)
                                    {
                                        hDShadowSettings.cascadeShadowSplitCount.value = 3;
                                    }
                                    else
                                    {
                                        hDShadowSettings.cascadeShadowSplitCount.value = 4;
                                    }

                                    hDShadowSettings.cascadeShadowSplit0.value = profile.cascadeSplit1;
                                    hDShadowSettings.cascadeShadowSplit1.value = profile.cascadeSplit2;
                                    hDShadowSettings.cascadeShadowSplit2.value = profile.cascadeSplit3;
                                }

                                //Contact Shadows
                                ContactShadows contactShadowsSettings;
                                if (volumeProfile.TryGet(out contactShadowsSettings))
                                {
                                    contactShadowsSettings.active = profile.useContactShadows;
                                    contactShadowsSettings.length.value = profile.contactShadowsLength;
                                    contactShadowsSettings.distanceScaleFactor.value = profile.contactShadowsDistanceScaleFactor;
                                    contactShadowsSettings.maxDistance.value = profile.contactShadowsMaxDistance;
                                    contactShadowsSettings.fadeDistance.value = profile.contactShadowsFadeDistance;
                                    contactShadowsSettings.sampleCount.value = profile.contactShadowsSampleCount;
                                    contactShadowsSettings.opacity.value = profile.contactShadowsOpacity;
                                }

#if UNITY_2018_3_OR_NEWER
                                //Micro Shadows
                                MicroShadowing microShadowingSettings;
                                if (volumeProfile.TryGet(out microShadowingSettings))
                                {
                                    microShadowingSettings.active = profile.useMicroShadowing;
                                    microShadowingSettings.opacity.value = profile.microShadowOpacity;
                                }
#endif

                                //Volumetric Light
                                VolumetricLightingController volumetricLightingControllerSettings;
                                if (volumeProfile.TryGet(out volumetricLightingControllerSettings))
                                {
                                    volumetricLightingControllerSettings.depthExtent.value = profile.volumetricDistanceRange;
                                    volumetricLightingControllerSettings.sliceDistributionUniformity.value = profile.volumetricSliceDistributionUniformity;
                                }

#if UNITY_2018_3_OR_NEWER
                                //Indirect Lighting
                                IndirectLightingController indirectLightingControllerSettings;
                                if (volumeProfile.TryGet(out indirectLightingControllerSettings))
                                {
                                    indirectLightingControllerSettings.indirectDiffuseIntensity.value = profile.indirectDiffuseIntensity;
                                    indirectLightingControllerSettings.indirectSpecularIntensity.value = profile.indirectSpecularIntensity;
                                }
#endif

                                //Screen Space Reflection
                                ScreenSpaceReflection screenSpaceReflectionSettings;
                                if (volumeProfile.TryGet(out screenSpaceReflectionSettings))
                                {
                                    screenSpaceReflectionSettings.active = profile.enableScreenSpaceReflections;
                                    screenSpaceReflectionSettings.screenFadeDistance.value = profile.screenEdgeFadeDistance;
                                    screenSpaceReflectionSettings.rayMaxIterations.value = profile.maxNumberOfRaySteps;
                                    screenSpaceReflectionSettings.depthBufferThickness.value = profile.objectThickness;
                                    screenSpaceReflectionSettings.minSmoothness.value = profile.minSmoothness;
                                    screenSpaceReflectionSettings.smoothnessFadeStart.value = profile.smoothnessFadeStart;
                                    screenSpaceReflectionSettings.reflectSky.value = profile.reflectSky;
                                }

                                //Screen Space Refraction
                                ScreenSpaceRefraction screenSpaceRefractionSettings;
                                if (volumeProfile.TryGet(out screenSpaceRefractionSettings))
                                {
                                    screenSpaceRefractionSettings.active = profile.enableScreenSpaceRefractions;
                                    screenSpaceRefractionSettings.screenFadeDistance.value = profile.screenWeightDistance;
                                }
                            }
                        }

                        //Baking Sky Setup
#if UNITY_2018_3
                    BakingSky bakingSkySettings = volumeObject.GetComponent<BakingSky>();
                    if (profile.useBakingSky)
                    {
                        if (skyProfiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                        {
                            if (bakingSkySettings == null)
                            {
                                bakingSkySettings = volumeObject.AddComponent<BakingSky>();
                            }

                            bakingSkySettings.profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));

                            if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.HDRISky)
                            {
                                bakingSkySettings.bakingSkyUniqueID = 1;
                            }
                            else if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                            {
                                bakingSkySettings.bakingSkyUniqueID = 2;
                            }
                            else
                            {
                                bakingSkySettings.bakingSkyUniqueID = 3;
                            }
                        }
                        else
                        {
                            if (bakingSkySettings != null)
                            {
                                Object.DestroyImmediate(bakingSkySettings);
                            }
                        }
                    }
                    else
                    {
                        if (bakingSkySettings != null)
                        {
                            Object.DestroyImmediate(bakingSkySettings);
                        }
                    }
#elif UNITY_2019_1_OR_NEWER
                        StaticLightingSky bakingSkySettings = volumeObject.GetComponent<StaticLightingSky>();
                        if (profile.useBakingSky)
                        {
                            if (skyProfiles.systemTypes != AmbientSkiesConsts.SystemTypes.ThirdParty)
                            {
                                if (bakingSkySettings == null)
                                {
                                    bakingSkySettings = volumeObject.AddComponent<StaticLightingSky>();
                                }

                                bakingSkySettings.profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath(hdVolumeProfile));

                                if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.HDRISky)
                                {
                                    bakingSkySettings.staticLightingSkyUniqueID = 1;
                                }
                                else if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                                {
                                    bakingSkySettings.staticLightingSkyUniqueID = 2;
                                }
                                else
                                {
                                    bakingSkySettings.staticLightingSkyUniqueID = 3;
                                }
                            }
                            else
                            {
                                if (bakingSkySettings != null)
                                {
                                    Object.DestroyImmediate(bakingSkySettings);
                                }
                            }
                        }
                        else
                        {
                            if (bakingSkySettings != null)
                            {
                                Object.DestroyImmediate(bakingSkySettings);
                            }
                        }
#endif
#endif
                    }
                    else
                    {
#if HDPipeline
                        VolumeProfile volumeSettings;

                        GameObject volumeObject = GameObject.Find(volumeName);
                        if (volumeObject == null)
                        {
                            volumeObject = new GameObject(volumeName);
                            volumeObject.AddComponent<Volume>();
                            volumeObject.transform.SetParent(parentObject.transform);

                            Volume volumeSet = volumeObject.GetComponent<Volume>();
                            volumeSet.isGlobal = true;
                            volumeSet.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SkyboxUtils.GetAssetPath("Ambient Skies HD Volume Profile"));

                            volumeSettings = volumeSet.sharedProfile;
                        }
                        else
                        {
                            volumeSettings = volumeObject.GetComponent<Volume>().sharedProfile;
                        }

                        if (volumeSettings != null)
                        {
                            EditorUtility.SetDirty(volumeSettings);

                            LinearFog linearFog;
                            if (volumeSettings.TryGet(out linearFog))
                            {
                                linearFog.active = true;
                                linearFog.density.value = 1f;
                                linearFog.fogStart.value = 150f;
                                linearFog.fogEnd.value = 850f;
                                linearFog.fogHeightStart.value = -700f;
                                linearFog.fogHeightEnd.value = 400f;
                                linearFog.maxFogDistance.value = 5000f;
                                linearFog.mipFogNear.value = 0f;
                                linearFog.mipFogFar.value = 1000f;
                                linearFog.mipFogMaxMip.value = 0.5f;
                            }

                            VolumetricFog volumetricFog;
                            if (volumeSettings.TryGet(out volumetricFog))
                            {
                                volumetricFog.active = false;
                            }

                            ExponentialFog exponentialFog;
                            if (volumeSettings.TryGet(out exponentialFog))
                            {
                                exponentialFog.active = false;
                            }

                            VisualEnvironment visualEnvironment;
                            if (volumeSettings.TryGet(out visualEnvironment))
                            {
                                visualEnvironment.fogType.value = FogType.Linear;
                                visualEnvironment.skyType.value = 2;
                            }

                            HDRISky hDRISky;
                            if (volumeSettings.TryGet(out hDRISky))
                            {
                                hDRISky.active = false;
                            }

                            ProceduralSky proceduralSky;
                            if (volumeSettings.TryGet(out proceduralSky))
                            {
                                proceduralSky.active = true;
                                proceduralSky.sunSize.value = 0.03f;
                                proceduralSky.sunSizeConvergence.value = 10f;
                            }

                            GradientSky gradientSky;
                            if (volumeSettings.TryGet(out gradientSky))
                            {
                                gradientSky.active = false;
                            }
                        }

                        GameObject mainLight = SkyboxUtils.GetMainDirectionalLight();
                        if (mainLight != null)
                        {
                            Light light = mainLight.GetComponent<Light>();
                            if (light != null)
                            {
                                light.color = SkyboxUtils.GetColorFromHTML("FFDCC5");
                                light.intensity = 3.14f;
                            }

                            HDAdditionalLightData hDAdditionalLightData = mainLight.GetComponent<HDAdditionalLightData>();
                            if (hDAdditionalLightData != null)
                            {
                                hDAdditionalLightData.intensity = 3.14f;
                            }
                        }
#endif
                    }
                }
                else
                {
                    GameObject environmentVolume = GameObject.Find(volumeName);
                    if (environmentVolume != null)
                    {
                        Object.DestroyImmediate(environmentVolume);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");
                }
            }
            else
            {
                if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
                {
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
                else
                {
                    //Removes the object
                    GameObject volumeObject = GameObject.Find(volumeName);
                    if (volumeObject != null)
                    {
                        Object.DestroyImmediate(volumeObject);
                    }

                    SkyboxUtils.DestroyParent("Ambient Skies Environment");
                }
            }
        }

        /// <summary>
        /// Fixes realtime reflection probes in hd to set the main one to baked
        /// </summary>
        public static void FixHDReflectionProbes()
        {
            ReflectionProbe[] reflectionProbes = Object.FindObjectsOfType<ReflectionProbe>();
            if (reflectionProbes != null)
            {
                foreach(ReflectionProbe probe in reflectionProbes)
                {
                    if (probe.name == "Global Reflection Probe")
                    {
                        probe.mode = ReflectionProbeMode.Baked;
                    }
                }
#if HDPipeline
                HDAdditionalReflectionData[] reflectionData = Object.FindObjectsOfType<HDAdditionalReflectionData>();
                if (reflectionData != null)
                {
                    foreach(HDAdditionalReflectionData data in reflectionData)
                    {
                        if (data.gameObject.name == "Global Reflection Probe")
                        {
#if UNITY_2018_3
                            data.mode = ReflectionProbeMode.Baked;
#elif UNITY_2019_1_OR_NEWER
                            data.mode = ProbeSettings.Mode.Baked;
#endif
                        }
                    }
                }
#endif
            }
        }

        #endregion

        #region Set Scripting Defines

        /// <summary>
        /// Set up the High Definition defines
        /// </summary>
        public static void SetHighDefinitionDefinesStatic()
        {
#if UNITY_2018_3_OR_NEWER
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject HDPipeline
            if (!currBuildSettings.Contains("HDPipeline"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";HDPipeline");
            }
#endif
        }

        /// <summary>
        /// Set up the Lightweight defines
        /// </summary>
        public static void SetLightweightDefinesStatic()
        {
#if UNITY_2018_3_OR_NEWER
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject LWPipeline
            if (!currBuildSettings.Contains("LWPipeline"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";LWPipeline");
            }
#endif
        }

        /// <summary>
        /// Set up the Lightweight defines
        /// </summary>
        public static void SetAmbientSkiesDefinesStatic()
        {
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject LWPipeline
            if (!currBuildSettings.Contains("AMBIENT_SKIES"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";AMBIENT_SKIES");
            }
        }

        #endregion

        #region Helper Methos

        /// <summary>
        /// Set linear deffered lighting (the best for outdoor scenes)
        /// </summary>
        public static void SetLinearDeferredLighting()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;
#if UNITY_5_5_OR_NEWER
            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            tier1.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1, tier1);
            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            tier2.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2, tier2);
            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
            tier3.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3, tier3);
#else
            PlayerSettings.renderingPath = RenderingPath.DeferredShading;
#endif
        }

        /// <summary>
        /// Get the currently active terrain - or any terrain
        /// </summary>
        /// <returns>A terrain if there is one</returns>
        public static Terrain[] GetActiveTerrain()
        {
            //Grab active terrain if we can
            Terrain[] terrain = Terrain.activeTerrains;
            if (terrain != null)
            {
                return terrain;
            }

            return null;
        }

        #endregion

        #endregion
    }
}