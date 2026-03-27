namespace Tudormobile.Dbx;

/// <summary>
/// Represents configuration options for the Dbx service.
/// </summary>
public class DbxOptions
{
    /// <summary>
    /// Gets or sets the root directory used by the repository.
    /// If <see langword="null"/>, defaults to a subdirectory named <c>data</c> inside
    /// the current working directory.
    /// </summary>
    public string? DataPath { get; set; }

    /// <summary>
    /// Gets or sets the prefix to be applied to api endpoints.
    /// If <see langword="null"/>, no prefix is used and dbx endpoints are mapped to 'dbx/...'
    /// </summary>
    public string? Prefix { get; set; }
}
