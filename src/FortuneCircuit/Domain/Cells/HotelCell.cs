using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Domain.Cells;

public sealed class HotelCell : Cell
{
    // Ownership lives on the cell (the hotel IS the asset). Stored as id, not a
    // reference, to avoid lifecycle/aliasing concerns and keep serialization trivial.
    public int? OwnerId { get; private set; }

    public int Price { get; }
    public int Rent  { get; }

    public HotelCell(int index, int price, int rent) : base(index)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price), price, "must be >= 0");
        if (rent  < 0) throw new ArgumentOutOfRangeException(nameof(rent),  rent,  "must be >= 0");
        Price = price;
        Rent  = rent;
    }

    public override char Symbol => 'H';

    public override CellEffect ApplyEffect(Player player)
    {
        if (OwnerId is null)
            return new CellEffect.BuyHotel(Price);

        if (OwnerId == player.Id)
            return new CellEffect.OwnHotelVisit();

        return new CellEffect.PayRent(OwnerId.Value, Rent);
    }

    /// <summary>
    /// Called only by the engine when it commits a <see cref="CellEffect.BuyHotel"/>.
    /// Kept internal-ish via single call site; throws if ownership is reassigned.
    /// </summary>
    public void AssignOwner(int playerId)
    {
        if (OwnerId is not null)
            throw new InvalidOperationException($"Hotel at {Index} is already owned by player {OwnerId}.");
        OwnerId = playerId;
    }
}
