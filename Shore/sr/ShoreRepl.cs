using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.IO;
using Shore.Text;

namespace Shore
{
    internal sealed class ShoreRepl : Repl
    {
        private Compilation? _previous;
        private bool _showTree;
        private bool _showProgram;
        private readonly Dictionary<VariableSymbol, object> _variables = new Dictionary<VariableSymbol, object>();

        protected override void RenderLine(string line)
        {
            var tokens = NodeTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                var isKeyword = token.Type.ToString().EndsWith("Keyword");
                var isNumber = token.Type == TokType.NumberToken;
                var isString = token.Type == TokType.StringToken;
                var isIdentifier = token.Type == TokType.IdentifierToken;

                if (isKeyword)
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (isIdentifier)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (isNumber)
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if (isString)
                    Console.ForegroundColor = ConsoleColor.Magenta;
                else
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                if (isKeyword) Console.ForegroundColor = ConsoleColor.Blue;
                else if (!isNumber) Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write(token.Text);
                Console.ResetColor();
            }
        }

        protected override void EvaluateCommand(string input)
        {
            switch (input)
            {
                case "#showTree":
                    _showTree = !_showTree;
                    Console.WriteLine(_showTree ? "Showing Node tree." : "Hiding Node tree.");
                    break;
                case "#showProgram":
                    _showProgram = !_showProgram;
                    Console.WriteLine(_showProgram ? "Showing Bound tree." : "Hiding Bound tree.");
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    _previous = null;
                    _variables.Clear();
                    break;
                default:
                    base.EvaluateCommand(input);
                    break;
            }
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            var nodeTree = NodeTree.Parse(text);
            
            return !(nodeTree.Root.Members.Last().GetLastToken().IsMissing);
        }

        protected override void EvaluateSubmission(string text)
        {
            var nodeTree = NodeTree.Parse(text);
            var compilation = _previous == null ? new Compilation(nodeTree) : _previous.ContinueWith(nodeTree);

            if (_showTree) nodeTree.Root.WriteTo(Console.Out);
            if (_showProgram) compilation.EmitTree(Console.Out);

            var result = compilation.Evaluate(_variables);

            if (!result.Diagnostics.Any())
            {
                if (result.Value is not null)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(result.Value);
                    Console.ResetColor();
                }

                _previous = compilation;
            }
            else Console.Out.WriteDiagnostics(result.Diagnostics, nodeTree);
        }
    }
}