using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Tudormobile.Dbx;

internal class DbxService : IDbxService
{
    private readonly string _dataPath;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly DbxRepository _repository;
    internal string DataPath => _dataPath;

    public DbxService(ILogger<IDbxService> logger, IHttpContextAccessor httpContextAccessor, IOptions<DbxOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(httpContextAccessor, nameof(httpContextAccessor));
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        _dataPath = options.Value.DataPath ?? "data";
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _repository = new DbxRepository();
    }

    public DbxService(DbxOptions options) : this(
            NullLogger<IDbxService>.Instance,
            new HttpContextAccessor(),
            Options.Create(options))
    { }

    /// <inheritdoc/>
    public Task<DbxResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DbxResponse
        {
            Success = true,
            Data = new { Assembly.GetExecutingAssembly().GetName().Version },
        });
    }

    /// <inheritdoc/>
    public Task<DbxResponse> CreateItemAsync(string id, JsonElement content, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<DbxResponse> ListItemsAsync(string id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<DbxResponse> GetItemAsync(string id, string itemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<DbxResponse> ReplaceItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<DbxResponse> UpdateItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public Task<DbxResponse> DeleteItemAsync(string id, string itemId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    private void LogApiRequest([CallerMemberName] string callerName = "")
    {
        if (!_logger.IsEnabled(LogLevel.Information))
            return;

        _logger.LogInformation("{ServiceName}, {CallerName}, {RemoteIpAddress}",
            nameof(IDbxService), callerName,
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress);
    }
}
