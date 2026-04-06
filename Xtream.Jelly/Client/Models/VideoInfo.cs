using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class VideoInfo
{
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("codec_name")]
    public string CodecName { get; set; } = string.Empty;

    [JsonProperty("profile")]
    public string Profile { get; set; } = string.Empty;

    [JsonProperty("width")]
    public int Width { get; set; }

    [JsonProperty("height")]
    public int Height { get; set; }

    [JsonProperty("display_aspect_ratio")]
    public string AspectRatio { get; set; } = string.Empty;

    [JsonProperty("pix_fmt")]
    public string PixelFormat { get; set; } = string.Empty;

    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("color_range")]
    public string ColorRange { get; set; } = string.Empty;

    [JsonProperty("color_space")]
    public string ColorSpace { get; set; } = string.Empty;

    [JsonProperty("color_transfer")]
    public string ColorTransfer { get; set; } = string.Empty;

    [JsonProperty("color_primaries")]
    public string ColorPrimaries { get; set; } = string.Empty;

    [JsonProperty("is_avc")]
    public bool IsAVC { get; set; }

    [JsonProperty("bits_per_raw_sample")]
    public int BitsPerRawSample { get; set; }
}
