using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public class PatrolTypeAttribute : PropertyAttribute
{
    public readonly string type;

    public PatrolTypeAttribute(string type)
    {
        this.type = type;
    }
}