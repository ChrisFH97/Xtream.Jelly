using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class Episode
{
    [JsonProperty("id")]
    public int EpisodeId { get; set; }

    [JsonProperty("episode_num")]
    public int EpisodeNum { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    [JsonProperty("container_extension")]
    public string ContainerExtension { get; set; } = string.Empty;

    [JsonConverter(typeof(OnlyObjectConverter<EpisodeInfo>))]
    [JsonProperty("info")]
    public EpisodeInfo? Info { get; set; } = new EpisodeInfo();

    [JsonProperty("custom_sid")]
    public string CustomSid { get; set; } = string.Empty;

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("added")]
    public DateTime? Added { get; set; }

    [JsonProperty("season")]
    public int Season { get; set; }

    [JsonProperty("direct_source")]
    public string DirectSource { get; set; } = string.Empty;
}
