using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Infrastructure.Dice;
using FortuneCircuit.Infrastructure.Logging;
using FortuneCircuit.Presentation;

namespace FortuneCircuit;

internal static class Program
{
    /// <summary>
    /// Optional CLI overrides:
    ///   --turns N         override MaxTurnsPerPlayer (default 10)
    ///   --players N       override PlayerCount       (default 2)
    ///   --board "E,H,..." override board layout      (default: standard 31-cell)
    ///   --verbose         enable per-turn Debug log lines
    ///   --quiet           suppress all logs (only narrator/scoreboard)
    /// </summary>
    private static int Main(string[] args)
    {
        try
        {
            var (turns, playerCount, boardLayout, logLevel) = ParseArgs(args);

            var config       = GameConfig.Standard(playerCount, turns);
            var logger       = new ConsoleLogger(logLevel);
            var board        = boardLayout is null ? Board.Standard(config) : Board.FromLayout(boardLayout, config);
            var players      = Enumerable.Range(1, config.PlayerCount)
                                         .Select(id => new Player(id, $"Player-{id}", config.InitialMoney))
                                         .ToList();
            var dice         = new RandomDiceProvider(config.PlayerCount * config.MaxTurnsPerPlayer);
            var game         = new Game(players, board, dice, config);
            var effects      = new EffectProcessor(players.ToDictionary(p => p.Id), logger);
            var engine       = new GameEngine(game, effects, logger);

            var narrator = new GameNarrator();
            narrator.PrintBoard(game.GetBoard());

            var turnResults = engine.Execute();

            narrator.PrintTurns(turnResults);
            narrator.PrintStats(turnResults, game.GetPlayers());
            new ScoreReporter(new WorthCalculator(config)).Print(game.GetPlayers(), game.GetBoard());
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    private static (int? turns, int? playerCount, string? boardLayout, LogLevel logLevel) ParseArgs(string[] args)
    {
        int? turns = null, playerCount = null;
        string? boardLayout = null;
        LogLevel logLevel = LogLevel.Info;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--turns"   when i + 1 < args.Length: turns       = int.Parse(args[++i]); break;
                case "--players" when i + 1 < args.Length: playerCount = int.Parse(args[++i]); break;
                case "--board"   when i + 1 < args.Length: boardLayout = args[++i];            break;
                case "--verbose": logLevel = LogLevel.Debug; break;
                case "--quiet":   logLevel = LogLevel.Error; break;
            }
        }

        return (turns, playerCount, boardLayout, logLevel);
    }
}
