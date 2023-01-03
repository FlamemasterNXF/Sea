using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Binding.ControlFlow;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace Shore.CodeAnalysis
{
    public sealed class Compilation
    { 
        public bool IsScript { get; }
        private BoundGlobalScope? _globalScope;
        public Compilation? Previous { get; }
        public ImmutableArray<NodeTree> NodeTrees { get; }
        public FunctionSymbol MainFunction => GlobalScope.MainFunction;
        public ImmutableArray<FunctionSymbol?> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol?> Variables => GlobalScope.Variables;

        private Compilation(bool isScript, Compilation? previous, params NodeTree[] nodeTrees)
        {
            IsScript = isScript;
            Previous = previous;
            NodeTrees = nodeTrees.ToImmutableArray();
        }

        public static Compilation Create(params NodeTree[] nodeTrees) => new Compilation(false, null, nodeTrees);

        public static Compilation CreateScript(Compilation? previous, params NodeTree[] nodeTrees) =>
            new Compilation(true, previous, nodeTrees);

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, NodeTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }
        
        public IEnumerable<Symbol?> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();

            while (submission != null)
            {
                var builtinFunctions = BuiltinFunctions.GetAll().ToList();
                
                foreach (var function in submission.Functions) if (seenSymbolNames.Add(function.Name)) yield return function;
                foreach (var variable in submission.Variables) if (seenSymbolNames.Add(variable.Name)) yield return variable;
                foreach (var builtin in builtinFunctions) if (seenSymbolNames.Add(builtin.Name)) yield return builtin;

                submission = submission.Previous;
            }
        }

        private BoundProgram GetProgram()
        {
            var previous = Previous == null ? null : Previous.GetProgram();
            return Binder.BindProgram(IsScript, previous, GlobalScope);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables,
            Dictionary<VariableSymbol, object[]> arrays,
            Dictionary<VariableSymbol, Dictionary<VariableSymbol, object>> lists,
            Dictionary<VariableSymbol, Dictionary<object, object>> dicts)
        {
            var parseDiagnostics = NodeTrees.SelectMany(nt => nt.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);
            
            var program = GetProgram();
            
            /*
            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDirectory, "cfg.dot");
            var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
                ? program.Functions.Last().Value
                : program.Statement;
            var cfg = ControlFlowGraph.Create(cfgStatement);
            using (var streamWriter = new StreamWriter(cfgPath)) cfg.WriteTo(streamWriter);
            */
            
            if (program.Diagnostics.Any()) return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);
            

            var evaluator = new Evaluator(program, variables, arrays, lists, dicts);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            if (GlobalScope.MainFunction is not null) EmitTree(GlobalScope.MainFunction, writer);
            else if (GlobalScope.ScriptFunction is not null) EmitTree(GlobalScope.ScriptFunction, writer);
        }
        
        public void EmitTree(FunctionSymbol? symbol, TextWriter writer)
        {
            var program = GetProgram();

            symbol.WriteTo(writer);
            writer.WriteLine();
            
            if (!program.Functions.TryGetValue(symbol, out var body)) return;
            body.WriteTo(writer);
        }
    }
}