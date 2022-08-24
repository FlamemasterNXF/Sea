using System;
using System.Text;
using System.Text.RegularExpressions;
namespace Sea
{ 
    public class Program
    {
        public static void Main()
        {
            new ErrorConfig().ErrorSetup();
            new Lexer().Lex("global gwa gwa int8 hat int16 hhhhhhhhh global + - * / % = [ { (");
            new Parser().Parse(Lexer._tokens, true);
        }
    }
    internal class Message{
        internal static bool _errored = false;

        internal static void _writeWithColor(ConsoleColor color, string text){
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        internal static void _throw(byte severity, string message){
            if(severity==1) _writeWithColor(ConsoleColor.Gray, $"INFO: {message}");
            if(severity==2) _writeWithColor(ConsoleColor.Yellow, $"WARNING: {message}");
            if(severity==3){ _writeWithColor(ConsoleColor.Red, $"ERROR: {message}"); _errored = true; };
        }
    };
    internal class Lexer
    {
        internal readonly string[] _toks =
        {
            "+", "-", "*", "/", "%", "=",
            "[", "{", "(",
        };

        internal readonly string[] _keywords =
        {
            "global", "local", "embedded",
            "const", "simple", "unsigned",
            "bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64",
            "break", "return", "continue"
        };
        internal static List<string> _tokens = new List<string>{};
        internal static List<int> _tokIndexes = new List<int>{};
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
                _tokIndexes.Add(groups[0].Index);
                if(debug) Console.WriteLine("'{0}' repeated at position {1}", groups[0].Value, groups[0].Index);

            }
        }
    }
    internal class Parser{
        internal static List<string> _nodes = new List<string>{};

        private string[] aMods = {"global", "local", "embedded"};
        private string[] mods = {"const", "simple", "unsigned"};
        private string[] types = {"bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64"};
        private string[] lManagers = {"break", "return", "continue"};
        private string[] operators = {"+", "-", "*", "/", "%", "="};
        private string[] specialToks = {"[", "(", "{"};

        internal void MakeValueNode(string aMod, string mod, string type, string value, string all, string? special = null){

        }
        internal void MakeObjectNode(){} //soon (for things within curly braces)

        private void simpleError(int i, int x){
            if(ErrorConfig.errorFlags[i] > 1) Message._throw(ErrorConfig.errorFlags[i], $"{ErrorConfig.errorNames[i]} at {Lexer._tokIndexes[x]}");
        }

        internal void Parse(List<string> toks, bool debug=false){
            for (int i = 0; i < Lexer._tokens.Count; i++)
            {
                if(Message._errored) break;
                if(debug) Message._throw(1, "Info Test");
                if(debug) Message._throw(2, "Warn Test");
                if(debug) Message._throw(3, "Error Test");

                if(Array.Exists(aMods, x => x == toks[i])){
                    List<string> strings = new List<string>(){toks[i]};

                    if(!Array.Exists(mods, x=> x== toks[i+1])){ 
                        simpleError(1, Lexer._tokIndexes[i]);
                        if(!Message._errored) strings.Add("simple");
                    }
                    else strings.Add(toks[i+1]);

                    if(!Array.Exists(types, x=> x== toks[i+2])){
                        simpleError(2, Lexer._tokIndexes[i]);
                        //no implicit casts for now
                    }
                    else strings.Add(toks[i+2]);
                }
            }
        }
    };
}

