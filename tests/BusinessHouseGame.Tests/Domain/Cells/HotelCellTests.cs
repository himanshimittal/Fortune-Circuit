using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Domain.Effects;
using Xunit;

namespace FortuneCircuit.Tests.Domain.Cells;

public class HotelCellTests
{
    private static readonly GameConfig Cfg = GameConfig.Standard();

    private static HotelCell NewHotel(int index = 0) => new(index, Cfg.HotelPrice, Cfg.HotelRent);

    [Fact]
    public void Unowned_ApplyEffect_YieldsBuyHotel()
    {
        var buy = Assert.IsType<CellEffect.BuyHotel>(
            NewHotel(3).ApplyEffect(new Player(1, "A", 1000)));

        Assert.Equal(Cfg.HotelPrice, buy.Price);
    }

    [Fact]
    public void OwnedBySelf_ApplyEffect_YieldsOwnVisit()
    {
        var hotel = NewHotel();
        hotel.AssignOwner(1);

        Assert.IsType<CellEffect.OwnHotelVisit>(
            hotel.ApplyEffect(new Player(1, "A", 100)));
    }

    [Fact]
    public void OwnedByOther_ApplyEffect_YieldsPayRent()
    {
        var hotel = NewHotel();
        hotel.AssignOwner(2);

        var rent = Assert.IsType<CellEffect.PayRent>(
            hotel.ApplyEffect(new Player(1, "A", 100)));

        Assert.Equal(2, rent.OwnerId);
        Assert.Equal(Cfg.HotelRent, rent.Amount);
    }

    [Fact]
    public void AssignOwner_Twice_Throws()
    {
        var hotel = NewHotel();
        hotel.AssignOwner(1);
        Assert.Throws<InvalidOperationException>(() => hotel.AssignOwner(2));
    }
}
