//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.

namespace AmbientSkies
{
    /// <summary>
    /// Settings for an ambient lightmapping settings
    /// </summary>
    [System.Serializable]
    public class AmbientLightingProfile
    {
        #region Lightmaps Defaults Variables
        //Lightmapping settings
        public string name;
        public int profileIndex;
        public string assetName;

        //Default lightmapping settings
        public bool defaultEnableLightmapSettings;
        public bool deaultShowLightmapSettings;
        public bool defaultRealtimeGlobalIllumination;
        public bool defaultBakedGlobalIllumination;
        public AmbientSkiesConsts.LightmapperMode defaultLightmappingMode = AmbientSkiesConsts.LightmapperMode.Enlighten;
        public float defaultIndirectRelolution;
        public float defaultLightmapResolution;
        public int defaultLightmapPadding;
        public bool defaultUseHighResolutionLightmapSize;
        public bool defaultCompressLightmaps;
        public bool defaultAmbientOcclusion;
        public float defaultMaxDistance;
        public float defaultIndirectContribution;
        public float defaultDirectContribution;
        public bool defaultUseDirectionalMode;
        public float defaultLightIndirectIntensity = 2f;
        public float defaultLightBoostIntensity = 6f;
        public bool defaultFinalGather = false;
        public int defaultFinalGatherRayCount = 256;
        public bool defaultFinalGatherDenoising = true;
        public bool defaultAutoLightmapGeneration = false;
        #endregion

        #region Lightmaps Current Variables
        //Current lightmapping settings
        public bool enableLightmapSettings;
        public bool showLightmapSettings;
        public bool realtimeGlobalIllumination;
        public bool bakedGlobalIllumination;
        public AmbientSkiesConsts.LightmapperMode lightmappingMode = AmbientSkiesConsts.LightmapperMode.Enlighten;
        public float indirectRelolution;
        public float lightmapResolution;
        public int lightmapPadding;
        public bool useHighResolutionLightmapSize;
        public bool compressLightmaps;
        public bool ambientOcclusion;
        public float maxDistance;
        public float indirectContribution;
        public float directContribution;
        public bool useDirectionalMode;
        public float lightIndirectIntensity = 2f;
        public float lightBoostIntensity = 6f;
        public bool finalGather = false;
        public int finalGatherRayCount = 256;
        public bool finalGatherDenoising = true;
        public bool autoLightmapGeneration = false;
        #endregion

        #region Defaults Setup
        public void RevertToDefault()
        {
            enableLightmapSettings = defaultEnableLightmapSettings;
            showLightmapSettings = deaultShowLightmapSettings;
            realtimeGlobalIllumination = defaultRealtimeGlobalIllumination;
            bakedGlobalIllumination = defaultBakedGlobalIllumination;
            lightmappingMode = defaultLightmappingMode;
            indirectRelolution = defaultIndirectRelolution;
            lightmapResolution = defaultLightmapResolution;
            lightmapPadding = defaultLightmapPadding;
            useHighResolutionLightmapSize = defaultUseHighResolutionLightmapSize;
            compressLightmaps = defaultCompressLightmaps;
            ambientOcclusion = defaultAmbientOcclusion;
            maxDistance = defaultMaxDistance;
            indirectContribution = defaultIndirectContribution;
            directContribution = defaultDirectContribution;
            useDirectionalMode = defaultUseDirectionalMode;
            lightIndirectIntensity = defaultLightIndirectIntensity;
            lightBoostIntensity = defaultLightBoostIntensity;
            finalGather = defaultFinalGather;
            finalGatherRayCount = defaultFinalGatherRayCount;
            finalGatherDenoising = defaultFinalGatherDenoising;
            autoLightmapGeneration = defaultAutoLightmapGeneration;
        }

        public void SaveCurrentToDefault()
        {
            defaultEnableLightmapSettings = enableLightmapSettings;
            deaultShowLightmapSettings = showLightmapSettings;
            defaultRealtimeGlobalIllumination = realtimeGlobalIllumination;
            defaultBakedGlobalIllumination = bakedGlobalIllumination;
            defaultLightmappingMode = lightmappingMode;
            defaultIndirectRelolution = indirectRelolution;
            defaultLightmapResolution = lightmapResolution;
            defaultLightmapPadding = lightmapPadding;
            defaultUseHighResolutionLightmapSize = useHighResolutionLightmapSize;
            defaultCompressLightmaps = compressLightmaps;
            defaultAmbientOcclusion = ambientOcclusion;
            defaultMaxDistance = maxDistance;
            defaultIndirectContribution = indirectContribution;
            defaultDirectContribution = directContribution;
            defaultUseDirectionalMode = useDirectionalMode;
            defaultLightIndirectIntensity = lightIndirectIntensity;
            defaultLightBoostIntensity = lightBoostIntensity;
            defaultFinalGather = finalGather;
            defaultFinalGatherRayCount = finalGatherRayCount;
            defaultFinalGatherDenoising = finalGatherDenoising;
            defaultAutoLightmapGeneration = autoLightmapGeneration;
        }
        #endregion
    }
}