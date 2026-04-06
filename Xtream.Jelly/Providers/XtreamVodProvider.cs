using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using Xtream.Jelly.Client;
using Xtream.Jelly.Client.Models;
using Xtream.Jelly.Service;

namespace Xtream.Jelly.Providers;

/// <summary>
/// The Xtream Jelly VOD metadata provider.
/// </summary>
public class XtreamVodProvider(ILogger<VodChannel> logger, IProviderManager providerManager, IXtreamClient xtreamClient) : ICustomMetadataProvider<Movie>, IPreRefreshProvider
{
    /// <summary>
    /// The name of the provider.
    /// </summary>
    public const string ProviderName = "XtreamJellyVodProvider";

    /// <inheritdoc/>
    public string Name => ProviderName;

    /// <inheritdoc/>
    public async Task<ItemUpdateType> FetchAsync(Movie item, MetadataRefreshOptions options, CancellationToken cancellationToken)
    {
        string? idStr = item.GetProviderId(ProviderName);
        if (idStr is not null)
        {
            logger.LogDebug("Getting metadata for movie {Id}", idStr);
            int id = int.Parse(idStr, CultureInfo.InvariantCulture);
            VodStreamInfo vod = await xtreamClient.GetVodInfoAsync(Plugin.Instance.Creds, id, cancellationToken).ConfigureAwait(false);
            VodInfo? i = vod.Info;

            if (i is null)
            {
                return ItemUpdateType.None;
            }

            item.Overview ??= i.Plot;
            item.PremiereDate ??= i.ReleaseDate;
            item.RunTimeTicks ??= i.DurationSecs * TimeSpan.TicksPerSecond;
            item.TotalBitrate ??= i.Bitrate;

            if (i.Genre is string genres)
            {
                item.Genres ??= genres.Split(',').Select(genre => genre.Trim()).ToArray();
            }

            if (!item.HasProviderId(MetadataProvider.Tmdb))
            {
                if (i.TmdbId is int tmdbId)
                {
                    options.ReplaceAllMetadata = true;
                    item.SetProviderId(MetadataProvider.Tmdb, tmdbId.ToString(CultureInfo.InvariantCulture));
                }
                else if (Plugin.Instance.Configuration.IsTmdbVodOverride)
                {
                    RemoteSearchQuery<MovieInfo> query = new()
                    {
                        SearchInfo = new()
                        {
                            Name = StreamService.ParseName(vod.MovieData?.Name ?? string.Empty).Title,
                            Year = item.PremiereDate?.Year,
                        },
                        SearchProviderName = "TheMovieDb",
                    };
                    IEnumerable<RemoteSearchResult> results = await providerManager.GetRemoteSearchResults<Movie, MovieInfo>(query, cancellationToken).ConfigureAwait(false);
                    if (results.Any())
                    {
                        RemoteSearchResult tmdbMovie = results.First();
                        if (tmdbMovie.HasProviderId(MetadataProvider.Tmdb))
                        {
                            string? queryId = tmdbMovie.GetProviderId(MetadataProvider.Tmdb);
                            if (queryId is not null)
                            {
                                options.ReplaceAllMetadata = true;
                                item.SetProviderId(MetadataProvider.Tmdb, queryId);
                            }
                        }
                    }
                }
            }
        }

        return ItemUpdateType.MetadataImport;
    }
}
