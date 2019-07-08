//Copyright © 2019 Procedural Worlds Pty Limited. All Rights Reserved.
using UnityEngine;
using UnityEditor;
using AmbientSkies.Internal;
using PWCommon1;

namespace AmbientSkies
{
    /// <summary>
    /// Custom Editor
    /// </summary>
    [CustomEditor(typeof(AmbientSkyProfiles))]
    public class AmbientSkyProfilesEditor : PWEditor, IPWEditor
    {
        private EditorUtils m_editorUtils;

        #region Target variables

        private AmbientSkyProfiles m_profile;

        #endregion
        
        #region Constructors destructors and related delegates

        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
            }
        }

        void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this);
            }
            m_profile = (AmbientSkyProfiles) target;
        }

        #endregion

        #region GUI main

        public override void OnInspectorGUI()
        {
            m_profile = (AmbientSkyProfiles) target;
            if (m_profile != null)
            {
                if (m_profile.m_editSettings)
                {
                    EditorGUI.BeginChangeCheck();
                    DrawDefaultInspector();
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (m_profile.m_editSettings)
                        {
                            //Make sure profile indexes are correct
                            for (int profileIdx = 0; profileIdx < m_profile.m_skyProfiles.Count; profileIdx++)
                            {
                                m_profile.m_skyProfiles[profileIdx].profileIndex = profileIdx;
                            }
                            for (int profileIdx = 0; profileIdx < m_profile.m_ppProfiles.Count; profileIdx++)
                            {
                                m_profile.m_ppProfiles[profileIdx].profileIndex = profileIdx;
                            }
                        }
                        EditorUtility.SetDirty(m_profile);
                    }
                }
            }
        }

        #endregion
    }
}