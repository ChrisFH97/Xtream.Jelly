using System;
using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class Season
{
    [JsonProperty("air_date")]
    public DateTime? AirDate { get; set; }

    [JsonProperty("episode_count")]
    public int EpisodeCount { get; set; }

    [JsonProperty("id")]
    public int SeasonId { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonProperty("season_number")]
    public int SeasonNumber { get; set; }

    [JsonProperty("cover")]
    public string Cover { get; set; } = string.Empty;

    [JsonProperty("cover_big")]
    public string CoverBig { get; set; } = string.Empty;
}
