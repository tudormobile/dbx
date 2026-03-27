using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Tudormobile.Dbx;

/// <summary>
/// Defines the Dbx service contract.
/// </summary>
public interface IDbxService
{
    /// <summary>
    /// Creates a new instance of an object that implements the IDbxService interface.
    /// </summary>
    /// <param name="options">The configuration options for the DbxService. Cannot be null.</param>
    /// <returns>An instance of IDbxService initialized with the provided options.</returns>
    static IDbxService Create(DbxOptions options) => new DbxService(options);

    /// <summary>
    /// Retrieves the service status.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IResult"/> containing the service status.</returns>
    Task<DbxResponse> GetStatusAsync(CancellationToken cancellationToken = default);

    Task<DbxResponse> CreateItemAsync(string id, JsonElement content, CancellationToken cancellationToken = default);
    Task<DbxResponse> ListItemsAsync(string id, CancellationToken cancellationToken = default);
    Task<DbxResponse> GetItemAsync(string id, string itemId, CancellationToken cancellationToken = default);
    Task<DbxResponse> ReplaceItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default);
    Task<DbxResponse> UpdateItemAsync(string id, string itemId, JsonElement content, CancellationToken cancellationToken = default);
    Task<DbxResponse> DeleteItemAsync(string id, string itemId, CancellationToken cancellationToken = default);
}
