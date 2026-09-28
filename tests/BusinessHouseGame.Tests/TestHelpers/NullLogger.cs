using FortuneCircuit.Infrastructure.Logging;

namespace FortuneCircuit.Tests;

/// <summary>Discards all log records. Used by tests that don't care about logging.</summary>
internal sealed class NullLogger : ILogger
{
    public void Log(LogLevel level, string message, object? data = null) { }
}
