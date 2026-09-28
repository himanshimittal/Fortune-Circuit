using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Domain.Effects;
using FortuneCircuit.Infrastructure.Logging;

namespace FortuneCircuit.Application;

/// <summary>
/// Builds an <see cref="IEffectContext"/> for each effect and delegates the commit
/// to the effect itself (double dispatch). This keeps the processor closed against
/// new effect types — adding a new effect requires only a new <see cref="CellEffect"/>
/// subclass, never an edit here.
/// </summary>
public sealed class EffectProcessor : IEffectProcessor
{
    private readonly IReadOnlyDictionary<int, Player> _playersById;
    private readonly ILogger _logger;

    public EffectProcessor(IReadOnlyDictionary<int, Player> playersById, ILogger logger)
    {
        _playersById = playersById ?? throw new ArgumentNullException(nameof(playersById));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public (TurnAction Action, int? AffectedOwnerId) Apply(CellEffect effect, Player player, Cell cell)
    {
        if (effect is null) throw new ArgumentNullException(nameof(effect));
        if (player is null) throw new ArgumentNullException(nameof(player));
        if (cell is null) throw new ArgumentNullException(nameof(cell));

        var ctx = new EffectContext(player, cell, _playersById, _logger);
        return effect.Apply(ctx);
    }

    /// <summary>
    /// Domain-facing context implementation. The only place infrastructure (logger,
    /// roster lookup) is bridged into the pure-domain effect hierarchy.
    /// </summary>
    private sealed class EffectContext : IEffectContext
    {
        private readonly IReadOnlyDictionary<int, Player> _playersById;
        private readonly ILogger _logger;

        public EffectContext(
            Player actingPlayer,
            Cell cell,
            IReadOnlyDictionary<int, Player> playersById,
            ILogger logger)
        {
            ActingPlayer = actingPlayer;
            Cell = cell;
            _playersById = playersById;
            _logger = logger;
        }

        public Player ActingPlayer { get; }
        public Cell Cell { get; }

        public Player Owner(int playerId)
        {
            if (!_playersById.TryGetValue(playerId, out var owner))
                throw new InvalidOperationException(
                    $"Hotel at index {Cell.Index} has unknown owner id {playerId}.");
            return owner;
        }

        public void OnNegativeBalance(Player player)
            => _logger.Log(LogLevel.Warn, "NegativeBalance", new { player.Id, player.Name, player.Money });
    }
}
