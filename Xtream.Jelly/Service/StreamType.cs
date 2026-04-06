namespace Xtream.Jelly.Service;

/// <summary>
/// An enum describing the Xtream stream types.
/// </summary>
public enum StreamType : int
{
    /// <summary>
    /// Live IPTV.
    /// </summary>
    Live = 0,

    /// <summary>
    /// Catch up IPTV.
    /// </summary>
    CatchUp = 1,

    /// <summary>
    /// On-demand series grouped in seasons and episodes.
    /// </summary>
    Series = 2,

    /// <summary>
    /// Video on-demand.
    /// </summary>
    Vod = 3,
}
