using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SensorToolkit
{
    [CustomEditor(typeof(TriggerSensor))]
    [CanEditMultipleObjects]
    public class TriggerSensorEditor : Editor
    {
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectionMode;
        SerializedProperty requiresLineOfSight;
        SerializedProperty blocksLineOfSight;
        SerializedProperty lineOfSightUpdateMode;
        SerializedProperty checkLineOfSightInterval;
        SerializedProperty testLOSTargetsOnly;
        SerializedProperty numberOfRays;
        SerializedProperty minimumVisibility;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;

        TriggerSensor triggerSensor;
        bool showEvents = false;

        void OnEnable()
        {
            if (serializedObject == null) return;

            triggerSensor = serializedObject.targetObject as TriggerSensor;
            ignoreList = serializedObject.FindProperty("IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("EnableTagFilter");
            tagFilter = serializedObject.FindProperty("AllowedTags");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            requiresLineOfSight = serializedObject.FindProperty("RequiresLineOfSight");
            blocksLineOfSight = serializedObject.FindProperty("BlocksLineOfSight");
            lineOfSightUpdateMode = serializedObject.FindProperty("LineOfSightUpdateMode");
            checkLineOfSightInterval = serializedObject.FindProperty("CheckLineOfSightInterval");
            testLOSTargetsOnly = serializedObject.FindProperty("TestLOSTargetsOnly");
            numberOfRays = serializedObject.FindProperty("NumberOfRays");
            minimumVisibility = serializedObject.FindProperty("MinimumVisibility");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            triggerSensor.OnSensorUpdate += onSensorUpdate;

            triggerSensor.ShowRayCastDebug = new HashSet<GameObject>();
        }

        void OnDisable()
        {
            triggerSensor.OnSensorUpdate -= onSensorUpdate;
            triggerSensor.ShowRayCastDebug = null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(ignoreList, true);
            tagFilterEditor();
            EditorGUILayout.PropertyField(detectionMode);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(requiresLineOfSight);
            if (requiresLineOfSight.boolValue)
            {
                EditorGUILayout.PropertyField(blocksLineOfSight);
                EditorGUILayout.PropertyField(lineOfSightUpdateMode);
                if ((TriggerSensor.UpdateMode)lineOfSightUpdateMode.enumValueIndex == TriggerSensor.UpdateMode.FixedInterval)
                {
                    EditorGUILayout.PropertyField(checkLineOfSightInterval);
                }
                EditorGUILayout.PropertyField(testLOSTargetsOnly);
                if (!testLOSTargetsOnly.boolValue)
                {
                    EditorGUILayout.PropertyField(numberOfRays);
                }
                EditorGUILayout.PropertyField(minimumVisibility);
            }

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events"))
            {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
            }

            if (EditorApplication.isPlaying || EditorApplication.isPaused)
            {
                if (triggerSensor.RequiresLineOfSight)
                    displayDetectedObjectsRaycast();
                else
                    displayDetectedObjects();
            }

            displayErrors();

            serializedObject.ApplyModifiedProperties();
        }

        void tagFilterEditor()
        {
            EditorGUILayout.PropertyField(tagFilterEnabled);
            if (tagFilterEnabled.boolValue)
            {
                EditorGUILayout.PropertyField(tagFilter, true);
            }
        }

        void displayDetectedObjects()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("*** Objects Detected ***");
            foreach (GameObject go in triggerSensor.DetectedObjects)
            {
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            }
        }

        void displayDetectedObjectsRaycast()
        {
            var detected = triggerSensor.DetectedObjects;
            var undetected = triggerSensor.ObjectVisibilities.Keys.Where(go => !detected.Contains(go)).ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("*** Objects Detected ***");
            foreach (GameObject go in detected)
            {
                EditorGUILayout.BeginHorizontal();
                var debug = triggerSensor.ShowRayCastDebug.Contains(go);
                if (debug = EditorGUILayout.Toggle(debug))
                {
                    triggerSensor.ShowRayCastDebug.Add(go);
                }
                else
                {
                    triggerSensor.ShowRayCastDebug.Remove(go);
                }
                EditorGUILayout.PrefixLabel(string.Format("{0:P}", triggerSensor.GetVisibility(go)));
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            GUIStyle redText = new GUIStyle(EditorStyles.label);
            redText.normal.textColor = Color.red;
            EditorGUILayout.LabelField("*** Not Detected ***", redText);
            foreach (GameObject go in undetected)
            {
                EditorGUILayout.BeginHorizontal();
                var debug = triggerSensor.ShowRayCastDebug.Contains(go);
                if (debug = EditorGUILayout.Toggle(debug))
                {
                    triggerSensor.ShowRayCastDebug.Add(go);
                }
                else
                {
                    triggerSensor.ShowRayCastDebug.Remove(go);
                }
                EditorGUILayout.PrefixLabel(string.Format("{0:P}", triggerSensor.GetVisibility(go)), redText);
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

            if (EditorApplication.isPaused)
            {
                SceneView.RepaintAll();
            }
        }

        void displayErrors()
        {
            EditorGUILayout.Space();
            if (!checkForTriggers())
            {
                EditorGUILayout.HelpBox("Needs active Trigger Collider to detect GameObjects!", MessageType.Warning);
            }
            if (triggerSensor.DetectionMode == SensorMode.Colliders && triggerSensor.GetComponent<Rigidbody>() == null)
            {
                EditorGUILayout.HelpBox("In order to detect GameObjects without RigidBodies the TriggerSensor must itself have a RigidBody! Recommend adding a kinematic RigidBody.", MessageType.Warning);
            }
        }

        bool checkForTriggers()
        {
            var hasRB = triggerSensor.GetComponent<Rigidbody>() != null;
            if (hasRB)
            {
                foreach (Collider c in triggerSensor.GetComponentsInChildren<Collider>())
                {
                    if (c.enabled && c.isTrigger) return true;
                }
            }
            else
            {
                foreach (Collider c in triggerSensor.GetComponents<Collider>())
                {
                    if (c.enabled && c.isTrigger) return true;
                }
            }
            return false;
        }

        void onSensorUpdate()
        {
            Repaint();
        }
    }
}