using System;
using System.Text;
using System.Text.RegularExpressions;
namespace Sea
{ 
    public class Program
    {
        public static void Main()
        {
            new Lexer().Lex("global gwa gwa int8 hat int16 hhhhhhhhh global + - * / % = [ { (");
        }
    }
    internal class Lexer
    {
        private readonly string[] _tokens =
        {
            "+", "-", "*", "/", "%", "=",
            "[", "{", "(",
        };

        private readonly string[] _keywords =
        {
            "global", "local", "embedded",
            "bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64",
            "const", "simple", "unsigned",
            "break", "return", "continue"
        };
        internal void Lex(string text)
        {
            StringBuilder keyBuilder = new StringBuilder();
            foreach (string value in _keywords)
            {
                keyBuilder.Append(value);
                keyBuilder.Append('|');
            }
            string keyBuilderS = keyBuilder.ToString().Remove(keyBuilder.Length-1);
            StringBuilder tokBuilder = new StringBuilder();
            foreach (string value in _tokens)
            {
                tokBuilder.Append('\\');
                tokBuilder.Append(value);
                tokBuilder.Append('|');
            }
            string tokBuilderS = tokBuilder.ToString().Remove(tokBuilder.Length-1);
            
            string pattern = @"\b("+keyBuilderS+")\\b|"+tokBuilderS;
            MatchCollection tokens = Regex.Matches(text, pattern);

            Console.WriteLine("{0} matches found in:\n   {1}", tokens.Count, text);

            foreach (Match match in tokens)
            {
                GroupCollection groups = match.Groups;
                Console.WriteLine("'{0}' repeated at position {1}", groups[0].Value, groups[0].Index);
            }
            Console.WriteLine(pattern);
        }
    }
}

