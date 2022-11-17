using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.IO;

namespace sc
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Invalid Script Passed to MC: <path>");
                return 1;
            }

            var paths = GetFilePaths(args);
            var nodeTrees = new List<NodeTree>();
            var hasErrors = false;

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine($"fatal: File '{(string)path}' doesn't exist");
                    hasErrors = true;
                    continue;
                }

                var nodeTree = NodeTree.Load(path);
                nodeTrees.Add(nodeTree);
            }

            if (hasErrors) return 1;
            
            var compilation = Compilation.Create(nodeTrees.ToArray());
            var result = compilation.Evaluate(new Dictionary<VariableSymbol?, object?>());

            if (!result.Diagnostics.Any() && result.Value is not null) Console.WriteLine(result.Value);
            else
            {
                Console.Error.WriteDiagnostics(result.Diagnostics);
                return 1;
            }

            return 0;
        }

        private static IEnumerable<string> GetFilePaths(IEnumerable<string> paths)
        {
            var result = new SortedSet<string>();

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                    result.UnionWith(Directory.EnumerateFiles(path, "*.sea", SearchOption.AllDirectories));
                else result.Add(path);
            }

            return result;
        }
    }
}