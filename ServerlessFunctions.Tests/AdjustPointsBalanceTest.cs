using Xunit;
using Amazon.Lambda.TestUtilities;

namespace ServerlessFunctions.Tests;

public class AdjustPointsBalanceTest
{
    [Fact]
    public void TestToUpperFunction()
    {
        var function = new AdjustPointsBalance();
        var context = new TestLambdaContext();
    }
}
