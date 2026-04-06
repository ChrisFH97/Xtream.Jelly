using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Xtream.Jelly.Configuration;

/// <summary>
/// Dictionary implementation that can be serialized to and from XML.
/// </summary>
/// <typeparam name="TKey">The dictionary key type.</typeparam>
/// <typeparam name="TValue">The dictionary value type.</typeparam>
[Serializable]
[XmlRoot("Dictionary")]
public sealed class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
where TKey : notnull
{
    private const string ItemTag = "Item";
    private const string KeyTag = "Key";
    private const string ValueTag = "Value";

    private static readonly XmlSerializer _keySerializer = new(typeof(TKey));
    private static readonly XmlSerializer _valueSerializer = new(typeof(TValue));

    /// <summary>Initializes a new instance of the
    /// <see cref="SerializableDictionary{TKey, TValue}"/> class.
    /// </summary>
    public SerializableDictionary()
    {
    }

    /// <inheritdoc />
    public XmlSchema? GetSchema() => null;

    /// <inheritdoc />
    public void ReadXml(XmlReader reader)
    {
        var wasEmpty = reader.IsEmptyElement;
        reader.Read();
        if (wasEmpty)
        {
            return;
        }

        try
        {
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                ReadItem(reader);
                reader.MoveToContent();
            }
        }
        finally
        {
            reader.ReadEndElement();
        }
    }

    /// <inheritdoc />
    public void WriteXml(XmlWriter writer)
    {
        foreach (var keyValuePair in this)
        {
            WriteItem(writer, keyValuePair);
        }
    }

    private void ReadItem(XmlReader reader)
    {
        reader.ReadStartElement(ItemTag);
        try
        {
            Add(ReadKey(reader), ReadValue(reader));
        }
        finally
        {
            reader.ReadEndElement();
        }
    }

    private static TKey ReadKey(XmlReader reader)
    {
        reader.ReadStartElement(KeyTag);
        try
        {
            return (TKey?)_keySerializer.Deserialize(reader) ?? throw new SerializationException("Key cannot be null");
        }
        finally
        {
            reader.ReadEndElement();
        }
    }

    private static TValue ReadValue(XmlReader reader)
    {
        reader.ReadStartElement(ValueTag);
        try
        {
            return (TValue?)_valueSerializer.Deserialize(reader) ?? throw new SerializationException("Value cannot be null");
        }
        finally
        {
            reader.ReadEndElement();
        }
    }

    private static void WriteItem(XmlWriter writer, KeyValuePair<TKey, TValue> keyValuePair)
    {
        writer.WriteStartElement(ItemTag);
        try
        {
            WriteKey(writer, keyValuePair.Key);
            WriteValue(writer, keyValuePair.Value);
        }
        finally
        {
            writer.WriteEndElement();
        }
    }

    private static void WriteKey(XmlWriter writer, TKey key)
    {
        writer.WriteStartElement(KeyTag);
        try
        {
            _keySerializer.Serialize(writer, key);
        }
        finally
        {
            writer.WriteEndElement();
        }
    }

    private static void WriteValue(XmlWriter writer, TValue value)
    {
        writer.WriteStartElement(ValueTag);
        try
        {
            _valueSerializer.Serialize(writer, value);
        }
        finally
        {
            writer.WriteEndElement();
        }
    }
}
