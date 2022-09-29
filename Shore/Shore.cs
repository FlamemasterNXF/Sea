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

            var path = File.Exists($"{Directory.GetCurrentDirectory()}/Base.sea")
                ? $"{Directory.GetCurrentDirectory()}/Base.sea"
                : "bin/Debug/net6.0/Base.sea";
            if (args.Length == 0) args = new string[] { $"{path}" };

            // READING FILE
            var text = File.ReadAllText(args[0]).Replace("\r", "");
            var dev = @"def global bool lol = 100";
            var code = Interpreter.Interpret(dev);
            var script = CSharpScript.Create(code, ScriptOptions.Default.WithImports("System", "System.Math"));

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
                Console.WriteLine($"Compiling Please Wait [{".".Repeat(i)}{" ".Repeat(3 - i)}]      ");
                i %= 3;
            }

            Console.SetCursorPosition(0, 0);
            Console.WriteLine("                                                   ");
            Console.SetCursorPosition(0, 0);

            sw.Stop();
            Console.WriteLine($"Compiled Successfully in: [{sw.Elapsed}]!");

            try { script.RunAsync().GetAwaiter().GetResult(); }
            catch (Exception e)
            {
                var msg = e.Message;
                Console.WriteLine(
                    $"[{msg[(msg.IndexOf(':', msg.IndexOf(':') + 1) + 2)..]}] AT LINE [{msg[1..msg.IndexOf(',')]}]");
            }

            // CONSOLE WINDOW CONTROL
            Console.WriteLine("Press C to close this window :)");
            try { while (Console.ReadKey(true).Key != ConsoleKey.C) Console.Read(); }
            catch (Exception) { Console.WriteLine($"Console Window not found!"); }
        }
    }
}