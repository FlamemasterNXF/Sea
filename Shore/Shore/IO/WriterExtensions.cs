using System.CodeDom.Compiler;
using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.IO
{
    public static class WriterExtensions
    {
        private static bool IsConsole(this TextWriter writer)
        {
            if (writer == Console.Out) return !Console.IsOutputRedirected;
            if (writer == Console.Error) return !Console.IsErrorRedirected && !Console.IsOutputRedirected;
            
            return writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole();
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
                
                writer.WriteLine();

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write($"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): ");
                writer.WriteLine(diagnostic);
                writer.ResetForeground();

                var prefixSpan = TextSpan.FromBounds(line.Start, span.Start);
                var suffixSpan = TextSpan.FromBounds(span.End, line.End);

                var prefix = text.ToString(prefixSpan);
                var error = text.ToString(span);
                var suffix = text.ToString(suffixSpan);

                writer.Write("    ");
                writer.Write(prefix);

                writer.SetForeground(ConsoleColor.DarkRed);
                writer.Write(error);
                writer.ResetForeground();

                writer.Write(suffix);
                writer.WriteLine();
            }

            writer.WriteLine();
        }

        public static void SetForeground(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsole()) Console.ForegroundColor = color;
        }
        
        public static void ResetForeground(this TextWriter writer)
        {
            if (writer.IsConsole()) Console.ResetColor();
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