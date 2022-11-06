using System.Reflection;

namespace Shore.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

        public IEnumerable<BoundNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = (BoundNode)property.GetValue(this);
                    if (child is not null) yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    IEnumerable<BoundNode>? children = (IEnumerable<BoundNode>?)property.GetValue(this);
                    foreach (BoundNode? child in children) if (child is not null) yield return child;
                }
            }
        }

        private IEnumerable<(string Name, object Value)> GetProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.Name == nameof(Kind) || property.Name == nameof(BoundBinaryExpression.Op)) continue;

                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType)) continue;

                var value = property.GetValue(this);
                if (value is not null) yield return (property.Name, value);
            }
        }

        public void WriteTo(TextWriter writer) => LogNode(writer, this);

        private static void LogNode(TextWriter writer, BoundNode node, string indent = "", bool isLast = false)
        {
            var isToConsole = writer == Console.Out;
            var marker = isLast ? "└──" : "├──";

            if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;
            
            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole) Console.ForegroundColor = GetColor(node);

            var text = GetText(node);
            writer.Write(text);

            var isFirstProperty = true;

            foreach (var property in node.GetProperties())
            {
                if (isFirstProperty) isFirstProperty = false;
                else
                {
                    if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkGray;
                    writer.Write(",");
                }
                
                writer.Write(" ");

                if (isToConsole) Console.ForegroundColor = ConsoleColor.Yellow;
                
                writer.Write(property.Name);

                if (isToConsole) Console.ForegroundColor = ConsoleColor.DarkYellow;
                
                writer.Write(property.Value);
            }
            
            if (isToConsole) Console.ResetColor();

            writer.WriteLine();

            indent += isLast ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
                LogNode(writer, child, indent, child == lastChild);
        }

        private static string GetText(BoundNode node)
        {
            return node switch
            {
                BoundBinaryExpression b => b.Op.Kind.ToString() + "Expression",
                BoundUnaryExpression u => u.Op.Kind.ToString() + "Expression",
                _ => node.Kind.ToString()
            };
        }

        private static ConsoleColor GetColor(BoundNode node)
        {
            return node switch
            {
                BoundExpression => ConsoleColor.Blue,
                BoundStatement => ConsoleColor.Cyan,
                _ => ConsoleColor.Yellow
            };
        }

        public override string ToString()
        {
            using var writer = new StringWriter();
            WriteTo(writer);
            return writer.ToString();
        }
    }
}