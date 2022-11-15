using System.Collections.Immutable;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class NodeTree
    {
        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationUnitNode Root { get; }

        private delegate void ParseHandler(NodeTree tree, out CompilationUnitNode? root,
            out ImmutableArray<Diagnostic> diagnostics);

        private NodeTree(SourceText text, ParseHandler handler)
        {
            Text = text;

            handler(this, out var root, out var diagnostics);
            
            Diagnostics = diagnostics;
            Root = root;
        }

        public static NodeTree Load(string fileName)
        {
            var text = File.ReadAllText(fileName);
            var sourceText = SourceText.From(text, fileName);
            return Parse(sourceText);
        }

        private static void Parse(NodeTree nodeTree, out CompilationUnitNode root,
            out ImmutableArray<Diagnostic> diagnostics)
        {
            var parser = new Parser(nodeTree);
            root = parser.ParseCompilationUnit();
            diagnostics = parser.Diagnostics.ToImmutableArray();
        }

        public static NodeTree Parse(string text)
        {
            var sourceText = SourceText.From(text);
            return Parse(sourceText);
        }

        private static NodeTree Parse(SourceText text) => new NodeTree(text, Parse);

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
            var tokens = new List<Token>();

            void ParseTokens(NodeTree tree, out CompilationUnitNode? root, out ImmutableArray<Diagnostic> diagnostics)
            {
                root = null;

                var lexer = new Lexer(tree);
                while (true)
                {
                    var token = lexer.Lex();
                    if (token.Type == TokType.EndOfFileToken)
                    {
                        root = new CompilationUnitNode(tree, ImmutableArray<MemberNode>.Empty, token);
                        break;
                    }
                    
                    tokens.Add(token);
                }

                diagnostics = lexer.Diagnostics.ToImmutableArray();
            }

            var nodeTree = new NodeTree(text, ParseTokens);
            diagnostics = nodeTree.Diagnostics.ToImmutableArray();
            return tokens.ToImmutableArray();
        }
    }
}