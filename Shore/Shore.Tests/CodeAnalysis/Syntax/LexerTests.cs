using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;
using Xunit;

namespace Shore.Tests.CodeAnalysis.Syntax
{
    public class LexerTest
    {
        [Fact]
        public void Lexer_Tests_AllTokens()
        {
            var tokenTypes = Enum.GetValues(typeof(TokType)).Cast<TokType>()
                .Where(t => t != TokType.SingleLineCommentToken && t != TokType.MultiLineCommentToken)
                .Where(t => t.ToString().EndsWith("Keyword") || t.ToString().EndsWith("Token"));
            var testedTokenTypes = GetTokens().Concat(GetSeparators()).Select(t => t.type);
            var untestedTokenTypes = new SortedSet<TokType>(tokenTypes);
            untestedTokenTypes.Remove(TokType.UnknownToken);
            untestedTokenTypes.Remove(TokType.EndOfFileToken);
            untestedTokenTypes.ExceptWith(testedTokenTypes);
            
            Assert.Empty(untestedTokenTypes);
        }

        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            var text = "\"text";
            var tokens = NodeTree.ParseTokens(text, out var diagnostics);
            var token = Assert.Single(tokens);
            Assert.Equal(TokType.StringToken, token.Type);
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal(new TextSpan(0, 1), diagnostic.Location.Span);
            Assert.Equal("Unterminated String.", diagnostic.Message);
        }

        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(TokType type, string text)
        {
            var tokens = NodeTree.ParseTokens(text);
            var token = Assert.Single(tokens);
            Assert.Equal(type, token.Type);
            Assert.Equal(text, token.Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(TokType type1, string text1, TokType type2, string text2)
        {
            var text = text1 + text2;
            var tokens = NodeTree.ParseTokens(text).ToArray();
            
            Assert.Equal(2, tokens.Length);
            Assert.Equal(type1, tokens[0].Type);
            Assert.Equal(text1, tokens[0].Text);
            Assert.Equal(type2, tokens[1].Type);
            Assert.Equal(text2, tokens[1].Text);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorsData))]
        public void Lexer_Lexes_TokenParis_WithSeparators(TokType type1, string text1, TokType separatorType,
            string separatorText, TokType type2, string text2)
        {
            var text = text1 + separatorText + text2;
            var tokens = NodeTree.ParseTokens(text).ToArray();
            
            Assert.Equal(3, tokens.Length);
            Assert.Equal(type1, tokens[0].Type);
            Assert.Equal(text1, tokens[0].Text);
            Assert.Equal(separatorType, tokens[1].Type);
            Assert.Equal(separatorText, tokens[1].Text);
            Assert.Equal(type2, tokens[2].Type);
            Assert.Equal(text2, tokens[2].Text);
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var tok in GetTokens().Concat(GetSeparators()))
            {
                yield return new object[] { tok.type, tok.text };
            }
        }
        
        public static IEnumerable<object[]> GetTokenPairsData()
        {
            return GetTokenPairs().Select(t => new object[] { t.type1, t.text1, t.type2, t.text2 });
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorsData()
        {
            return GetTokenPairsWithSeparators().Select(t => new object[]
                { t.t1Kind, t.t1Text, t.separatorKind, t.separatorText, t.t2Kind, t.t2Text });
        }

        private static IEnumerable<(TokType type, string text)> GetTokens()
        {
            var fixedTokens = Enum.GetValues(typeof(TokType)).Cast<TokType>()
                .Select(t => (type: t, text: SyntaxFacts.GetText(t))).Where(t => t.text != null);
            var dynamicTokens = new[]
            {
                (TokType.NumberToken, "1"),
                (TokType.NumberToken, "123"),
                (TokType.IdentifierToken, "a"),
                (TokType.IdentifierToken, "abc"),
                (TokType.StringToken, "\"Test\""),
                (TokType.StringToken, "\"Te\"\"st\""),
            };

            return fixedTokens.Concat(dynamicTokens);
        }

        private static IEnumerable<(TokType type, string text)> GetSeparators()
        {
            return new[]
            {
                (TokType.WhitespaceToken, " "),
                (TokType.WhitespaceToken, "  "),
                (TokType.WhitespaceToken, "\r"),
                (TokType.WhitespaceToken, "\n"),
                (TokType.WhitespaceToken, "\r\n"),
                (TokType.MultiLineCommentToken, "/**/"),
            };
        }

        private static bool RequiresSeparator(TokType type1, TokType type2)
        {
            var oneIsKeyword = type1.ToString().EndsWith("Keyword");
            var twoIsKeyword = type2.ToString().EndsWith("Keyword");

            if (type1 is TokType.IdentifierToken && type2 is TokType.IdentifierToken) return true;
            if (type1 is TokType.NumberToken || type2 is TokType.NumberToken) return true;
            
            switch (oneIsKeyword)
            {
                case true when twoIsKeyword:
                case true when type2 is TokType.IdentifierToken:
                    return true;
            }

            switch (type1)
            {
                case TokType.LeftShiftToken when type2 is TokType.LessThanToken:
                case TokType.RightShiftToken when type2 is TokType.GreaterThanToken:
                case TokType.LessThanToken when type2 is TokType.LeftShiftToken:
                case TokType.GreaterThanToken when type2 is TokType.RightShiftToken:
                case TokType.GreaterThanToken when type2 is TokType.GreaterThanToken:
                case TokType.GreaterThanToken when type2 is TokType.GreaterThanOrEqualToken:
                case TokType.LessThanToken when type2 is TokType.LessThanOrEqualToken:
                case TokType.LessThanToken when type2 is TokType.LessThanToken:
                case TokType.AmpersandToken when type2 is TokType.AmpersandToken:
                case TokType.AmpersandToken when type2 is TokType.DoubleAmpersandToken:
                case TokType.PipeToken when type2 is TokType.PipeToken:
                case TokType.PipeToken when type2 is TokType.DoublePipeToken:
                case TokType.SlashToken when type2 is TokType.SlashToken:
                case TokType.SlashToken when type2 is TokType.StarToken:
                case TokType.SlashToken when type2 is TokType.SingleLineCommentToken:
                case TokType.SlashToken when type2 is TokType.MultiLineCommentToken:
                case TokType.SlashToken when type2 is TokType.DoubleStarToken:
                case TokType.GreaterThanToken when type2 is TokType.EqualsToken:
                case TokType.GreaterThanToken when type2 is TokType.DoubleEqualsToken:
                case TokType.LessThanToken when type2 is TokType.EqualsToken:
                case TokType.LessThanToken when type2 is TokType.DoubleEqualsToken:
                case TokType.IdentifierToken when twoIsKeyword:
                case TokType.BangToken when type2 is TokType.EqualsToken:
                case TokType.BangToken when type2 is TokType.DoubleEqualsToken:
                case TokType.EqualsToken when type2 is TokType.EqualsToken:
                case TokType.EqualsToken when type2 is TokType.DoubleEqualsToken:
                case TokType.StarToken when type2 is TokType.DoubleStarToken:
                case TokType.StarToken when type2 is TokType.StarToken:
                case TokType.NumberToken when type2 is TokType.NumberToken:
                case TokType.StringToken when type2 is TokType.StringToken:
                    return true;
                default:
                    return false;
            }
        }

        private static IEnumerable<(TokType type1, string text1, TokType type2, string text2)> GetTokenPairs()
        {
            return from t1 in GetTokens() from t2 in GetTokens() where !RequiresSeparator(t1.type, t2.type) select (t1.type, t1.text, t2.type, t2.text);
        }

        private static IEnumerable<(TokType t1Kind, string t1Text, TokType separatorKind, string separatorText, 
            TokType t2Kind, string t2Text)> GetTokenPairsWithSeparators()
        {
            return from t1 in GetTokens()
                from t2 in GetTokens()
                where RequiresSeparator(t1.type, t2.type)
                from s in GetSeparators()
                where !RequiresSeparator(t1.type, s.type) && !RequiresSeparator(s.type, t2.type)
                select (t1.type, t1.text, s.type, s.text, t2.type, t2.text);
        }
    }
}