namespace Xtream.Jelly.Client;

/// <summary>
/// Connection credentials for the Xtream API.
/// </summary>
public class ConnectionInfo(string baseUrl, string username, string password)
{
    /// <summary>
    /// Gets or sets the base url including protocol and port number, without trailing slash.
    /// </summary>
    public string BaseUrl { get; set; } = baseUrl;

    /// <summary>
    /// Gets or sets the username for authentication.
    /// </summary>
    public string UserName { get; set; } = username;

    /// <summary>
    /// Gets or sets the password for authentication.
    /// </summary>
    public string Password { get; set; } = password;
}
