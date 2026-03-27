namespace Tudormobile.Dbx;

/// <summary>
/// Represents the result of an API call, including success status and any returned data.
/// </summary>
/// <remarks>Use this type to inspect whether an API operation succeeded and to access any associated data or
/// error information. The response may contain data on success or error details on failure. This type is
/// immutable.</remarks>
public sealed record class DbxResponse
{
    /// <summary>
    /// Gets a value indicating whether the API call was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the data returned by the API call, can be <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// May contain error information on failure. Can be null on success when no data is returned.
    /// </remarks>
    public object? Data { get; init; }
}
