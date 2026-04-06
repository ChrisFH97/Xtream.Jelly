namespace Xtream.Jelly.Configuration;

/// <summary>
/// Override configuration for a Live TV channel.
/// </summary>
public class ChannelOverrides
{
    /// <summary>
    /// Gets or sets the TV channel number.
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Gets or sets the TV channel name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the url of the channel logo.
    /// </summary>
    public string? LogoUrl { get; set; }
}
