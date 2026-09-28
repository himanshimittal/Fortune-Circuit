using FortuneCircuit.Domain.Cells;

namespace FortuneCircuit.Domain.Effects;

/// <summary>
/// Runtime services an effect needs to commit itself. Implemented by the application
/// layer and passed to <see cref="CellEffect.Apply"/>. Domain effects depend only on
/// this small surface — never on logging, dice, or other infrastructure types.
/// </summary>
public interface IEffectContext
{
    Player ActingPlayer { get; }

    Cell Cell { get; }

    /// <summary>Resolves a player by id. Throws if the id is not in the roster.</summary>
    Player Owner(int playerId);

    /// <summary>Reports that a player's balance has just become negative.</summary>
    void OnNegativeBalance(Player player);
}
