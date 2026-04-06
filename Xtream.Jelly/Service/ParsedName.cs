#pragma warning disable CA1815
#pragma warning disable CA1819
namespace Xtream.Jelly.Service;

/// <summary>
/// A struct which holds information of parsed stream names.
/// </summary>
public readonly struct ParsedName
{
    /// <summary>
    /// Gets the parsed title.
    /// </summary>
    public string Title { get; init; }

    /// <summary>
    /// Gets the parsed tags.
    /// </summary>
    public string[] Tags { get; init; }
}
