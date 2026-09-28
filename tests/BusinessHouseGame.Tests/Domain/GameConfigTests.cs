using FortuneCircuit.Domain;
using Xunit;

namespace FortuneCircuit.Tests.Domain;

public class GameConfigTests
{
    [Fact]
    public void Standard_NoOverrides_UsesCanonicalDefaults()
    {
        var cfg = GameConfig.Standard();

        Assert.Equal(GameConfig.StandardPlayerCount, cfg.PlayerCount);
        Assert.Equal(GameConfig.StandardInitialMoney, cfg.InitialMoney);
        Assert.Equal(GameConfig.StandardHotelPrice, cfg.HotelPrice);
        Assert.Equal(GameConfig.StandardHotelRent, cfg.HotelRent);
        Assert.Equal(GameConfig.StandardJailFine, cfg.JailFine);
        Assert.Equal(GameConfig.StandardTreasureReward, cfg.TreasureReward);
        Assert.Equal(GameConfig.StandardMaxTurnsPerPlayer, cfg.MaxTurnsPerPlayer);
    }

    [Fact]
    public void Standard_AppliesOverrides()
    {
        var cfg = GameConfig.Standard(playerCount: 4, maxTurnsPerPlayer: 3);

        Assert.Equal(4, cfg.PlayerCount);
        Assert.Equal(3, cfg.MaxTurnsPerPlayer);
    }

    [Fact]
    public void Construct_RejectsInvalidPlayerCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameConfig.Standard(playerCount: 0));
    }

    [Fact]
    public void Construct_RejectsInvalidMaxTurns()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GameConfig.Standard(maxTurnsPerPlayer: 0));
    }

    [Fact]
    public void Construct_RejectsNegativeMonetaryValues()
    {
        Assert.Throws<ArgumentException>(() => new GameConfig(
            playerCount: 2, initialMoney: 1000, hotelPrice: -1,
            hotelRent: 50, jailFine: 150, treasureReward: 200, maxTurnsPerPlayer: 10));
    }

    [Fact]
    public void StandardRuleValues_AreFixed()
    {
        Assert.Equal(10,   GameConfig.StandardMaxTurnsPerPlayer);
        Assert.Equal(2,    GameConfig.StandardPlayerCount);
        Assert.Equal(1000, GameConfig.StandardInitialMoney);
        Assert.Equal(200,  GameConfig.StandardHotelPrice);
        Assert.Equal(50,   GameConfig.StandardHotelRent);
        Assert.Equal(150,  GameConfig.StandardJailFine);
        Assert.Equal(200,  GameConfig.StandardTreasureReward);
    }
}
