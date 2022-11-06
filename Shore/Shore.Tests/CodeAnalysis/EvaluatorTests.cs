using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Syntax.Nodes;
using Xunit;

namespace Shore.Tests.CodeAnalysis
{
    public class EvaluatorTests
    {
        [Theory]
        [InlineData("1", 1)]
        [InlineData("+1", 1)]
        [InlineData("-1", -1)]
        [InlineData("(1)", 1)]
        [InlineData("13 + 14", 27)]
        [InlineData("96 - 3", 93)]
        [InlineData("9 * 10", 90)]
        [InlineData("18 / 9", 2)]
        [InlineData("(1 + 2) * 3", 9)]
        [InlineData("1 == 2", false)]
        [InlineData("3 == 3", true)]
        [InlineData("12 != 4", true)]
        [InlineData("13 != 13", false)]
        [InlineData("false == true", false)]
        [InlineData("false == false", true)]
        [InlineData("true == true", true)]
        [InlineData("true != true", false)]
        [InlineData("true != false", true)]
        [InlineData("false != false", false)]
        [InlineData("false", false)]
        [InlineData("true", true)]
        [InlineData("!true", false)]
        [InlineData("!false", true)]
        [InlineData("let a = 10", 10)]
        [InlineData("{ let b = 0 (b = 10) * b}", 100)]
        public void EvaluatorTests_RoundTrips(string text, object expectedValue)
        {
            var nodeTree = NodeTree.Parse(text);
            var compilation = new Compilation(nodeTree);
            var variables = new Dictionary<VariableSymbol, object>();
            var result = compilation.Evaluate(variables);
            
            Assert.Empty(result.Diagnostics);
            Assert.Equal(expectedValue, result.Value);
        }
    }
}