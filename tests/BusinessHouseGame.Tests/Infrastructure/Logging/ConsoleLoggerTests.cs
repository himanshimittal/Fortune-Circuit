using FortuneCircuit.Infrastructure.Logging;
using Xunit;

namespace FortuneCircuit.Tests.Infrastructure.Logging;

public class ConsoleLoggerTests
{
    [Fact]
    public void Log_BelowMinLevel_IsSuppressed()
    {
        var @out = new StringWriter();
        var err = new StringWriter();
        var log = new ConsoleLogger(LogLevel.Warn, correlationId: "test1234", @out: @out, err: err);

        log.Log(LogLevel.Debug, "ignored");
        log.Log(LogLevel.Info, "ignored");

        Assert.Equal(string.Empty, @out.ToString());
        Assert.Equal(string.Empty, err.ToString());
    }

    [Fact]
    public void Log_WarnAndError_GoToStdErr_NotStdOut()
    {
        var @out = new StringWriter();
        var err = new StringWriter();
        var log = new ConsoleLogger(LogLevel.Debug, correlationId: "abcd1234", @out: @out, err: err);

        log.Log(LogLevel.Warn, "uh-oh");
        log.Log(LogLevel.Error, "boom");

        Assert.Equal(string.Empty, @out.ToString());
        Assert.Contains("uh-oh", err.ToString());
        Assert.Contains("boom", err.ToString());
    }

    [Fact]
    public void Log_AppendsStructuredData_WhenProvided()
    {
        var @out = new StringWriter();
        var log = new ConsoleLogger(LogLevel.Debug, correlationId: "abcd1234", @out: @out, err: TextWriter.Null);

        log.Log(LogLevel.Info, "event", new { Player = "Alice", Score = 42 });

        var line = @out.ToString();
        Assert.Contains("event", line);
        Assert.Contains("::", line);
        Assert.Contains("Alice", line);
        Assert.Contains("42", line);
    }

}
