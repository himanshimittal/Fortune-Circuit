namespace FortuneCircuit.Domain.Effects;

/// <summary>
/// What happened when a <see cref="CellEffect"/> committed. Produced by the domain,
/// surfaced in <c>TurnResult</c>, consumed by logging/narrator/stats. Enum (not string)
/// so the compiler catches typos and switches stay exhaustive.
/// </summary>
public enum TurnAction
{
    None,
    PaidJailFine,
    CollectedTreasure,
    BoughtHotel,
    VisitedOwnHotel,
    PaidRent,
}
