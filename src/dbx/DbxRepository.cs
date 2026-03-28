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
            var json = await File.ReadAllTextAsync(itemFile, cancellationToken);
            return JsonElement.Parse(json);
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
