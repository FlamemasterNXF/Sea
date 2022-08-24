﻿using System;
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

        internal void Parse(string text, bool debug=false){
            for (int i = 0; i < Lexer._tokens.Count; i++)
            {
                //Need Errors
            }
        }
    };
}

