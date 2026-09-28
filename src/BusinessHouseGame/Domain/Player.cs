namespace FortuneCircuit.Domain;

/// <summary>
/// Represents a single participant. State mutation goes through Credit/Debit/MoveTo
/// so invariants and future audit hooks live in one place.
/// </summary>
public sealed class Player
{
    public int Id { get; }
    public string Name { get; }
    public int Money { get; private set; }
    public int Position { get; private set; }

    public Player(int id, string name, int initialMoney)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), id, "Player id must be >= 1.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name is required.", nameof(name));
        if (initialMoney < 0)
            throw new ArgumentOutOfRangeException(nameof(initialMoney), initialMoney, "Initial money must be >= 0.");

        Id = id;
        Name = name;
        Money = initialMoney;
        Position = 0;
    }

    public void Credit(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be non-negative.");
        Money += amount;
    }

    /// <summary>
    /// Debits unconditionally. Negative balances are allowed per current spec; bankruptcy
    /// handling would be added here as a guard or status flag.
    /// </summary>
    public void Debit(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Debit amount must be non-negative.");
        Money -= amount;
    }

    public void MoveTo(int newPosition)
    {
        if (newPosition < 0)
            throw new ArgumentOutOfRangeException(nameof(newPosition), newPosition, "Position must be >= 0.");
        Position = newPosition;
    }
}
