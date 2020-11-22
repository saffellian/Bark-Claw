using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SensorToolkit
{
    [CustomEditor(typeof(RangeSensor2D))]
    [CanEditMultipleObjects]
    public class RangeSensor2DEditor : Editor
    {
        SerializedProperty detectionRange;
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty detectsOnLayers;
        SerializedProperty sensorUpdateMode;
        SerializedProperty checkInterval;
        SerializedProperty detectionMode;
        SerializedProperty requiresLineOfSight;
        SerializedProperty blocksLineOfSight;
        SerializedProperty testLOSTargetsOnly;
        SerializedProperty numberOfRays;
        SerializedProperty minimumVisibility;
        SerializedProperty initialBufferSize;
        SerializedProperty dynamicallyIncreaseBufferSize;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;

        RangeSensor2D rangeSensor;
        bool isTesting = false;
        bool showEvents = false;

        void OnEnable()
        {
            if (serializedObject == null) return;

            rangeSensor = serializedObject.targetObject as RangeSensor2D;
            detectionRange = serializedObject.FindProperty("SensorRange");
            ignoreList = serializedObject.FindProperty("IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("EnableTagFilter");
            tagFilter = serializedObject.FindProperty("AllowedTags");
            detectsOnLayers = serializedObject.FindProperty("DetectsOnLayers");
            sensorUpdateMode = serializedObject.FindProperty("SensorUpdateMode");
            checkInterval = serializedObject.FindProperty("CheckInterval");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            requiresLineOfSight = serializedObject.FindProperty("RequiresLineOfSight");
            blocksLineOfSight = serializedObject.FindProperty("BlocksLineOfSight");
            testLOSTargetsOnly = serializedObject.FindProperty("TestLOSTargetsOnly");
            numberOfRays = serializedObject.FindProperty("NumberOfRays");
            minimumVisibility = serializedObject.FindProperty("MinimumVisibility");
            initialBufferSize = serializedObject.FindProperty("InitialBufferSize");
            dynamicallyIncreaseBufferSize = serializedObject.FindProperty("DynamicallyIncreaseBufferSize");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            rangeSensor.OnSensorUpdate += onSensorUpdate;

            rangeSensor.ShowRayCastDebug = new HashSet<GameObject>();
        }

        void OnDisable()
        {
            stopTesting();
            rangeSensor.OnSensorUpdate -= onSensorUpdate;
            rangeSensor.ShowRayCastDebug = null;
        }

        public override void OnInspectorGUI()
        {
            if (rangeSensor.transform.hasChanged)
            {
                stopTesting();
                rangeSensor.transform.hasChanged = false;
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(detectionRange);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ignoreList, true);
            tagFilterEditor();
            EditorGUILayout.PropertyField(detectsOnLayers);
            EditorGUILayout.PropertyField(detectionMode);
            EditorGUILayout.PropertyField(sensorUpdateMode);
            if ((RangeSensor.UpdateMode)sensorUpdateMode.enumValueIndex == RangeSensor.UpdateMode.FixedInterval)
            {
                EditorGUILayout.PropertyField(checkInterval);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(requiresLineOfSight);
            if (requiresLineOfSight.boolValue)
            {
                EditorGUILayout.PropertyField(blocksLineOfSight);
                EditorGUILayout.PropertyField(testLOSTargetsOnly);
                if (!testLOSTargetsOnly.boolValue)
                {
                    EditorGUILayout.PropertyField(numberOfRays);
                }
                EditorGUILayout.PropertyField(minimumVisibility);
            }

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(initialBufferSize);
            EditorGUILayout.PropertyField(dynamicallyIncreaseBufferSize);
            if (rangeSensor.CurrentBufferSize != 0 && rangeSensor.CurrentBufferSize != rangeSensor.InitialBufferSize) {
                EditorGUILayout.HelpBox("Buffer size expanded to: " + rangeSensor.CurrentBufferSize, MessageType.Info);
            }

            if (EditorGUI.EndChangeCheck())
            {
                stopTesting();
            }

            EditorGUILayout.Space();

            if (!isTesting && !Application.isPlaying)
            {
                if (GUILayout.Button("Test", GUILayout.Width(100)))
                {
                    startTesting();
                }
            }

            if (EditorApplication.isPlaying || EditorApplication.isPaused || isTesting)
            {
                if (rangeSensor.RequiresLineOfSight)
                    displayDetectedObjectsRaycast();
                else
                    displayDetectedObjects();
            }

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
            foreach (GameObject go in rangeSensor.DetectedObjects)
            {
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            }
        }

        void displayDetectedObjectsRaycast()
        {
            var detected = rangeSensor.DetectedObjects;
            var undetected = rangeSensor.ObjectVisibilities.Keys.Where(go => !detected.Contains(go)).ToList();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("*** Objects Detected ***");
            foreach (GameObject go in detected)
            {
                EditorGUILayout.BeginHorizontal();
                var debug = rangeSensor.ShowRayCastDebug.Contains(go);
                if (debug = EditorGUILayout.Toggle(debug))
                {
                    rangeSensor.ShowRayCastDebug.Add(go);
                }
                else
                {
                    rangeSensor.ShowRayCastDebug.Remove(go);
                }
                EditorGUILayout.PrefixLabel(string.Format("{0:P}", rangeSensor.GetVisibility(go)));
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
                var debug = rangeSensor.ShowRayCastDebug.Contains(go);
                if (debug = EditorGUILayout.Toggle(debug))
                {
                    rangeSensor.ShowRayCastDebug.Add(go);
                }
                else
                {
                    rangeSensor.ShowRayCastDebug.Remove(go);
                }
                EditorGUILayout.PrefixLabel(string.Format("{0:P}", rangeSensor.GetVisibility(go)), redText);
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                EditorGUILayout.EndHorizontal();
            }

            if (EditorApplication.isPaused || isTesting)
            {
                SceneView.RepaintAll();
            }
        }

        void onSensorUpdate()
        {
            Repaint();
        }

        void startTesting()
        {
            if (isTesting || Application.isPlaying || rangeSensor == null) return;

            isTesting = true;
            rangeSensor.SendMessage("testSensor");
            SceneView.RepaintAll();
        }

        void stopTesting()
        {
            if (!isTesting || Application.isPlaying || rangeSensor == null) return;

            isTesting = false;
            rangeSensor.SendMessage("reset");
            SceneView.RepaintAll();
        }
    }
}