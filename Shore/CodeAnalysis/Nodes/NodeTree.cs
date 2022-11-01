using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Nodes
{
    public sealed class NodeTree
    {
        public IReadOnlyList<string> Diagnostics { get; }
        public ExpressionNode Root { get; }
        public Token EndOfFileToken { get; }

        public NodeTree(IEnumerable<string> diagnostics, ExpressionNode root, Token endOfFileToken)
        {
            Diagnostics = diagnostics.ToArray();
            Root = root;
            EndOfFileToken = endOfFileToken;
        }

        public static NodeTree Parse(string text)
        {
            var parser = new Parser(text);
            return parser.Parse();
        }
    }
}