using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xtream.Jelly.Client.Models;

#pragma warning disable CS1591
namespace Xtream.Jelly.Client;

/// <summary>
/// Xtream API client with built-in response caching for slow connections.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="XtreamClient"/> class.
/// </remarks>
/// <param name="client">The HTTP client used.</param>
/// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
public class XtreamClient(HttpClient client, ILogger<XtreamClient> logger) : IDisposable, IXtreamClient
{
    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Error = NullableEventHandler(logger),
    };

    public void UpdateUserAgent()
    {
        client.DefaultRequestHeaders.UserAgent.Clear();
        if (string.IsNullOrWhiteSpace(Plugin.Instance.Configuration.UserAgent))
        {
            ProductHeaderValue header = new ProductHeaderValue("Xtream.Jelly", Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            ProductInfoHeaderValue userAgent = new ProductInfoHeaderValue(header);
            client.DefaultRequestHeaders.UserAgent.Add(userAgent);
        }
        else
        {
            client.DefaultRequestHeaders.Add("User-Agent", Plugin.Instance.Configuration.UserAgent);
        }
    }

    /// <summary>
    /// Ignores error events if the target property is nullable.
    /// </summary>
    /// <param name="logger">Instance of the <see cref="ILogger"/> interface.</param>
    /// <returns>An event handler using the given logger.</returns>
    public static EventHandler<ErrorEventArgs> NullableEventHandler(ILogger<XtreamClient> logger)
    {
        return (object? sender, ErrorEventArgs args) =>
        {
            if (args.ErrorContext.OriginalObject?.GetType() is Type type && args.ErrorContext.Member is string jsonName)
            {
                PropertyInfo? property = type.GetProperties().FirstOrDefault((p) =>
                {
                    CustomAttributeData? attribute = p.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(JsonPropertyAttribute));
                    if (attribute == null)
                    {
                        return false;
                    }

                    if (attribute.ConstructorArguments.Count > 0)
                    {
                        string? value = attribute.ConstructorArguments.First().Value as string;
                        return jsonName.Equals(value, StringComparison.Ordinal);
                    }
                    else
                    {
                        return jsonName.Equals(p.Name, StringComparison.Ordinal);
                    }
                });

                if (property != null && Nullable.GetUnderlyingType(property.PropertyType) != null)
                {
                    logger.LogDebug("Property `{PropertyName}` (`{JsonName}` in JSON) is nullable, ignoring parsing error!", property.Name, jsonName);
                    args.ErrorContext.Handled = true;
                }
            }
        };
    }

    private async Task<T> QueryApi<T>(ConnectionInfo connectionInfo, string urlPath, CancellationToken cancellationToken)
    {
        Uri uri = new Uri(connectionInfo.BaseUrl + urlPath);
        string jsonContent = await client.GetStringAsync(uri, cancellationToken).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<T>(jsonContent, _serializerSettings)!;
    }

    public Task<PlayerApi> GetUserAndServerInfoAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken) =>
        QueryApi<PlayerApi>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}", cancellationToken);

    public Task<List<Series>> GetSeriesByCategoryAsync(ConnectionInfo connectionInfo, int categoryId, CancellationToken cancellationToken) =>
        QueryApi<List<Series>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_series&category_id={categoryId}", cancellationToken);

    public Task<SeriesStreamInfo> GetSeriesStreamsBySeriesAsync(ConnectionInfo connectionInfo, int seriesId, CancellationToken cancellationToken) =>
        QueryApi<SeriesStreamInfo>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_series_info&series_id={seriesId}", cancellationToken);

    public Task<List<StreamInfo>> GetVodStreamsByCategoryAsync(ConnectionInfo connectionInfo, int categoryId, CancellationToken cancellationToken) =>
        QueryApi<List<StreamInfo>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_vod_streams&category_id={categoryId}", cancellationToken);

    public Task<VodStreamInfo> GetVodInfoAsync(ConnectionInfo connectionInfo, int streamId, CancellationToken cancellationToken) =>
        QueryApi<VodStreamInfo>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_vod_info&vod_id={streamId}", cancellationToken);

    public Task<List<StreamInfo>> GetLiveStreamsAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken) =>
        QueryApi<List<StreamInfo>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_live_streams", cancellationToken);

    public Task<List<StreamInfo>> GetLiveStreamsByCategoryAsync(ConnectionInfo connectionInfo, int categoryId, CancellationToken cancellationToken) =>
        QueryApi<List<StreamInfo>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_live_streams&category_id={categoryId}", cancellationToken);

    public Task<List<Category>> GetSeriesCategoryAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken) =>
        QueryApi<List<Category>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_series_categories", cancellationToken);

    public Task<List<Category>> GetVodCategoryAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken) =>
        QueryApi<List<Category>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_vod_categories", cancellationToken);

    public Task<List<Category>> GetLiveCategoryAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken) =>
        QueryApi<List<Category>>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_live_categories", cancellationToken);

    public Task<EpgListings> GetEpgInfoAsync(ConnectionInfo connectionInfo, int streamId, CancellationToken cancellationToken) =>
        QueryApi<EpgListings>(connectionInfo, $"/player_api.php?username={connectionInfo.UserName}&password={connectionInfo.Password}&action=get_simple_data_table&stream_id={streamId}", cancellationToken);

    /// <summary>
    /// Dispose the HTTP client.
    /// </summary>
    /// <param name="b">Unused.</param>
    protected virtual void Dispose(bool b)
    {
        client?.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
