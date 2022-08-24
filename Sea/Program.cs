using System;
using System.Text;
using System.Text.RegularExpressions;
namespace Sea
{ 
    public class Program
    {
        public static void Main()
        {
            new Lexer().Lex("global gwa gwa int8 hat int16 hhhhhhhhh global + - * / % = [ { (", true);
        }
    }
    internal class Lexer
    {
        static readonly string[] _toks =
        {
            "+", "-", "*", "/", "%", "=",
            "[", "{", "(",
        };

        static readonly string[] _keywords =
        {
            "global", "local", "embedded",
            "bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64",
            "const", "simple", "unsigned",
            "break", "return", "continue"
        };
        static List<string> _tokens = new List<string>{};
        internal void Lex(string text, bool debug=false)
        {
            StringBuilder keyBuilder = new StringBuilder();
            foreach (string value in _keywords)
            {
                keyBuilder.Append(value);
                keyBuilder.Append('|');
            }
            string keyBuilderS = keyBuilder.ToString().Remove(keyBuilder.Length-1);
            StringBuilder tokBuilder = new StringBuilder();
            foreach (string value in _toks)
            {
                tokBuilder.Append('\\');
                tokBuilder.Append(value);
                tokBuilder.Append('|');
            }
            string tokBuilderS = tokBuilder.ToString().Remove(tokBuilder.Length-1);
            
            string pattern = @"\b("+keyBuilderS+")\\b|"+tokBuilderS;
            MatchCollection tokensDetected = Regex.Matches(text, pattern);

            if(debug) Console.WriteLine("{0} matches found in:\n   {1}", tokensDetected.Count, text);

            foreach (Match match in tokensDetected)
            {
                GroupCollection groups = match.Groups;
                _tokens.Add(groups[0].Value);
                if(debug) Console.WriteLine("'{0}' repeated at position {1}", groups[0].Value, groups[0].Index);

            }
            if(debug) Console.WriteLine(pattern);
            if(debug){ for (int i = 0; i < _tokens.Count; i++){ Console.WriteLine(_tokens[i]); } }
        }
    }
}

