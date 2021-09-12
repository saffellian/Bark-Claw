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

public interface ISaveable
{
    SaveableData GetObjectState();
    void ApplyObjectState(string objectJson);
    string GetDictionaryKey();
}
