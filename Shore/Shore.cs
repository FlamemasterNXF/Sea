using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Shore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            
            Interpreter.Init();

            string path = File.Exists($"{Directory.GetCurrentDirectory()}/Base.sea") ? $"{Directory.GetCurrentDirectory()}/Base.sea" : "bin/Debug/net6.0/Base.sea";
            if (args.Length == 0) args = new string[] { $"{path}" };

            // READING FILE
            string text = File.ReadAllText(args[0]).Replace("\r", "");
            string dev = @"print Math.Pow(54,54)";
            var code = Interpreter.Interpret(text);
            var script = CSharpScript.Create(code, ScriptOptions.Default.WithImports("System", "System.Math"));

            string Repeat(string repeater, int amount)
            {
                return string.Join("", Enumerable.Repeat(repeater, amount));
            }
            var i = 0;
            var running = true;

            Task.Run(async () =>
            {
                script.Compile();
                running = false;
            });
            
            while (running)
            {
                i++;
                Task.Delay(150).GetAwaiter().GetResult();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(
                    $"Compiling Please Wait [{Repeat(".", i)}{Repeat(" ", 3 - i)}]      ");
                i %= 3;
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine("                                                   ");
            Console.SetCursorPosition(0, 0);
            
            sw.Stop();
            Console.WriteLine($"Compiled Successfully in: [{sw.Elapsed}]!");

            script.RunAsync().GetAwaiter().GetResult();
            
            // CONSOLE WINDOW CONTROL
            Console.WriteLine("Press C to close this window :)");
            try { while (Console.ReadKey(true).Key != ConsoleKey.C) Console.Read(); }
            catch (Exception) { Console.WriteLine($"Console Window not found!"); }
        }
    }
}

