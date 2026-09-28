using FortuneCircuit.Domain.Cells;

namespace FortuneCircuit.Domain.Effects;

/// <summary>
/// Discriminated description of what a cell wants to happen. Each effect knows how
/// to commit itself against an <see cref="IEffectContext"/> — the application-layer
/// processor merely wires up the context, so adding a new effect requires no
/// changes outside this hierarchy (Open/Closed Principle).
/// </summary>
public abstract record CellEffect
{
    /// <summary>
    /// Apply the effect to the supplied context. Returns the action that occurred
    /// and, where relevant, the id of the other player affected (e.g. a hotel
    /// owner receiving rent).
    /// </summary>
    public abstract (TurnAction Action, int? AffectedOwnerId) Apply(IEffectContext ctx);

    public sealed record None : CellEffect
    {
        public override (TurnAction, int?) Apply(IEffectContext ctx) => (TurnAction.None, null);
    }

    public sealed record PayFine(int Amount) : CellEffect
    {
        public override (TurnAction, int?) Apply(IEffectContext ctx)
        {
            ctx.ActingPlayer.Debit(Amount);
            if (ctx.ActingPlayer.Money < 0)
                ctx.OnNegativeBalance(ctx.ActingPlayer);
            return (TurnAction.PaidJailFine, null);
        }
    }

    public sealed record CollectTreasure(int Amount) : CellEffect
    {
        public override (TurnAction, int?) Apply(IEffectContext ctx)
        {
            ctx.ActingPlayer.Credit(Amount);
            return (TurnAction.CollectedTreasure, null);
        }
    }

    public sealed record BuyHotel(int Price) : CellEffect
    {
        public override (TurnAction, int?) Apply(IEffectContext ctx)
        {
            ctx.ActingPlayer.Debit(Price);
            ((HotelCell)ctx.Cell).AssignOwner(ctx.ActingPlayer.Id);
            if (ctx.ActingPlayer.Money < 0)
                ctx.OnNegativeBalance(ctx.ActingPlayer);
            return (TurnAction.BoughtHotel, null);
        }
    }

    public sealed record OwnHotelVisit : CellEffect
    {
        public override (TurnAction, int?) Apply(IEffectContext ctx) => (TurnAction.VisitedOwnHotel, null);
    }

    public sealed record PayRent(int OwnerId, int Amount) : CellEffect
    {
        public override (TurnAction, int?) Apply(IEffectContext ctx)
        {
            // Invariant: a hotel's OwnerId must always refer to a player in the roster.
            // Unreachable through normal play, but Owner() guards by throwing.
            var owner = ctx.Owner(OwnerId);
            ctx.ActingPlayer.Debit(Amount);
            owner.Credit(Amount);
            if (ctx.ActingPlayer.Money < 0)
                ctx.OnNegativeBalance(ctx.ActingPlayer);
            return (TurnAction.PaidRent, OwnerId);
        }
    }
}
