using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis
{
    public sealed class Compilation
    {
        public NodeTree NodeTree { get; }

        public Compilation(NodeTree nodeTree)
        {
            NodeTree = nodeTree;
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var binder = new Binder(variables);
            var boundTree = binder.BindExpression(NodeTree.Root.Expression);

            var diagnostics = NodeTree.Diagnostics.Concat(binder.Diagnostics).ToImmutableArray();
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            var evaluator = new Evaluator(boundTree, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}