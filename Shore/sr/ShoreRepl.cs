using Shore;
using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.IO;

namespace sr
{
    internal sealed class ShoreRepl : Repl
    {
        private static bool _loadingSubmission;
        private static readonly Compilation EmptyCompilation = Compilation.CreateScript(null);
        private Compilation? _previous;
        private bool _showTree;
        private bool _showProgram;
        private readonly Dictionary<VariableSymbol, object?> _variables = new();
        private readonly Dictionary<VariableSymbol, object?[]> _arrays = new();

        public ShoreRepl() => LoadSubmissions();

        protected override void RenderLine(string line)
        {
            var tokens = NodeTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                var isKeyword = token.Type.ToString().EndsWith("Keyword");
                var isNumber = token.Type == TokType.NumberToken;
                var isString = token.Type == TokType.StringToken;
                var isIdentifier = token.Type == TokType.IdentifierToken;
                var isComment = token.Type is TokType.SingleLineCommentToken or TokType.MultiLineCommentToken;

                if (isKeyword) Console.ForegroundColor = ConsoleColor.Blue;
                else if (isIdentifier) Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (isNumber) Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isString) Console.ForegroundColor = ConsoleColor.Magenta;
                else if (isComment) Console.ForegroundColor = ConsoleColor.Green;
                else Console.ForegroundColor = ConsoleColor.DarkGray;

                if (isKeyword) Console.ForegroundColor = ConsoleColor.Blue;
                else if (!isNumber) Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write(token.Text);
                Console.ResetColor();
            }
        }

        [Command("cls", "Clears the Console")]
        private void EvaluateCls() => Console.Clear();

        [Command("reset", "Clears all previous submissions")]
        private void EvaluateReset()
        {
            _previous = null;
            _variables.Clear();
            _arrays.Clear();
            ClearSubmissions();
        }

        [Command("showTree", "Shows the Parse tree")]
        private void EvaluateShowTree()
        {
            _showTree = !_showTree;
            Console.WriteLine(_showTree ? "Showing Parse tree." : "Hiding Parse tree.");
        }

        [Command("showProgram", "Shows the Bound tree")]
        private void EvaluateShowProgram()
        {
            _showProgram = !_showProgram;
            Console.WriteLine(_showProgram ? "Showing Bound tree." : "Hiding Bound tree.");
        }
        
        [Command("load", "Loads a Script")]
        private void EvaluateLoad(string path)
        {
            path = Path.GetFullPath(path);

            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"fatal: '{path}' does not exist.");
                Console.ResetColor();
                return;
            }

            var text = File.ReadAllText(path);
            EvaluateSubmission(text);
        }
        
        [Command("ls", "Lists all Symbols")]
        private void EvaluateLs()
        {
            var compilation = _previous ?? EmptyCompilation;
            var symbols = compilation.GetSymbols().OrderBy(s => s.Kind).ThenBy(s => s.Name);

            foreach (var symbol in symbols)
            {
                symbol.WriteTo(Console.Out);
                Console.WriteLine();
            }
        }
        
        [Command("dump", "Shows Bound Tree of a Function")]
        private void EvaluateDump(string functionName)
        {
            if (_previous == null) return;

            var compilation = _previous ?? EmptyCompilation;
            var symbol = compilation.GetSymbols().OfType<FunctionSymbol>().SingleOrDefault(f => f.Name == functionName);
            
            if (symbol == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"error: Function '{functionName}' does not exist");
                Console.ResetColor();
                return;
            }

            compilation.EmitTree(symbol, Console.Out);
        }
        
        [Command("exit", "Exits the REPL")]
        private void EvaluateExit()
        {
            Environment.Exit(0);
        }
        
        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            var nodeTree = NodeTree.Parse(text);

            var lastMember = nodeTree.Root.Members.LastOrDefault();
            return lastMember != null && !lastMember.GetLastToken().IsMissing;
        }

        protected override void EvaluateSubmission(string text)
        {
            var nodeTree = NodeTree.Parse(text);
            var compilation = Compilation.CreateScript(_previous, nodeTree);

            if (_showTree) nodeTree.Root.WriteTo(Console.Out);
            if (_showProgram) compilation.EmitTree(Console.Out);

            var result = compilation.Evaluate(_variables, _arrays);

            if (!result.Diagnostics.Any())
            {
                if (result.Value is not null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }

                _previous = compilation;
                
                SaveSubmission(text);
            }
            else Console.Out.WriteDiagnostics(result.Diagnostics);
        }
        
        private static string GetSubmissionsDirectory()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var submissionsDirectory = Path.Combine(localAppData, "Minsk", "Submissions");
            return submissionsDirectory;
        }

        private void LoadSubmissions()
        {
            var submissionsDirectory = GetSubmissionsDirectory();
            if (!Directory.Exists(submissionsDirectory)) return;

            var files = Directory.GetFiles(submissionsDirectory).OrderBy(f => f).ToArray();
            if (files.Length == 0) return;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Loaded {files.Length} submission(s)");
            Console.ResetColor();

            _loadingSubmission = true;

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                EvaluateSubmission(text);
            }

            _loadingSubmission = false;
        }

        private static void ClearSubmissions()
        {
            var dir = GetSubmissionsDirectory();
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
        }

        private static void SaveSubmission(string text)
        {
            if (_loadingSubmission) return;

            var submissionsDirectory = GetSubmissionsDirectory();
            Directory.CreateDirectory(submissionsDirectory);
            var count = Directory.GetFiles(submissionsDirectory).Length;
            var name = $"submission{count:0000}";
            var fileName = Path.Combine(submissionsDirectory, name);
            File.WriteAllText(fileName, text);
        }
    }
}