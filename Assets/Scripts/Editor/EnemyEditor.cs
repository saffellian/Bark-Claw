using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy))]
[CanEditMultipleObjects]
public class EnemyEditor : Editor
{
    private List<string> hideThese = new List<string>();

    public override void OnInspectorGUI()
    {
        
        //base.OnInspectorGUI();
        serializedObject.Update();

        hideThese = new List<string>();
        SerializedProperty prop = serializedObject.GetIterator();

        if (prop.NextVisible(true))
        {
            do
            {
                if (hideThese.Contains(prop.name))
                    continue;

                if (prop.name == "enemyType")
                {
                    if (prop.enumValueIndex == 0)
                    {
                        if (!hideThese.Contains("attackStrafeDistance"))
                        {
                            hideThese.Add("attackStrafeDistance");
                            hideThese.Add("attackStrafeDepth");
                            hideThese.Add("attackStrafeWidth");
                            hideThese.Add("projectile");
                            hideThese.Add("projectileSpawn");
                        }

                        hideThese.Remove("chaseDistance");
                        hideThese.Remove("attackSpeed");
                        hideThese.Remove("attackDamage");
                    }
                    else if (prop.enumValueIndex == 1)
                    {
                        if (!hideThese.Contains("chaseDistance"))
                        {
                            hideThese.Add("chaseDistance");
                            hideThese.Add("attackSpeed");
                            hideThese.Add("attackDamage");
                        }

                        hideThese.Remove("attackStrafeDistance");
                        hideThese.Remove("attackStrafeDepth");
                        hideThese.Remove("attackStrafeWidth");
                        hideThese.Remove("projectile");
                        hideThese.Remove("projectileSpawn");
                    }
                }
                else if (prop.name == "patrolType")
                {
                    if (prop.enumValueIndex == 0)
                    {
                        if (!hideThese.Contains("patrolRegion"))
                        {
                            hideThese.Add("patrolRegion");
                            hideThese.Remove("patrolPoints");
                        }
                    }
                    else
                    {
                        if (!hideThese.Contains("patrolPoints"))
                        {
                            hideThese.Add("patrolPoints");
                            hideThese.Remove("patrolRegion");
                        }
                    }
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name), true);
            } 
            while (prop.NextVisible(false));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
