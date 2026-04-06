using MediaBrowser.Controller;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Providers;
using Microsoft.Extensions.DependencyInjection;
using Xtream.Jelly.Client;
using Xtream.Jelly.Providers;

namespace Xtream.Jelly;

/// <inheritdoc />
public class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<IXtreamClient, XtreamClient>();
        serviceCollection.AddSingleton<ILiveTvService, LiveTvService>();
        serviceCollection.AddSingleton<IChannel, CatchupChannel>();
        serviceCollection.AddSingleton<IChannel, SeriesChannel>();
        serviceCollection.AddSingleton<IChannel, VodChannel>();
        serviceCollection.AddSingleton<IPreRefreshProvider, XtreamVodProvider>();
    }
}
