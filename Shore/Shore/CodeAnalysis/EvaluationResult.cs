using System.Collections.Immutable;

namespace Shore.CodeAnalysis
{
    public sealed class EvaluationResult
    {
        public bool HasDanger { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public object? Value { get; }

        public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? value)
        {
            Diagnostics = diagnostics;
            Value = value;

            foreach (var diagnostic in diagnostics.Where(diagnostic => diagnostic.IsError)) HasDanger = true;
        }
    }
}