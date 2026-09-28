using FortuneCircuit.Domain;
using FortuneCircuit.Domain.Cells;
using FortuneCircuit.Infrastructure;

namespace FortuneCircuit.Application;

/// <summary>
/// Pure function that computes a player's total worth = cash + (owned-hotel-count × HotelPrice).
/// Extracted so worth logic can be reused (rankings, UI, end-of-game summaries) and tested
/// in isolation from any output concern.
/// </summary>
public sealed class WorthCalculator
{
    private readonly GameConfig _config;

    public WorthCalculator(GameConfig config)
        => _config = config ?? throw new ArgumentNullException(nameof(config));

    public int Compute(Player player, Board board)
    {
        if (player is null) throw new ArgumentNullException(nameof(player));
        if (board is null) throw new ArgumentNullException(nameof(board));

        int hotels = board.Cells.OfType<HotelCell>().Count(h => h.OwnerId == player.Id);
        return player.Money + hotels * _config.HotelPrice;
    }
}
