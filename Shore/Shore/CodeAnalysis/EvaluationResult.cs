using System.Collections.Immutable;

namespace Shore.CodeAnalysis
{
    public sealed class EvaluationResult
    {
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public object? Value { get; }

        public EvaluationResult(ImmutableArray<Diagnostic> diagnostics, object? value)
        {
            Diagnostics = diagnostics;
            Value = value;
        }
    }
}