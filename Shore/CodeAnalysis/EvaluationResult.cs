namespace Shore.CodeAnalysis
{
    public sealed class EvaluationResult
    {
        public IReadOnlyList<Diagnostic> Diagnostics { get; }
        public object? Value { get; }

        public EvaluationResult(IEnumerable<Diagnostic> diagnostics, object? value)
        {
            Diagnostics = diagnostics.ToArray();
            Value = value;
        }
    }
}