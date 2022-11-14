using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.IO;

namespace sc
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Invalid Script Passed to MC: <path>");
                return;
            }

            if (args.Length > 1)
            {
                Console.WriteLine("Multiple File Paths are Unsupported");
                return;
            }

            var path = args.Single();
            if (path == String.Empty)
            {
                Console.Error.WriteLine("Invalid Script Passed to MC: <path>");
                return;
            }
            
            var text = File.ReadAllText(path);
            var nodeTree = NodeTree.Parse(text);
            var compilation = new Compilation(nodeTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            if (!result.Diagnostics.Any() && result.Value is not null) Console.WriteLine(result.Value);
            else Console.Error.WriteDiagnostics(result.Diagnostics, nodeTree);
        }
    }
}