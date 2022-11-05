using System.Reflection;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public abstract class Node
    {
        public abstract TokType Type { get; }

        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }
        
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
                else if (typeof(IEnumerable<Node>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<Node>)property.GetValue(this);
                    foreach (var child in children) yield return child;
                }
            }
        }

        public void WriteTo(TextWriter writer)
        {
            LogNode(writer, this);
        }
        private static void LogNode(TextWriter writer, Node node, string indent = "", bool last = false)
        {
            var marker = last ? "└──" : "├──";

            writer.Write(indent);
            writer.Write(marker);
            writer.Write(node.Type);

            if (node is Token t && t.Value is not null)
            {
                writer.Write(" ");
                writer.Write(t.Value);   
            }
            
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