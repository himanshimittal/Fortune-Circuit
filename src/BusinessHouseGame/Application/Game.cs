using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Infrastructure.Dice;

namespace FortuneCircuit.Application;

/// <summary>
/// Aggregate of the per-game state: players, board, dice and the configuration that
/// produced them. Constructed once in the composition root; passed read-only to the engine.
/// </summary>
public sealed class Game
{
    private readonly IReadOnlyList<Player> _players;
    private readonly Board _board;
    private readonly IDiceProvider _dice;
    private readonly GameConfig _config;

    public Game(IReadOnlyList<Player> players, Board board, IDiceProvider dice, GameConfig config)
    {
        _players = players ?? throw new ArgumentNullException(nameof(players));
        _board   = board   ?? throw new ArgumentNullException(nameof(board));
        _dice    = dice    ?? throw new ArgumentNullException(nameof(dice));
        _config  = config  ?? throw new ArgumentNullException(nameof(config));

        if (_players.Count != _config.PlayerCount)
            throw new ArgumentException(
                $"Player count ({_players.Count}) does not match configuration ({_config.PlayerCount}).",
                nameof(players));
    }

    public Board GetBoard() => _board;
    public IReadOnlyList<Player> GetPlayers() => _players;
    public IDiceProvider GetDice() => _dice;
    public GameConfig GetConfig() => _config;
}
