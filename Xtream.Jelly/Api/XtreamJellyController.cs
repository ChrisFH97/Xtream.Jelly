using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Xtream.Jelly.Api.Models;
using Xtream.Jelly.Client;
using Xtream.Jelly.Client.Models;

namespace Xtream.Jelly.Api;

/// <summary>
/// The Xtream Jelly configuration API with built-in pagination and caching
/// designed for slow connections and TV interfaces.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class XtreamJellyController(IXtreamClient xtreamClient, IMemoryCache memoryCache, ILogger<XtreamJellyController> logger) : ControllerBase
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private static CategoryResponse CreateCategoryResponse(Category category) =>
        new() { Id = category.CategoryId, Name = category.CategoryName };

    private static ItemResponse CreateItemResponse(StreamInfo stream) =>
        new() { Id = stream.StreamId, Name = stream.Name, HasCatchup = stream.TvArchive, CatchupDuration = stream.TvArchiveDuration };

    private static ItemResponse CreateItemResponse(Series series) =>
        new() { Id = series.SeriesId, Name = series.Name, HasCatchup = false, CatchupDuration = 0 };

    private static ChannelResponse CreateChannelResponse(StreamInfo stream) =>
        new() { Id = stream.StreamId, LogoUrl = stream.StreamIcon, Name = stream.Name, Number = stream.Num };

    private static PagedResponse<T> Paginate<T>(IEnumerable<T> items, int page, int pageSize)
    {
        var allItems = items.ToList();
        var paged = allItems.Skip(page * pageSize).Take(pageSize);
        return new PagedResponse<T>
        {
            Items = paged,
            TotalCount = allItems.Count,
            Page = page,
            PageSize = pageSize,
        };
    }

    private async Task<List<T>> GetCachedAsync<T>(string cacheKey, Func<Task<List<T>>> factory)
    {
        if (memoryCache.TryGetValue(cacheKey, out List<T>? cached) && cached != null)
        {
            return cached;
        }

        List<T> result = await factory().ConfigureAwait(false);
        memoryCache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    /// <summary>
    /// Test the configured provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The provider test response.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("TestProvider")]
    public async Task<ActionResult<ProviderTestResponse>> TestProvider(CancellationToken cancellationToken)
    {
        Plugin plugin = Plugin.Instance;
        PlayerApi info = await xtreamClient.GetUserAndServerInfoAsync(plugin.Creds, cancellationToken).ConfigureAwait(false);
        return Ok(new ProviderTestResponse()
        {
            ActiveConnections = info.UserInfo.ActiveCons,
            ExpiryDate = info.UserInfo.ExpDate,
            MaxConnections = info.UserInfo.MaxConnections,
            ServerTime = info.ServerInfo.TimeNow,
            ServerTimezone = info.ServerInfo.Timezone,
            Status = info.UserInfo.Status,
            SupportsMpegTs = info.UserInfo.AllowedOutputFormats.Contains("ts"),
        });
    }

    /// <summary>
    /// Get paginated Live TV categories.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of categories.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("LiveCategories")]
    public async Task<ActionResult<PagedResponse<CategoryResponse>>> GetLiveCategories(
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        Plugin plugin = Plugin.Instance;
        List<Category> categories = await GetCachedAsync(
            "xj-live-categories",
            () => xtreamClient.GetLiveCategoryAsync(plugin.Creds, cancellationToken)).ConfigureAwait(false);

        IEnumerable<CategoryResponse> responses = categories.Select(CreateCategoryResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            responses = responses.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(responses, page, size));
    }

    /// <summary>
    /// Get paginated Live TV streams for a category.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of streams.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("LiveCategories/{categoryId}")]
    public async Task<ActionResult<PagedResponse<ItemResponse>>> GetLiveStreams(
        int categoryId,
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        Plugin plugin = Plugin.Instance;
        List<StreamInfo> streams = await GetCachedAsync(
            $"xj-live-streams-{categoryId}",
            () => xtreamClient.GetLiveStreamsByCategoryAsync(plugin.Creds, categoryId, cancellationToken)).ConfigureAwait(false);

        IEnumerable<ItemResponse> responses = streams.Select(CreateItemResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            responses = responses.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(responses, page, size));
    }

    /// <summary>
    /// Get paginated VOD categories.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of categories.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("VodCategories")]
    public async Task<ActionResult<PagedResponse<CategoryResponse>>> GetVodCategories(
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        Plugin plugin = Plugin.Instance;
        List<Category> categories = await GetCachedAsync(
            "xj-vod-categories",
            () => xtreamClient.GetVodCategoryAsync(plugin.Creds, cancellationToken)).ConfigureAwait(false);

        IEnumerable<CategoryResponse> responses = categories.Select(CreateCategoryResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            responses = responses.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(responses, page, size));
    }

    /// <summary>
    /// Get paginated VOD streams for a category.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of streams.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("VodCategories/{categoryId}")]
    public async Task<ActionResult<PagedResponse<ItemResponse>>> GetVodStreams(
        int categoryId,
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        Plugin plugin = Plugin.Instance;
        List<StreamInfo> streams = await GetCachedAsync(
            $"xj-vod-streams-{categoryId}",
            () => xtreamClient.GetVodStreamsByCategoryAsync(plugin.Creds, categoryId, cancellationToken)).ConfigureAwait(false);

        IEnumerable<ItemResponse> responses = streams.Select(CreateItemResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            responses = responses.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(responses, page, size));
    }

    /// <summary>
    /// Get paginated Series categories.
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of categories.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("SeriesCategories")]
    public async Task<ActionResult<PagedResponse<CategoryResponse>>> GetSeriesCategories(
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        Plugin plugin = Plugin.Instance;
        List<Category> categories = await GetCachedAsync(
            "xj-series-categories",
            () => xtreamClient.GetSeriesCategoryAsync(plugin.Creds, cancellationToken)).ConfigureAwait(false);

        IEnumerable<CategoryResponse> responses = categories.Select(CreateCategoryResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            responses = responses.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(responses, page, size));
    }

    /// <summary>
    /// Get paginated Series for a category.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of series.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("SeriesCategories/{categoryId}")]
    public async Task<ActionResult<PagedResponse<ItemResponse>>> GetSeriesStreams(
        int categoryId,
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        Plugin plugin = Plugin.Instance;
        List<Series> series = await GetCachedAsync(
            $"xj-series-{categoryId}",
            () => xtreamClient.GetSeriesByCategoryAsync(plugin.Creds, categoryId, cancellationToken)).ConfigureAwait(false);

        IEnumerable<ItemResponse> responses = series.Select(CreateItemResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            responses = responses.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(responses, page, size));
    }

    /// <summary>
    /// Get all configured TV channels (paginated).
    /// </summary>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="search">Optional search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged list of channels.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpGet("LiveTv")]
    public async Task<ActionResult<PagedResponse<ChannelResponse>>> GetLiveTvChannels(
        [FromQuery] int page = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        int size = pageSize ?? Plugin.Instance.Configuration.PageSize;
        IEnumerable<StreamInfo> streams = await Plugin.Instance.StreamService.GetLiveStreams(cancellationToken).ConfigureAwait(false);

        IEnumerable<ChannelResponse> channels = streams.Select(CreateChannelResponse);
        if (!string.IsNullOrWhiteSpace(search))
        {
            channels = channels.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(Paginate(channels, page, size));
    }

    /// <summary>
    /// Invalidate all cached data. Useful when configuration changes.
    /// </summary>
    /// <returns>Ok result.</returns>
    [Authorize(Policy = "RequiresElevation")]
    [HttpPost("ClearCache")]
    public ActionResult ClearCache()
    {
        if (memoryCache is MemoryCache mc)
        {
            mc.Compact(1.0);
        }

        logger.LogInformation("Xtream Jelly cache cleared");
        return Ok();
    }
}
