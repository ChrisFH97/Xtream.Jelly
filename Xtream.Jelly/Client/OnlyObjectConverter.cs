using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xtream.Jelly.Client;

/// <summary>
/// Converts only object tokens, returning null for non-object JSON.
/// </summary>
/// <typeparam name="T">The object type to convert.</typeparam>
public class OnlyObjectConverter<T> : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType) => objectType == typeof(T);

    /// <inheritdoc/>
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        JToken token = JToken.Load(reader);
        if (token.Type == JTokenType.Object)
        {
            return token.ToObject<T>();
        }

        return null;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
