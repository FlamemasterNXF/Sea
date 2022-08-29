using System;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
namespace Sea
{
    public class Program
    {
        public static void Main()
        {
            new ErrorConfig().ErrorSetup();
            new Lexer().Lex("local string localTest = \"hat!!!!!!\" localTest global int16 globalTest");
            new Parser().Parse(Lexer._tokens);
            new Interpreter().Interpret(Parser._nodes, true);
        }
    }
    internal class Message
    {
        internal static bool _errored = false;

        internal static void _writeWithColor(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        internal static void _throw(byte severity, string message)
        {
            if (severity == 1) _writeWithColor(ConsoleColor.Gray, $"INFO: {message}");
            if (severity == 2) _writeWithColor(ConsoleColor.Yellow, $"WARNING: {message}");
            if (severity == 3) { _writeWithColor(ConsoleColor.Red, $"ERROR: {message}"); _errored = true; };
        }
    }
    internal class Position
    {
        public Position(string txt, int idx, int lnIdx, int ln)
        {
            index = idx;
            lineIdx = lnIdx;
            line = ln;
            text = txt;
        }
        internal int index;
        internal int lineIdx;
        internal int line;
        private readonly string text;
        internal void advance(char cChar)
        {
            ++this.index;
            ++this.lineIdx;

            /*if(cChar == "\n"){
                ++this.line;
                this.lineIdx = 0;
            }
            */
        }
        internal Position copy(){ return new Position(text, index, lineIdx, line);}
    }
    
    internal class Lexer
    {
        internal static readonly string digits = "0123456789";
        internal static readonly string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal static List<string> _tokens = new List<string> { };
        internal static List<string> _ids = new List<string> { };
        internal static List<string> _numbers = new List<string> { };
        internal static List<string> _strings = new List<string> { };
        internal static List<string> _variables = new List<string> { };
        private readonly List<string> numberTypes = new List<string>() { "int8", "int16", "int", "int32", "int64", "float32", "float64" };
        private bool shouldLex = true;
        private char cChar;
        private char pChar;
        private Position pos;
        private void advance(string text, bool debug)
        {
            pChar = cChar;
            pos.advance(cChar);
            if (pos.index < text.Length) { cChar = text[pos.index]; if(debug)Console.WriteLine(cChar); }
            else shouldLex = false;
        }

        private void makeNumber(string text, bool debug)
        {
            string number = "";
            string digitsF = "0123456789.";
            bool isDecimal = false;
            Position positionStart = pos.copy();
            while (shouldLex && digitsF.Contains(cChar))
            {
                if (cChar == '.')
                {
                    if (isDecimal) { Message._throw(3, "Cannot have multipe decimal points in a float."); return; }
                    isDecimal = true;
                    number += ".";
                    advance(text, debug);
                }
                else
                {
                    number += cChar;
                    advance(text, debug);
                }
            }
            if (debug) Console.WriteLine($"Added {number}");
            _variables.Add(number);
            _numbers.Add(number);
            _tokens.Add(number);
        }
        private void makeID(string text, bool debug)
        {
            string id = "";
            string lettersF = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            Position positionStart = pos.copy();
            while (shouldLex && lettersF.Contains(cChar.ToString()))
            {
                id += cChar;
                advance(text, debug);
            }
            if (debug) Console.WriteLine($"Added {id}");
            if (id == "int" || id == "float") { makeNumberType(id, text, debug); if (debug) Console.WriteLine("shifted to numberType"); }
            else { _ids.Add(id); _tokens.Add(id); }
        }
        private void makeNumberType(string t, string text, bool debug)
        {
            string digitsT = "123468";
            string type = t;
            while (shouldLex && digitsT.Contains(cChar.ToString()))
            {
                type += cChar;
                advance(text, debug);
            }
            if (!numberTypes.Contains(type)) Message._throw(3, "Invalid Type");
            if (!Message._errored) _tokens.Add(type);
        }
        private void makeString(string text, bool debug)
        {
            string str = "";
            Position positionStart = pos.copy();
            advance(text, debug);
            while (shouldLex && cChar != '"')
            {
                str += cChar;
                advance(text, debug);
            }
            advance(text, debug);
            if (debug) Console.WriteLine($"Added {str}");
            _variables.Add(str);
            _strings.Add(str);
            _tokens.Add(str);
        }
        private void makeOp(char type, string text, bool debug)
        {
            if (!shouldLex) return;
            if (type == '+')
            {
                advance(text, debug);
                if (cChar == '+') { _tokens.Add("++"); advance(text, debug); }
                else if (cChar == '=') { _tokens.Add("+="); advance(text, debug); }
                else { _tokens.Add("+"); }
            }
            if (type == '-')
            {
                advance(text, debug);
                if (cChar == '-') { _tokens.Add("--"); advance(text, debug); }
                else if (cChar == '=') { _tokens.Add("+="); advance(text, debug); }
                else _tokens.Add("-");
            }
            if (type == '*')
            {
                advance(text, debug);
                if (cChar == '=')
                {
                    advance(text, debug);
                    if (cChar == '=') { _tokens.Add("*=="); advance(text, debug); }
                    else _tokens.Add("*=");
                }
                else _tokens.Add("*");
            }
            if (type == '/')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("/="); advance(text, debug); }
                else _tokens.Add("/");
            }
            if (type == '%')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("%="); advance(text, debug); }
                else _tokens.Add("%");
            }
            if (type == '=')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("=="); advance(text, debug); }
                else _tokens.Add("=");
            }
            if (type == '!')
            {
                advance(text, debug);
                if (cChar == '=')
                {
                    advance(text, debug);
                    if (cChar == '=') { _tokens.Add("!=="); advance(text, debug); }
                    else Message._throw(3, "Expected '==' after '!' ");
                }
                else Message._throw(3, "Expected '==' after '!' ");
            }
            if (type == '>')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add(">="); advance(text, debug); }
                else _tokens.Add(">");
            }
            if (type == '<')
            {
                advance(text, debug);
                if (cChar == '=') { _tokens.Add("<="); advance(text, debug); }
                else _tokens.Add(">");
            }
        }

        internal void Lex(string text, bool debug = false)
        {
            Console.WriteLine($"Your input was: {text}");
            pos = new Position(text, -1, -1, 0);
            advance(text, debug);
            cChar = text[pos.index];
            pChar = text[pos.index];

            while (shouldLex && !Message._errored)
            {
                if (cChar == ' ') advance(text, debug);
                else if (digits.Contains(cChar)) makeNumber(text, debug);
                else if (letters.Contains(cChar)) makeID(text, debug);
                else if (cChar == '"') makeString(text,  debug);
                else if (cChar == '+') makeOp('+', text, debug);
                else if (cChar == '-') makeOp('-', text, debug);
                else if (cChar == '*') makeOp('*', text, debug);
                else if (cChar == '/') makeOp('/', text, debug);
                else if (cChar == '%') makeOp('%', text, debug);
                else if (cChar == '=') makeOp('=', text, debug);
                else if (cChar == '!') makeOp('!', text, debug);
                else if (cChar == '>') makeOp('>', text, debug);
                else if (cChar == '<') makeOp('<', text, debug);
                else if (cChar == '(') { _tokens.Add("("); advance(text, debug); }
                else if (cChar == ')') { _tokens.Add(")"); advance(text, debug); }
                else if (cChar == '[') { _tokens.Add("["); advance(text, debug); }
                else if (cChar == ']') { _tokens.Add("]"); advance(text, debug); }
                else if (cChar == '{') { _tokens.Add("{"); advance(text, debug); }
                else if (cChar == '}') { _tokens.Add("}"); advance(text, debug); }
                else { shouldLex = false; }
            }
        }
    }
   
    internal class Parser
    {
        internal static readonly Dictionary<string, dynamic> _nodes = new Dictionary<string, dynamic>(){
            {"valueNodes", new List<ValueNode>(){}},
            {"equationNodes", new List<EquationNode>(){}},
            {"accessNodes", new List<string>(){}},
        };
        
        private readonly Dictionary<string, List<string>> tokens = new Dictionary<string, List<string>>(){
            {"aMods", new List<string>() { "global", "local", "embedded" }},
            {"mods", new List<string>() { "const", "simple", "unsigned" }},
            {"types", new List<string>() { "bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64" }},
            {"lManagers", new List<string>() { "break", "return", "continue" }},
            {"ops", new List<string>() { "+", "-", "*", "/", "%" }},
            {"special", new List<string>() { "[", "]", "(", ")", "{", "}" }}
        };

        internal record ValueNode(string aMod, string mod, string type, string id, string scope, string all, string value, string special){
            internal string aMod{get; init;} = aMod;
            internal string mod{get; init;} = mod;
            internal string type{get; init;} = type;
            internal string id{get; init;} = id;
            internal string scope{get; init;} = scope;
            internal string all{get; init;} = all;
            internal string value{get; init;} = value;
            internal string special{get; init;} = special;
        }
        private record EquationNode(string v1, string op, string v2){
            internal string v1{get; init;} = v1;
            internal string op{get; init;} = op;
            internal string v2{get; init;} = v2;
        }
        private int tokIdx = -1;

        private void MakeValueNode(string aMod, string mod, string type, string id, string scope, string all, string value = "null", string special = "null")
        {
            _nodes["valueNodes"].Add(new ValueNode(aMod, mod, type, id, scope, all, value, special));
        }
        private void MakeEquationNode(string v1, string op, string v2)
        {
            _nodes["equationNodes"].Add(new EquationNode(v1, op, v2));
        }
        private void MakeAccessNode(string name){
            _nodes["accessNodes"].Add(name);
        }
        private void MakeObjectNode() { } //soon (for things within curly braces)

        private void simpleError(string i)
        {
            if (ErrorConfig._errors[i] > 1) Message._throw(ErrorConfig._errors[i], i);
        }
        private bool severe(string i){
            return (ErrorConfig._errors[i] == 3);
        }

        private string all(List<string> strings)
        {
            StringBuilder allBuilder = new StringBuilder();
            foreach (string str in strings)
            {
                allBuilder.Append(str);
            }
            return (allBuilder.ToString());
        }
        private bool shouldParse()
        {
            if(Message._errored) return false;
            else{ return tokIdx < Lexer._tokens.Count; }
        }
        private void advance()
        {
            if (shouldParse()) ++tokIdx;
        }
        private void makeValue(List<string> toks, bool debug)
        {
            List<string> strings = new List<string>() { };
            bool typeGot = false;
            bool hasValue = false;
            strings.Add(toks[tokIdx]);
            advance();
            if (tokens["mods"].Contains(toks[tokIdx]) && shouldParse())
            {
                strings.Add(toks[tokIdx]);
                advance();
            }
            else if (tokens["types"].Contains(toks[tokIdx]) && shouldParse() && !severe("NO_MOD"))
            {
                strings.Add("simple");
                strings.Add(toks[tokIdx]);
                advance();
                typeGot = true;
                simpleError("NO_MOD");
            }
            else
            {
                if(severe("NO_MOD")){ simpleError("NO_MOD"); } else { simpleError("NO_TYPE"); }
                return;
            }

            if (!typeGot && !Message._errored && shouldParse())
            {
                if (tokens["types"].Contains(toks[tokIdx]))
                {
                    strings.Add(toks[tokIdx]);
                    advance();
                    typeGot = true;
                }
                else
                {
                    simpleError("NO_TYPE");
                    return;
                }
            }

            if (shouldParse() && Lexer._ids.Contains(toks[tokIdx]))
            {
                strings.Add(toks[tokIdx]);
                advance();
            }
            else
            {
                simpleError("NO_NAME");
                return;
            }

            if (shouldParse() && toks[tokIdx] == "=")
            {
                strings.Add(toks[tokIdx]);
                advance();
                hasValue = true;
            }
            else
            {
                MakeValueNode(strings[0], strings[1], strings[2], strings[3], "TOP_LEVEL", all(strings));
                simpleError("NULL_VALUE");
                if (debug) Console.WriteLine($"Variable declared: {all(strings)}");
                tokIdx = tokIdx-1;
                return;
            }

            if (shouldParse() && hasValue && Lexer._variables.Contains(toks[tokIdx]))
            {
                strings.Add(toks[tokIdx]);
                MakeValueNode(strings[0], strings[1], strings[2], strings[3], "TOP_LEVEL", all(strings), strings[5]);
                if (debug) Console.WriteLine($"Variable declared: {all(strings)}");
            }
            else
            {
                simpleError("NO_VALUE");
                return;
            }
        }
        private void makeEquation(List<string> toks, bool debug)
        {
            if (toks[tokIdx] == "(")
            {
                while (toks[tokIdx] != ")" && shouldParse())
                {
                    advance();
                    if (!Lexer._numbers.Contains(toks[tokIdx])) { simpleError("EXPECTED_NUM"); return; }
                    string num1 = toks[tokIdx];
                    advance();
                    if (!tokens["ops"].Contains(toks[tokIdx])) { simpleError("EXPECTED_OP"); return; }
                    string op = toks[tokIdx];
                    advance();
                    if (!Lexer._numbers.Contains(toks[tokIdx])) { simpleError("EXPECTED_NUM");; return; }
                    MakeEquationNode(num1, op, toks[tokIdx]);
                    if(debug)Console.WriteLine($"Equation declared: ({num1} {op} {toks[tokIdx]})");
                    advance();
                }
                return;
            }
            if(Lexer.digits.Contains(toks[tokIdx]))
            {
                    string num1 = toks[tokIdx];
                    advance();
                    if (!tokens["ops"].Contains(toks[tokIdx])) { simpleError("EXPECTED_OP"); return; }
                    string op = toks[tokIdx];
                    advance();
                    if (!Lexer._numbers.Contains(toks[tokIdx])) { simpleError("EXPECTED_NUM");; return; }
                    MakeEquationNode(num1, op, toks[tokIdx]);
                    if(debug)Console.WriteLine($"Equation declared: {num1} {op} {toks[tokIdx]}");
                
            }
        }

        internal void Parse(List<string> toks, bool debug = false)
        {
            while (shouldParse())
            {
                advance();
                if (shouldParse() && tokens["aMods"].Contains(toks[tokIdx])) { makeValue(toks, debug); }
                else if (shouldParse() && (toks[tokIdx] == "(")) { makeEquation(toks, debug); }
                else if (shouldParse() && Lexer.digits.Contains(toks[tokIdx])) { makeEquation(toks, debug); }
                else if (shouldParse() && Lexer._ids.Contains(toks[tokIdx])) { MakeAccessNode(toks[tokIdx]); }
            }
        }
    };
    
    internal class Interpreter{
        /* NOTE: This is the best I can do for now.
            globals[node][0] = mod
            globals[node][1] = type
            globals[node][2] = type

            For locals increase those indexes by 1
            locals[node][0] = scope
        */
        private Dictionary<string, string> varAccess = new Dictionary<string, string>(){};
        private Dictionary<string, List<string>> globals = new Dictionary<string, List<string>>(){};
        private Dictionary<string, List<string>> locals = new Dictionary<string, List<string>>(){};
        internal void Interpret(Dictionary<string, dynamic> nodes, bool debug){
            foreach (var node in nodes["valueNodes"])
            { 
                varAccess.Add(node.id, node.aMod);

                if(node.aMod == "global"){ globals.Add(node.id, new List<string>(){node.mod, node.type, node.value}); if(debug)Console.WriteLine($"Made Global Var {node.id} with Type {node.type}"); }
                else{ locals.Add(node.id, new List<string>(){node.scope, node.mod, node.type, node.value}); if(debug)Console.WriteLine($"Made Local Var {node.id} with Scope {node.scope} and Type {node.type}"); }
            }
            foreach (var node in nodes["accessNodes"]){
                try
                {
                    if(varAccess[node] == "local"){
                        if(locals[node][2] == "null"){ Console.WriteLine("null"); }
                        else{ Console.WriteLine(locals[node][3]); }
                    }
                    else if(varAccess[node] == "global"){
                        if(globals[node][2] == "null"){ Console.WriteLine("null"); }
                        else{ Console.WriteLine(globals[node][2]); }
                    }
                }
                catch (System.Exception)
                {
                    Message._throw(3, $"{node} is undefined!");
                }
            }
        }
    }
}

