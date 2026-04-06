namespace Xtream.Jelly.Api.Models;

/// <summary>
/// A response model for Xtream categories.
/// </summary>
public class CategoryResponse
{
    /// <summary>
    /// Gets or sets the Xtream API id of the category.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
