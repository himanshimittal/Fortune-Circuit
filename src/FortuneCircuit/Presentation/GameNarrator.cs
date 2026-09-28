using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Effects;
using FortuneCircuit.Infrastructure;

namespace FortuneCircuit.Presentation;

/// <summary>
/// Verbose, human-readable play-by-play output. Pure presentation — consumes the
/// turn-result stream produced by the engine and writes a narrative to a
/// <see cref="TextWriter"/>. Domain types never call into this.
/// </summary>
public sealed class GameNarrator
{
    private readonly TextWriter _out;

    public GameNarrator(TextWriter? writer = null)
    {
        _out = writer ?? Console.Out;
    }

    /// <summary>Prints the board as a ruler-like strip with indices on top and symbols below.</summary>
    public void PrintBoard(Board board)
    {
        _out.WriteLine();
        _out.WriteLine("== Board ==");
        var idx = new System.Text.StringBuilder();
        var sym = new System.Text.StringBuilder();
        for (int i = 0; i < board.Size; i++)
        {
            idx.Append($"{i,3} ");
            sym.Append($"  {board.CellAt(i).Symbol} ");
        }
        _out.WriteLine(idx.ToString().TrimEnd());
        _out.WriteLine(sym.ToString().TrimEnd());
        _out.WriteLine();
    }

    /// <summary>Prints one line per turn: who, dice roll, where, what happened, balance.</summary>
    public void PrintTurns(IReadOnlyList<TurnResult> turns)
    {
        _out.WriteLine("== Play-by-play ==");
        foreach (var t in turns)
        {
            string description = t.Action switch
            {
                TurnAction.None              => "stepped on an empty cell",
                TurnAction.PaidJailFine      => "went to JAIL and paid a fine",
                TurnAction.CollectedTreasure => "found TREASURE",
                TurnAction.BoughtHotel       => $"BOUGHT the hotel at {t.ToIndex}",
                TurnAction.VisitedOwnHotel   => "visited their own hotel (free)",
                TurnAction.PaidRent          => $"paid RENT to player {t.AffectedOwnerId}",
                _                            => t.Action.ToString(),
            };

            int delta = t.MoneyAfter - t.MoneyBefore;
            string deltaStr = delta == 0 ? "      " : (delta > 0 ? $"+{delta,4}" : $"{delta,5}");
            _out.WriteLine(
                $"  T{t.TurnNumber,3} | {t.PlayerName,-10} | dice {t.DiceValue,2} | " +
                $"{t.FromIndex,2}->{t.ToIndex,2} [{t.CellSymbol}] | {description,-40} | " +
                $"${t.MoneyAfter,5} ({deltaStr})");
        }
        _out.WriteLine();
    }

    /// <summary>Per-player counters: how many times each player landed on each cell type.</summary>
    public void PrintStats(IReadOnlyList<TurnResult> turns, IReadOnlyList<Player> players)
    {
        _out.WriteLine("== Per-player stats ==");
        _out.WriteLine($"  {"Player",-10} {"Jail",5} {"Treasure",10} {"Bought",8} {"OwnVisit",10} {"Rent paid",10} {"Rent recv",10}");
        foreach (var p in players)
        {
            int jail     = turns.Count(t => t.PlayerId == p.Id && t.Action == TurnAction.PaidJailFine);
            int treasure = turns.Count(t => t.PlayerId == p.Id && t.Action == TurnAction.CollectedTreasure);
            int bought   = turns.Count(t => t.PlayerId == p.Id && t.Action == TurnAction.BoughtHotel);
            int ownVisit = turns.Count(t => t.PlayerId == p.Id && t.Action == TurnAction.VisitedOwnHotel);
            int rentPaid = turns.Count(t => t.PlayerId == p.Id && t.Action == TurnAction.PaidRent);
            int rentRecv = turns.Count(t => t.Action == TurnAction.PaidRent && t.AffectedOwnerId == p.Id);
            _out.WriteLine(
                $"  {p.Name,-10} {jail,5} {treasure,10} {bought,8} {ownVisit,10} {rentPaid,10} {rentRecv,10}");
        }
        _out.WriteLine();
    }
}
