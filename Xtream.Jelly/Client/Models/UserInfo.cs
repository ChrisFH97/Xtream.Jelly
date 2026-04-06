using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class UserInfo
{
    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("password")]
    public string Password { get; set; } = string.Empty;

    [JsonProperty("auth")]
    public int Auth { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("exp_date")]
    public DateTime? ExpDate { get; set; }

    [JsonConverter(typeof(StringBoolConverter))]
    [JsonProperty("is_trial")]
    public bool? IsTrial { get; set; }

    [JsonProperty("active_cons")]
    public int ActiveCons { get; set; }

    [JsonConverter(typeof(UnixDateTimeConverter))]
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("max_connections")]
    public int MaxConnections { get; set; }

#pragma warning disable CA2227
    [JsonProperty("allowed_output_formats")]
    public ICollection<string> AllowedOutputFormats { get; set; } = new List<string>();
#pragma warning restore CA2227
}
