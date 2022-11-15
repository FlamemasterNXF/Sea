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
        private BoundGlobalScope? _globalScope;
        public Compilation? Previous { get; }
        public ImmutableArray<NodeTree> NodeTrees { get; }
        public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
        public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;

        public Compilation(params NodeTree[] nodeTrees)
            : this(null, nodeTrees)
        {
        }

        private Compilation(Compilation? previous, params NodeTree[] nodeTrees)
        {
            Previous = previous;
            NodeTrees = nodeTrees.ToImmutableArray();
        }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if (_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, NodeTrees);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }

                return _globalScope;
            }
        }
        
        public IEnumerable<Symbol> GetSymbols()
        {
            var submission = this;
            var seenSymbolNames = new HashSet<string>();

            while (submission != null)
            {
                const ReflectionBindingFlags bindingFlags = ReflectionBindingFlags.Static |
                                                            ReflectionBindingFlags.Public |
                                                            ReflectionBindingFlags.NonPublic;
                
                var builtinFunctions = typeof(BuiltinFunctions)
                    .GetFields(bindingFlags).Where(fi => fi.FieldType == typeof(FunctionSymbol))
                    .Select(fi => (FunctionSymbol)fi.GetValue(obj: null)!).ToList();

                
                foreach (var builtin in builtinFunctions) if (seenSymbolNames.Add(builtin.Name)) yield return builtin;
                foreach (var function in submission.Functions) if (seenSymbolNames.Add(function.Name)) yield return function;
                foreach (var variable in submission.Variables) if (seenSymbolNames.Add(variable.Name)) yield return variable;

                submission = submission.Previous;
            }
        }

        public Compilation ContinueWith(NodeTree nodeTree) => new Compilation(this, nodeTree);

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var parseDiagnostics = NodeTrees.SelectMany(nt => nt.Diagnostics);

            var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);
            
            var program = Binder.BindProgram(GlobalScope);
            
            var appPath = Environment.GetCommandLineArgs()[0];
            var appDirectory = Path.GetDirectoryName(appPath);
            var cfgPath = Path.Combine(appDirectory, "cfg.dot");
            var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
                ? program.Functions.Last().Value
                : program.Statement;
            var cfg = ControlFlowGraph.Create(cfgStatement);
            using (var streamWriter = new StreamWriter(cfgPath)) cfg.WriteTo(streamWriter);
            
            if (program.Diagnostics.Any()) return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);
            

            var evaluator = new Evaluator(program, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }

        public void EmitTree(TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);

            if (program.Statement.Statements.Any()) program.Statement.WriteTo(writer);
            else
            {
                foreach (var function in program.Functions
                             .Where(function => GlobalScope.Functions.Contains(function.Key)))
                {
                    function.Key.WriteTo(writer);
                    function.Value.WriteTo(writer);
                }
            }
        }
        
        public void EmitTree(FunctionSymbol symbol, TextWriter writer)
        {
            var program = Binder.BindProgram(GlobalScope);

            symbol.WriteTo(writer);
            writer.WriteLine();
            
            if (!program.Functions.TryGetValue(symbol, out var body)) return;
            body.WriteTo(writer);
        }
    }
}