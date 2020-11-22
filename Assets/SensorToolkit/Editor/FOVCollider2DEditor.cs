using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SensorToolkit
{
    [CustomEditor(typeof(FOVCollider2D))]
    [CanEditMultipleObjects]
    public class FOVCollider2DEditor : Editor
    {
        SerializedProperty length;
        SerializedProperty baseSize;
        SerializedProperty fovAngle;
        SerializedProperty resolution;

        void OnEnable()
        {
            if (serializedObject == null) return;

            length = serializedObject.FindProperty("Length");
            baseSize = serializedObject.FindProperty("BaseSize");
            fovAngle = serializedObject.FindProperty("FOVAngle");
            resolution = serializedObject.FindProperty("Resolution");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(length);
            EditorGUILayout.PropertyField(baseSize);
            EditorGUILayout.PropertyField(fovAngle);
            EditorGUILayout.PropertyField(resolution);

            serializedObject.ApplyModifiedProperties();
        }
    }
}