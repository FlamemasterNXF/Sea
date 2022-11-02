using Shore.CodeAnalysis;
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

                var tree = NodeTree.Parse(line);

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    LogNode(tree.Root);
                    Console.ResetColor();
                }

                if (tree.Diagnostics.Any())
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    foreach (var diagnostic in tree.Diagnostics) Console.WriteLine(diagnostic);
                    Console.ResetColor();
                }
                else
                {
                    var e = new Evaluator(tree.Root);
                    var result = e.Evaluate();
                    Console.WriteLine(result);
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

