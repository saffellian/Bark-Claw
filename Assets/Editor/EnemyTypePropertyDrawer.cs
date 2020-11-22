using UnityEditor;
using UnityEngine;
using System;
using System.Text.RegularExpressions;

[CustomPropertyDrawer(typeof(EnemyTypeAttribute))]
public class EnemyTypePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
    {
        EnemyTypeAttribute enemy = attribute as EnemyTypeAttribute;
        int index = property.serializedObject.FindProperty("enemyType").enumValueIndex;
        string[] values = Regex.Split(enemy.type.ToLower(), @"\s*,\s*");
        bool match = false;
        for (int i = 0; i < values.Length; i++)
        {
            match = values[i].Trim() == property.serializedObject.FindProperty("enemyType").enumDisplayNames[index].ToLower();
            if (match)
                break;
        }
        
        GUI.enabled = match;

        if (match)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        EnemyTypeAttribute enemy = attribute as EnemyTypeAttribute;
        int index = property.serializedObject.FindProperty("enemyType").enumValueIndex;
        string[] values = Regex.Split(enemy.type.ToLower(), @"\s*,\s*");
        bool match = false;
        for (int i = 0; i < values.Length; i++)
        {
            match = values[i].Trim() == property.serializedObject.FindProperty("enemyType").enumDisplayNames[index].ToLower();
            if (match)
                break;
        }

        if (!match)
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }

        return EditorGUI.GetPropertyHeight(property, label);
    }
}