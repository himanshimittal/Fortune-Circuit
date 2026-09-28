using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;

namespace FortuneCircuit.Infrastructure;

/// <summary>
/// Immutable layout of the board. Cell instances themselves may hold mutable state
/// (e.g. hotel ownership), but the sequence and identity of cells is fixed.
/// </summary>
public sealed class Board
{
    /// <summary>
    /// The canonical Business House Game board. The size and the symbol at every
    /// index are part of the rules and are not configurable at runtime. Tests that
    /// need a smaller synthetic board use <see cref="FromLayout"/> instead.
    /// </summary>
    public const string StandardLayout =
        "E,E,J,H,E,J,T,T,E,E,H,J,T,H,E,E,J,H,E,T,J,T,E,E,H,J,T,H,E,H,E";

    /// <summary>Number of cells in the canonical board.</summary>
    public const int StandardSize = 31;

    private readonly IReadOnlyList<Cell> _cells;

    public int Size => _cells.Count;

    /// <summary>Read-only view of the cells in board order. Callers query/filter as needed.</summary>
    public IReadOnlyList<Cell> Cells => _cells;

    /// <summary>
    /// Construct a board from an arbitrary, validated cell list. Most callers want
    /// <see cref="Standard"/> (the canonical board) or <see cref="FromLayout"/>
    /// (a layout string).
    /// </summary>
    public Board(IReadOnlyList<Cell> cells)
    {
        if (cells is null || cells.Count == 0)
            throw new ArgumentException("Board must contain at least one cell.", nameof(cells));
        _cells = cells;
    }

    /// <summary>
    /// Parse a comma-separated layout string of single-character cell symbols
    /// (E, J, T, H). The standard board is <see cref="StandardLayout"/>; callers
    /// may supply any valid layout.
    /// </summary>
    public static Board FromLayout(string commaSeparatedSymbols, GameConfig cfg)
    {
        if (string.IsNullOrWhiteSpace(commaSeparatedSymbols))
            throw new ArgumentException("Layout is required.", nameof(commaSeparatedSymbols));
        if (cfg is null) throw new ArgumentNullException(nameof(cfg));

        var tokens = commaSeparatedSymbols.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var cells = new Cell[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i].Length != 1)
                throw new ArgumentException($"Invalid cell token '{tokens[i]}' at index {i}.");
            cells[i] = Cell.Create(tokens[i][0], i, cfg);
        }
        return new Board(cells);
    }

    /// <summary>
    /// Builds a fresh instance of the canonical, fixed game board. Returns a new
    /// <see cref="Board"/> each call so per-game mutable cell state (hotel
    /// ownership) is isolated between runs.
    /// </summary>
    public static Board Standard(GameConfig cfg) => FromLayout(StandardLayout, cfg);

    /// <summary>Circular forward movement. Centralized so modulo logic exists in one place.</summary>
    public int Advance(int fromIndex, int steps)
    {
        if (steps < 0)
            throw new ArgumentOutOfRangeException(nameof(steps), "Dice value must be non-negative.");
        return ((fromIndex + steps) % Size + Size) % Size;
    }

    public Cell CellAt(int index) => _cells[index];
}
