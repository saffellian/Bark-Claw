using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public class EnemyTypeAttribute : PropertyAttribute
{
    public readonly string type;

    public EnemyTypeAttribute(string type)
    {
        this.type = type;
    }
}