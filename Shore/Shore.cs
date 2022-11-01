using System;
using System.Text;
using System.Data;
namespace Shore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // CONSOLE WINDOW CONTROL
            Console.WriteLine("Press C to close this window :)");
            try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            catch (Exception){ Console.WriteLine($"Console Window not found!"); }
        }
    }
}

