using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

public class SceneObjectJsonConverter : JsonConverter
{
    private readonly Type[] _types;

    public SceneObjectJsonConverter(params Type[] types)
    {
        _types = types;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        JToken t = JToken.FromObject(value);
        t.WriteTo(writer);
        writer.Close();
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            existingValue = existingValue ?? serializer.ContractResolver.ResolveContract(objectType).DefaultCreator();
            serializer.Populate(reader, existingValue);
            
            return existingValue;
        }
        else if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }
        else
        {
            throw new JsonSerializationException();
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return _types.Any(t => t == objectType);
    }
}
