using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Infrastructure;

namespace FortuneCircuit.Presentation;

/// <summary>
/// Sole writer of user-facing scoreboard output. Domain types do not write to stdout.
/// Worth computation is delegated to <see cref="WorthCalculator"/>.
/// </summary>
public sealed class ScoreReporter
{
    private readonly TextWriter _out;
    private readonly WorthCalculator _worth;

    public ScoreReporter(WorthCalculator worth, TextWriter? writer = null)
    {
        _worth = worth ?? throw new ArgumentNullException(nameof(worth));
        _out = writer ?? Console.Out;
    }

    public void Print(IReadOnlyList<Player> players, Board board)
    {
        var ranked = players
            .Select(p => new { Player = p, Worth = _worth.Compute(p, board) })
            .OrderByDescending(x => x.Worth)
            .ThenBy(x => x.Player.Id);

        foreach (var r in ranked)
            _out.WriteLine($"{r.Player.Name} has total worth {r.Worth}");
    }
}
