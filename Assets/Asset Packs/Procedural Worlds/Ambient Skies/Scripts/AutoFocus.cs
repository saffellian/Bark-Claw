//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace AmbientSkies
{
    /// <summary>
    /// A script to handle auto focus for Ambient Skies
    /// </summary>
    public class AutoFocus : MonoBehaviour
    {
        #region Variables
        /// <summary>
        /// Type of tracking to use.
        /// </summary>
        public AmbientSkiesConsts.DOFTrackingType m_trackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;

        /// <summary>
        /// An additional manual offset to be added to the focus - will also be used when fixed offset is selected
        /// </summary>
        public float m_focusOffset = 0f;

        /// <summary>
        /// Source camera object - where the ray shoots from
        /// </summary>
        public Camera m_sourceCamera;

        /// <summary>
        /// Target object for dof - set this to a physical object at start to track that object - leave it null
        /// to have it updated automatically based on raycast
        /// </summary>
        public GameObject m_targetObject;

        /// <summary>
        /// Which layers we will hit
        /// </summary>
        public LayerMask m_targetLayer = 1;

        /// <summary>
        /// Maximum focus distance when follow mouse and follow screen selected
        /// </summary>
        public float m_maxFocusDistance = 100f;

        /// <summary>
        /// The actual focus distance value
        /// </summary>
        public float m_actualFocusDistance = 1f;

        /// <summary>
        /// Processing profile that is current selected
        /// </summary>
        [HideInInspector]
        public AmbientPostProcessingProfile m_processingProfile;

        /// <summary>
        /// Interpolate focus over time
        /// </summary>
        //public bool m_interpolateFocus = true;

        /// <summary>
        /// Time to interpolate
        /// </summary>
        //public float m_interpolationTime = 0.75f;

        /// <summary>
        /// Determine if we got a hit within the max distance
        /// </summary>
        private bool m_maxDistanceExceeded = false;

        /// <summary>
        /// Our DOF object
        /// </summary>
        private DepthOfField m_dof;

        /// <summary>
        /// Our last hit point
        /// </summary>
        private Vector3 m_dofTrackingPoint = Vector3.negativeInfinity;
        #endregion

        #region Start and Update
        /// <summary>
        /// Get the main camera if it doesnt exist
        /// </summary>
        void Start()
        {
            SetupAutoFocus();
        }

        /// <summary>
        /// Apply on disable
        /// </summary>
        private void OnDisable()
        {
            if (m_processingProfile != null)
            {
                m_processingProfile.depthOfFieldFocusDistance = m_actualFocusDistance;
            }
        }

        /// <summary>
        /// Process DOF update
        /// </summary>
        void LateUpdate()
        {
            if (m_sourceCamera == null || m_dof == null)
            {
                return;
            }

            //Setup 
            SetupAutoFocus();

            //Update the focus target
            UpdateDofTrackingPoint();

            //Set focus distance
            if (m_trackingType == AmbientSkiesConsts.DOFTrackingType.FixedOffset || m_maxDistanceExceeded)
            {
                m_dof.focusDistance.value = m_maxFocusDistance + m_focusOffset;
            }

            m_actualFocusDistance = m_dof.focusDistance.value;

            if (m_processingProfile != null)
            {
                m_processingProfile.depthOfFieldFocusDistance = m_actualFocusDistance;
            }
        }
        #endregion

        #region Depth Of Field Functions
        /// <summary>
        /// Do a raycast to update the focus target
        /// </summary>
        void UpdateDofTrackingPoint()
        {
            switch (m_trackingType)
            {
                case AmbientSkiesConsts.DOFTrackingType.LeftMouseClick:
                {
                    if (Input.GetMouseButton(0))
                    {
                        RaycastHit hit;
                        Ray ray = m_sourceCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, m_maxFocusDistance, m_targetLayer))
                        {
                            m_maxDistanceExceeded = false;
                            m_dofTrackingPoint = hit.point;

                            m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                        }
                        else
                        {
                            m_maxDistanceExceeded = true;
                        }
                    }
                    break;
                }
                case AmbientSkiesConsts.DOFTrackingType.RightMouseClick:
                {
                    if (Input.GetMouseButton(1))
                    {
                        RaycastHit hit;
                        Ray ray = m_sourceCamera.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, m_maxFocusDistance, m_targetLayer))
                        {
                            m_maxDistanceExceeded = false;
                            m_dofTrackingPoint = hit.point;

                            m_dof.focusDistance.value = (m_sourceCamera.transform.position - m_dofTrackingPoint).magnitude + m_focusOffset;
                        }
                        else
                        {
                            m_maxDistanceExceeded = true;
                        }
                    }
                    break;
                }
                case AmbientSkiesConsts.DOFTrackingType.FollowScreen:
                {
                    RaycastHit hit;
                    Ray ray = m_sourceCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    if (Physics.Raycast(ray, out hit, m_maxFocusDistance, m_targetLayer))
                    {
                        m_maxDistanceExceeded = false;
                        m_dofTrackingPoint = hit.point;
                    }
                    else
                    {
                        m_maxDistanceExceeded = true;
                    }
                    break;
                }
                case AmbientSkiesConsts.DOFTrackingType.FollowTarget:
                {
                    m_dofTrackingPoint = m_targetObject.transform.position;
                    break;
                }
            }
        }

        /// <summary>
        /// Setup autofocus object
        /// </summary>
        void SetupAutoFocus()
        {
            //Set up main camera
            if (m_sourceCamera == null)
            {
                m_sourceCamera = GetMainCamera();
                if (m_sourceCamera == null)
                {
                    Debug.Log("DOF Autofocus exiting, unable to find main camera!");
                    enabled = false;
                    return;
                }
            }

            //Determine tracking type
            if (m_trackingType == AmbientSkiesConsts.DOFTrackingType.FollowTarget && m_targetObject == null)
            {
                Debug.Log("Tracking target is missing, falling back to follow screen!");
                m_trackingType = AmbientSkiesConsts.DOFTrackingType.FollowScreen;
            }

            //Find our DOF component
            if (m_dof == null)
            {
                GameObject ppObj = GameObject.Find("Global Post Processing");
                if (ppObj == null)
                {
                    Debug.Log("DOF Autofocus exiting, unable to global post processing object!");
                    enabled = false;
                    return;
                }

                PostProcessVolume ppVolume = ppObj.GetComponent<PostProcessVolume>();
                {
                    if (ppVolume == null)
                    {
                        Debug.Log("DOF Autofocus exiting, unable to global post processing volume!");
                        enabled = false;
                        return;
                    }
                }

                PostProcessProfile ppProfile = ppVolume.sharedProfile;
                if (ppProfile == null)
                {
                    Debug.Log("DOF Autofocus exiting, unable to global post processing profile!");
                    enabled = false;
                    return;
                }

                if (!ppProfile.HasSettings<DepthOfField>())
                {
                    Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                    enabled = false;
                    return;
                }

                if (!ppProfile.TryGetSettings<DepthOfField>(out m_dof))
                {
                    Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                    m_dof = null;
                    enabled = false;
                    return;
                }
                else
                {
                    m_dof.focusDistance.overrideState = true;
                    m_actualFocusDistance = m_dof.focusDistance.value;
                }
            }
            else
            {
                m_dof.focusDistance.overrideState = true;
                m_actualFocusDistance = m_dof.focusDistance.value;
            }
        }
        #endregion

        #region Utils
        /// <summary>
        /// Get the main camera or null if none available
        /// </summary>
        /// <returns>Main camera or null</returns>
        Camera GetMainCamera()
        {
            GameObject mainCameraObject = GameObject.Find("Main Camera");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponent<Camera>();
            }

            mainCameraObject = GameObject.Find("Camera");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponent<Camera>();
            }

            mainCameraObject = GameObject.Find("FirstPersonCharacter");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponentInChildren<Camera>();
            }

            mainCameraObject = GameObject.Find("FlyCam");
            if (mainCameraObject != null)
            {
                return mainCameraObject.GetComponent<Camera>();
            }

            if (Camera.main != null)
            {
                return Camera.main;
            }

            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();
            foreach (var camera in cameras)
            {
                return camera;
            }

            return null;
        }
        #endregion
    }
}
#endif