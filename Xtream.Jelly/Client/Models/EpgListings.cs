using System.Collections.Generic;
using Newtonsoft.Json;

#pragma warning disable CS1591
#pragma warning disable CA2227
namespace Xtream.Jelly.Client.Models;

public class EpgListings
{
    [JsonProperty("epg_listings")]
    public ICollection<EpgInfo> Listings { get; set; } = new List<EpgInfo>();
}
#pragma warning restore CA2227
#pragma warning restore CS1591
