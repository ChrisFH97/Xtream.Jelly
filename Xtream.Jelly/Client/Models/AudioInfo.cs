using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class AudioInfo
{
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("codec_name")]
    public string CodecName { get; set; } = string.Empty;

    [JsonProperty("profile")]
    public string Profile { get; set; } = string.Empty;

    [JsonProperty("sample_fmt")]
    public string SampleFormat { get; set; } = string.Empty;

    [JsonProperty("sample_rate")]
    public int SampleRate { get; set; }

    [JsonProperty("channels")]
    public int Channels { get; set; }

    [JsonProperty("channel_layout")]
    public string ChannelLayout { get; set; } = string.Empty;

    [JsonProperty("bit_rate")]
    public int Bitrate { get; set; }
}
