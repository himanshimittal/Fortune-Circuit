namespace FortuneCircuit.Infrastructure.Dice;

/// <summary>
/// Two six-sided dice. Range [2..12] with a triangular distribution, matching the
/// natural shape of 2d6.
/// </summary>
public sealed class RandomDiceProvider : IDiceProvider
{
    private readonly Random _rng = new();
    private int _rollsRemaining;

    public RandomDiceProvider(int maxRolls)
    {
        if (maxRolls < 0)
            throw new ArgumentOutOfRangeException(nameof(maxRolls), "must be >= 0");

        _rollsRemaining = maxRolls;
    }

    public bool HasNext() => _rollsRemaining > 0;

    public int Next()
    {
        if (!HasNext())
            throw new InvalidOperationException("No dice rolls remaining.");

        _rollsRemaining--;
        int d1 = _rng.Next(1, 7);
        int d2 = _rng.Next(1, 7);
        return d1 + d2;
    }
}
