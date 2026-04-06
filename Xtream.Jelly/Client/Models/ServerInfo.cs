using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class ServerInfo
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;

    [JsonProperty("port")]
    public string Port { get; set; } = string.Empty;

    [JsonProperty("rtmp_port")]
    public string RtmpPort { get; set; } = string.Empty;

    [JsonProperty("timezone")]
    public string Timezone { get; set; } = string.Empty;

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("timestamp_now")]
    public DateTime TimeNow { get; set; }
}
