using System;
using System.Text;
using Newtonsoft.Json;

namespace Xtream.Jelly.Client;

/// <summary>
/// Converts strings from and to base64.
/// </summary>
public class Base64Converter : JsonConverter
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

        byte[] bytes = Convert.FromBase64String((string)reader.Value);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            throw new ArgumentException("Value cannot be null.");
        }

        byte[] bytes = Encoding.UTF8.GetBytes((string)value);
        writer.WriteValue(Convert.ToBase64String(bytes));
    }
}
