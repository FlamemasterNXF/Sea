using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Xunit;

namespace Shore.Tests.CodeAnalysis.Syntax
{
    public class SyntaxFactsTests
    {
        [Theory]
        [MemberData(nameof(GetSyntaxKindData))]
        public void SyntaxFact_GetsText(TokType type)
        {
            var text = SyntaxFacts.GetText(type);
            if (text == null) return;

            var tokens = NodeTree.ParseTokens(text);
            var token = Assert.Single(tokens);
            Assert.Equal(type, token.Type);
            Assert.Equal(text, token.Text);
        }

        public static IEnumerable<object[]> GetSyntaxKindData()
        {
            var types = (TokType[])Enum.GetValues(typeof(TokType));
            foreach (var type in types) yield return new object[] { type };
        }
    }
}