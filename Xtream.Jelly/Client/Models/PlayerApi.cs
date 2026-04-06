using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class PlayerApi
{
    [JsonProperty("user_info")]
    public UserInfo UserInfo { get; set; } = new UserInfo();

    [JsonProperty("server_info")]
    public ServerInfo ServerInfo { get; set; } = new ServerInfo();
}
