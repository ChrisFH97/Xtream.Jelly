using System;

namespace Xtream.Jelly.Api.Models;

/// <summary>
/// A response model for Xtream provider tests.
/// </summary>
public class ProviderTestResponse
{
    /// <summary>
    /// Gets or sets the status of provider.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the account expiry date.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the current simultaneous connections.
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// Gets or sets the maximum simultaneous connections.
    /// </summary>
    public int MaxConnections { get; set; }

    /// <summary>
    /// Gets or sets the server time.
    /// </summary>
    public DateTime ServerTime { get; set; }

    /// <summary>
    /// Gets or sets the server time zone.
    /// </summary>
    public string ServerTimezone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether or not MPEG-TS is supported.
    /// </summary>
    public bool SupportsMpegTs { get; set; }
}
