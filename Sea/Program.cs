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
            new Lexer().Lex("global float32 theFirst = 96");
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
        private readonly List<string> numberTypes = new List<string>(){ "int8", "int16", "int", "int32", "int64", "float32", "float64" };
        private char? currentChar;
        private char? previousChar;
        private bool shouldLex = true;
    
        internal void Lex(string text, bool debug=false)
        {
            //string text = Regex.Replace( txt, @"\s", "" );
            Position pos = new Position(text, 0, 0, 0);
            string digits = "0123456789";
            string digitsF = "0123456789.";
            string digitsT = "123468";
            string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lettersF = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            
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
                _tokens.Add(number);
            }
            void makeID(){
                string id = "";
                Position positionStart = pos.copy();
                while(shouldLex && lettersF.Contains(currentChar.ToString())){
                    id += currentChar;
                    advance();
                }
                if(debug)Console.WriteLine($"Added {id}");
                if(id=="int" || id=="float"){ makeNumberType(id); if(debug)Console.WriteLine("shifted to numberType"); }
                else{ _ids.Add(id); _tokens.Add(id); }
            }
            void makeNumberType(string t){
                string type = t;
                while(shouldLex && digitsT.Contains(currentChar.ToString())){
                    type += currentChar;
                    advance();
                }
                if(!numberTypes.Contains(type)) Message._throw(3, "Invalid Type");
                if(!Message._errored) _tokens.Add(type);
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
                _tokens.Add(str);
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
                else{ shouldLex = false; }
            }
        }
    }
    internal class Parser{
        internal static List<string> _nodes = new List<string>{};

        private List<string> aMods = new List<string>(){"global", "local", "embedded"};
        private List<string> mods = new List<string>(){"const", "simple", "unsigned"};
        private List<string> types = new List<string>(){"bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64"};
        private List<string> lManagers = new List<string>(){"break", "return", "continue"};
        private List<string> operators = new List<string>(){"+", "-", "*", "/", "%", "="};
        private List<string> specialToks = new List<string>(){"[", "(", "{"};

        internal void MakeValueNode(string aMod, string mod, string type, string id, string all, string value = "null", string special = "null"){

        }
        internal void MakeObjectNode(){} //soon (for things within curly braces)

        private void simpleError(int i, string x){
            if(ErrorConfig.errorFlags[i] > 1) Message._throw(ErrorConfig.errorFlags[i], $"{ErrorConfig.errorNames[i]} at {x}");
        }

        private string all(List<string> strings){
            StringBuilder allBuilder = new StringBuilder();
            foreach (string str in strings)
            {   
                allBuilder.Append(str);
            }
            return(allBuilder.ToString());
        }

        internal void Parse(List<string> toks, bool debug=false){
            for (int i = 0; i < Lexer._tokens.Count; i++)
            {
                bool shouldParse(){
                     return i < toks.Count;
                }

                if(Message._errored) break;
                if(debug) Message._throw(1, "Info Test");
                if(debug) Message._throw(2, "Warn Test");
                if(debug) Message._throw(3, "Error Test");

                if(aMods.Contains(toks[i]) && shouldParse()){
                    List<string> strings = new List<string>(){};
                    bool typeGot = false;
                    bool hasValue = false;
                    strings.Add(toks[i]);
                    i = i+1;

                    if(mods.Contains(toks[i]) && shouldParse()){
                        strings.Add(toks[i]);
                        i=i+1;
                    }
                    else if(types.Contains(toks[i]) && shouldParse()){
                        strings.Add("simple");
                        strings.Add(toks[i]);
                        i=i+1;
                        typeGot = true;
                    }
                    else{
                        Message._throw(3, "Expected type or modifier");
                    }

                    if(!typeGot && Message._errored && shouldParse()){
                        if(types.Contains(toks[i])){
                            strings.Add(toks[i]);
                            i=i+1;
                            typeGot = true;
                        }
                        else{
                            Message._throw(3, "Expected type");
                        }
                    }

                    if(Lexer._ids.Contains(toks[i]) && shouldParse()){
                        strings.Add(toks[i]);
                        i=i+1;
                    }
                    else{
                        Message._throw(3, "Expected ID");
                    }

                    if(shouldParse() && toks[i]=="="){
                        strings.Add(toks[i]);
                        i=i+1;
                        hasValue = true;
                    }
                    else{
                        Console.WriteLine(strings.Count);
                        MakeValueNode(strings[0], strings[1], strings[2], strings[3], all(strings));
                        if(debug)Console.WriteLine(all(strings));
                    }

                    if(shouldParse() && hasValue && Lexer._numbers.Contains(toks[i])){
                        strings.Add(toks[i]);
                        MakeValueNode(strings[0], strings[1], strings[2], strings[3], all(strings), strings[5]);
                        if(debug)Console.WriteLine(all(strings));
                    }
                    else{
                        Message._throw(3, "Expected Value");
                    }
                }
            }
        }
    };
}

