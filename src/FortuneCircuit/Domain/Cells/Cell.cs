using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Domain.Cells;

/// <summary>
/// Base type for every cell on the board. Subclasses describe rules by returning a
/// <see cref="CellEffect"/> — they never mutate cross-player state directly.
/// </summary>
public abstract class Cell
{
    public int Index { get; }
    public abstract char Symbol { get; }

    protected Cell(int index) => Index = index;

    public abstract CellEffect ApplyEffect(Player player);

    public static Cell Create(char symbol, int index, GameConfig cfg) => symbol switch
    {
        'E' => new EmptyCell(index),
        'J' => new JailCell(index, cfg.JailFine),
        'T' => new TreasureCell(index, cfg.TreasureReward),
        'H' => new HotelCell(index, cfg.HotelPrice, cfg.HotelRent),
        _ => throw new ArgumentException($"Unknown cell symbol '{symbol}' at index {index}.", nameof(symbol)),
    };
}
