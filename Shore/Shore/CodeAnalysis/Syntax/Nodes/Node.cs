using System.Reflection;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public abstract class Node
    {
        public NodeTree NodeTree { get; }
        public abstract TokType Type { get; }

        
        protected Node(NodeTree nodeTree)
        {
            NodeTree = nodeTree;
        }

        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }

        public TextLocation Location => new (NodeTree.Text, Span);
        
        public IEnumerable<Node> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (typeof(Node).IsAssignableFrom(property.PropertyType))
                {
                    var child = (Node)property.GetValue(this);
                    yield return child;
                }
                else if (typeof(SeparatedNodeList).IsAssignableFrom(property.PropertyType))
                {
                    var separatedNodeList = (SeparatedNodeList)property.GetValue(this);
                    foreach (var child in separatedNodeList.GetWithSeparators()) yield return child;
                }
                else if (typeof(IEnumerable<Node>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<Node>)property.GetValue(this);
                    foreach (var child in children) yield return child;
                }
            }
        }
        
        public Token GetLastToken()
        {
            if (this is Token token) return token;

            return GetChildren().LastOrDefault()?.GetChildren() == null
                ? new Token(NodeTree, TokType.NullToken, 1, "@", null)
                : GetChildren().LastOrDefault().GetLastToken();
        }

        public void WriteTo(TextWriter writer)
        {
            LogNode(writer, this);
        }
        
        private static void LogNode(TextWriter writer, Node node, string indent = "", bool last = false)
        {
            var isToConsole = writer == Console.Out;
            var marker = last ? "└──" : "├──";

            if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;
            
            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole) Console.ForegroundColor = node is Token ? ConsoleColor.Blue : ConsoleColor.Cyan;
            writer.Write(node.Type);

            if (node is Token t && t.Value is not null)
            {
                writer.Write(" ");
                writer.Write(t.Value);   
            }
            
            if (isToConsole) Console.ResetColor();

            writer.WriteLine();
            indent += last ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                LogNode(writer, child, indent, child == lastChild);
            }
        }

        public override string ToString()
        {
            using var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}