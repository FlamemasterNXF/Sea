using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Xunit;

namespace Shore.Tests.CodeAnalysis.Syntax
{
    public class LexerTest
    {
        [Fact]
        public void Lexer_Tests_AllTokens()
        {
            var tokenTypes = Enum.GetValues(typeof(TokType)).Cast<TokType>()
                .Where(t => t.ToString().EndsWith("Keyword") || t.ToString().EndsWith("Token"));
            var testedTokenTypes = GetTokens().Concat(GetSeparators()).Select(t => t.type);
            var untestedTokenTypes = new SortedSet<TokType>(tokenTypes);
            untestedTokenTypes.Remove(TokType.UnknownToken);
            untestedTokenTypes.Remove(TokType.EndOfFileToken);
            untestedTokenTypes.ExceptWith(testedTokenTypes);
            
            Assert.Empty(untestedTokenTypes);
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
            Assert.Equal(tokens[0].Type, type1);
            Assert.Equal(tokens[0].Text, text1);
            Assert.Equal(tokens[1].Type, type2);
            Assert.Equal(tokens[1].Text, text2);
        }

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeperatorData))]
        public void Lexer_Lexes_TokenParis_WithSeparators(TokType type1, string text1, TokType separatorType,
            string separatorText, TokType type2, string text2)
        {
            var text = text1 + separatorText + text2;
            var tokens = NodeTree.ParseTokens(text).ToArray();
            
            Assert.Equal(3, tokens.Length);
            Assert.Equal(tokens[0].Type, type1);
            Assert.Equal(tokens[0].Text, text1);
            Assert.Equal(tokens[1].Type, separatorType);
            Assert.Equal(tokens[1].Text, separatorText);
            Assert.Equal(tokens[2].Type, type2);
            Assert.Equal(tokens[2].Text, text2);
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
            foreach (var tokPair in GetTokenPairs())
            {
                yield return new object[] { tokPair.type1, tokPair.text1, tokPair.type2, tokPair.text2 };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeperatorData()
        {
            foreach (var tokPair in GetTokenPairsWithSeparators())
            {
                yield return new object[]
                {
                    tokPair.type1, tokPair.text1, tokPair.separatorType, tokPair.separatorText, tokPair.type2,
                    tokPair.text2
                };
            }
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
            };
        }

        private static bool RequiresSeparator(TokType type1, TokType type2)
        {
            var oneIsKeyword = type1.ToString().EndsWith("Keyword");
            var twoIsKeyword = type2.ToString().EndsWith("Keyword");

            if (type1 == TokType.IdentifierToken && type2 == TokType.IdentifierToken) return true;
            
            switch (oneIsKeyword)
            {
                case true when twoIsKeyword:
                case true when type2 == TokType.IdentifierToken:
                    return true;
            }

            switch (type1)
            {
                case TokType.GreaterThanToken when type2 == TokType.EqualsToken:
                case TokType.GreaterThanToken when type2 == TokType.DoubleEqualsToken:
                case TokType.LessThanToken when type2 == TokType.EqualsToken:
                case TokType.LessThanToken when type2 == TokType.DoubleEqualsToken:
                    return true;
                default:
                    return type1 switch
                    {
                        TokType.IdentifierToken when twoIsKeyword => true,
                        TokType.NumberToken when type2 == TokType.NumberToken => true,
                        TokType.BangToken when type2 == TokType.EqualsToken => true,
                        TokType.BangToken when type2 == TokType.DoubleEqualsToken => true,
                        TokType.EqualsToken when type2 == TokType.EqualsToken => true,
                        TokType.EqualsToken when type2 == TokType.DoubleEqualsToken => true,
                        _ => false
                    };
            }
        }

        private static IEnumerable<(TokType type1, string text1, TokType type2, string text2)> GetTokenPairs()
        {
            return from t1 in GetTokens() from t2 in GetTokens() where !RequiresSeparator(t1.type, t2.type) select (t1.type, t1.text, t2.type, t2.text);
        }

        private static IEnumerable<(TokType type1, string text1, TokType separatorType, string separatorText, TokType type2, string text2)> GetTokenPairsWithSeparators()
        {
            return from t1 in GetTokens() from t2 in GetTokens() where RequiresSeparator(t1.type, t2.type) from s in GetSeparators() select (t1.type, t1.text, s.type, s.text, t2.type, t2.text);
        }
    }
}