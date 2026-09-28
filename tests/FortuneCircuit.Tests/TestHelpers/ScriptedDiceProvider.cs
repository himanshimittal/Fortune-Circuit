using FortuneCircuit.Infrastructure.Dice;

namespace FortuneCircuit.Tests;

/// <summary>Deterministic dice provider returning scripted values; throws when exhausted.</summary>
internal sealed class ScriptedDiceProvider : IDiceProvider
{
    private readonly Queue<int> _rolls;
    public ScriptedDiceProvider(params int[] rolls)
    {
        if (rolls is null) throw new ArgumentNullException(nameof(rolls));
        foreach (var r in rolls)
            if (r < 0)
                throw new ArgumentOutOfRangeException(nameof(rolls), r, "Scripted dice roll must be >= 0.");
        _rolls = new Queue<int>(rolls);
    }
    public bool HasNext() => _rolls.Count > 0;
    public int Next() => _rolls.Count == 0
        ? throw new InvalidOperationException("Scripted dice exhausted.")
        : _rolls.Dequeue();
}
