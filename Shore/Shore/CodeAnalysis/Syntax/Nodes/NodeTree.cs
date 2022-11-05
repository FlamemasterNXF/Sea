using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class NodeTree
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ExpressionNode Root { get; }
        public Token EndOfFileToken { get; }

        public NodeTree(ImmutableArray<Diagnostic> diagnostics, ExpressionNode root, Token endOfFileToken)
        {
            Diagnostics = diagnostics;
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public static NodeTree Parse(string text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }

        public static IEnumerable<Token> ParseTokens(string text)
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