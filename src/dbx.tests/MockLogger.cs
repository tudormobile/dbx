using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Dbx.Tests;

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
