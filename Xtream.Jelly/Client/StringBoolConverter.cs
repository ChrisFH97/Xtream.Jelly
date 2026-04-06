using System;
using Newtonsoft.Json;

namespace Xtream.Jelly.Client;

/// <summary>
/// Converts "1"/"0" strings to boolean.
/// </summary>
public class StringBoolConverter : JsonConverter
{
    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => objectType == typeof(string);

    /// <inheritdoc />
    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value == null)
        {
            throw new ArgumentException("Value cannot be null.");
        }

        return "1".Equals((string)reader.Value, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            throw new ArgumentException("Value cannot be null.");
        }

        string result = (bool)value ? "1" : "0";
        writer.WriteValue(result);
    }
}
