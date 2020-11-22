using UnityEngine;
using UnityEditor;
using System.Collections;

namespace SensorToolkit
{
    [CustomEditor(typeof(SteeringRig2D))]
    public class SteeringRig2DEditor : Editor
    {
        SerializedProperty ignoreList;
        SerializedProperty avoidanceSensitivity;
        SerializedProperty maxAvoidanceLength;
        SerializedProperty rotateTowardsTarget;
        SerializedProperty rb;
        SerializedProperty turnForce;
        SerializedProperty moveForce;
        SerializedProperty strafeForce;
        SerializedProperty turnSpeed;
        SerializedProperty moveSpeed;
        SerializedProperty strafeSpeed;
        SerializedProperty stoppingDistance;
        SerializedProperty destinationTransform;
        SerializedProperty faceTowardsTransform;

        SteeringRig2D steeringRig;

        void OnEnable()
        {
            if (serializedObject == null) return;

            steeringRig = serializedObject.targetObject as SteeringRig2D;
            ignoreList = serializedObject.FindProperty("IgnoreList");
            avoidanceSensitivity = serializedObject.FindProperty("AvoidanceSensitivity");
            maxAvoidanceLength = serializedObject.FindProperty("MaxAvoidanceLength");
            rotateTowardsTarget = serializedObject.FindProperty("RotateTowardsTarget");
            rb = serializedObject.FindProperty("RB");
            turnForce = serializedObject.FindProperty("TurnForce");
            moveForce = serializedObject.FindProperty("MoveForce");
            strafeForce = serializedObject.FindProperty("StrafeForce");
            turnSpeed = serializedObject.FindProperty("TurnSpeed");
            moveSpeed = serializedObject.FindProperty("MoveSpeed");
            strafeSpeed = serializedObject.FindProperty("StrafeSpeed");
            stoppingDistance = serializedObject.FindProperty("StoppingDistance");
            destinationTransform = serializedObject.FindProperty("DestinationTransform");
            faceTowardsTransform = serializedObject.FindProperty("FaceTowardsTransform");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(ignoreList, true);
            EditorGUILayout.PropertyField(avoidanceSensitivity);
            EditorGUILayout.PropertyField(maxAvoidanceLength);
            EditorGUILayout.PropertyField(rotateTowardsTarget);
            EditorGUILayout.PropertyField(rb);
            if (rb.objectReferenceValue != null)
            {
                if ((rb.objectReferenceValue as Rigidbody2D).isKinematic)
                {
                    EditorGUILayout.PropertyField(turnSpeed);
                    EditorGUILayout.PropertyField(moveSpeed);
                    EditorGUILayout.PropertyField(strafeSpeed);
                }
                else
                {
                    EditorGUILayout.PropertyField(turnForce);
                    EditorGUILayout.PropertyField(moveForce);
                    EditorGUILayout.PropertyField(strafeForce);
                }
                EditorGUILayout.PropertyField(stoppingDistance);
                EditorGUILayout.PropertyField(destinationTransform);
                EditorGUILayout.PropertyField(faceTowardsTransform);
            }

            displayErrors();

            serializedObject.ApplyModifiedProperties();
        }

        void displayErrors()
        {
            EditorGUILayout.Space();
            var raySensors = steeringRig.GetComponentsInChildren<RaySensor2D>();

            if (raySensors.Length == 0)
            {
                EditorGUILayout.HelpBox("Steering Rig looks for child Ray Sensors to calculate avoidance vectors, you should add some.", MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < raySensors.Length; i++)
                {
                    if (raySensors[i].IgnoreList != null && raySensors[i].IgnoreList.Count > 0 && raySensors[i].IgnoreList != steeringRig.IgnoreList)
                    {
                        EditorGUILayout.HelpBox("One or more of the steering ray sensors has objects assigned to its IgnoreList parameter. "
                            + "These will be overwritten by the steering rigs IgnoreList.", MessageType.Warning);
                        break;
                    }
                }
            }
        }
    }
}