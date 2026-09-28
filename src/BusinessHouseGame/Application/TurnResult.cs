using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Application;

/// <summary>
/// Observable record of a single turn. Used for structured logging, test assertions,
/// and any future replay/UI layer. <see cref="GameEngine"/> is the only producer.
/// </summary>
public sealed record TurnResult(
    int TurnNumber,
    int PlayerId,
    string PlayerName,
    int DiceValue,
    int FromIndex,
    int ToIndex,
    char CellSymbol,
    TurnAction Action,
    int MoneyBefore,
    int MoneyAfter,
    int? AffectedOwnerId);
