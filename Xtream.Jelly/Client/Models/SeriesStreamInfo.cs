using System.Collections.Generic;
using Newtonsoft.Json;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client.Models;

public class SeriesStreamInfo
{
    [JsonProperty("seasons")]
#pragma warning disable CA2227
    public ICollection<Season> Seasons { get; set; } = new List<Season>();
#pragma warning restore CA2227

    [JsonProperty("info")]
    public SeriesInfo Info { get; set; } = new SeriesInfo();

    [JsonProperty("episodes")]
#pragma warning disable CA2227
    public Dictionary<int, ICollection<Episode>> Episodes { get; set; } = new Dictionary<int, ICollection<Episode>>();
#pragma warning restore CA2227
}
