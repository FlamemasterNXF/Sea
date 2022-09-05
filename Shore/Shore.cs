using System;
using System.Text;
using System.Data;
namespace Shore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // USING DEFAULT FILE
            string path = File.Exists($"{Directory.GetCurrentDirectory()}/Base.sea")?$"{Directory.GetCurrentDirectory()}/Base.sea":"bin/Debug/net6.0/Base.sea";
            if(args.Length == 0){ args = new string[]{$"{path}"}; }

            // READING FILE
            string text = File.ReadAllText(args[0]).Replace(Environment.NewLine, " ");
            string dev = "1+-(1*3)+5*2+4!-2^2+(4+3(2*3))+5";

            // INTERPRETER 
            new ErrorConfig().ErrorSetup();
            new Docks().Dock(dev);
            new Lexer().Lex(dev, Docks._dockValues["LEX_INPUT_RETURN"], Docks._dockValues["LEX_DEBUG"]);
            new Parser().Parse(Lexer._tokens, Docks._dockValues["PARSE_DEBUG"]);
            new Interpreter().Interpret(Parser._nodes, Docks._dockValues["INTERPRET_DEBUG"]);

            // CONSOLE WINDOW CONTROL
            Console.WriteLine("Press C to close this window :)");
            try{ while(Console.ReadKey(true).Key != ConsoleKey.C){ Console.Read(); } }
            catch (System.Exception){ Console.WriteLine($"Console Window not found!"); }
        }
    }
    internal class Message
    {
        internal static bool _errored = false;

        internal static void _writeWithColor(ConsoleColor color, string text)
        {
            ConsoleColor def = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = def; 
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

            /*if(cChar.ToString() == Environment.NewLine){
                ++this.line;
                this.lineIdx = -1;
            }*/
        }
        internal Position copy(){ return new Position(text, index, lineIdx, line);}

    }
    
    internal class Docks
    {
        internal static bool _hasDocks;
        internal static Dictionary<string, bool> _dockValues = new Dictionary<string, bool>{
            {"LEX_INPUT_RETURN", false}, {"LEX_DEBUG", false}, {"PARSE_DEBUG", false}, {"INTERPRET_DEBUG", false}
        };
        internal static int _dockEndIndex = -1;
        private readonly List<string> docks = new List<string>{"DOCKS", "LEX_INPUT_RETURN", "LEX_DEBUG", "PARSE_DEBUG", "INTERPRET_DEBUG"};
        private char cChar;
        private char pChar;
        private Position pos;
        private bool end = false;
        private void advance(string text)
        {
            pChar = cChar;
            pos.advance(cChar);
            if(cChar == ';'){ end = true; _dockEndIndex = pos.index; return;}
            if (pos.index < text.Length) { cChar = text[pos.index]; }
            else end = true;
        }
        private bool readableToBool(string r){
            if(r=="FALSE")return false;
            else if(r=="TRUE")return true;
            else return false;
        }
        private void dockCheck(string text){
            string txt = "";
            Position positionStart = pos.copy();
            while (cChar != '"')
            {
                if(cChar == ' ') advance(text);
                else{ 
                    txt += cChar;
                    advance(text);
                }
            }
            if(txt == "DOCKS=") _hasDocks = true;
            else{ Message._throw(3, "Expected \"DOCKS= ...\""); _hasDocks = false; return; }
        }
        private void makeKeyword(string text)
        {
            string key = "";
            Position positionStart = pos.copy();
            advance(text);
            while(cChar == ' ') advance(text);
            while (cChar != '"' && !end)
            {
                key += cChar;
                advance(text);
            }
            advance(text);
            while(cChar == ' ') advance(text);
            if(docks.Contains(key)){
                if(cChar != ':'){ Message._throw(3, "Expected Dock Value"); return; }
                makeDockValue(text, key);
            }
            else { Message._throw(3, "Unknown Dock"); return; }
        }
        private void makeDockValue(string text, string key){
            string value = "";
            Position positionStart = pos.copy();
            advance(text);
            while(cChar == ' ') advance(text);
            while(cChar != ',' && !end && cChar != ';')
            {
                value += cChar;
                advance(text);    
            }
            advance(text);
            if(cChar == ' ') advance(text);
            if(value == "TRUE" || value == "FALSE"){ _dockValues[key] = readableToBool(value); }
            else{ Message._throw(3, "Expected Value of \"TRUE\" or \"FALSE\""); return; }
        }

        internal void Dock(string text){
            pos = new Position(text, -1, -1, 0);
            advance(text);
            cChar = text[pos.index];
            pChar = text[pos.index];
            if(cChar == 'D'){ dockCheck(text); }
            else{ _hasDocks = false; return; }

            while(cChar.ToString() != Environment.NewLine && !Message._errored && !end){
                if(cChar == '"') makeKeyword(text);
                else if (cChar == ' ') advance(text); 
            }
        }
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
            if (pos.index < text.Length) { cChar = text[pos.index]; if(debug)Console.WriteLine($"DEBUG(L): {cChar}"); }
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
            if (debug) Console.WriteLine($"DEBUG(L): Added {number}");
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
            if (debug) Console.WriteLine($"DEBUG(L): Added {id}");
            if (id == "int" || id == "float") { makeNumberType(id, text, debug); if(debug)Console.WriteLine("DEBUG(L): Shifted to numberType"); }
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
                if(cChar == '~') str += Environment.NewLine;
                else str += cChar;
                advance(text, debug);
            }
            advance(text, debug);
            if (debug) Console.WriteLine($"DEBUG(L): Added {str}");
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
                else _tokens.Add("!");
            }
            if (type == '^')
            {
                advance(text, debug);
                _tokens.Add("^");
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

        internal void Lex(string text, bool disp = false, bool debug = false)
        {
            if(disp)Message._writeWithColor(ConsoleColor.DarkCyan, $"Your input was: {text}");
            if(Docks._hasDocks){ pos = new Position(text, Docks._dockEndIndex, Docks._dockEndIndex, 0); }
            else{ pos = new Position(text, -1, -1, 0); }
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
                else if (cChar == '^') makeOp('^', text, debug);
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
            {"equationNodes", new List<string>(){}},
            {"accessNodes", new List<string>(){}},
        };
        
        private readonly Dictionary<string, List<string>> tokens = new Dictionary<string, List<string>>(){
            {"aMods", new List<string>() { "global", "local", "embedded" }},
            {"mods", new List<string>() { "const", "simple", "unsigned" }},
            {"types", new List<string>() { "bool", "char", "string", "byte", "int8", "int16", "int", "int32", "int64", "float32", "float64" }},
            {"lManagers", new List<string>() { "break", "return", "continue" }},
            {"ops", new List<string>() { "+", "-", "*", "/", "%", "!", "^", "(", ")" }},
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
        private int tokIdx = -1;

        private void MakeValueNode(string aMod, string mod, string type, string id, string scope, string all, string value = "null", string special = "null")
        {
            _nodes["valueNodes"].Add(new ValueNode(aMod, mod, type, id, scope, all, value, special));
        }
        private void MakeEquationNode(string equation)
        {
            _nodes["equationNodes"].Add(equation);
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
                if (debug) Console.WriteLine($"DEBUG(P): Variable declared: {all(strings)}");
                tokIdx = tokIdx-1;
                return;
            }

            if (shouldParse() && hasValue && Lexer._variables.Contains(toks[tokIdx]))
            {
                strings.Add(toks[tokIdx]);
                MakeValueNode(strings[0], strings[1], strings[2], strings[3], "TOP_LEVEL", all(strings), strings[5]);
                if (debug) Console.WriteLine($"DEBUG(P): Variable declared: {all(strings)}");
            }
            else
            {
                simpleError("NO_VALUE");
                return;
            }
        }
        private void makeEquation(List<string> toks, bool debug)
        {
            string equation = "";
            while (shouldParse() && (tokens["ops"].Contains(toks[tokIdx]) || Lexer._numbers.Contains(toks[tokIdx])))
            {
                equation += toks[tokIdx];
                advance();
            }
            if(debug) Console.WriteLine(equation);
            MakeEquationNode(equation);
            return;               
        }

        internal void Parse(List<string> toks, bool debug)
        {
            while (shouldParse())
            {
                advance();
                if (shouldParse() && tokens["aMods"].Contains(toks[tokIdx])) { makeValue(toks, debug); }
                else if (shouldParse() && ((toks[tokIdx] == "(") || Lexer._numbers.Contains(toks[tokIdx]))) { makeEquation(toks, debug); }
                else if (shouldParse() && Lexer._ids.Contains(toks[tokIdx])) { MakeAccessNode(toks[tokIdx]); }
            }
        }
    };
    
    internal class Interpreter
    {
        /* NOTE: This is the best I can do for now.
            globals[node][0] = mod
            globals[node][1] = type
            globals[node][2] = value

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

                if(node.aMod == "global"){ globals.Add(node.id, new List<string>(){node.mod, node.type, node.value}); if(debug)Console.WriteLine($"DEBUG(I): Made Global Var {node.id} with Type {node.type}"); }
                else{ locals.Add(node.id, new List<string>(){node.scope, node.mod, node.type, node.value}); if(debug)Console.WriteLine($"DEBUG(I): Made Local Var {node.id} with Scope {node.scope} and Type {node.type}"); }
            }
            foreach (string node in nodes["accessNodes"]){
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
            foreach (string node in nodes["equationNodes"]){
                try
                {
                    Console.WriteLine(Calculator.Calculate(node));
                }
                catch (System.Exception)
                {
                    Message._throw(3, $"Invalid Equation.");
                }
            }
        }
    }
}

