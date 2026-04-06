using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xtream.Jelly.Client;

/// <summary>
/// Converts singular tokens to lists.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
public class SingularToListConverter<T> : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType) => objectType == typeof(T);

    /// <inheritdoc/>
    public override ICollection<T>? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.StartObject:
                T? result = serializer.Deserialize<T>(reader);
                if (result is null)
                {
                    return null;
                }

                return [result];
            case JsonToken.StartArray:
                return serializer.Deserialize<List<T>>(reader);
            case JsonToken.String:
                if (typeof(T) == typeof(string))
                {
                    goto case JsonToken.StartObject;
                }

                goto default;
            case JsonToken.Null:
                return [];
            default:
                throw new JsonReaderException("The JsonReader points to an unexpected point.");
        }
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is T typedValue)
        {
            value = new List<T> { typedValue };
        }

        serializer.Serialize(writer, value);
    }
}
