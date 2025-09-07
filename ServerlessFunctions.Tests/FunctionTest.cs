using Xunit;
using Amazon.Lambda.TestUtilities;

namespace ServerlessFunctions.Tests;

public class FunctionTest
{
    [Fact]
    public void TestToUpperFunction()
    {
        var function = new Function();
        var context = new TestLambdaContext();
        
        var upperCase = function.FunctionHandler("hello world", context);

        Assert.Equal("HELLO WORLD", upperCase);
    }
}
