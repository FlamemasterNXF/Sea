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
            new Lexer().Lex("+ 123 += \"HELLO CHAT\" ++ gwa");
            //new Parser().Parse(Lexer._tokens, true);
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
    }
    internal class Position{
        internal int index;
        internal int lineIdx;
        internal int line;
        string text;
        public Position(string txt, int idx, int lnIdx, int ln)
        {
            index = idx;
            lineIdx = lnIdx;
            line = ln;
            text = txt;
        }
        internal void advance(string? currentChar=null){
                ++this.index;
                ++this.lineIdx;

                if(currentChar == "\n"){
                    ++this.line;
                    this.lineIdx = 0;
                }
         }
         internal void reverse(){
            if(this.lineIdx-- == -1){ --this.line; this.lineIdx = 0; }
            else{
                --this.index;
                --this.lineIdx;
            }
         }
         internal Position copy(){
            return new Position(text, index, lineIdx, line);
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
        internal static List<string> _ids = new List<string>{};
        internal static List<string> _numbers = new List<string>{};
        internal static List<string> _strings = new List<string>{};
        private char? currentChar;
        private char? previousChar;
        private bool shouldLex = true;
    
        internal void Lex(string text, bool debug=false)
        {
            //string text = Regex.Replace( txt, @"\s", "" );
            Position pos = new Position(text, 0, 0, 0);
            string digits = "0123456789";
            string digitsF = "0123456789.";
            string letters = "abcdefghijklmnopqrstuvwxyz";
            string lettersF = "abcdefghijklmnopqrstuvwxyz_";
            
            void advance(){
                previousChar = currentChar;
                pos.advance(currentChar.ToString());
                if(pos.index < text.Length) currentChar = text[pos.index];
                else shouldLex = false;
            }

            void makeNumber(){
                string number = "";
                bool isDecimal = false;
                Position positionStart = pos.copy();
                while(shouldLex && digitsF.Contains(currentChar.ToString())){
                    if(currentChar == '.'){
                        if(isDecimal){ Message._throw(3, "Cannot have multipe decimal points in a float."); break; }
                        isDecimal = true;
                        number += ".";
                    }
                    else{
                        number += currentChar;
                    }
                    advance();
                }
                if(debug)Console.WriteLine($"Added {number}");
                _numbers.Add(number);
            }
            void makeID(){
                string id = "";
                Position positionStart = pos.copy();
                while(shouldLex && lettersF.Contains(currentChar.ToString())){
                    id += currentChar;
                    advance();
                }
                if(debug)Console.WriteLine($"Added {id}");
                _ids.Add(id);
            }
            void makeString(){
                string str = "";
                Position positionStart = pos.copy();
                advance();
                while(shouldLex && currentChar != '"'){
                    str += currentChar;
                    advance();
                }
                advance();
                if(debug)Console.WriteLine($"Added {str}");
                _strings.Add(str);
            }
            void makeOp(char type){
                if(!shouldLex) return;
                if(type=='+'){
                    advance();
                    if(currentChar == '+'){ _tokens.Add("++"); advance(); }
                    else if (currentChar == '='){ _tokens.Add("+=") ; advance(); }
                    else{ _tokens.Add("+"); }
                }
                if(type=='-'){
                    advance();
                    if(currentChar == '-'){ _tokens.Add("--"); advance(); }
                    else if(currentChar == '='){ _tokens.Add("+="); advance(); }
                    else _tokens.Add("-"); 
                }
                if(type=='*'){
                    advance();
                    if(currentChar == '='){
                        advance(); 
                        if(currentChar == '='){ _tokens.Add("*=="); advance(); }
                        else _tokens.Add("*=");
                    }
                    else _tokens.Add("*");
                }
                if(type=='/'){
                    advance();
                    if(currentChar == '='){ _tokens.Add("/="); advance(); }
                    else _tokens.Add("/"); 
                }
                if(type=='%'){
                    advance();
                    if(currentChar == '='){ _tokens.Add("%="); advance(); }
                    else _tokens.Add("%"); 
                }
                if(type=='='){
                    advance();
                    if(currentChar == '='){ _tokens.Add("=="); advance(); }
                    else _tokens.Add("=");
                }
                if(type=='!'){
                    advance();
                    if(currentChar == '='){
                        advance();
                        if(currentChar == '='){ _tokens.Add("!=="); advance(); }
                        else Message._throw(3, "Expected '==' after '!' ");
                    }
                    else Message._throw(3, "Expected '==' after '!' ");
                }
                if(type=='>'){
                    advance();
                    if(currentChar == '='){ _tokens.Add(">="); advance(); }
                    else _tokens.Add(">");
                }
                if(type=='<'){
                    advance();
                    if(currentChar == '='){ _tokens.Add("<="); advance(); }
                    else _tokens.Add(">");
                }
            }
            
            currentChar = text[0];
            while(shouldLex && !Message._errored){
                string workable = currentChar.ToString();
                if(digits.Contains(workable)) makeNumber();
                if(letters.Contains(workable)) makeID();
                if(currentChar == '"') makeString();
                if(currentChar == '+') makeOp('+');
                if(currentChar == '-') makeOp('-');
                if(currentChar == '*') makeOp('*');
                if(currentChar == '/') makeOp('/');
                if(currentChar == '%') makeOp('%');
                if(currentChar == '=') makeOp('=');
                if(currentChar == '!') makeOp('!');
                if(currentChar == '>') makeOp('>');
                if(currentChar == '<') makeOp('<');
                if(currentChar == '(') _tokens.Add("(");
                if(currentChar == ')') _tokens.Add(")");
                if(currentChar == '[') _tokens.Add("[");
                if(currentChar == ']') _tokens.Add("]");
                if(currentChar == '{') _tokens.Add("{");
                if(currentChar == '}') _tokens.Add("}");
                if(currentChar == ' ') advance();
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
            if(ErrorConfig.errorFlags[i] > 1) Message._throw(ErrorConfig.errorFlags[i], $"{ErrorConfig.errorNames[i]} at {Lexer._tokens[x]}");
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
                        //simpleError(1, Lexer._tokIndexes[i]);
                        if(!Message._errored) strings.Add("simple");
                    }
                    else strings.Add(toks[i+1]);

                    if(!Array.Exists(types, x=> x== toks[i+2])){
                        //simpleError(2, Lexer._tokIndexes[i]);
                        //no implicit casts for now
                    }
                    else strings.Add(toks[i+2]);
                }
            }
        }
    };
}

