using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Infrastructure.Dice;
using FortuneCircuit.Infrastructure.Logging;

namespace FortuneCircuit.Application;

/// <summary>
/// Orchestrates turns. Knows nothing about specific cell rules — it asks each cell
/// for a <see cref="Domain.Effects.CellEffect"/> and delegates the commit to
/// <see cref="EffectProcessor"/>.
/// </summary>
public sealed class GameEngine
{
    private readonly IReadOnlyList<Player> _players;
    private readonly Board _board;
    private readonly IDiceProvider _dice;
    private readonly GameConfig _config;
    private readonly ILogger _logger;
    private readonly IEffectProcessor _effects;

    public GameEngine(Game game, IEffectProcessor effects, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(game);

        _players = game.GetPlayers();
        _board   = game.GetBoard();
        _dice    = game.GetDice();
        _config  = game.GetConfig();
        _effects = effects ?? throw new ArgumentNullException(nameof(effects));
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    public IReadOnlyList<TurnResult> Execute()
    {
        LogGameStarted(_config);

        var results = new List<TurnResult>();
        int turnNumber = 0;

        for (int round = 0; round < _config.MaxTurnsPerPlayer; round++)
        {
            if (!PlayRound(round, results, ref turnNumber))
            {
                LogGameFinished(results.Count);
                return results;
            }
        }

        LogGameFinished(results.Count);
        return results;
    }

    /// <summary>Plays one round (every player takes a turn). Returns false if dice ran out mid-round.</summary>
    private bool PlayRound(int round, List<TurnResult> results, ref int turnNumber)
    {
        foreach (var player in _players)
        {
            if (!_dice.HasNext())
            {
                _logger.Log(LogLevel.Warn, "DiceExhausted", new { Round = round });
                return false;
            }

            turnNumber++;
            var result = PlayTurn(turnNumber, player);
            results.Add(result);
            _logger.Log(LogLevel.Debug, "Turn", result);
        }
        return true;
    }

    private void LogGameStarted(GameConfig cfg) =>
        _logger.Log(LogLevel.Info, "GameStarted", new
        {
            Players = _players.Count,
            BoardSize = _board.Size,
            MaxTurns = cfg.MaxTurnsPerPlayer
        });

    private void LogGameFinished(int turnsPlayed) =>
        _logger.Log(LogLevel.Info, "GameFinished", new { TurnsPlayed = turnsPlayed });

    private TurnResult PlayTurn(int turnNumber, Player player)
    {
        int dice = _dice.Next();
        int from = player.Position;
        int to = _board.Advance(from, dice);
        player.MoveTo(to);

        var cell = _board.CellAt(to);
        int moneyBefore = player.Money;
        var effect = cell.ApplyEffect(player);
        var (action, affectedOwnerId) = _effects.Apply(effect, player, cell);

        return new TurnResult(
            TurnNumber: turnNumber,
            PlayerId: player.Id,
            PlayerName: player.Name,
            DiceValue: dice,
            FromIndex: from,
            ToIndex: to,
            CellSymbol: cell.Symbol,
            Action: action,
            MoneyBefore: moneyBefore,
            MoneyAfter: player.Money,
            AffectedOwnerId: affectedOwnerId);
    }
}
