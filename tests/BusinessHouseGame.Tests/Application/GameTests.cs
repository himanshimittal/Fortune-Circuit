using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;
using Xunit;

namespace FortuneCircuit.Tests.Application;

public class GameTests
{
    private static Game Build(GameConfig cfg, Board? board = null)
    {
        var actualBoard = board ?? Board.Standard(cfg);
        var players = Enumerable.Range(1, cfg.PlayerCount)
            .Select(i => new Player(i, $"Player-{i}", cfg.InitialMoney))
            .ToList();
        var dice = new ScriptedDiceProvider();
        return new Game(players, actualBoard, dice, cfg);
    }

    [Fact]
    public void Ctor_DefaultsToCanonicalConfigAndStandardBoard()
    {
        var cfg = GameConfig.Standard();
        var game = Build(cfg);

        Assert.Equal(GameConfig.StandardPlayerCount, game.GetPlayers().Count);
        Assert.Equal(GameConfig.StandardInitialMoney, game.GetPlayers()[0].Money);
        Assert.Equal("Player-1", game.GetPlayers()[0].Name);
        Assert.Equal(Board.StandardSize, game.GetBoard().Size);
    }

    [Fact]
    public void Ctor_RespectsConfigOverrides()
    {
        var cfg = GameConfig.Standard(playerCount: 4, maxTurnsPerPlayer: 3);
        var game = Build(cfg);

        Assert.Equal(4, game.GetPlayers().Count);
        Assert.All(game.GetPlayers(), p => Assert.Equal(GameConfig.StandardInitialMoney, p.Money));
    }

    [Fact]
    public void Ctor_UsesProvidedLayout()
    {
        var cfg = GameConfig.Standard();
        var board = Board.FromLayout("E,H,T", cfg);
        var game = Build(cfg, board);

        Assert.Equal(3, game.GetBoard().Size);
    }

    [Fact]
    public void Ctor_PlayerCountMismatch_Throws()
    {
        var cfg = GameConfig.Standard(playerCount: 3);
        var board = Board.Standard(cfg);
        var players = new[] { new Player(1, "P1", cfg.InitialMoney) }; // only 1, but cfg says 3
        Assert.Throws<ArgumentException>(() =>
            new Game(players, board, new ScriptedDiceProvider(), cfg));
    }
}
