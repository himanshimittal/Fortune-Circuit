using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Infrastructure;
using Xunit;

namespace FortuneCircuit.Tests.Application;

public class WorthCalculatorTests
{
    private static readonly GameConfig Cfg = GameConfig.Standard();
    private readonly WorthCalculator _calc = new(Cfg);

    [Fact]
    public void Compute_NoHotels_ReturnsCashOnly()
    {
        var player = new Player(1, "A", 500);
        var board = Board.FromLayout("E,E,E", Cfg);

        Assert.Equal(500, _calc.Compute(player, board));
    }

    [Fact]
    public void Compute_IncludesOwnedHotels()
    {
        var player = new Player(1, "A", 100);
        var board = Board.FromLayout("H,H,E,H", Cfg);
        ((HotelCell)board.CellAt(0)).AssignOwner(1);
        ((HotelCell)board.CellAt(1)).AssignOwner(1);
        ((HotelCell)board.CellAt(3)).AssignOwner(2); // not the player's

        Assert.Equal(100 + 2 * Cfg.HotelPrice, _calc.Compute(player, board));
    }
}
