using FortuneCircuit.Application;
using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Domain.Effects;
using FortuneCircuit.Infrastructure.Logging;
using Xunit;

namespace FortuneCircuit.Tests.Application;

public class EffectProcessorTests
{
    private static readonly GameConfig Cfg = GameConfig.Standard();

    private static EffectProcessor MakeProcessor(out Player p1, out Player p2, ILogger? logger = null)
    {
        p1 = new Player(1, "P1", 1000);
        p2 = new Player(2, "P2", 1000);
        var dict = new[] { p1, p2 }.ToDictionary(p => p.Id);
        return new EffectProcessor(dict, logger ?? new NullLogger());
    }

    private static HotelCell NewHotel(int index = 0) => new(index, Cfg.HotelPrice, Cfg.HotelRent);
    private static JailCell NewJail(int index = 0) => new(index, Cfg.JailFine);
    private static TreasureCell NewTreasure(int index = 0) => new(index, Cfg.TreasureReward);

    [Fact]
    public void Apply_PayFine_DebitsPlayer()
    {
        var proc = MakeProcessor(out var p1, out _);
        var (action, _2) = proc.Apply(new CellEffect.PayFine(Cfg.JailFine), p1, NewJail());

        Assert.Equal(TurnAction.PaidJailFine, action);
        Assert.Equal(1000 - Cfg.JailFine, p1.Money);
    }

    [Fact]
    public void Apply_CollectTreasure_CreditsPlayer()
    {
        var proc = MakeProcessor(out var p1, out _);
        var (action, _2) = proc.Apply(new CellEffect.CollectTreasure(Cfg.TreasureReward), p1, NewTreasure());

        Assert.Equal(TurnAction.CollectedTreasure, action);
        Assert.Equal(1000 + Cfg.TreasureReward, p1.Money);
    }

    [Fact]
    public void Apply_BuyHotel_DebitsAndAssignsOwner()
    {
        var proc = MakeProcessor(out var p1, out _);
        var hotel = NewHotel();

        var (action, _2) = proc.Apply(new CellEffect.BuyHotel(Cfg.HotelPrice), p1, hotel);

        Assert.Equal(TurnAction.BoughtHotel, action);
        Assert.Equal(1000 - Cfg.HotelPrice, p1.Money);
        Assert.Equal(p1.Id, hotel.OwnerId);
    }

    [Fact]
    public void Apply_PayRent_TransfersToOwner()
    {
        var proc = MakeProcessor(out var p1, out var p2);

        var (action, owner) = proc.Apply(new CellEffect.PayRent(2, Cfg.HotelRent), p1, NewHotel());

        Assert.Equal(TurnAction.PaidRent, action);
        Assert.Equal(2, owner);
        Assert.Equal(1000 - Cfg.HotelRent, p1.Money);
        Assert.Equal(1000 + Cfg.HotelRent, p2.Money);
    }

    [Fact]
    public void Apply_PayRent_UnknownOwner_Throws()
    {
        var proc = MakeProcessor(out var p1, out _);

        Assert.Throws<InvalidOperationException>(() =>
            proc.Apply(new CellEffect.PayRent(999, Cfg.HotelRent), p1, NewHotel()));
    }

    [Fact]
    public void Apply_NegativeBalance_LogsWarning()
    {
        var log = new RecordingLogger();
        var p1 = new Player(1, "P1", 10);
        var proc = new EffectProcessor(new[] { p1 }.ToDictionary(p => p.Id), log);

        proc.Apply(new CellEffect.PayFine(Cfg.JailFine), p1, NewJail());

        Assert.True(p1.Money < 0);
        Assert.Contains(log.Records, r => r.Level == LogLevel.Warn && r.Message == "NegativeBalance");
    }

    [Fact]
    public void Apply_BuyHotel_InsufficientFunds_AllowsNegativeAndLogs()
    {
        var log = new RecordingLogger();
        var p1 = new Player(1, "P1", 50);
        var proc = new EffectProcessor(new[] { p1 }.ToDictionary(p => p.Id), log);
        var hotel = NewHotel();

        var (action, _2) = proc.Apply(new CellEffect.BuyHotel(Cfg.HotelPrice), p1, hotel);

        Assert.Equal(TurnAction.BoughtHotel, action);
        Assert.Equal(50 - Cfg.HotelPrice, p1.Money);
        Assert.Equal(p1.Id, hotel.OwnerId);
        Assert.Contains(log.Records, r => r.Level == LogLevel.Warn && r.Message == "NegativeBalance");
    }

    private sealed class RecordingLogger : ILogger
    {
        public List<(LogLevel Level, string Message, object? Data)> Records { get; } = new();
        public void Log(LogLevel level, string message, object? data = null)
            => Records.Add((level, message, data));
    }
}

