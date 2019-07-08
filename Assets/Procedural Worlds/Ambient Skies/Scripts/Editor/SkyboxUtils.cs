//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
#if HDPipeline
using UnityEngine.Experimental.Rendering.HDPipeline;
#endif
#if LWPipeline && UNITY_2018_3
using UnityEngine.Experimental.Rendering.LightweightPipeline;
#endif
#if LWPipeline && UNITY_2019_1_OR_NEWER
using UnityEngine.Rendering.LWRP;
#endif
using UnityEditor.SceneManagement;

namespace AmbientSkies
{
    /// <summary>
    /// Skybox utilities
    /// </summary>
    public static class SkyboxUtils
    {
        #region Utils

        #region Get/Set From Profile

        /// <summary>
        /// Get current profile index of currently active skybox
        /// </summary>
        /// <param name="profiles">Profile list to search</param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromActiveSkybox(AmbientSkyProfiles profiles, AmbientSkyboxProfile skyboxProfile, bool isSkyboxLoaded)
        {
            if (profiles.m_selectedRenderPipeline == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                Material skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
                GameObject volumeObject = GameObject.Find("High Definition Environment Volume");
                if (volumeObject == null)
                {
                    if (skyMaterial.shader == Shader.Find("Skybox/Procedural"))
                    {
                        //Grab the texture
                        Texture skyTexture = skyMaterial.GetTexture("_Tex");
                        for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                        {
                            if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                            {
                                return profiles.m_skyProfiles[texIdx].profileIndex;
                            }
                            if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                            {
                                return profiles.m_skyProfiles[texIdx].profileIndex;
                            }
                        }
                    }
                    else
                    {
                        //Grab the texture
                        Texture skyTexture = skyMaterial.GetTexture("_Tex");
                        for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                        {
                            if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                            {
                                Debug.Log("Setting User settings option");
                                return profiles.m_skyProfiles[texIdx].profileIndex;
                            }
                            if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                            {
                                Debug.Log("Return normal settings");
                                return profiles.m_skyProfiles[texIdx].profileIndex;
                            }
                        }
                    }

                    return -1;
                }
#if HDPipeline
                else
                {
                    VolumeProfile volumeProfiles = GetVolumeProfile();
                    if (volumeProfiles == null)
                    {
                        return -1;
                    }
                    else
                    {
                        HDRISky hDRISky;
                        if (volumeProfiles.TryGet(out hDRISky))
                        {
                            Cubemap skyCubemap = null;

                            if (hDRISky.active)
                            {
                                skyCubemap = hDRISky.hdriSky.value;
                                if (skyCubemap == null)
                                {
                                    return -1;
                                }
                                else
                                {
                                    for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                    {
                                        if (profiles.m_skyProfiles[texIdx].assetName == skyCubemap.name)
                                        {
                                            return profiles.m_skyProfiles[texIdx].profileIndex;
                                        }
                                    }
                                }
                            }

                            ProceduralSky proceduralSky;
                            if (volumeProfiles.TryGet(out proceduralSky))
                            {
                                if (proceduralSky.active)
                                {
                                    skyCubemap = AssetDatabase.LoadAssetAtPath<Cubemap>(GetAssetPath("Sky05Low"));
                                    if (skyCubemap == null)
                                    {
                                        return -1;
                                    }
                                    else
                                    {
                                        for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                        {
                                            if (profiles.m_skyProfiles[texIdx].assetName == skyCubemap.name)
                                            {
                                                return profiles.m_skyProfiles[texIdx].profileIndex;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_skyProfiles[texIdx].assetName == skyCubemap.name)
                                    {
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                        }
                    }
                }
#endif
            }
            else
            {
                if (isSkyboxLoaded)
                {
                    Material skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
                    //See if we have one of our own skyboxes, abort if not
                    if (skyMaterial == null)
                    {
                        return -1;
                    }
                    else
                    {
                        Material skybox1 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky01High"));
                        Material skybox2 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky01Low"));
                        Material skybox3 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky01Mid"));
                        Material skybox4 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky05High"));
                        Material skybox5 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky05Low"));
                        Material skybox6 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky05Mid"));
                        Material skybox7 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky06High"));
                        Material skybox8 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky06Low"));
                        Material skybox9 = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Sky06Mid"));

                        if (skybox1 == null && skybox2 == null && skybox3 == null && skybox4 == null && skybox5 == null && skybox6 == null && skybox7 == null && skybox8 == null && skybox9 == null)
                        {
                            for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                            {
                                if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                {
                                    return profiles.m_skyProfiles[texIdx].profileIndex;
                                }
                            }
                        }
                        else
                        {
                            RenderSettings.skybox = skyMaterial;

                            if (skyMaterial.shader == Shader.Find("Skybox/Procedural"))
                            {
                                //Grab the texture
                                Texture skyTexture = skyMaterial.GetTexture("_Tex");
                                for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                    {
                                        Debug.Log("Setting User settings option");
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                    if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                                    {
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                            else
                            {
                                //Grab the texture
                                Texture skyTexture = skyMaterial.GetTexture("_Tex");
                                for (int texIdx = 0; texIdx < profiles.m_skyProfiles.Count; texIdx++)
                                {
                                    if (profiles.m_skyProfiles[texIdx].isPWProfile == false)
                                    {
                                        Debug.Log("Setting User settings option");
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                    if (profiles.m_skyProfiles[texIdx].assetName == skyTexture.name)
                                    {
                                        return profiles.m_skyProfiles[texIdx].profileIndex;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Using user lighting and Skybox, changes will not be made");
                }
            }

            return -1;
        }

        /// <summary>
        /// Get current profile index that has the profile name
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="name"></param>
        /// <returns>Profile index, or -1 if failed</returns>
        public static int GetProfileIndexFromProfileName(AmbientSkyProfiles profiles, string name)
        {
            for (int idx = 0; idx < profiles.m_skyProfiles.Count; idx++)
            {
                if (profiles.m_skyProfiles[idx].name == name)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Create a skybox profile from the active skybox
        /// </summary>
        /// <param name="profiles">Profiles list</param>
        /// <returns>True if sucessful</returns>
        public static bool CreateProfileFromActiveSkybox(AmbientSkyProfiles profiles)
        {
            //Check the profiles
            if (profiles == null)
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Profile list must not be null - Aborting!");
                return false;
            }

            //Get the skybox
            Material skyMaterial = RenderSettings.skybox;
            if (skyMaterial == null)
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Missing skybox - Aborting!");
                return false;
            }

            if (skyMaterial.name != "Ambient Skies Skybox")
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Can only create from ambient skies profile - Aborting!");
                return false;
            }

            //Conditionally load the skybox texture 
            Texture skyboxTexture = skyMaterial.GetTexture("_Tex");
            if (skyboxTexture == null)
            {
                Debug.LogError("AmbientSkies:CreateProfileFromActiveSkybox() : Missing skybox texture - Aborting!");
                return false;
            }

            //Start picking values out of the skybox
            AmbientSkyboxProfile profile = new AmbientSkyboxProfile();

            profile.name = skyboxTexture.name;
            profile.assetName = skyboxTexture.name;
            profile.skyboxTint = skyMaterial.GetColor("_Tint");
            profile.skyboxExposure = skyMaterial.GetFloat("_Exposure");
            profile.skyboxRotation = skyMaterial.GetFloat("_Rotation");
            profile.skyboxGroundIntensity = RenderSettings.ambientIntensity;
            profile.fogColor = RenderSettings.fogColor;
            profile.fogDistance = RenderSettings.fogEndDistance;

            //Set up the light if we have one
            GameObject lightObj = GetMainDirectionalLight();

            if (lightObj != null)
            {
                //Get sun rotation
                profile.sunRotation = lightObj.transform.rotation.eulerAngles;

                //Now get the light values
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    profile.sunColor = light.color;
                    profile.sunIntensity = light.intensity;
                }
            }

            //Copy to defaults
            profile.SaveCurrentToDefault();

            //Add it and exit
            profiles.m_skyProfiles.Add(profile);
            return true;
        }

        /// <summary>
        /// Load the selected profile and apply to the skybox
        /// </summary>
        /// <param name="profiles">The profiles object</param>
        /// <param name="profileName">The name of the profile to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileName(AmbientSkyProfiles profiles, string profileName, bool useDefaults)
        {
            AmbientSkyboxProfile p = profiles.m_skyProfiles.Find(x => x.name == profileName);
            if (p == null)
            {
                Debug.LogWarning("Invalid profile name supplied, can not apply skybox settings!");
                return;
            }
            SetSkybox(profiles, p, useDefaults, profiles.m_selectedRenderPipeline);            
        }

        /// <summary>
        /// Load the selected profile and apply to the skybox
        /// </summary>
        /// <param name="profiles">The profiles to get the asset from</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromAssetName(AmbientSkyProfiles profiles, string assetName, bool useDefaults)
        {
            AmbientSkyboxProfile p = profiles.m_skyProfiles.Find(x => x.assetName == assetName);
            if (p == null)
            {
                Debug.LogWarning("Invalid asset name supplied, can not apply skybox settings!");
                return;
            }
            SetSkybox(profiles, p, useDefaults, profiles.m_selectedRenderPipeline);
        }

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="isPWProfile"></param>
        /// <returns></returns>
        public static int GetFromIsPWProfile(AmbientSkyProfiles profiles, bool isPWProfile)
        {
            for (int idx = 0; idx < profiles.m_skyProfiles.Count; idx++)
            {
                if (profiles.m_skyProfiles[idx].isPWProfile == isPWProfile)
                {
                    return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Loads the first is or is not PW Profile
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="isPWProfile"></param>
        /// <returns></returns>
        public static AmbientSkyboxProfile GetProfileFromIsPWProfile(AmbientSkyProfiles profiles, bool isPWProfile)
        {
            AmbientSkyboxProfile profile = profiles.m_skyProfiles.Find(x => x.isPWProfile == false);
            if (profile != null)
            {
                return profile;
            }
            return null;
        }

        /// <summary>
        /// Load the selected profile and apply to the skybox
        /// </summary>
        /// <param name="profiles">The profiles object</param>
        /// <param name="profileIndex">The zero based index to load</param>
        /// <param name="useDefaults">Whether to load default settings or current user settings.</param>
        public static void SetFromProfileIndex(AmbientSkyProfiles profiles, int profileIndex, bool useDefaults)
        {
            if (profileIndex < 0 || profileIndex >= profiles.m_skyProfiles.Count)
            {
                Debug.LogWarning("Invalid profile index selected, can not apply skybox settings!");
                return;
            }

            SetSkybox(profiles, profiles.m_skyProfiles[profileIndex], useDefaults, profiles.m_selectedRenderPipeline);
        }

        #endregion

        #region Skybox Setup

        /// <summary>
        /// This method will set up the skybox from the specified profile
        /// </summary>
        /// <param name="profile">Profile to set the skybox up from</param>
        /// <param name="useDefaults">Use default settings or current settings</param>
        public static void SetSkybox(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, bool useDefaults, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            //Check the profile
            if (profile == null)
            {
                Debug.LogError("AmbientSkies:SetSkybox() : Profile must not be null - Aborting!");
                return;
            }

            if (profile.useSkies)
            {
#if GAIA_PRESENT
                if (RenderSettings.skybox == null || RenderSettings.skybox.name != "Ambient Skies Skybox")
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
                }

                SetGaiaParenting(true);
#endif
                if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.AmbientSkies)
                {
                    RemoveNewSceneObject(skyProfiles);

                    SetEnableSunDisk(profile);

                    if (skyProfiles.useTimeOfDay == AmbientSkiesConsts.DisableAndEnable.Enable)
                    {
                        AddTimeOfDay(profile, skyProfiles);

                        SetTimeOfDaySettings(profile, skyProfiles);

                        SetQualitySettings(profile);

                        AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile");
                    }
                    else
                    {
                        //Quality shadowmask settings
                        QualitySettings.shadowmaskMode = profile.shadowmaskMode;

                        RemoveEnviro(true);

                        RemoveTimeOfDay();

                        SetSkyboxRotation(skyProfiles, profile, renderPipelineSettings);                      
                  
                        if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn || renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
                        {
                            if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                            {
                                if (profile.isProceduralSkybox)
                                {
                                    LoadSkyboxMaterial(skyProfiles, profile, false);
                                }
                                else
                                {
                                    LoadSkyboxMaterial(skyProfiles, profile, true);
                                }
                            }
                            else
                            {
                                LoadSkyboxMaterial(skyProfiles, profile, false);
                            }

                            SetQualitySettings(profile);

                            SetAmbientLightingAndFog(profile, skyProfiles);

                            SetSunLightSettings(profile, skyProfiles);

#if LWPipeline
                            LightweightRenderPipelineAsset lightweightRender = AssetDatabase.LoadAssetAtPath<LightweightRenderPipelineAsset>(GetAssetPath("Procedural Worlds Lightweight Pipeline Profile Ambient Skies"));
                            if (lightweightRender != null)
                            {
                                lightweightRender.shadowDistance = profile.shadowDistance;
                            }
#endif
                            DestroyParent("Ambient Skies Environment");
                        }
                        else
                        {
                            SetSunLightSettings(profile, skyProfiles);

                            AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile");

                            DestroyParent("Ambient Skies Environment");
                        }
                    }

                    VSyncSettings(profile, skyProfiles);

                    MarkActiveSceneAsDirty();
                }
                else if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
                {
                    if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.BuiltIn || renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.Lightweight)
                    {
                        RemoveSkyBox();
                        DestroyParent("Ambient Skies Environment");
                    }
                    else
                    {
                        RemoveHDSkybox(profile, skyProfiles, renderPipelineSettings);
                    }

                    MarkActiveSceneAsDirty();
                }
                else
                {
                    AddSkyboxIfNull("Default-Sky");
                }
                /*
                else if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.WorldManagerAPI)
                {

                }
                else
                {

                }
                */
            }
            else
            {
                RemoveTimeOfDay();

                AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile");

                RemoveDensityFogVolume();

                RemoveHorizonSky();

                DestroyParent("Ambient Skies Environment");
            }

            if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.DefaultProcedural)
            {
                RemoveTimeOfDay();

                RemoveDensityFogVolume();

                RemoveHorizonSky();

                DestroyParent("Ambient Skies Environment");
            }
        }

        /// <summary>
        /// Remove the skybox and set back to the procedural skybox
        /// </summary>
        public static void RemoveSkyBox()
        {
#if GAIA_PRESENT
            if (RenderSettings.skybox.name != "Ambient Skies Skybox")
            {
                RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxUtils.GetAssetPath("Ambient Skies Skybox"));
            }
#endif

            //Check for skybox material
            Material skyMaterial = RenderSettings.skybox;
            //If not empty
            if (skyMaterial != null)
            {
                //Set the material shader
                skyMaterial.shader = Shader.Find("Skybox/Procedural");
                skyMaterial.SetColor("_GroundColor", GetColorFromHTML("7B9AB2"));
            }

            GameObject lightObject = GetMainDirectionalLight();
            if (lightObject != null)
            {
                lightObject.transform.rotation = Quaternion.Euler(133f, 0f, 0f);

                Light light = lightObject.GetComponent<Light>();
                if (light != null)
                {
                    light.color = GetColorFromHTML("FFEADD");
                    light.intensity = 1.35f;
                }
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.fogColor = GetColorFromHTML("83A1B0");
        }

        /// <summary>
        /// Sets skybox if it is null
        /// </summary>
        /// <param name="skyboxName"></param>
        public static void AddSkyboxIfNull(string skyboxName)
        {
            //Get skybox material
            Material skyboxAsset = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath(skyboxName));
            //If asset is found
            if (skyboxAsset != null)
            {
                //If skybox in scene is empty
                if (RenderSettings.skybox == null)
                {
                    //Apply skybox
                    RenderSettings.skybox = skyboxAsset;
                }
            }

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
        /// Removes the skybox for HD Pipeline and sets it back to a procedural skybox
        /// </summary>
        public static void RemoveHDSkybox(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
#if HDPipeline
            //Get the main directional light
            GameObject lightObj = GetMainDirectionalLight();

            //Set up the light if we have one
            if (lightObj != null)
            {
                //Set light rotation
                Vector3 rotation = new Vector3(profile.lightRotation, 0f, 0f);
                lightObj.transform.SetPositionAndRotation(lightObj.transform.position, Quaternion.Euler(rotation));

                //Now set the light values
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.color = profile.sunColor;
                    light.intensity = profile.sunIntensity * 2;
                }

                HDAdditionalLightData hDAdditionalLightData = lightObj.GetComponent<HDAdditionalLightData>();
                if (hDAdditionalLightData != null)
                {
                    hDAdditionalLightData.intensity = profile.sunIntensity * 2;
                    hDAdditionalLightData.intensity = profile.proceduralSunIntensity * 2;
                }
            }

            VolumeProfile volumeProfile = GetVolumeProfile();
            if (volumeProfile != null)
            {
                VisualEnvironment visualEnvironment;
                if (volumeProfile.TryGet(out visualEnvironment))
                {
                    visualEnvironment.skyType.value = 2;
                }

                HDRISky hDRISky;
                if (volumeProfile.TryGet(out hDRISky))
                {
                    hDRISky.active = false;
                }

                ProceduralSky proceduralSky;
                if (volumeProfile.TryGet(out proceduralSky))
                {
                    proceduralSky.active = true;
                    proceduralSky.enableSunDisk.value = true;
                }

                GradientSky gradientSky;
                if (volumeProfile.TryGet(out gradientSky))
                {
                    gradientSky.active = false;
                }
            }
            AmbientSkiesPipelineUtils.SetupHDEnvironmentalVolume(profile, skyProfiles, profile.profileIndex, renderPipelineSettings, "High Definition Environment Volume", "Ambient Skies HD Volume Profile");

            DestroyParent("Ambient Skies Environment");
#endif
        }

        /// <summary>
        /// Looks for fog density volume then removes it fromt he scene
        /// </summary>
        public static void RemoveDensityFogVolume()
        {
            GameObject densityVolume = GameObject.Find("Density Volume");
            if (densityVolume != null)
            {
                Object.DestroyImmediate(densityVolume);
            }

            DestroyParent("Ambient Skies Environment");
        }

        /// <summary>
        /// Sets material to HDRI cubemap shader
        /// </summary>
        /// <param name="profile"></param>
        public static void HDRISky(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            Material skyMat = RenderSettings.skybox;
            if (skyMat != null)
            {
                string skyName = GetAssetPath("Ambient Skies Skybox");
                if (!string.IsNullOrEmpty(skyName))
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(skyName);
                    if (material != null)
                    {
                        if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                        {
                            material.shader = Shader.Find("Skybox/Cubemap");
                        }
                        else
                        {
                            material.shader = Shader.Find("Skybox/Procedural");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the skybox rotation
        /// </summary>
        /// <returns>Returns the rotation or zero if no skybox</returns>
        public static float GetSkyboxRotation()
        {
            if (RenderSettings.skybox != null)
            {
                return RenderSettings.skybox.GetFloat("_Rotation");
            }
            return 0f;
        }

        /// <summary>
        /// Gets and returns the volume profile
        /// </summary>
        /// <returns>Volume Profile</returns>
#if HDPipeline
        public static VolumeProfile GetVolumeProfile()
        {
            //Hd Pipeline Volume Setup
            GameObject parentObject = GetOrCreateParentObject("Ambient Skies Environment", false);
            GameObject volumeObject = GameObject.Find("High Definition Environment Volume");
            if (volumeObject == null)
            {
                volumeObject = new GameObject("High Definition Environment Volume");
                volumeObject.layer = LayerMask.NameToLayer("TransparentFX");
                volumeObject.transform.SetParent(parentObject.transform);
            }

            Volume volumeSettings = volumeObject.GetComponent<Volume>();
            if (volumeSettings == null)
            {
                volumeSettings = volumeObject.AddComponent<Volume>();
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 1f;
            }
            else
            {
                volumeSettings.isGlobal = true;
                volumeSettings.blendDistance = 5f;
                volumeSettings.weight = 1f;
                volumeSettings.priority = 1f;
            }

            //Finds the volume in the scene
            Volume volume = Object.FindObjectOfType<Volume>();
            if (volume != null)
            {
                //If Missing it'll add it to the volume
                volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(GetAssetPath("Ambient Skies HD Volume Profile"));
                //Gets the profile
                VolumeProfile volumeProfile = volume.sharedProfile;
                //Returns the profile
                return volumeProfile;
            }

            //Else return null
            return null;
        }
#endif

        /// <summary>
        /// Set the skybox rotation in degrees - will also rotate the directional light
        /// </summary>
        /// <param name="angleDegrees">Angle from 0..360.</param>
        public static void SetSkyboxRotation(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, AmbientSkiesConsts.RenderPipelineSettings renderPipelineSettings)
        {
            GameObject directionalLight = GetMainDirectionalLight();
            if (renderPipelineSettings == AmbientSkiesConsts.RenderPipelineSettings.HighDefinition)
            {
                if (skyProfiles.skyType == AmbientSkiesConsts.VolumeSkyType.ProceduralSky)
                {
                    //Correct the angle
                    float angleDegrees = profile.proceduralSkyboxRotation % 360f;

                    float sunAngle = profile.proceduralSkyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
                else
                {
                    //Correct the angle
                    float angleDegrees = profile.skyboxRotation % 360f;

                    float sunAngle = profile.skyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
            }
            else
            {
                if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.ProceduralSky)
                {
                    //Correct the angle
                    float angleDegrees = profile.proceduralSkyboxRotation % 360f;

                    float sunAngle = profile.proceduralSkyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
                else
                {
                    //Correct the angle
                    float angleDegrees = profile.skyboxRotation % 360f;

                    float sunAngle = profile.skyboxPitch;

                    //Set new directional light rotation
                    if (directionalLight != null)
                    {
                        Vector3 rotation = directionalLight.transform.rotation.eulerAngles;
                        rotation.y = angleDegrees;
                        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, rotation.y, 0f);
                    }
                }
            }

            if (RenderSettings.skybox != null)
            {
                //Correct the angle
                float angleDegrees = profile.proceduralSkyboxRotation % 360f;

                //Set new skybox rotation
                RenderSettings.skybox.SetFloat("_Rotation", angleDegrees);
            }
        }

        /// <summary>
        /// Get the skybox exposure
        /// </summary>
        /// <returns>Returns skybox exposure or 1f if none set</returns>
        public static float GetSkyboxExposure()
        {
            if (RenderSettings.skybox != null)
            {
                return RenderSettings.skybox.GetFloat("_Exposure");
            }
            return 1f;
        }

        /// <summary>
        /// Set the skybox exposure.
        /// </summary>
        /// <param name="exposure">Exposure</param>
        public static void SetSkyboxExposure(float exposure)
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetFloat("_Exposure", exposure);
            }
        }

        /// <summary>
        /// Set the skybox tint.
        /// </summary>
        /// <param name="tint">Tint</param>
        public static void SetSkyboxTint(Color tint)
        {
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_Tint", tint);
            }
        }

        /// <summary>
        /// Enables and disables the sun disk
        /// </summary>
        /// <param name="sunDiskEnabled"></param>
        public static void SetEnableSunDisk(AmbientSkyboxProfile profile)
        {
            Material skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(GetAssetPath("Ambient Skies Skybox"));
            if (skyMaterial == null)
            {
                Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                return;
            }
            if (profile.enableSunDisk)
            {
                skyMaterial.SetFloat("_SunDisk", 2f);
            }
            else
            {
                skyMaterial.SetFloat("_SunDisk", 0f);
            }
        }

        /// <summary>
        /// Sets the quality settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetQualitySettings(AmbientSkyboxProfile profile)
        {            
            QualitySettings.shadowResolution = profile.shadowResolution;
            QualitySettings.shadowProjection = profile.shadowProjection;
            QualitySettings.shadowDistance = profile.shadowDistance;

            switch (profile.cascadeCount)
            {
                case AmbientSkiesConsts.ShadowCascade.CascadeCount1:
                    QualitySettings.shadowCascades = 1;
                    break;
                case AmbientSkiesConsts.ShadowCascade.CascadeCount2:
                    QualitySettings.shadowCascades = 2;
                    break;
                case AmbientSkiesConsts.ShadowCascade.CascadeCount3:
                    QualitySettings.shadowCascades = 3;
                    break;
                case AmbientSkiesConsts.ShadowCascade.CascadeCount4:
                    QualitySettings.shadowCascades = 4;
                    break;
            }
        }     

        /// <summary>
        /// Sets the ambient lighting and fog settings for Built-In and LWRP
        /// </summary>
        /// <param name="profile"></param>
        public static void SetAmbientLightingAndFog(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            //Set ambient mode & intensity
            switch (profile.ambientMode)
            {
                case AmbientSkiesConsts.AmbientMode.Color:
                    RenderSettings.ambientMode = AmbientMode.Flat;
                    RenderSettings.ambientSkyColor = profile.skyColor;
                    break;
                case AmbientSkiesConsts.AmbientMode.Gradient:
                    RenderSettings.ambientMode = AmbientMode.Trilight;
                    RenderSettings.ambientSkyColor = profile.skyColor;
                    RenderSettings.ambientEquatorColor = profile.equatorColor;
                    RenderSettings.ambientGroundColor = profile.groundColor;
                    break;
                case AmbientSkiesConsts.AmbientMode.Skybox:
                    RenderSettings.ambientMode = AmbientMode.Skybox;
                    RenderSettings.ambientIntensity = profile.skyboxGroundIntensity;
                    break;
            }           

            //Setup the fog
            switch (profile.fogType)
            {
                case AmbientSkiesConsts.VolumeFogType.None:
                    RenderSettings.fog = false;
                    break;
                case AmbientSkiesConsts.VolumeFogType.Exponential:
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Exponential;
                    if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                    {
                        RenderSettings.fogDensity = profile.fogDensity;
                        RenderSettings.fogColor = profile.fogColor;
                    }
                    else
                    {
                        RenderSettings.fogDensity = profile.proceduralFogDensity;
                        RenderSettings.fogColor = profile.proceduralFogColor;
                    }
                    break;
                case AmbientSkiesConsts.VolumeFogType.ExponentialSquared:
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                    {
                        RenderSettings.fogDensity = profile.fogDensity;
                        RenderSettings.fogColor = profile.fogColor;
                    }
                    else
                    {
                        RenderSettings.fogDensity = profile.proceduralFogDensity;
                        RenderSettings.fogColor = profile.proceduralFogColor;
                    }
                    break;
                case AmbientSkiesConsts.VolumeFogType.Linear:
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Linear;
                    if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                    {
                        RenderSettings.fogColor = profile.fogColor;
                        RenderSettings.fogStartDistance = profile.nearFogDistance;
                        RenderSettings.fogEndDistance = profile.fogDistance;
                    }
                    else
                    {
                        RenderSettings.fogColor = profile.proceduralFogColor;
                        RenderSettings.fogStartDistance = profile.proceduralNearFogDistance;
                        RenderSettings.fogEndDistance = profile.proceduralFogDistance;
                    }
                    break;
            }

            if (profile.fogType != AmbientSkiesConsts.VolumeFogType.Volumetric)
            {
                GameObject densityVolumeObject1 = GameObject.Find("Density Volume");
                if (densityVolumeObject1 != null)
                {
                    Object.DestroyImmediate(densityVolumeObject1);
                }
            }
        }

        /// <summary>
        /// Sets up the sun light settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetSunLightSettings(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            GameObject mainLight = GetMainDirectionalLight();
            //Set up the light if we have one
            if (mainLight != null)
            {
                //Now set the light values
                Light light = mainLight.GetComponent<Light>();
                if (light != null)
                {
                    light.shadows = profile.shadowType;

                    if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                    {
                        light.color = profile.sunColor;
                        light.intensity = profile.sunIntensity;
                        light.shadowStrength = profile.shadowStrength;
                        light.bounceIntensity = profile.indirectLightMultiplier;
                    }
                    else
                    {
                        light.intensity = profile.proceduralSunIntensity;
                        light.color = profile.proceduralSunColor;
                        light.shadowStrength = profile.shadowStrength;
                        light.bounceIntensity = profile.indirectLightMultiplier;
                    }
                }

                //Apply the ligh to the fog
                RenderSettings.sun = light;
            }
        }

        /// <summary>
        /// Sets up the skyhbox loading and settings
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="isHDRI"></param>
        public static void LoadSkyboxMaterial(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, bool isHDRI)
        {
            //Is a HDRI Skybox
            if (isHDRI)
            {
                //Conditionally load the skybox material
                bool loadSkyboxMaterial = false;
                Material skyMaterial = RenderSettings.skybox;
                if (skyMaterial == null)
                {
                    loadSkyboxMaterial = true;
                }
                else
                {
                    if (skyMaterial.name != "Ambient Skies Skybox")
                    {
                        loadSkyboxMaterial = true;
                    }
                }

                if (loadSkyboxMaterial)
                {
                    //Get the skybox material path
                    string skyMaterialPath = GetAssetPath("Ambient Skies Skybox");
                    if (string.IsNullOrEmpty(skyMaterialPath))
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }
                    skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyMaterialPath);
                    if (skyMaterial == null)
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }
                    RenderSettings.skybox = skyMaterial;
                }

                //Conditionally load the skybox texture 
                bool loadSkyboxTexture = false;
                Texture skyboxTexture = skyMaterial.GetTexture("_Tex");
                if (skyboxTexture == null)
                {
                    loadSkyboxTexture = true;
                }
                else
                {
                    if (skyboxTexture.name != profile.assetName)
                    {
                        loadSkyboxTexture = true;
                    }
                    else
                    {
                        loadSkyboxTexture = true;
                    }
                }
                if (loadSkyboxTexture)
                {
                    string hdrSkyPath;
                    if (!profile.isPWProfile)
                    {
                        if (profile.customSkybox == null)
                        {
                            hdrSkyPath = "";
                        }
                        else
                        {
                            hdrSkyPath = GetAssetPath(profile.customSkybox.name);
                            profile.assetName = profile.customSkybox.name;
                        }
                    }
                    else
                    {
                        hdrSkyPath = GetAssetPath(profile.assetName);
                    }

                    //Get the skybox asset path                       
                    if (string.IsNullOrEmpty(hdrSkyPath) && profile.isPWProfile)
                    {
                        Debug.LogErrorFormat("AmbientSkies:SetSkybox : Unable to load '{0}' skybox asset - Aborting! ", profile.assetName, " Please ensure you have a cubemap selected");

                        return;
                    }
                    else if (string.IsNullOrEmpty(hdrSkyPath) && !profile.isPWProfile)
                    {
                        Debug.LogError("Missing Cubemap from custom skybox settings. Please ensure you have a cubemap selected in the custom skybox... Aborting!");
                        return;
                    }

                    skyMaterial.SetTexture("_Tex", AssetDatabase.LoadAssetAtPath<Texture>(hdrSkyPath));
                }

                skyMaterial.shader = Shader.Find("Skybox/Cubemap");

                skyMaterial.SetColor("_Tint", profile.skyboxTint);
                skyMaterial.SetFloat("_Exposure", profile.skyboxExposure);
                skyMaterial.SetFloat("_Rotation", profile.skyboxRotation);

                //Do bake for reflection probe - bit only if a major load operation
                if (loadSkyboxMaterial || loadSkyboxTexture)
                {
                    LightingUtils.BakeGlobalReflectionProbe(false);
                }
            }
            //Is a Procedural Skybox
            else
            {
                //Conditionally load the skybox material
                bool loadSkyboxMaterial = false;
                Material skyMaterial = RenderSettings.skybox;
                if (skyMaterial == null)
                {
                    loadSkyboxMaterial = true;
                }
                else
                {
                    if (skyMaterial.name != "Ambient Skies Skybox")
                    {
                        loadSkyboxMaterial = true;
                    }
                }

                if (loadSkyboxMaterial)
                {
                    //Get the skybox material path
                    string skyMaterialPath = GetAssetPath("Ambient Skies Skybox");
                    if (string.IsNullOrEmpty(skyMaterialPath))
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }

                    skyMaterial = AssetDatabase.LoadAssetAtPath<Material>(skyMaterialPath);
                    if (skyMaterial == null)
                    {
                        Debug.LogError("AmbientSkies:SetSkybox() : Unable to load 'Ambient Skies Skybox' material - Aborting!");
                        return;
                    }

                    RenderSettings.skybox = skyMaterial;
                }

                skyMaterial.shader = Shader.Find("Skybox/Procedural");

                SetEnableSunDisk(profile);
                skyMaterial.SetFloat("_SunSize", profile.proceduralSunSize);
                skyMaterial.SetFloat("_SunSizeConvergence", profile.proceduralSunSizeConvergence);
                skyMaterial.SetFloat("_AtmosphereThickness", profile.proceduralAtmosphereThickness);
                skyMaterial.SetColor("_SkyTint", profile.proceduralSkyTint);
                skyMaterial.SetColor("_GroundColor", profile.proceduralGroundColor);
                skyMaterial.SetFloat("_Exposure", profile.proceduralSkyExposure);
            }
            
        }

        /// <summary>
        /// Unparents and reparents gameobject in Gaia to new Ambient Skies
        /// </summary>
        /// <param name="alsoReparent"></param>
        public static void SetGaiaParenting(bool alsoReparent)
        {
            GameObject newParentObject = GetOrCreateParentObject("Ambient Skies Environment", false);

            GameObject mainLight = GetMainDirectionalLight();
            GameObject postProcessingObject = GameObject.Find("Global Post Processing");
            GameObject reflectionProbeObject = GameObject.Find("Global Reflection Probe");
            GameObject ambientAudioObject = GameObject.Find("Ambient Audio");
            if (mainLight != null)
            {
                mainLight.transform.SetParent(null);
                if (alsoReparent)
                {
                    mainLight.transform.SetParent(newParentObject.transform);
                }
            }
            if (postProcessingObject != null)
            {
                postProcessingObject.transform.SetParent(null);
                if (alsoReparent)
                {
                    postProcessingObject.transform.SetParent(newParentObject.transform);
                }
            }
            if (reflectionProbeObject != null)
            {
                reflectionProbeObject.transform.SetParent(null);
                if (alsoReparent)
                {
                    reflectionProbeObject.transform.SetParent(newParentObject.transform);
                }
            }
            if (ambientAudioObject != null)
            {
                ambientAudioObject.transform.SetParent(null);
                if (alsoReparent)
                {
                    ambientAudioObject.transform.SetParent(newParentObject.transform);
                }
            }

            //Find parent object
            GameObject oldGaiaAmbientParent = GameObject.Find("Ambient Skies Samples");
            if (oldGaiaAmbientParent != null)
            {
                //Find parents in parent object
                Transform[] parentChilds = oldGaiaAmbientParent.GetComponentsInChildren<Transform>();
                if (parentChilds.Length == 1)
                {
                    //Destroy object if object is empty
                    Object.DestroyImmediate(oldGaiaAmbientParent);
                }
            }
        }

        /// <summary>
        /// Removes enviro from the scene
        /// </summary>
        /// <param name="removeAllObjects"></param>
        public static void RemoveEnviro(bool removeAllObjects)
        {
            if (removeAllObjects)
            {
                GameObject enviroObject1 = GameObject.Find("Enviro Sky Manager");
                if (enviroObject1 != null)
                {
                    Object.DestroyImmediate(enviroObject1);
                }
                GameObject enviroObject2 = GameObject.Find("EnviroSky Standard");
                if (enviroObject2 != null)
                {
                    Object.DestroyImmediate(enviroObject2);
                }
                GameObject enviroObject3 = GameObject.Find("EnviroSky Lite");
                if (enviroObject3 != null)
                {
                    Object.DestroyImmediate(enviroObject3);
                }
                GameObject enviroObject4 = GameObject.Find("EnviroSky Lite for Mobiles");
                if (enviroObject4 != null)
                {
                    Object.DestroyImmediate(enviroObject4);
                }
                GameObject enviroObject5 = GameObject.Find("EnviroSky Standard for VR");
                if (enviroObject5 != null)
                {
                    Object.DestroyImmediate(enviroObject5);
                }
                GameObject enviroObject6 = GameObject.Find("Enviro Effects");
                if (enviroObject6 != null)
                {
                    Object.DestroyImmediate(enviroObject6);
                }
                GameObject enviroObject7 = GameObject.Find("Enviro Directional Light");
                if (enviroObject7 != null)
                {
                    Object.DestroyImmediate(enviroObject7);
                }
                GameObject enviroObject8 = GameObject.Find("Enviro Sky Manager for GAIA");
                if (enviroObject8 != null)
                {
                    Object.DestroyImmediate(enviroObject8);
                }
                GameObject enviroObject9 = GameObject.Find("Enviro Effects LW");
                if (enviroObject9 != null)
                {
                    Object.DestroyImmediate(enviroObject9);
                }

                GameObject sun = GetMainDirectionalLight();
                if (sun != null)
                {
                    RenderSettings.sun = sun.GetComponent<Light>();
                }
            }
        }

        #endregion

        #region Time Of Day Setup

        /// <summary>
        /// Adds time of day
        /// </summary>
        /// <param name="profile"></param>
        public static void AddTimeOfDay(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            GameObject mainLight = GetMainDirectionalLight();
            if (mainLight != null)
            {
                Light light = mainLight.GetComponent<Light>();
                light.enabled = false;
            }

            HDRISky(profile, skyProfiles);

            Object timeOfDaySystem = GameObject.Find("AS Time Of Day");
            GameObject parentObject = GetOrCreateParentObject("Ambient Skies Environment", false);
            if (timeOfDaySystem == null)
            {
                timeOfDaySystem = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(GetAssetPath("AS Time Of Day")));

                GameObject tod = GameObject.Find("AS Time Of Day");
                if (tod != null)
                {
                    tod.transform.SetParent(parentObject.transform);
                }
            }
        }

        /// <summary>
        /// Sets the time of day settings
        /// </summary>
        /// <param name="profile"></param>
        public static void SetTimeOfDaySettings(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            AmbientSkiesTimeOfDay timeOfDay = Object.FindObjectOfType<AmbientSkiesTimeOfDay>();
            if (timeOfDay != null)
            {
                if (!skyProfiles.pauseTime && Application.isPlaying)
                {
                    //profile.currentTimeOfDay = timeOfDay.m_currentTime;
                }

                //Apply all main script objects
                timeOfDay.m_ambientSkiesProfileVol1 = skyProfiles;
                timeOfDay.m_timeOfDayProfile = skyProfiles.timeOfDayProfile;
                timeOfDay.m_ambientSkyboxProfile = profile;

                timeOfDay.m_timeOfDayProfile.m_renderPipeline = skyProfiles.m_selectedRenderPipeline;

                timeOfDay.m_timeOfDayProfile.m_daySunIntensity = skyProfiles.daySunIntensity;
                timeOfDay.m_timeOfDayProfile.m_daySunGradientColor = skyProfiles.daySunGradientColor;
                timeOfDay.m_timeOfDayProfile.m_nightSunIntensity = skyProfiles.nightSunIntensity;
                timeOfDay.m_timeOfDayProfile.m_nightSunGradientColor = skyProfiles.nightSunGradientColor;
                timeOfDay.m_timeOfDayProfile.m_startFogDistance = skyProfiles.startFogDistance;
                timeOfDay.m_timeOfDayProfile.m_dayFogDensity = skyProfiles.dayFogDensity;
                timeOfDay.m_timeOfDayProfile.m_dayFogDistance = skyProfiles.dayFogDistance;
                timeOfDay.m_timeOfDayProfile.m_nightFogDensity = skyProfiles.nightFogDensity;
                timeOfDay.m_timeOfDayProfile.m_nightFogDistance = skyProfiles.nightFogDistance;
                timeOfDay.m_timeOfDayProfile.m_dayFogColor = skyProfiles.dayFogColor;
                timeOfDay.m_timeOfDayProfile.m_nightFogColor = skyProfiles.nightFogColor;
                timeOfDay.m_timeOfDayProfile.m_dayColor = skyProfiles.dayPostFXColor;
                timeOfDay.m_timeOfDayProfile.m_nightColor = skyProfiles.nightPostFXColor;
                timeOfDay.m_timeOfDayProfile.m_dayTempature = skyProfiles.dayTempature;
                timeOfDay.m_timeOfDayProfile.m_nightTempature = skyProfiles.nightTempature;
                timeOfDay.m_timeOfDayProfile.m_lightAnisotropy = skyProfiles.lightAnisotropy;
                timeOfDay.m_timeOfDayProfile.m_lightProbeDimmer = skyProfiles.lightProbeDimmer;
                timeOfDay.m_timeOfDayProfile.m_lightDepthExtent = skyProfiles.lightDepthExtent;
                timeOfDay.m_timeOfDayProfile.m_sunSize = skyProfiles.sunSize;
                timeOfDay.m_timeOfDayProfile.m_skyExposure = skyProfiles.skyExposure;
                timeOfDay.m_timeOfDayProfile.m_realtimeGIUpdate = skyProfiles.realtimeGIUpdate;
                timeOfDay.m_timeOfDayProfile.m_gIUpdateIntervalInSeconds = skyProfiles.gIUpdateInterval;
                timeOfDay.m_timeOfDayProfile.m_pauseTime = skyProfiles.pauseTime;
                timeOfDay.m_timeOfDayProfile.m_syncPostFXToTimeOfDay = skyProfiles.syncPostProcessing;
                timeOfDay.m_timeOfDayProfile.m_sunRotation = skyProfiles.skyboxRotation;
                timeOfDay.m_timeOfDayProfile.m_dayLengthInSeconds = skyProfiles.dayLengthInSeconds;
                timeOfDay.m_timeOfDayProfile.m_environmentSeason = skyProfiles.currentSeason;
                timeOfDay.m_timeOfDayProfile.m_hemisphereOrigin = skyProfiles.hemisphereOrigin;
                timeOfDay.m_timeOfDayProfile.m_day = skyProfiles.dayDate;
                timeOfDay.m_timeOfDayProfile.m_month = skyProfiles.monthDate;
                timeOfDay.m_timeOfDayProfile.m_year = skyProfiles.yearDate;
                timeOfDay.m_timeOfDayProfile.m_nightLengthInSeconds = skyProfiles.nightLengthInSeconds;

                timeOfDay.m_timeOfDayProfile.m_currentTime = skyProfiles.currentTimeOfDay;
                timeOfDay.m_timeOfDayProfile.m_pauseTimeKey = skyProfiles.pauseTimeKey;
                timeOfDay.m_timeOfDayProfile.m_incrementUpKey = skyProfiles.incrementUpKey;
                timeOfDay.m_timeOfDayProfile.m_incrementDownKey = skyProfiles.incrementDownKey;
                timeOfDay.m_timeOfDayProfile.m_timeToAddOrRemove = skyProfiles.timeToAddOrRemove;
                timeOfDay.m_timeOfDayProfile.m_rotateSunLeftKey = skyProfiles.rotateSunLeftKey;
                timeOfDay.m_timeOfDayProfile.m_rotateSunRightKey = skyProfiles.rotateSunRightKey;
                timeOfDay.m_timeOfDayProfile.m_sunRotationAmount = skyProfiles.sunRotationAmount;
                if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Exponential)
                {
                    RenderSettings.fog = true;
                    timeOfDay.m_timeOfDayProfile.m_fogMode = FogMode.Exponential;
                }
                else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                {
                    RenderSettings.fog = true;
                    timeOfDay.m_timeOfDayProfile.m_fogMode = FogMode.Linear;
                }
                else if (profile.fogType == AmbientSkiesConsts.VolumeFogType.None)
                {
                    RenderSettings.fog = false;
                }
            }
        }

        /// <summary>
        /// Removes time of day system
        /// </summary>
        public static void RemoveTimeOfDay()
        {
            GameObject timeOfDay = GameObject.Find("AS Time Of Day");
            if (timeOfDay != null)
            {
                Object.DestroyImmediate(timeOfDay);
            }

            GameObject mainLight = GetMainDirectionalLight();
            if (mainLight != null)
            {
                Light light = mainLight.GetComponent<Light>();
                if (light != null)
                {
                    light.enabled = true;
                    RenderSettings.sun = light;
                }
            }

            DestroyParent("Ambient Skies Environment");
        }

        #endregion

        #region Horizon Setup

        /// <summary>
        /// Removes the Horizon Sky
        /// </summary>
        public static void RemoveHorizonSky()
        {
            GameObject horizonSky = GameObject.Find("Ambient Skies Horizon");

            if (horizonSky != null)
            {
                Object.DestroyImmediate(horizonSky);
            }
        }

        #endregion

        #region Sun Setup

        /// <summary>
        /// Get the intensition of the sun
        /// </summary>
        /// <returns>Intensity of the sun</returns>
        public static float GetSunIntensity()
        {
            GameObject directionalLightObj = GetMainDirectionalLight();
            if (directionalLightObj != null && directionalLightObj.GetComponent<Light>() != null)
            {
                return directionalLightObj.GetComponent<Light>().intensity;
            }
            return 1f;
        }

        /// <summary>
        /// Set the intensity of the sun
        /// </summary>
        /// <param name="intensity">New sun intensity</param>
        public static void SetSunIntensity(float intensity)
        {
            GameObject directionalLightObj = GetMainDirectionalLight();
            if (directionalLightObj != null && directionalLightObj.GetComponent<Light>() != null)
            {
                directionalLightObj.GetComponent<Light>().intensity = intensity;
            }
        }

        /// <summary>
        /// Set the color of the sun
        /// </summary>
        /// <param name="sunColor">New sun color</param>
        public static void SetSunColor(Color sunColor)
        {
            GameObject directionalLightObj = GetMainDirectionalLight();
            if (directionalLightObj != null && directionalLightObj.GetComponent<Light>() != null)
            {
                directionalLightObj.GetComponent<Light>().color = sunColor;
            }
        }

        #endregion

        #region Fog Setup
        
        /// <summary>
        /// Set the color of the fog
        /// </summary>
        /// <param name="fogColor">New fog color</param>
        public static void SetFogColor(Color fogColor)
        {
            RenderSettings.fogColor = fogColor;
        }

        #endregion

        #region Quality Settings Setup

        /// <summary>
        /// Sets the vsync count for your project in the quality settings
        /// </summary>
        /// <param name="profile"></param>
        public static void VSyncSettings(AmbientSkyboxProfile profile, AmbientSkyProfiles skyProfiles)
        {
            //Set vsync mode
            switch (skyProfiles.vSyncMode)
            {
                case AmbientSkiesConsts.VSyncMode.DontSync:
                    QualitySettings.vSyncCount = 0;
                    break;
                case AmbientSkiesConsts.VSyncMode.EveryVBlank:
                    QualitySettings.vSyncCount = 1;
                    break;
                case AmbientSkiesConsts.VSyncMode.EverySecondVBlank:
                    QualitySettings.vSyncCount = 2;
                    break;
            }
        }

        #endregion

        #endregion

        #region Custom Utils

        /// <summary>
        /// This removes the gameobject that is created on a new scene
        /// </summary>
        /// <param name="skyProfiles"></param>
        public static void RemoveNewSceneObject(AmbientSkyProfiles skyProfiles)
        {
            if (skyProfiles.systemTypes == AmbientSkiesConsts.SystemTypes.AmbientSkies)
            {
                GameObject newSceneObject = GameObject.Find("Ambient Skies New Scene Object (Don't Delete Me)");
                if (newSceneObject != null)
                {
                    Object.DestroyImmediate(newSceneObject);
                }
            }
        }

        /// <summary>
        /// Sets current open scene as active
        /// </summary>
        public static void MarkActiveSceneAsDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        /// <summary>
        /// Setup the main camera for use with HDR skyboxes
        /// </summary>
        /// <param name="mainCameraObj"></param>
        public static void SetupMainCamera(GameObject mainCameraObj)
        {
            if (mainCameraObj.GetComponent<FlareLayer>() == null)
            {
                mainCameraObj.AddComponent<FlareLayer>();
            }

            if (mainCameraObj.GetComponent<AudioListener>() == null)
            {
                mainCameraObj.AddComponent<AudioListener>();
            }

            Camera mainCamera = mainCameraObj.GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = mainCameraObj.AddComponent<Camera>();
            }

#if UNITY_5_6_OR_NEWER
            mainCamera.allowHDR = true;
#else
                mainCamera.hdr = true;
#endif

#if UNITY_2017_0_OR_NEWER
                mainCamera.allowMSAA = false;
#endif
        }

        /// <summary>
        /// Get a color from a html string
        /// </summary>
        /// <param name="htmlString">Color in RRGGBB or RRGGBBBAA or #RRGGBB or #RRGGBBAA format.</param>
        /// <returns>Color or white if unable to parse it.</returns>
        public static Color GetColorFromHTML(string htmlString)
        {
            Color color = Color.white;
            if (!htmlString.StartsWith("#"))
            {
                htmlString = "#" + htmlString;
            }
            if (!ColorUtility.TryParseHtmlString(htmlString, out color))
            {
                color = Color.white;
            }
            return color;
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>The path or null</returns>
        public static string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }

        /// <summary>
        /// Get or create a parent object
        /// </summary>
        /// <param name="parentGameObject">Name of the parent object to get or create</param>
        /// <returns>Parent objet</returns>
        public static GameObject GetOrCreateParentObject(string parentGameObject, bool parentToGaia)
        {
            //Get the parent object
            GameObject theParentGo = GameObject.Find(parentGameObject);

            if (theParentGo == null)
            {
                theParentGo = GameObject.Find("Ambient Skies Environment");

                if (theParentGo == null)
                {
                    theParentGo = new GameObject("Ambient Skies Environment");
                }
            }

            if (parentToGaia)
            {
                GameObject gaiaParent = GameObject.Find("Gaia Environment");
                if (gaiaParent != null)
                {
                    theParentGo.transform.SetParent(gaiaParent.transform);
                }
            }

            return theParentGo;
        }

        /// <summary>
        /// Get or create the main camera in the scene
        /// </summary>
        /// <returns>Existing or new main camera</returns>
        public static GameObject GetOrCreateMainCamera()
        {
            //Get or create the main camera
            GameObject mainCameraObj = null;

            if (Camera.main != null)
            {
                mainCameraObj = Camera.main.gameObject;
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = GameObject.Find("Main Camera");
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = GameObject.Find("Camera");
            }

            if (mainCameraObj == null)
            {
                Camera[] cameras = Object.FindObjectsOfType<Camera>();
                foreach (var camera in cameras)
                {
                    mainCameraObj = camera.gameObject;
                    break;
                }
            }

            if (mainCameraObj == null)
            {
                mainCameraObj = new GameObject("Main Camera");
                mainCameraObj.tag = "MainCamera";
                SetupMainCamera(mainCameraObj);
            }

            return mainCameraObj;
        }

        /// <summary>
        /// Get the main directional light in the scene
        /// </summary>
        /// <returns>Main light or null</returns>
        public static GameObject GetMainDirectionalLight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                //Grab the first directional light we can find
                Light[] lights = Object.FindObjectsOfType<Light>();
                foreach (var light in lights)
                {
                    if (light.type == LightType.Directional)
                    {
                        lightObj = light.gameObject;
                    }
                }

                if (lightObj == null)
                {
                    lightObj = new GameObject("Directional Light");
                    lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                    Light lightSettings = lightObj.AddComponent<Light>();
                    lightSettings.type = LightType.Directional;
                }
            }
            return lightObj;
        }

        /// <summary>
        /// Find parent object and destroys it if it's empty
        /// </summary>
        /// <param name="parentGameObject"></param>
        public static void DestroyParent(string parentGameObject)
        {
            //If string isn't empty
            if(!string.IsNullOrEmpty(parentGameObject))
            {
                //If string doesn't = Ambient Skies Environment
                if (parentGameObject != "Ambient Skies Environment")
                {
                    //Sets the paramater to Ambient Skies Environment
                    parentGameObject = "Ambient Skies Environment";
                }

                //Find parent object
                GameObject parentObject = GameObject.Find(parentGameObject);
                if (parentObject != null)
                {
                    //Find parents in parent object
                    Transform[] parentChilds = parentObject.GetComponentsInChildren<Transform>();
                    if (parentChilds.Length == 1)
                    {
                        //Destroy object if object is empty
                        Object.DestroyImmediate(parentObject);
                    }
                }
            }
        }
#endregion
    }
}