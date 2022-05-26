using UnityEngine;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class SaveableData
{
    public SaveableData(string dictionaryKey, string jsonContent)
    {
        this.dictionaryKey = dictionaryKey;
        this.jsonContent = jsonContent;
    }

    public string dictionaryKey;
    public string jsonContent;
}

public class Saveable : MonoBehaviour
{
    protected Dictionary<string, object> saveData = new Dictionary<string, object>();

    public virtual SaveableData GetObjectState()
    {
        return null;
    }

    public virtual void ApplyObjectState(string objectJson)
    {
        return;
    }

    public string GetDictionaryKey()
    {
        return $"{Regex.Replace(gameObject.name, @"\s+|[^A-Za-z0-9_\-]", "")}:{this.GetType().Name}";
    }
}

public interface ISaveable
{
    SaveableData GetObjectState();
    void ApplyObjectState(string objectJson);
    string GetDictionaryKey();
}
