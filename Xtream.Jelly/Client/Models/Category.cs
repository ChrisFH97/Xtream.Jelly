using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class Category
{
    [JsonProperty("category_id")]
    public int CategoryId { get; set; }

    [JsonProperty("category_name")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonProperty("parent_id")]
    public int ParentId { get; set; }
}
