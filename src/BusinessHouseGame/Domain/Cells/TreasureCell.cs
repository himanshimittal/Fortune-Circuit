using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Domain.Cells;

public sealed class TreasureCell : Cell
{
    public int Reward { get; }

    public TreasureCell(int index, int reward) : base(index)
    {
        if (reward < 0) throw new ArgumentOutOfRangeException(nameof(reward), reward, "must be >= 0");
        Reward = reward;
    }

    public override char Symbol => 'T';
    public override CellEffect ApplyEffect(Player player) => new CellEffect.CollectTreasure(Reward);
}
