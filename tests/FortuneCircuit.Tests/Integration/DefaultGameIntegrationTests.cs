using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Infrastructure.Dice;
using FortuneCircuit.Infrastructure.Logging;
using FortuneCircuit.Presentation;
using Xunit;

namespace FortuneCircuit.Tests.Integration;

/// <summary>
/// End-to-end coverage: the real composition root wires everything correctly and the
/// game completes with a well-formed scoreboard.
/// </summary>
public class DefaultGameIntegrationTests
{
    private static Game BuildDefaultGame(GameConfig cfg, IDiceProvider? dice = null, Board? board = null)
    {
        var actualBoard = board ?? Board.Standard(cfg);
        var players = Enumerable.Range(1, cfg.PlayerCount)
            .Select(id => new Player(id, $"Player-{id}", cfg.InitialMoney))
            .ToList();
        var actualDice = dice ?? new RandomDiceProvider(cfg.PlayerCount * cfg.MaxTurnsPerPlayer);
        return new Game(players, actualBoard, actualDice, cfg);
    }

    private static GameEngine BuildEngine(Game game, ILogger logger)
    {
        var effects = new EffectProcessor(game.GetPlayers().ToDictionary(p => p.Id), logger);
        return new GameEngine(game, effects, logger);
    }

    [Fact]
    public void DefaultGame_ProducesScoreboard()
    {
        var cfg = GameConfig.Standard();
        var game = BuildDefaultGame(cfg);
        var engine = BuildEngine(game, new NullLogger());
        var turns = engine.Execute();

        Assert.Equal(cfg.PlayerCount * cfg.MaxTurnsPerPlayer, turns.Count);

        var sw = new StringWriter();
        new ScoreReporter(new WorthCalculator(cfg), sw).Print(game.GetPlayers(), game.GetBoard());
        var lines = sw.ToString().TrimEnd().Split(Environment.NewLine);

        Assert.Equal(game.GetPlayers().Count, lines.Length);
        foreach (var line in lines)
            Assert.Matches(@"^.+ has total worth -?\d+$", line);
    }

    [Fact]
    public void DefaultGame_HotelOwnersAreValid()
    {
        var cfg = GameConfig.Standard();
        var game = BuildDefaultGame(cfg);
        var engine = BuildEngine(game, new NullLogger());
        engine.Execute();

        var ids = game.GetPlayers().Select(p => p.Id).ToHashSet();
        for (int i = 0; i < game.GetBoard().Size; i++)
            if (game.GetBoard().CellAt(i) is HotelCell h && h.OwnerId.HasValue)
                Assert.Contains(h.OwnerId.Value, ids);
    }

    [Fact]
    public void GoldenMaster_ScriptedFourTurnGame_ProducesExactLeaderboard()
    {
        // Hand-verified turn-by-turn on a 6-cell board: E H T E J H (indices 0..5).
        //   T1: P1@0 roll 1 -> H(1) Buy   -> 1000 - 200 = 800
        //   T2: P2@0 roll 2 -> T(2) Treas -> 1000 + 200 = 1200
        //   T3: P1@1 roll 3 -> J(4) Fine  ->  800 - 150 = 650
        //   T4: P2@2 roll 3 -> H(5) Buy   -> 1200 - 200 = 1000
        var cfg = GameConfig.Standard(playerCount: 2, maxTurnsPerPlayer: 2);
        var board = Board.FromLayout("E,H,T,E,J,H", cfg);
        var players = new[]
        {
            new Player(1, "P1", cfg.InitialMoney),
            new Player(2, "P2", cfg.InitialMoney),
        };
        var game = new Game(players, board, new ScriptedDiceProvider(1, 2, 3, 3), cfg);
        var engine = BuildEngine(game, new NullLogger());

        engine.Execute();

        Assert.Equal(650,  players[0].Money);
        Assert.Equal(1000, players[1].Money);
        Assert.Equal(1, ((HotelCell)board.CellAt(1)).OwnerId);
        Assert.Equal(2, ((HotelCell)board.CellAt(5)).OwnerId);
    }
}
