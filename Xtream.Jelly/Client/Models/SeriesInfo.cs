using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class SeriesInfo
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("cover")]
    public string Cover { get; set; } = string.Empty;

    [JsonProperty("plot")]
    public string Plot { get; set; } = string.Empty;

    [JsonProperty("cast")]
    public string Cast { get; set; } = string.Empty;

    [JsonProperty("director")]
    public string Director { get; set; } = string.Empty;

    [JsonProperty("genre")]
    public string Genre { get; set; } = string.Empty;

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("last_modified")]
    public DateTime LastModified { get; set; }

    [JsonProperty("rating")]
    public decimal Rating { get; set; }

    [JsonProperty("rating_5based")]
    public decimal Rating5Based { get; set; }

    [JsonProperty("backdrop_path")]
#pragma warning disable CA2227
    public ICollection<string> BackdropPaths { get; set; } = new List<string>();
#pragma warning restore CA2227

    [JsonProperty("youtube_trailer")]
    public string YoutubeTrailer { get; set; } = string.Empty;

    [JsonProperty("episode_run_time")]
    public int EpisodeRunTime { get; set; }

    [JsonProperty("category_id")]
    public int CategoryId { get; set; }
}
