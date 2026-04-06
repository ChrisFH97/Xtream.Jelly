using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using Xtream.Jelly.Client;
using Xtream.Jelly.Client.Models;
using Xtream.Jelly.Service;

namespace Xtream.Jelly;

/// <summary>
/// The Xtream Jelly catch-up channel.
/// </summary>
public class CatchupChannel(ILogger<CatchupChannel> logger, IXtreamClient xtreamClient) : IChannel, IDisableMediaSourceDisplay
{
    private readonly ILogger<CatchupChannel> _logger = logger;

    /// <inheritdoc />
    public string? Name => "Xtream Jelly Catch-up";

    /// <inheritdoc />
    public string? Description => "Rewatch IPTV streamed from the Xtream-compatible server.";

    /// <inheritdoc />
    public string DataVersion => Plugin.Instance.DataVersion + DateTime.Today.ToShortDateString();

    /// <inheritdoc />
    public string HomePageUrl => string.Empty;

    /// <inheritdoc />
    public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

    /// <inheritdoc />
    public InternalChannelFeatures GetChannelFeatures()
    {
        return new InternalChannelFeatures
        {
            ContentTypes = [
                ChannelMediaContentType.TvExtra,
            ],
            MediaTypes = [
                ChannelMediaType.Video
            ],
        };
    }

    /// <inheritdoc />
    public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
    {
        switch (type)
        {
            default:
                throw new ArgumentException("Unsupported image type: " + type);
        }
    }

    /// <inheritdoc />
    public IEnumerable<ImageType> GetSupportedChannelImages() => new List<ImageType>
    {
    };

    /// <inheritdoc />
    public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(query.FolderId))
            {
                return await GetChannels(cancellationToken).ConfigureAwait(false);
            }

            Guid guid = Guid.Parse(query.FolderId);
            StreamService.FromGuid(guid, out int prefix, out int categoryId, out int channelId, out int date);

            if (date == 0)
            {
                return await GetDays(categoryId, channelId, cancellationToken).ConfigureAwait(false);
            }

            return await GetStreams(categoryId, channelId, date, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get channel items");
            throw;
        }
    }

    private async Task<ChannelItemResult> GetChannels(CancellationToken cancellationToken)
    {
        Plugin plugin = Plugin.Instance;
        List<ChannelItemInfo> items = [];
        foreach (StreamInfo channel in await plugin.StreamService.GetLiveStreamsWithOverrides(cancellationToken).ConfigureAwait(false))
        {
            if (!channel.TvArchive)
            {
                continue;
            }

            ParsedName parsedName = StreamService.ParseName(channel.Name);
            items.Add(new ChannelItemInfo()
            {
                Id = StreamService.ToGuid(StreamService.CatchupPrefix, channel.CategoryId ?? 0, channel.StreamId, 0).ToString(),
                ImageUrl = channel.StreamIcon,
                Name = parsedName.Title,
                Tags = new List<string>(parsedName.Tags),
                Type = ChannelItemType.Folder,
            });
        }

        ChannelItemResult result = new ChannelItemResult()
        {
            Items = items,
            TotalRecordCount = items.Count
        };
        return result;
    }

    private async Task<ChannelItemResult> GetDays(int categoryId, int channelId, CancellationToken cancellationToken)
    {
        Plugin plugin = Plugin.Instance;

        List<StreamInfo> streams = await xtreamClient.GetLiveStreamsByCategoryAsync(plugin.Creds, categoryId, cancellationToken).ConfigureAwait(false);
        StreamInfo channel = streams.FirstOrDefault(s => s.StreamId == channelId)
            ?? throw new ArgumentException($"Channel with id {channelId} not found in category {categoryId}");
        ParsedName parsedName = StreamService.ParseName(channel.Name);

        List<ChannelItemInfo> items = [];
        for (int i = 0; i <= channel.TvArchiveDuration; i++)
        {
            DateTime channelDay = DateTime.Today.AddDays(-i);
            int day = (int)(channelDay - DateTime.UnixEpoch).TotalDays;
            items.Add(new()
            {
                Id = StreamService.ToGuid(StreamService.CatchupPrefix, channel.CategoryId ?? 0, channel.StreamId, day).ToString(),
                ImageUrl = channel.StreamIcon,
                Name = channelDay.ToLocalTime().ToString("ddd dd'-'MM'-'yyyy", CultureInfo.InvariantCulture),
                Tags = new List<string>(parsedName.Tags),
                Type = ChannelItemType.Folder,
            });
        }

        ChannelItemResult result = new()
        {
            Items = items,
            TotalRecordCount = items.Count
        };
        return result;
    }

    private async Task<ChannelItemResult> GetStreams(int categoryId, int channelId, int day, CancellationToken cancellationToken)
    {
        DateTime start = DateTime.UnixEpoch.AddDays(day);
        DateTime end = start.AddDays(1);
        Plugin plugin = Plugin.Instance;

        List<StreamInfo> streams = await xtreamClient.GetLiveStreamsByCategoryAsync(plugin.Creds, categoryId, cancellationToken).ConfigureAwait(false);
        StreamInfo channel = streams.FirstOrDefault(s => s.StreamId == channelId)
            ?? throw new ArgumentException($"Channel with id {channelId} not found in category {categoryId}");
        EpgListings epgs = await xtreamClient.GetEpgInfoAsync(plugin.Creds, channelId, cancellationToken).ConfigureAwait(false);
        List<ChannelItemInfo> items = [];

        if (epgs.Listings.Count == 0)
        {
            int durationMinutes = 24 * 60;
            return new()
            {
                Items = new List<ChannelItemInfo>()
                    {
                        new()
                        {
                            ContentType = ChannelMediaContentType.TvExtra,
                            Id = StreamService.ToGuid(StreamService.CatchupStreamPrefix, channelId, 0, day).ToString(),
                            IsLiveStream = false,
                            MediaSources = [
                                plugin.StreamService.GetMediaSourceInfo(StreamType.CatchUp, channelId, start: start, durationMinutes: durationMinutes)
                            ],
                            MediaType = ChannelMediaType.Video,
                            Name = $"No EPG available",
                            RunTimeTicks = durationMinutes * TimeSpan.TicksPerMinute,
                            Type = ChannelItemType.Media,
                        }
                    },
                TotalRecordCount = 1
            };
        }

        foreach (EpgInfo epg in epgs.Listings.Where(epg => epg.Start <= end && epg.End >= start))
        {
            ParsedName parsedName = StreamService.ParseName(epg.Title);
            int durationMinutes = (int)Math.Ceiling((epg.End - epg.Start).TotalMinutes);
            string dateTitle = epg.Start.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);
            List<MediaSourceInfo> sources = [
                plugin.StreamService.GetMediaSourceInfo(StreamType.CatchUp, channelId, start: epg.StartLocalTime, durationMinutes: durationMinutes)
            ];

            items.Add(new()
            {
                ContentType = ChannelMediaContentType.TvExtra,
                DateCreated = epg.Start,
                Id = StreamService.ToGuid(StreamService.CatchupStreamPrefix, channel.StreamId, epg.Id, day).ToString(),
                IsLiveStream = false,
                MediaSources = sources,
                MediaType = ChannelMediaType.Video,
                Name = $"{dateTitle} - {parsedName.Title}",
                Overview = epg.Description,
                PremiereDate = epg.Start,
                RunTimeTicks = durationMinutes * TimeSpan.TicksPerMinute,
                Tags = new List<string>(parsedName.Tags),
                Type = ChannelItemType.Media,
            });
        }

        ChannelItemResult result = new()
        {
            Items = items,
            TotalRecordCount = items.Count
        };
        return result;
    }

    /// <inheritdoc />
    public bool IsEnabledFor(string userId)
    {
        return Plugin.Instance.Configuration.IsCatchupVisible;
    }
}
