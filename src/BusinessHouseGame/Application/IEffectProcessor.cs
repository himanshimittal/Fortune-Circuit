using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Application;

/// <summary>
/// Commits a <see cref="CellEffect"/> against the acting player, the cell, and the
/// rest of the roster. Implementations bridge any infrastructure (logging, lookups)
/// the effects require.
/// </summary>
public interface IEffectProcessor
{
    (TurnAction Action, int? AffectedOwnerId) Apply(CellEffect effect, Player player, Cell cell);
}
