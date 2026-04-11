using System.Text.Json;

namespace Tudormobile.Dbx;

/// <summary>
/// An repository implementation that stores each item as a JSON file.
/// </summary>
/// <remarks>
/// Each item is stored in its own subdirectory named after the "id":
/// <code>
/// {dataPath}/
///   {id}/
///     {itemId}.json
/// </code>
/// </remarks>
internal class DbxRepository
{
    private readonly string _dataPath;
    private static readonly SemaphoreSlim _fileSemaphore = new(10);
    private const int MaxItemsToLoad = 100;  // At class level

    /// <summary>
    /// Initializes a new instance of the repository.
    /// </summary>
    /// <param name="dataPath">The root directory under which data folders are stored.</param>
    public DbxRepository(string dataPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataPath);
        _dataPath = dataPath;
    }

    public Task<IReadOnlyList<string>> GetAllIdentifiersAsync(CancellationToken cancellationToken = default)
        // folders under _dataPath
        => GetIdentifiersAsync(_dataPath, Directory.EnumerateDirectories, cancellationToken);

    public Task<IReadOnlyList<string>> GetAllItemIdentifiersAsync(string id, CancellationToken cancellationToken = default)
        // *.json files under the id directory
        => GetIdentifiersAsync(IdDirectory(id), f => Directory.EnumerateFiles(f, "*.json"), cancellationToken);

    public async Task<int> CountAllItemIdentifiersAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        // *.json files under the id directory
        var identifiers = await GetIdentifiersAsync(IdDirectory(id), f => Directory.EnumerateFiles(f, "*.json"), cancellationToken);
        return identifiers.Count;
    }

    public async Task<(string ItemId, JsonElement Data)[]> GetAllItemsAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        var dir = IdDirectory(id);
        if (Directory.Exists(dir))
        {
            var itemIds = await GetAllItemIdentifiersAsync(id, cancellationToken);

            var tasks = itemIds.Take(MaxItemsToLoad).Select(async itemId =>
            {
                await _fileSemaphore.WaitAsync(cancellationToken);
                try 
                { 
                    var data = await GetItemAsync(id, itemId, cancellationToken);
                    return (itemId, data);
                }
                finally { _fileSemaphore.Release(); }
            });
            var items = await Task.WhenAll(tasks);
            return [.. items.Where(item => item.data.HasValue).Select(item => (item.itemId, item.data!.Value))];
        }
        return [];
    }

    public Task<bool> ItemExistsAsync(string id, string itemId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(itemId);
        cancellationToken.ThrowIfCancellationRequested();
        var dirname = IdDirectory(id);
        var filename = ItemIdFile(id, itemId);
        var exists = Directory.Exists(dirname) && File.Exists(filename);
        return Task.FromResult(exists);
    }

    public async Task<JsonElement?> GetItemAsync(string id, string itemId, CancellationToken cancellationToken = default)
    {
        var itemFile = ItemIdFile(id, itemId);
        if (File.Exists(itemFile))
        {
            try
            {
                var json = await File.ReadAllTextAsync(itemFile, cancellationToken);
                return JsonElement.Parse(json);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
        return null;
    }

    public async Task SaveItemAsync(string id, string itemId, JsonElement item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(itemId);
        var dir = IdDirectory(id);
        if (Directory.Exists(dir))
        {
            var json = JsonSerializer.Serialize(item);
            await File.WriteAllTextAsync(ItemIdFile(id, itemId), json, cancellationToken);
            return;
        }
        throw new FileNotFoundException("Identifier does not exist.");
    }

    public Task DeleteAsync(string id, string itemId, CancellationToken cancellationToken = default)
    {
        var itemFile = ItemIdFile(id, itemId);
        if (File.Exists(itemFile))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(itemFile);
        }
        return Task.CompletedTask;
    }

    private static Task<IReadOnlyList<string>> GetIdentifiersAsync(string folder, Func<string, IEnumerable<string>> enumerate, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            if (Directory.Exists(folder))
            {
                var results = new List<string>();
                foreach (var entry in enumerate(folder))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var entryName = Path.GetFileNameWithoutExtension(entry);
                    CheckForValidId(entryName);
                    results.Add(entryName);
                }
                return (IReadOnlyList<string>)results;
            }
            return [];
        }, cancellationToken);
    }

    private string IdDirectory(string id) => Path.Combine(_dataPath, CheckForValidId(id));
    private string ItemIdFile(string id, string itemId) => Path.ChangeExtension(Path.Combine(IdDirectory(id), CheckForValidItemId(itemId)), "json");

    private static string CheckForValidId(string id)
    {
        if (string.IsNullOrEmpty(id) || id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new FormatException($"'{id}' is not a valid asset identifier: contains invalid path characters.");
        return id;
    }

    private static string CheckForValidItemId(string itemId)
    {
        var id = CheckForValidId(itemId);

        if (id.Length != 64)
            throw new FormatException($"'{id}' is not a valid asset identifier: expected 64 hex characters, got {id.Length}.");

        foreach (var c in id)
        {
            if (c is not ((>= '0' and <= '9') or (>= 'a' and <= 'f')))
                throw new FormatException($"'{id}' is not a valid asset identifier: '{c}' is not a lowercase hex character.");
        }
        return id;
    }
}
