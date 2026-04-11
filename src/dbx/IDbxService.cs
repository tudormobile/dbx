using System.Text.Json;

namespace Tudormobile.Dbx;

/// <summary>
/// Defines the Dbx service contract.
/// </summary>
public interface IDbxService
{

    /// <summary>
    /// Creates a new instance of an object that implements the <see cref="IDbxService"/> interface.
    /// </summary>
    /// <param name="options">The configuration options for the DbxService. Cannot be null.</param>
    /// <returns>An instance of <see cref="IDbxService"/> initialized with the provided options.</returns>
    static IDbxService Create(DbxOptions options) => new DbxService(options);


    /// <summary>
    /// Retrieves the service status, including version and health information.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> containing the service status and version information.</returns>
    Task<DbxResponse> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the identifier status, including the item count.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> containing the identifier status including the item count.</returns>
    Task<DbxResponse> GetIdStatusAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all item identifiers in the specified collection.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> whose <c>Data</c> property contains a list of item identifiers.</returns>
    Task<DbxResponse> ListItemsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all items in the specified collection.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> whose <c>Data</c> property contains a list of items.</returns>
    Task<DbxResponse> GetItemsAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new item in the specified collection with a generated identifier.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="content">The JSON content to store as the item.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> whose <c>Data</c> property contains the new item identifier if successful.</returns>
    Task<DbxResponse> CreateItemAsync(string id, JsonElement content, CancellationToken cancellationToken = default);
    /// <summary>
    /// Retrieves a specific item from the specified collection.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="itemId">The item identifier (64 lowercase hex characters).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> whose <c>Data</c> property contains the item JSON if found, or an error message if not found.</returns>
    Task<DbxResponse> GetItemAsync(string id, string itemId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Replaces an existing item in the specified collection with new content.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="itemId">The item identifier (64 lowercase hex characters).</param>
    /// <param name="content">The new JSON content to store.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> whose <c>Data</c> property contains the item identifier if successful.</returns>
    Task<DbxResponse> ReplaceItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default);
    /// <summary>
    /// Updates properties of an existing item in the specified collection. Only properties present in <paramref name="content"/> are updated; others are left unchanged.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="itemId">The item identifier (64 lowercase hex characters).</param>
    /// <param name="content">A JSON object containing the properties to update.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> whose <c>Data</c> property contains the item identifier if successful.</returns>
    Task<DbxResponse> UpdateItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default);
    /// <summary>
    /// Deletes a specific item from the specified collection.
    /// </summary>
    /// <param name="id">The collection identifier (64 lowercase hex characters).</param>
    /// <param name="itemId">The item identifier (64 lowercase hex characters).</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="DbxResponse"/> indicating success or failure.</returns>
    Task<DbxResponse> DeleteItemAsync(string id, string itemId, CancellationToken cancellationToken = default);
}
