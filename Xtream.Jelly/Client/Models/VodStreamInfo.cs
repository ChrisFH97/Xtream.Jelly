using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class VodStreamInfo
{
    [JsonProperty("info")]
    [JsonConverter(typeof(OnlyObjectConverter<VodInfo>))]
    public VodInfo? Info { get; set; }

    [JsonProperty("movie_data")]
    [JsonConverter(typeof(OnlyObjectConverter<StreamInfo>))]
    public StreamInfo? MovieData { get; set; }
}
