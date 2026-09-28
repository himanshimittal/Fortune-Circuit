using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Domain.Cells;

public sealed class JailCell : Cell
{
    public int Fine { get; }

    public JailCell(int index, int fine) : base(index)
    {
        if (fine < 0) throw new ArgumentOutOfRangeException(nameof(fine), fine, "must be >= 0");
        Fine = fine;
    }

    public override char Symbol => 'J';
    public override CellEffect ApplyEffect(Player player) => new CellEffect.PayFine(Fine);
}
