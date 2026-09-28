namespace FortuneCircuit.Infrastructure.Logging;

/// <summary>
/// Default console sink. Writes Info/Debug to stdout and Warn/Error to stderr,
/// each line tagged with a short correlation id so concurrent runs are separable.
/// </summary>
public sealed class ConsoleLogger : ILogger
{
    private readonly LogLevel _minLevel;
    private readonly string _correlationId;
    private readonly TextWriter _out;
    private readonly TextWriter _err;

    public ConsoleLogger(
        LogLevel minLevel = LogLevel.Info,
        string? correlationId = null,
        TextWriter? @out = null,
        TextWriter? err = null)
    {
        _minLevel = minLevel;
        _correlationId = correlationId ?? Guid.NewGuid().ToString("N")[..8];
        _out = @out ?? Console.Out;
        _err = err ?? Console.Error;
    }

    public void Log(LogLevel level, string message, object? data = null)
    {
        if (level < _minLevel) return;

        var line = $"[{DateTime.UtcNow:HH:mm:ss.fff}] [{level}] [{_correlationId}] {message}";
        if (data is not null) line += $" :: {data}";

        if (level >= LogLevel.Warn)
            _err.WriteLine(line);
        else
            _out.WriteLine(line);
    }
}
