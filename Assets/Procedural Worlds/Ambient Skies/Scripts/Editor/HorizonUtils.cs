//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;

namespace AmbientSkies
{
    /// <summary>
    /// Horizon utility class
    /// </summary>
    public static class HorizonUtils
    {
        #region Utils

        /// <summary>
        /// Removes the Horizon Sky
        /// </summary>
        public static void RemoveHorizonSky()
        {
            GameObject horizonSky = GameObject.Find("Ambient Skies Horizon");

            if (horizonSky != null)
            {
                Object.DestroyImmediate(horizonSky);
                SkyboxUtils.DestroyParent("Ambient Skies Environment");
            }
        }

        /// <summary>
        /// Adds Horizon Sky to the scene
        /// </summary>
        /// <param name="horizonSkyPrefab"></param>
        public static void AddHorizonSky(string horizonSkyPrefab)
        {
            string skySphere = GetAssetPath(horizonSkyPrefab);
            GameObject theHorizonSky = AssetDatabase.LoadAssetAtPath<GameObject>(skySphere);

            if (theHorizonSky.GetComponent<HorizonSky>() == null)
            {
                theHorizonSky.AddComponent<HorizonSky>();
            }

            //Finds and assigns the gameobject if string isn't null
            if (!string.IsNullOrEmpty(skySphere) && theHorizonSky != null)
            {
                GameObject mainCameraObj = GetOrCreateMainCamera();
                GameObject theParentGo = GetOrCreateParentObject("Ambient Skies FX");
                Terrain activeTerrainInScene = Terrain.activeTerrain;
                float xFloat = 3000f;
                float yFloat = 3000f;
                float zFloat = 3000f;

                if (activeTerrainInScene != null)
                {
                    xFloat = activeTerrainInScene.terrainData.size.x;
                    yFloat = activeTerrainInScene.terrainData.size.y;
                    zFloat = activeTerrainInScene.terrainData.size.z;
                }

                //Spawn into your scene
                if (theHorizonSky != null && GameObject.Find("Ambient Skies Horizon") == null)
                {
                    Camera mainCamera = mainCameraObj.GetComponent<Camera>();
                    theHorizonSky = Object.Instantiate(theHorizonSky);
                    theHorizonSky.name = "Ambient Skies Horizon";

                    if (activeTerrainInScene != null)
                    {
                        theHorizonSky.transform.localScale = new Vector3(xFloat, yFloat * 2.5f, zFloat);
                    }

                    //Reconfigures the camera far clip plane to match the size or the terrain so no artifacts are created
                    if (mainCameraObj != null)
                    {
                        mainCamera.farClipPlane = xFloat + 50f;

                        if (activeTerrainInScene != null)
                        {
                            mainCamera.farClipPlane = xFloat + 952f;
                        }
                    }

                    Vector3 horizonPosition = theHorizonSky.transform.localPosition;
                    horizonPosition.y = -45f;
                    theHorizonSky.transform.localPosition = horizonPosition;

                    theHorizonSky.transform.parent = theParentGo.transform;
                }
            }
        }

        /// <summary>
        /// Sets the horizon shader material pramaters
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="enableSystem"></param>
        public static void SetHorizonShaderSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile)
        {
            if (profile.useSkies)
            {
                if (profile.horizonSkyEnabled)
                {
                    AddHorizonSky("Ambient Skies Horizon");

                    Material horizonMaterial = GetHorizonMaterial();
                    if (horizonMaterial != null)
                    {
                        ApplyHorizonSettings(skyProfiles, profile, horizonMaterial);
                    }             
                }
                else
                {
                    RemoveHorizonSky();
                }
            }
            else
            {
                RemoveHorizonSky();
            }
        }

        /// <summary>
        /// Gets our own horizon shader if in the scene
        /// </summary>
        /// <returns>The water material if there is one</returns>
        public static Material GetHorizonMaterial()
        {
            //Grabs water material and returns
            string horizonObject = SkyboxUtils.GetAssetPath("Horizon Sky Material");
            Material horizonMaterial;
            if (!string.IsNullOrEmpty(horizonObject))
            {
                horizonMaterial = AssetDatabase.LoadAssetAtPath<Material>(horizonObject);
                if (horizonMaterial != null)
                {
                    //returns the material
                    return horizonMaterial;
                }
            }
            return null;
        }

        /// <summary>
        /// Applies the settings to the horizons material and gameobject
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="horizonMaterial"></param>
        public static void ApplyHorizonSettings(AmbientSkyProfiles skyProfiles, AmbientSkyboxProfile profile, Material horizonMaterial)
        {
            if (horizonMaterial != null)
            {
                horizonMaterial.SetFloat("_Scattering", profile.horizonScattering);
                horizonMaterial.SetFloat("_FogDensity", profile.horizonFogDensity);
                horizonMaterial.SetFloat("_HorizonFalloff", profile.horizonFalloff);
                horizonMaterial.SetFloat("_HorizonBlend", profile.horizonBlend);
            }

            GameObject horizonObject = GameObject.Find("Ambient Skies Horizon");
            if (horizonObject != null)
            {
                if (profile.scaleHorizonObjectWithFog && profile.fogType == AmbientSkiesConsts.VolumeFogType.Linear)
                {
                    if (skyProfiles.skyTypeNonHD == AmbientSkiesConsts.SkyType.HDRISky)
                    {
                        horizonObject.transform.localScale = new Vector3(profile.fogDistance, profile.fogDistance * 1.2f, profile.fogDistance);
                    }
                    else
                    {
                        horizonObject.transform.localScale = new Vector3(profile.proceduralFogDistance, profile.proceduralFogDistance * 1.2f, profile.proceduralFogDistance);
                    }
                }
                else
                {
                    horizonObject.transform.localScale = profile.horizonSize;
                }

                if (!profile.followPlayer)
                {
                    horizonObject.transform.position = profile.horizonPosition;
                }
            }
            else
            {
                Debug.Log("Unable to find Ambient Skies Horizon Prefab, did you rename the object?");
            }

            HorizonSky horizonSettings = Object.FindObjectOfType<HorizonSky>();
            if (horizonSettings != null)
            {
                horizonSettings.m_followsCameraPosition = profile.followPlayer;
                horizonSettings.m_positionUpdate = profile.horizonUpdateTime;
            }
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
        /// Get or create a parent object
        /// </summary>
        /// <param name="parentGameObject">Name of the parent object to get or create</param>
        /// <returns>Parent objet</returns>
        public static GameObject GetOrCreateParentObject(string parentGameObject)
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

            return theParentGo;
        }

        /// <summary>
        /// Setup the main camera for use with HDR skyboxes
        /// </summary>
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

        #endregion
    }
}