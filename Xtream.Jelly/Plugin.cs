using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Tasks;
using Xtream.Jelly.Client;
using Xtream.Jelly.Configuration;
using Xtream.Jelly.Service;

namespace Xtream.Jelly;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private static Plugin? _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="taskManager">Instance of the <see cref="ITaskManager"/> interface.</param>
    /// <param name="xtreamClient">Instance of the <see cref="IXtreamClient"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ITaskManager taskManager, IXtreamClient xtreamClient)
        : base(applicationPaths, xmlSerializer)
    {
        _instance = this;
        XtreamClient = xtreamClient;
        if (XtreamClient is XtreamClient client)
        {
            client.UpdateUserAgent();
        }

        StreamService = new(xtreamClient);
        TaskService = new(taskManager);
    }

    /// <inheritdoc />
    public override string Name => "Xtream Jelly";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("a1b2c3d4-5678-90ab-cdef-123456789abc");

    /// <summary>
    /// Gets the Xtream connection info with credentials.
    /// </summary>
    public ConnectionInfo Creds => new(Configuration.BaseUrl, Configuration.Username, Configuration.Password);

    /// <summary>
    /// Gets the data version used to trigger a cache invalidation on plugin update or config change.
    /// </summary>
    public string DataVersion => Assembly.GetCallingAssembly().GetName().Version?.ToString() + Configuration.GetHashCode();

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin Instance => _instance ?? throw new InvalidOperationException("Plugin instance not available");

    /// <summary>
    /// Gets the stream service instance.
    /// </summary>
    public StreamService StreamService { get; init; }

    private IXtreamClient XtreamClient { get; init; }

    /// <summary>
    /// Gets the task service instance.
    /// </summary>
    public TaskService TaskService { get; init; }

    private static PluginPageInfo CreateStatic(string name) => new()
    {
        Name = name,
        EmbeddedResourcePath = string.Format(
            CultureInfo.InvariantCulture,
            "{0}.Configuration.Web.{1}",
            typeof(Plugin).Namespace,
            name),
    };

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            CreateStatic("XJCredentials.html"),
            CreateStatic("XJCredentials.js"),
            CreateStatic("XtreamJelly.css"),
            CreateStatic("XtreamJelly.js"),
            CreateStatic("XJLive.html"),
            CreateStatic("XJLive.js"),
            CreateStatic("XJLiveOverrides.html"),
            CreateStatic("XJLiveOverrides.js"),
            CreateStatic("XJSeries.html"),
            CreateStatic("XJSeries.js"),
            CreateStatic("XJVod.html"),
            CreateStatic("XJVod.js"),
        };
    }

    /// <inheritdoc />
    public override void UpdateConfiguration(BasePluginConfiguration configuration)
    {
        base.UpdateConfiguration(configuration);

        if (XtreamClient is XtreamClient client)
        {
            client.UpdateUserAgent();
        }

        // Force a refresh of TV guide on configuration update.
        TaskService.CancelIfRunningAndQueue(
            "Jellyfin.LiveTv",
            "Jellyfin.LiveTv.Guide.RefreshGuideScheduledTask");

        // Force a refresh of Channels on configuration update.
        TaskService.CancelIfRunningAndQueue(
            "Jellyfin.LiveTv",
            "Jellyfin.LiveTv.Channels.RefreshChannelsScheduledTask");
    }
}
