using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.misc
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
            
            return !(nodeTree.Root.Statement.GetLastToken().IsMissing);
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
            else
            {
                foreach (var diagnostic in result.Diagnostics)
                {
                    var lineIndex = nodeTree.Text.GetLineIndex(diagnostic.Span.Start);
                    var line = nodeTree.Text.Lines[lineIndex];
                    var lineNumber = lineIndex + 1;
                    var character = diagnostic.Span.Start - line.Start + 1;

                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"({lineNumber}, {character}): ");
                    Console.WriteLine(diagnostic);
                    Console.ResetColor();

                    var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                    var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                    var prefix = nodeTree.Text.ToString(prefixSpan);
                    var error = nodeTree.Text.ToString(diagnostic.Span);
                    var suffix = nodeTree.Text.ToString(suffixSpan);

                    Console.Write("    ");
                    Console.Write(prefix);

                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(error);
                    Console.ResetColor();

                    Console.Write(suffix);
                    Console.WriteLine();
                }

                Console.WriteLine();
            }
        }
    }
}