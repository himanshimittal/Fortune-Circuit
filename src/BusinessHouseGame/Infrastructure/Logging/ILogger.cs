namespace FortuneCircuit.Infrastructure.Logging;

public enum LogLevel { Debug, Info, Warn, Error }

/// <summary>
/// Minimal logging abstraction. Domain types do not depend on this — only the engine
/// and the composition root do. Structured fields are passed as an object so a real
/// production sink (Serilog, etc.) can render them as JSON.
/// </summary>
public interface ILogger
{
    void Log(LogLevel level, string message, object? data = null);
}
