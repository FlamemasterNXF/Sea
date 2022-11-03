using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            bool showTree = false;

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) return;

                if (line == "#SHOWTREE")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing Node Trees" : "Hiding Node Trees");
                    continue;
                }

                var nodeTree = NodeTree.Parse(line);
                var binder = new Binder();
                var boundTree = binder.BindExpression(nodeTree.Root);

                IReadOnlyList<string> diagnostics = nodeTree.Diagnostics.Concat(binder.Diagnostics).ToArray();

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    LogNode(nodeTree.Root);
                    Console.ResetColor();
                }

                if (!diagnostics.Any())
                {
                    var e = new Evaluator(boundTree);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in diagnostics) Console.WriteLine(diagnostic);
                    Console.ResetColor();
                }
            }

            // CONSOLE WINDOW CONTROL
            //Console.WriteLine("Press C to close this window :)");
            //try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            //catch (Exception){ Console.WriteLine($"Console Window not found!"); }
        }

        static void LogNode(Node node, string indent = "", bool last = false)
        {
            var marker = last ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Type);

            if (node is Token t && t.Value is not null)
            {
                Console.Write(" ");
                Console.Write(t.Value);   
            }
            
            Console.WriteLine();
            indent += last ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();
            foreach (var child in node.GetChildren())
            {
                LogNode(child, indent, child == lastChild);
            }
        }
    }
}

