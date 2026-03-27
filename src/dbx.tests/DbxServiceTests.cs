using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace Dbx.Tests;

[TestClass]
public class DbxServiceTests
{
    public TestContext TestContext { get; set; }    // MSTest will set this property

    [TestMethod]
    public async Task GetStatusAsync_ReturnsStatus()
    {
        var dbx = BuildService();

        var result = await dbx.GetStatusAsync(TestContext.CancellationToken);

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    public void CreateWithNullLogger_Throws()
    {
        var options = Options.Create(new DbxOptions());
        var accessor = new HttpContextAccessor();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxService(null!, accessor, options));

        Assert.AreEqual("logger", exception.ParamName);
    }

    [TestMethod]
    public void CreateWithNullAccessor_Throws()
    {
        var logger = NullLogger<IDbxService>.Instance;
        var options = Options.Create(new DbxOptions());

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxService(logger, null!, options));

        Assert.AreEqual("httpContextAccessor", exception.ParamName);
    }

    [TestMethod]
    public void CreateWithNullOptions_Throws()
    {
        var logger = NullLogger<IDbxService>.Instance;
        var accessor = new HttpContextAccessor();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => new DbxService(logger, accessor, null!));

        Assert.AreEqual("options", exception.ParamName);
    }

    // Creates IDbxService the same way a real ASP.NET Core host would.
    // Use this for tests that validate end-to-end behaviour rather than
    // internal construction details.
    private static IDbxService BuildService(Action<DbxOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbx(configure);
        var provider = services.BuildServiceProvider();
        return provider.CreateScope().ServiceProvider.GetRequiredService<IDbxService>();
    }

    [TestMethod]
    public void CreateWithNullDataPath_SetsDefaultDataPath()
    {
        var dataPath = "data";
        var logger = NullLogger<IDbxService>.Instance;
        var options = Options.Create(new DbxOptions());
        var accessor = new HttpContextAccessor();

        var service = new DbxService(logger, accessor, options);

        Assert.AreEqual(dataPath, service.DataPath);
    }

    [TestMethod]
    public void CreateWithDataPath_SetsDataPath()
    {
        var dataPath = "data_path";
        var logger = NullLogger<IDbxService>.Instance;
        var options = Options.Create(new DbxOptions() { DataPath = dataPath });
        var accessor = new HttpContextAccessor();

        var service = new DbxService(logger, accessor, options);

        Assert.AreEqual(dataPath, service.DataPath);
    }

    [TestMethod]
    public void CreateWithOptionsAndDataPath_SetsDataPath()
    {
        // Regression: secondary constructor was calling Options.Create(new DbxOptions())
        // instead of Options.Create(options), silently ignoring the provided DataPath.
        var dataPath = "data_path";

        var service = IDbxService.Create(new DbxOptions { DataPath = dataPath });

        Assert.AreEqual(dataPath, ((DbxService)service).DataPath);
    }

    [TestMethod]
    public void CreateWithOptionsAndNullDataPath_SetsDefaultDataPath()
    {
        var service = new DbxService(new DbxOptions());

        Assert.AreEqual("data", service.DataPath);
    }

}

[ExcludeFromCodeCoverage]
internal class MockLogger<T> : ILogger<T>
{
    public bool MessageWasLogged { get; private set; }
    public string? LoggedMessage { get; private set; }
    public LogLevel LoggedLogLevel { get; private set; }
    public Exception? LoggedException { get; private set; }
    public bool IsLogLevelEnabled { get; set; } = true;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => IsLogLevelEnabled;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LoggedLogLevel = logLevel;
        LoggedException = exception;
        LoggedMessage = formatter(state, exception);
        MessageWasLogged = true;
    }
}
