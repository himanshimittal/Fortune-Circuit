namespace FortuneCircuit.Infrastructure.Dice;

public interface IDiceProvider
{
    bool HasNext();
    int Next();
}
