using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Xunit;

namespace Shore.Tests.CodeAnalysis.Syntax
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsPrecedences(TokType op1, TokType op2)
        {
            var op1Precedence = op1.GetBinaryOperatorPrecedence();
            var op2Precedence = op2.GetBinaryOperatorPrecedence();
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = NodeTree.Parse(text).Root;

            if (op1Precedence >= op2Precedence)
            {
                using var e = new AssertingEnumerator(expression);
                e.AssertNode(TokType.BinaryExpression);
                e.AssertNode(TokType.BinaryExpression);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "a");
                e.AssertToken(op1, op1Text);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "b");
                e.AssertToken(op2, op2Text);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "c");
            }
            else
            {
                using var e = new AssertingEnumerator(expression);
                e.AssertNode(TokType.BinaryExpression);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "a");
                e.AssertToken(op1, op1Text);
                e.AssertNode(TokType.BinaryExpression);
                e.AssertNode(TokType.NameExpression);                    
                e.AssertToken(TokType.IdentifierToken, "b");
                e.AssertToken(op2, op2Text);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "c");
            }
        }

        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsPrecedences(TokType unaryType, TokType binaryType)
        {
            var unaryPrecedence = unaryType.GetUnaryOperatorPrecedence();
            var binaryPrecedence = binaryType.GetBinaryOperatorPrecedence();
            var unaryText = SyntaxFacts.GetText(unaryType);
            var binaryText = SyntaxFacts.GetText(binaryType);
            var text = $"{unaryText} a {binaryText} b";
            var expression = NodeTree.Parse(text).Root;

            if (unaryPrecedence >= binaryPrecedence)
            {
                using var e = new AssertingEnumerator(expression);
                e.AssertNode(TokType.BinaryExpression);
                e.AssertNode(TokType.UnaryExpression);
                e.AssertToken(unaryType, unaryText);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "a");
                e.AssertToken(binaryType, binaryText);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "b");
            }
            else
            {
                using var e = new AssertingEnumerator(expression);
                e.AssertNode(TokType.UnaryExpression);
                e.AssertToken(unaryType, unaryText);
                e.AssertNode(TokType.BinaryExpression);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "a");
                e.AssertToken(binaryType, binaryText);
                e.AssertNode(TokType.NameExpression);
                e.AssertToken(TokType.IdentifierToken, "b");
            }
        }
        

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            return from op1 in SyntaxFacts.GetBinaryOperatorTypes() from op2 in SyntaxFacts.GetBinaryOperatorTypes() select new object[] { op1, op2 };
        }

        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            return from u in SyntaxFacts.GetUnaryOperatorTypes() from b in SyntaxFacts.GetBinaryOperatorTypes() select new object[] { u, b };
        }
    }
}