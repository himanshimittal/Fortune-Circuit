using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Presentation;
using Xunit;

namespace FortuneCircuit.Tests.Presentation;

public class ScoreReporterTests
{
    [Fact]
    public void Print_SortsByWorthThenId()
    {
        var cfg = GameConfig.Standard();
        var sw = new StringWriter();
        var players = new[]
        {
            new Player(2, "Bob",   300),
            new Player(1, "Alice", 300),
            new Player(3, "Carol", 500),
        };

        new ScoreReporter(new WorthCalculator(cfg), sw).Print(players, Board.FromLayout("E", cfg));

        var lines = sw.ToString().TrimEnd().Split(Environment.NewLine);
        Assert.Equal("Carol has total worth 500", lines[0]);
        Assert.Equal("Alice has total worth 300", lines[1]);
        Assert.Equal("Bob has total worth 300", lines[2]);
    }
}
