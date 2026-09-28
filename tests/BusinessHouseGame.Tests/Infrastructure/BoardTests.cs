using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Infrastructure;
using Xunit;

namespace FortuneCircuit.Tests.Infrastructure;

public class BoardTests
{
    private static readonly GameConfig Cfg = GameConfig.Standard();

    // ----- Movement rule -----

    [Theory]
    [InlineData(0, 5, 5)]   // no wrap
    [InlineData(8, 2, 0)]   // wraps to start
    [InlineData(5, 7, 2)]   // wraps mid-board
    [InlineData(3, 0, 3)]   // zero step stays in place
    [InlineData(9, 1, 0)]   // last cell -> first (exact wrap edge)
    public void Advance_IsCircular(int from, int steps, int expected)
    {
        var board = Board.FromLayout("E,E,E,E,E,E,E,E,E,E", Cfg); // size 10
        Assert.Equal(expected, board.Advance(from, steps));
    }

    // ----- Cells accessor -----

    [Fact]
    public void Cells_ExposesLayoutInOrder()
    {
        var board = Board.FromLayout("H,E,J,T", Cfg);

        Assert.Equal(4, board.Cells.Count);
        Assert.IsType<HotelCell>(board.Cells[0]);
        Assert.IsType<EmptyCell>(board.Cells[1]);
        Assert.IsType<JailCell>(board.Cells[2]);
        Assert.IsType<TreasureCell>(board.Cells[3]);
    }

    // ----- Fixed-board pinning (the canonical Business House Game board) -----

    [Fact]
    public void StandardBoard_HasFixedSymbolSequence()
    {
        var board = Board.Standard(Cfg);
        var expected = "EEJHEJTTEEHJTHEEJHETJTEEHJTHEHE";
        for (int i = 0; i < board.Size; i++)
            Assert.Equal(expected[i], board.CellAt(i).Symbol);
    }

    [Fact]
    public void StandardBoard_ReturnsFreshInstance()
    {
        // Per-game isolation: hotel ownership on one board must not leak to another.
        var a = Board.Standard(Cfg);
        var b = Board.Standard(Cfg);
        var firstHotel = Enumerable.Range(0, a.Size).First(i => a.CellAt(i) is HotelCell);
        ((HotelCell)a.CellAt(firstHotel)).AssignOwner(1);

        Assert.Equal(1, ((HotelCell)a.CellAt(firstHotel)).OwnerId);
        Assert.Null(((HotelCell)b.CellAt(firstHotel)).OwnerId);
    }
}
