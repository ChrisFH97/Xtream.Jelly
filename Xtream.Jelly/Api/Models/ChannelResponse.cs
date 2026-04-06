namespace Xtream.Jelly.Api.Models;

/// <summary>
/// Override configuration for a Live TV channel.
/// </summary>
public class ChannelResponse
{
    /// <summary>
    /// Gets or sets the Xtream API id of the TV channel.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the TV channel number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the TV channel name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the url of the channel logo.
    /// </summary>
    public string LogoUrl { get; set; } = string.Empty;
}
