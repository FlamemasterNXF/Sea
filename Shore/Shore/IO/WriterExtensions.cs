using System.CodeDom.Compiler;
using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.IO
{
    public static class WriterExtensions
    {
        private static bool IsConsoleOut(this TextWriter writer)
        {
            if (writer == Console.Out) return true;
            return writer is IndentedTextWriter itw && itw.InnerWriter.IsConsoleOut();
        }

        public static void WriteDiagnostics(this TextWriter writer, IEnumerable<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics.OrderBy(d => d.Location.FileName)
                         .ThenBy(d => d.Location.Span.Start).ThenBy(d => d.Location.Span.Length))
            {
                var text = diagnostic.Location.Text;
                var fileName = diagnostic.Location.FileName;
                var startLine = diagnostic.Location.StartLine + 1;
                var startCharacter = diagnostic.Location.StartCharacter + 1;
                var endLine = diagnostic.Location.EndLine + 1;
                var endCharacter = diagnostic.Location.EndCharacter + 1;

                var span = diagnostic.Location.Span;
                var lineIndex = text.GetLineIndex(span.Start);
                var line = text.Lines[lineIndex];
                
                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
                Console.WriteLine(diagnostic);
                Console.ResetColor();

                var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
                var suffixSpan = TextSpan.FromBounds(span.End, line.End);

                var prefix = text.ToString(prefixSpan);
                var error = text.ToString(span);
                var suffix = text.ToString(suffixSpan);

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

        public static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut()) Console.ForegroundColor = color;
        }
        
        public static void ResetForeground(this TextWriter writer)
        {
            if (writer.IsConsoleOut()) Console.ResetColor();
        }
        
        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetForeground();
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetForeground();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Cyan);
            writer.Write(text);
            writer.ResetForeground();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetForeground();
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetForeground(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetForeground();
        }
    }
}