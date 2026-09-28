using FortuneCircuit.Infrastructure.Dice;
using Xunit;

namespace FortuneCircuit.Tests.Infrastructure.Dice;

public class RandomDiceProviderTests
{
    [Fact]
    public void Next_InTwoDiceRange()
    {
        // Property: 2d6 always lands in [2..12].
        var d = new RandomDiceProvider(50);
        while (d.HasNext())
            Assert.InRange(d.Next(), 2, 12);
    }
}
