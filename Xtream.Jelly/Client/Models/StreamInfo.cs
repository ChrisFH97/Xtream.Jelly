using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class StreamInfo
{
    [JsonProperty("num")]
    public int Num { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("stream_type")]
    public string StreamType { get; set; } = string.Empty;

    [JsonProperty("stream_id")]
    public int StreamId { get; set; }

    [JsonProperty("stream_icon")]
    public string StreamIcon { get; set; } = string.Empty;

    [JsonProperty("epg_channel_id")]
    public string EpgChannelId { get; set; } = string.Empty;

    [JsonProperty("added")]
    public string Added { get; set; } = string.Empty;

    [JsonProperty("category_id")]
    public int? CategoryId { get; set; }

    [JsonProperty("container_extension")]
    public string ContainerExtension { get; set; } = string.Empty;

    [JsonProperty("custom_sid")]
    public string CustomSid { get; set; } = string.Empty;

    [JsonProperty("tv_archive")]
    public bool TvArchive { get; set; }

    [JsonProperty("direct_source")]
    public string DirectSource { get; set; } = string.Empty;

    [JsonProperty("tv_archive_duration")]
    public int TvArchiveDuration { get; set; }
}
