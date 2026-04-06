namespace Xtream.Jelly.Api.Models;

/// <summary>
/// A response model for items inside an Xtream category.
/// </summary>
public class ItemResponse
{
    /// <summary>
    /// Gets or sets the Xtream API id of the item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether or not catch-up is supported.
    /// </summary>
    public bool HasCatchup { get; set; }

    /// <summary>
    /// Gets or sets the catch-up duration in days.
    /// </summary>
    public int CatchupDuration { get; set; }
}
