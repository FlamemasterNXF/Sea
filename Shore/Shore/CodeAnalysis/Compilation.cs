using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Binding.ControlFlow;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis
{
    public sealed class Compilation
    {
        private BoundGlobalScope? _globalScope;
        public Compilation? Previous { get; }
        public ImmutableArray<NodeTree> NodeTrees { get; }

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
    }
}