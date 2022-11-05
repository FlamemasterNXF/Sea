using System.Collections.Immutable;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class NodeTree
    {
        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ExpressionNode Root { get; }
        public Token EndOfFileToken { get; }

        public NodeTree(SourceText text, ImmutableArray<Diagnostic> diagnostics, ExpressionNode root, Token endOfFileToken)
        {
            Text = text;
            Diagnostics = diagnostics;
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public static NodeTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        private static NodeTree Parse(SourceText text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }

        public static IEnumerable<Token> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        private static IEnumerable<Token> ParseTokens(SourceText text)
        {
            var lexer = new Lexer(text);

            while (true)
            {
                var token = lexer.Lex();
                if (token.Type is TokType.EndOfFileToken) break;
                yield return token;
            }
        }
    }
}