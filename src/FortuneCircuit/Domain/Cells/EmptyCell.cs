using FortuneCircuit.Domain.Effects;

namespace FortuneCircuit.Domain.Cells;

public sealed class EmptyCell : Cell
{
    public EmptyCell(int index) : base(index) { }
    public override char Symbol => 'E';
    public override CellEffect ApplyEffect(Player player) => new CellEffect.None();
}
