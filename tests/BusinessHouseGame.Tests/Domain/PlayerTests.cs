using FortuneCircuit.Domain;
using Xunit;

namespace FortuneCircuit.Tests.Domain;

public class PlayerTests
{
    [Fact]
    public void Debit_AllowsNegativeBalance()
    {
        // Documented behavior: bankruptcy is allowed; the engine logs a warning.
        var p = new Player(1, "A", 100);
        p.Debit(500);
        Assert.Equal(-400, p.Money);
    }
}
