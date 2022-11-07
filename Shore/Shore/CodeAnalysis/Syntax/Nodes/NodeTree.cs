using System.Collections.Immutable;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class NodeTree
    {
        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationUnitNode Root { get; }

        private NodeTree(SourceText text)
        {
            var parser = new Parser(text);
            var root = parser.ParseCompilationUnit();
            
            Text = text;
            Diagnostics = parser.Diagnostics.ToImmutableArray();
            Root = root;
        }

        public static NodeTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        private static NodeTree Parse(SourceText text) => new NodeTree(text);

        public static ImmutableArray<Token> ParseTokens(string text)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText);
        }

        public static ImmutableArray<Token> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
        {
            var sourceText = SourceText.From(text);
            return ParseTokens(sourceText, out diagnostics);
        }

        public static ImmutableArray<Token> ParseTokens(SourceText text) => ParseTokens(text, out _);

        public static ImmutableArray<Token> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics)
        {
            IEnumerable<Token> LexTokens(Lexer lexer)
            {
                while (true)
                {
                    var token = lexer.Lex();
                    if (token.Type == TokType.EndOfFileToken) break;
                    yield return token;
                }
            }

            var lexer = new Lexer(text);
            var result = LexTokens(lexer).ToImmutableArray();
            diagnostics = lexer.Diagnostics.ToImmutableArray();
            return result;
        }
    }
}