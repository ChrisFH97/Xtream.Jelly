using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class EpgInfo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("epg_id")]
    public int EpgId { get; set; }

    [JsonConverter(typeof(Base64Converter))]
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("lang")]
    public string Language { get; set; } = string.Empty;

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("start_timestamp")]
    public DateTime Start { get; set; }

    [JsonProperty("start")]
    public DateTime StartLocalTime { get; set; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("stop_timestamp")]
    public DateTime End { get; set; }

    [JsonConverter(typeof(Base64Converter))]
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("channel_id")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonProperty("now_playing")]
    public bool NowPlaying { get; set; }

    [JsonProperty("has_archive")]
    public bool HasArchive { get; set; }
}
