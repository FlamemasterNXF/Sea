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

        public EvaluationResult Evaluate()
        {
            var binder = new Binder();
            var boundTree = binder.BindExpression(NodeTree.Root);

            var diagnostics = NodeTree.Diagnostics.Concat(binder.Diagnostics).ToArray();
            if (diagnostics.Any()) return new EvaluationResult(diagnostics, null);

            var evaluator = new Evaluator(boundTree);
            var value = evaluator.Evaluate();
            return new EvaluationResult(Array.Empty<Diagnostic>(), value);
        }
    }
}