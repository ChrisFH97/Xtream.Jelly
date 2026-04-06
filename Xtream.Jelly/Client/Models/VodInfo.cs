using System;
using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class VodInfo
{
    [JsonProperty("movie_image")]
    public string? MovieImage { get; set; }

    [JsonProperty("genre")]
    public string? Genre { get; set; }

    [JsonProperty("plot")]
    public string? Plot { get; set; }

    [JsonProperty("director")]
    public string? Director { get; set; }

    [JsonProperty("rating")]
    public decimal? Rating { get; set; }

    [JsonProperty("releasedate")]
    public DateTime? ReleaseDate { get; set; }

    [JsonProperty("duration_secs")]
    public int? DurationSecs { get; set; }

    [JsonProperty("tmdb_id")]
    public int? TmdbId { get; set; }

    [JsonProperty("bitrate")]
    public int Bitrate { get; set; }

    [JsonProperty("video")]
    [JsonConverter(typeof(OnlyObjectConverter<VideoInfo>))]
    public VideoInfo? Video { get; set; }

    [JsonProperty("audio")]
    [JsonConverter(typeof(OnlyObjectConverter<AudioInfo>))]
    public AudioInfo? Audio { get; set; }
}
