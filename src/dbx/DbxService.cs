using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
        _repository = new DbxRepository(_dataPath);
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

    public Task<DbxResponse> GetIdStatusAsync(string id, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () =>
        {
            int count = await _repository.CountAllItemIdentifiersAsync(id, cancellationToken);
            return (object?)new { id, count, };
        });
    }

    /// <inheritdoc/>
    public Task<DbxResponse> CreateItemAsync(string id, JsonElement content, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        var itemId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        return ExecuteAsync(async () =>
        {
            await _repository.SaveItemAsync(id, itemId, content, cancellationToken);
            return (object?)itemId;
        });
    }

    /// <inheritdoc/>
    public Task<DbxResponse> ListItemsAsync(string id, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () => (object?)await _repository.GetAllItemIdentifiersAsync(id, cancellationToken));
    }

    /// <inheritdoc/>
    public Task<DbxResponse> GetItemsAsync(string id, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () =>
        {
            var items = await _repository.GetAllItemsAsync(id, cancellationToken);
            return (object?)items.ToDictionary(item => item.ItemId, item => item.Data);
        });
    }

    /// <inheritdoc/>
    public Task<DbxResponse> GetItemAsync(string id, string itemId, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () =>
        {
            var item = await _repository.GetItemAsync(id, itemId, cancellationToken);
            if (!item.HasValue)
                throw new KeyNotFoundException($"Item '{itemId}' not found in '{id}'.");
            return (object?)item.Value;
        });
    }

    /// <inheritdoc/>
    public Task<DbxResponse> ReplaceItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () =>
        {
            await _repository.SaveItemAsync(id, itemId, content, cancellationToken);
            return (object?)itemId;
        });
    }

    /// <inheritdoc/>
    public Task<DbxResponse> UpdateItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () =>
        {
            var exists = await _repository.ItemExistsAsync(id, itemId, cancellationToken);
            if (exists)
            {
                var existingRecord = await _repository.GetItemAsync(id, itemId, cancellationToken);
                var updated = UpdateJsonElement(existingRecord, content);
                await _repository.SaveItemAsync(id, itemId, updated, cancellationToken);
                return (object?)itemId;
            }
            throw new KeyNotFoundException("Item was not found");
        });
    }

    private static JsonElement UpdateJsonElement(JsonElement? existingRecord, JsonElement content)
    {
        if (!existingRecord.HasValue || existingRecord.Value.ValueKind != JsonValueKind.Object)
            return existingRecord ?? default;

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);

        writer.WriteStartObject();
        foreach (var property in existingRecord.Value.EnumerateObject())
        {
            writer.WritePropertyName(property.Name);
            if (content.ValueKind == JsonValueKind.Object &&
                content.TryGetProperty(property.Name, out var replacement))
                replacement.WriteTo(writer);
            else
                property.Value.WriteTo(writer);
        }
        writer.WriteEndObject();
        writer.Flush();

        using var doc = JsonDocument.Parse(buffer.WrittenMemory);
        return doc.RootElement.Clone();
    }

    /// <inheritdoc/>
    public Task<DbxResponse> DeleteItemAsync(string id, string itemId, CancellationToken cancellationToken = default)
    {
        LogApiRequest();
        return ExecuteAsync(async () =>
        {
            await _repository.DeleteAsync(id, itemId, cancellationToken);
            return (object?)itemId;
        });
    }

    private static async Task<DbxResponse> ExecuteAsync(Func<Task<object?>> action)
    {
        try
        {
            return new DbxResponse { Success = true, Data = await action() };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new DbxResponse { Success = false, Data = ex.Message };
        }
    }

    private void LogApiRequest([CallerMemberName] string callerName = "")
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("{ServiceName}, {CallerName}, {RemoteIpAddress}",
                nameof(IDbxService), callerName,
                _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress);
        }
    }
}
