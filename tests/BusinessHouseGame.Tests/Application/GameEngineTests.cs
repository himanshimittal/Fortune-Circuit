using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Domain.Effects;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Infrastructure.Logging;
using Xunit;

namespace FortuneCircuit.Tests.Application;

public class GameEngineTests
{
    private static (GameEngine engine, IReadOnlyList<Player> players, Board board, GameConfig cfg) Build(
        string layout, int[] rolls, int playerCount = 1, int maxTurns = 1, int initialMoney = 1000,
        ILogger? logger = null)
    {
        var cfg = new GameConfig(
            playerCount:       playerCount,
            initialMoney:      initialMoney,
            hotelPrice:        GameConfig.StandardHotelPrice,
            hotelRent:         GameConfig.StandardHotelRent,
            jailFine:          GameConfig.StandardJailFine,
            treasureReward:    GameConfig.StandardTreasureReward,
            maxTurnsPerPlayer: maxTurns);

        var board = Board.FromLayout(layout, cfg);
        var players = Enumerable.Range(1, playerCount)
            .Select(i => new Player(i, $"P{i}", initialMoney)).ToList();
        var actualLogger = logger ?? new NullLogger();
        var game = new Game(players, board, new ScriptedDiceProvider(rolls), cfg);
        var effects = new EffectProcessor(players.ToDictionary(p => p.Id), actualLogger);
        var engine = new GameEngine(game, effects, actualLogger);
        return (engine, players, board, cfg);
    }

    // ----- The six game rules, integrated through the engine -----

    [Fact]
    public void Execute_JailCell_DebitsFine()
    {
        var (engine, players, _, cfg) = Build("E,J,E", new[] { 1 });
        engine.Execute();
        Assert.Equal(cfg.InitialMoney - cfg.JailFine, players[0].Money);
    }

    [Fact]
    public void Execute_TreasureCell_CreditsReward()
    {
        var (engine, players, _, cfg) = Build("E,T,E", new[] { 1 });
        engine.Execute();
        Assert.Equal(cfg.InitialMoney + cfg.TreasureReward, players[0].Money);
    }

    [Fact]
    public void Execute_UnownedHotel_BuysAndAssigns()
    {
        var (engine, players, board, cfg) = Build("E,H", new[] { 1 });
        engine.Execute();
        Assert.Equal(cfg.InitialMoney - cfg.HotelPrice, players[0].Money);
        Assert.Equal(1, ((HotelCell)board.CellAt(1)).OwnerId);
    }

    [Fact]
    public void Execute_HotelOwnedByOther_TransfersRent()
    {
        var (engine, players, _, cfg) = Build("E,H,E", new[] { 1, 1 }, playerCount: 2, maxTurns: 1);
        engine.Execute();

        Assert.Equal(cfg.InitialMoney - cfg.HotelPrice + cfg.HotelRent, players[0].Money);
        Assert.Equal(cfg.InitialMoney - cfg.HotelRent, players[1].Money);
    }

    [Fact]
    public void Execute_OwnHotelRevisit_NoChange()
    {
        var (engine, players, _, cfg) = Build("E,H,E,E,E", new[] { 1, 5 }, maxTurns: 2);
        var results = engine.Execute().ToList();

        Assert.Equal(TurnAction.BoughtHotel, results[0].Action);
        Assert.Equal(TurnAction.VisitedOwnHotel, results[1].Action);
        Assert.Equal(results[1].MoneyBefore, results[1].MoneyAfter);
        Assert.Equal(cfg.InitialMoney - cfg.HotelPrice, players[0].Money);
    }

    // ----- Orchestration -----

    [Fact]
    public void Execute_PlaysAllTurns_RoundRobin()
    {
        var (engine, _, _, _) = Build("E,E,E,E", new[] { 1, 1, 1, 1 }, playerCount: 2, maxTurns: 2);
        var results = engine.Execute();

        Assert.Equal(4, results.Count);
        Assert.Equal(new[] { 1, 2, 1, 2 }, results.Select(r => r.PlayerId));
    }

    [Fact]
    public void Execute_DiceExhausted_StopsEarly()
    {
        // Graceful early-stop when the dice provider runs dry mid-game.
        var (engine, _, _, _) = Build("E,E,E", new[] { 1 }, maxTurns: 3);
        Assert.Single(engine.Execute());
    }
}
