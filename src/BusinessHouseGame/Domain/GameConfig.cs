namespace FortuneCircuit.Domain;

/// <summary>
/// Immutable, injected game-wide rule values. Construct once in the composition root
/// and pass into every consumer that needs rule data.
/// </summary>
public sealed class GameConfig
{
    // ----- Canonical defaults (problem statement). -----
    public const int StandardPlayerCount       = 2;
    public const int StandardInitialMoney      = 1000;
    public const int StandardHotelPrice        = 200;
    public const int StandardHotelRent         = 50;
    public const int StandardJailFine          = 150;
    public const int StandardTreasureReward    = 200;
    public const int StandardMaxTurnsPerPlayer = 10;

    public int PlayerCount       { get; }
    public int InitialMoney      { get; }
    public int HotelPrice        { get; }
    public int HotelRent         { get; }
    public int JailFine          { get; }
    public int TreasureReward    { get; }
    public int MaxTurnsPerPlayer { get; }

    public GameConfig(
        int playerCount,
        int initialMoney,
        int hotelPrice,
        int hotelRent,
        int jailFine,
        int treasureReward,
        int maxTurnsPerPlayer)
    {
        PlayerCount       = playerCount;
        InitialMoney      = initialMoney;
        HotelPrice        = hotelPrice;
        HotelRent         = hotelRent;
        JailFine          = jailFine;
        TreasureReward    = treasureReward;
        MaxTurnsPerPlayer = maxTurnsPerPlayer;
        Validate();
    }

    /// <summary>Canonical defaults with optional CLI-tunable overrides.</summary>
    public static GameConfig Standard(int? playerCount = null, int? maxTurnsPerPlayer = null) =>
        new(
            playerCount:       playerCount       ?? StandardPlayerCount,
            initialMoney:      StandardInitialMoney,
            hotelPrice:        StandardHotelPrice,
            hotelRent:         StandardHotelRent,
            jailFine:          StandardJailFine,
            treasureReward:    StandardTreasureReward,
            maxTurnsPerPlayer: maxTurnsPerPlayer ?? StandardMaxTurnsPerPlayer);

    /// <summary>Fail-fast validation. Invoked from the constructor.</summary>
    public void Validate()
    {
        if (PlayerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(PlayerCount), PlayerCount, "must be >= 1");
        if (InitialMoney < 0)
            throw new ArgumentOutOfRangeException(nameof(InitialMoney), InitialMoney, "must be >= 0");
        if (HotelPrice < 0 || HotelRent < 0 || JailFine < 0 || TreasureReward < 0)
            throw new ArgumentException("Monetary rule values must be >= 0.");
        if (MaxTurnsPerPlayer < 1)
            throw new ArgumentOutOfRangeException(nameof(MaxTurnsPerPlayer), MaxTurnsPerPlayer, "must be >= 1");
    }
}
