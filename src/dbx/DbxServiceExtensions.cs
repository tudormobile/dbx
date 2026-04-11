using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Tudormobile.Dbx;

/// <summary>
/// Provides extension methods for registering and configuring Dbx services in an ASP.NET Core application.
/// </summary>
/// <remarks>These extension methods simplify the integration of Dbx functionality into the application's
/// dependency injection container and request pipeline. Use these methods during application startup to ensure that all
/// required services and endpoints are properly configured.</remarks>
public static class DbxServiceExtensions
{
    /// <summary>
    /// Adds Dbx services and configuration to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the Dbx services will be added. Cannot be null.</param>
    /// <param name="configure">An optional delegate to configure the Dbx options. If null, default options are used.</param>
    /// <remarks>This method registers the required services for Dbx.</remarks>
    /// <returns>The same service collection instance, to support method chaining.</returns>
    public static IServiceCollection AddDbx(
            this IServiceCollection services,
            Action<DbxOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpContextAccessor();
        services.Configure<DbxOptions>(options => configure?.Invoke(options));
        services.AddScoped<IDbxService, DbxService>();

        return services;
    }

    /// <summary>
    /// Configures the application to expose Dbx endpoints for monitoring and management.
    /// </summary>
    /// <remarks>
    /// This method adds endpoints to the application's request pipeline. Endpoints are always added
    /// under 'dbx/{endpoint_name}'. Any provided prefix is pre-pended ahead of the 'dbx/' string.
    /// </remarks>
    /// <param name="app">The web application to configure. Cannot be null.</param>
    /// <param name="prefix">An optional URL prefix to use for the Dbx endpoints. If null, the prefix from configuration is used; if both are
    /// null, no prefix is applied.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance, enabling method chaining.</returns>
    public static WebApplication UseDbx(this WebApplication app, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        var options = app.Services.GetRequiredService<IOptions<DbxOptions>>().Value;
        prefix ??= options.Prefix ?? string.Empty;
        prefix = prefix.EndsWith('/')
            ? string.Concat(prefix, "dbx")
            : string.Join('/', prefix, "dbx");

        var group = app.MapGroup(prefix);
        // Administrative Endpoints
        group.MapGet("status", (IDbxService dbx, CancellationToken cancellationToken)
            => dbx.GetStatusAsync(cancellationToken));

        group.MapGet("status/{id}", (IDbxService dbx, string id, CancellationToken cancellationToken)
            => dbx.GetIdStatusAsync(id, cancellationToken));

        // Identifier endpoints
        group.MapGet("list/{id}", (IDbxService dbx, string id, CancellationToken cancellationToken)
            => dbx.ListItemsAsync(id, cancellationToken));

        group.MapGet("{id}", (IDbxService dbx, string id, CancellationToken cancellationToken)
            => dbx.GetItemsAsync(id, cancellationToken));

        // CRUD Endpoints
        group.MapPost("{id}", (IDbxService dbx, string id, [FromBody] JsonElement content, CancellationToken cancellationToken)
            => dbx.CreateItemAsync(id, content, cancellationToken));

        group.MapGet("{id}/{itemId}", (IDbxService dbx, string id, string itemId, CancellationToken cancellationToken)
            => dbx.GetItemAsync(id, itemId, cancellationToken));

        group.MapPut("{id}/{itemId}", (IDbxService dbx, string id, string itemId, [FromBody] JsonElement content, CancellationToken cancellationToken)
            => dbx.ReplaceItemAsync(id, itemId, content, cancellationToken));

        group.MapPatch("{id}/{itemId}", (IDbxService dbx, string id, string itemId, [FromBody] JsonElement content, CancellationToken cancellationToken)
            => dbx.UpdateItemAsync(id, itemId, content, cancellationToken));

        group.MapDelete("{id}/{itemId}", (IDbxService dbx, string id, string itemId, CancellationToken cancellationToken)
            => dbx.DeleteItemAsync(id, itemId, cancellationToken));

        app.Logger.LogInformation("{ServiceName}, running, {Prefix}", nameof(DbxService), prefix);
        return app;
    }
}
