using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Effects;
using FortuneCircuit.Presentation;
using Xunit;

namespace FortuneCircuit.Tests.Presentation;

public class GameNarratorTests
{
    [Fact]
    public void PrintTurns_RendersOneLinePerTurn_WithKnownDescriptions()
    {
        var sw = new StringWriter();
        var turns = new[]
        {
            new TurnResult(1, 1, "P1", 3, 0, 3, 'H', TurnAction.BoughtHotel,        1000, 800, null),
            new TurnResult(2, 2, "P2", 4, 0, 4, 'T', TurnAction.CollectedTreasure,  1000, 1200, null),
            new TurnResult(3, 1, "P1", 2, 3, 5, 'J', TurnAction.PaidJailFine,       800,  650,  null),
            new TurnResult(4, 2, "P2", 1, 4, 5, 'J', TurnAction.PaidJailFine,       1200, 1050, null),
            new TurnResult(5, 1, "P1", 3, 5, 8, 'E', TurnAction.None,               650,  650,  null),
            new TurnResult(6, 2, "P2", 1, 5, 6, 'H', TurnAction.PaidRent,           1050, 1000, 1),
            new TurnResult(7, 1, "P1", 6, 8, 14, 'H', TurnAction.VisitedOwnHotel,   650,  650,  null),
        };

        new GameNarrator(sw).PrintTurns(turns);

        var output = sw.ToString();
        Assert.Contains("== Play-by-play ==", output);
        Assert.Contains("BOUGHT the hotel at 3", output);
        Assert.Contains("found TREASURE", output);
        Assert.Contains("went to JAIL and paid a fine", output);
        Assert.Contains("stepped on an empty cell", output);
        Assert.Contains("paid RENT to player 1", output);
        Assert.Contains("visited their own hotel (free)", output);

        // 7 turn lines + header + trailing blank line.
        var lines = output.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(8, lines.Length);
    }

    [Fact]
    public void PrintStats_CountsPerPlayerActionsCorrectly()
    {
        var sw = new StringWriter();
        var players = new[]
        {
            new Player(1, "P1", 1000),
            new Player(2, "P2", 1000),
        };
        var turns = new[]
        {
            new TurnResult(1, 1, "P1", 3, 0, 3, 'H', TurnAction.BoughtHotel,       1000, 800, null),
            new TurnResult(2, 2, "P2", 3, 0, 3, 'H', TurnAction.PaidRent,          1000, 950, 1),
            new TurnResult(3, 1, "P1", 2, 3, 5, 'J', TurnAction.PaidJailFine,      800,  650, null),
            new TurnResult(4, 2, "P2", 4, 3, 7, 'T', TurnAction.CollectedTreasure, 950,  1150, null),
            new TurnResult(5, 1, "P1", 9, 5, 14, 'H', TurnAction.VisitedOwnHotel,  650,  650, null),
        };

        new GameNarrator(sw).PrintStats(turns, players);

        var output = sw.ToString();
        Assert.Contains("== Per-player stats ==", output);
        Assert.Contains("P1", output);
        Assert.Contains("P2", output);
        // P1 bought 1, has 1 jail, 1 own-visit; received 1 rent
        // P2 collected 1 treasure, paid 1 rent
        // Spot-check via presence of expected numeric column values.
        Assert.Matches(@"P1\s+1\s+0\s+1\s+1\s+0\s+1", output);
        Assert.Matches(@"P2\s+0\s+1\s+0\s+0\s+1\s+0", output);
    }

}
