using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SensorToolkit
{
    [CustomEditor(typeof(RaySensor2D))]
    [CanEditMultipleObjects]
    public class RaySensor2DEditor : Editor
    {
        SerializedProperty length;
        SerializedProperty radius;
        SerializedProperty ignoreList;
        SerializedProperty tagFilterEnabled;
        SerializedProperty tagFilter;
        SerializedProperty obstructedByLayers;
        SerializedProperty detectsOnLayers;
        SerializedProperty detectionMode;
        SerializedProperty direction;
        SerializedProperty worldSpace;
        SerializedProperty sensorUpdateMode;
        SerializedProperty initialBufferSize;
        SerializedProperty dynamicallyIncreaseBufferSize;
        SerializedProperty onDetected;
        SerializedProperty onLostDetection;
        SerializedProperty onObstructed;
        SerializedProperty onClear;

        RaySensor2D raySensor;
        bool isTesting = false;
        bool showEvents = false;

        void OnEnable()
        {
            if (serializedObject == null) return;

            raySensor = serializedObject.targetObject as RaySensor2D;
            length = serializedObject.FindProperty("Length");
            radius = serializedObject.FindProperty("Radius");
            ignoreList = serializedObject.FindProperty("IgnoreList");
            tagFilterEnabled = serializedObject.FindProperty("EnableTagFilter");
            tagFilter = serializedObject.FindProperty("AllowedTags");
            obstructedByLayers = serializedObject.FindProperty("ObstructedByLayers");
            detectsOnLayers = serializedObject.FindProperty("DetectsOnLayers");
            detectionMode = serializedObject.FindProperty("DetectionMode");
            direction = serializedObject.FindProperty("Direction");
            worldSpace = serializedObject.FindProperty("WorldSpace");
            sensorUpdateMode = serializedObject.FindProperty("SensorUpdateMode");
            initialBufferSize = serializedObject.FindProperty("InitialBufferSize");
            dynamicallyIncreaseBufferSize = serializedObject.FindProperty("DynamicallyIncreaseBufferSize");
            onDetected = serializedObject.FindProperty("OnDetected");
            onLostDetection = serializedObject.FindProperty("OnLostDetection");
            onObstructed = serializedObject.FindProperty("OnObstruction");
            onClear = serializedObject.FindProperty("OnClear");
            raySensor.OnSensorUpdate += onSensorUpdate;
        }

        void OnDisable()
        {
            raySensor.OnSensorUpdate -= onSensorUpdate;
            stopTesting();
        }

        public override void OnInspectorGUI()
        {
            if (raySensor.transform.hasChanged)
            {
                stopTesting();
                raySensor.transform.hasChanged = false;
            }

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(length);
            EditorGUILayout.PropertyField(radius);
            EditorGUILayout.PropertyField(direction);
            EditorGUILayout.PropertyField(worldSpace);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ignoreList, true);
            tagFilterEditor();
            EditorGUILayout.PropertyField(obstructedByLayers);
            EditorGUILayout.PropertyField(detectsOnLayers);
            EditorGUILayout.PropertyField(detectionMode);
            EditorGUILayout.PropertyField(sensorUpdateMode);

            EditorGUILayout.Space();

            if (showEvents = EditorGUILayout.Foldout(showEvents, "Events")) {
                EditorGUILayout.PropertyField(onDetected);
                EditorGUILayout.PropertyField(onLostDetection);
                EditorGUILayout.PropertyField(onObstructed);
                EditorGUILayout.PropertyField(onClear);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(initialBufferSize);
            EditorGUILayout.PropertyField(dynamicallyIncreaseBufferSize);
            if (raySensor.CurrentBufferSize != 0 && raySensor.CurrentBufferSize != raySensor.InitialBufferSize) {
                EditorGUILayout.HelpBox("Buffer size expanded to: " + raySensor.CurrentBufferSize, MessageType.Info);
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
            foreach (GameObject go in raySensor.DetectedObjects)
            {
                EditorGUILayout.ObjectField(go, typeof(GameObject), true);
            }

            if (!raySensor.IsObstructed) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("*** Ray is Obstructed ***");
            EditorGUILayout.ObjectField(raySensor.ObstructedBy.gameObject, typeof(GameObject), true);
        }

        void onSensorUpdate()
        {
            Repaint();
        }

        void startTesting()
        {
            if (isTesting || Application.isPlaying || raySensor == null) return;

            isTesting = true;
            raySensor.SendMessage("testRay");
            SceneView.RepaintAll();
        }

        void stopTesting()
        {
            if (!isTesting || Application.isPlaying || raySensor == null) return;

            isTesting = false;
            raySensor.SendMessage("reset");
            SceneView.RepaintAll();
        }
    }
}