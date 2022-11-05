﻿using System.Text;
using Shore.CodeAnalysis;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.misc
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            bool showTree = false;
            var variables = new Dictionary<VariableSymbol, object>();
            var textBuilder = new StringBuilder();

            while (true)
            {
                if(textBuilder.Length == 0) Console.Write("> ");
                else Console.Write("| ");

                var input = Console.ReadLine();
                var isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank) break;
                    
                    if (input == "#SHOWTREE")
                    {
                        showTree = !showTree;
                        Console.WriteLine(showTree ? "Showing Node Trees" : "Hiding Node Trees");
                        continue;
                    }
                }

                textBuilder.AppendLine(input);
                var text = textBuilder.ToString();
                var nodeTree = NodeTree.Parse(text);

                if(!isBlank && nodeTree.Diagnostics.Any()) continue;

                var compilation = new Compilation(nodeTree);
                var result = compilation.Evaluate(variables);

                var diagnostics = result.Diagnostics;

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    nodeTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }

                if (!diagnostics.Any()) Console.WriteLine(result.Value);
                else
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        var lineIndex = nodeTree.Text.GetLineIndex(diagnostic.Span.Start);
                        var line = nodeTree.Text.Lines[lineIndex];
                        var lineNumber = lineIndex + 1;
                        var character = diagnostic.Span.Start - line.Start + 1;
                        
                        Console.WriteLine();

                        Console.ForegroundColor = diagnostic.IsError ? ConsoleColor.DarkRed : ConsoleColor.DarkYellow;
                        Console.Write($"({lineNumber}, {character}): ");
                        Console.WriteLine(diagnostic);
                        Console.ResetColor();

                        var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                        var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);
                        var prefix = nodeTree.Text.ToString(prefixSpan);
                        var message = nodeTree.Text.ToString(diagnostic.Span);
                        var suffix = nodeTree.Text.ToString(suffixSpan);

                        Console.Write("    ");
                        Console.Write(prefix);

                        Console.ForegroundColor = diagnostic.IsError ? ConsoleColor.DarkRed : ConsoleColor.DarkYellow;
                        Console.Write(message);
                        Console.ResetColor();
                        
                        Console.Write(suffix);
                        Console.WriteLine();
                    }
                    Console.WriteLine();
                }

                textBuilder.Clear();
            }

            // CONSOLE WINDOW CONTROL
            //Console.WriteLine("Press C to close this window :)");
            //try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            //catch (Exception){ Console.WriteLine($"Console Window not found!"); }
        }
    }
}

