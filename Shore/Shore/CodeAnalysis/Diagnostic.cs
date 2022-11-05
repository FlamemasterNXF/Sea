namespace Shore.CodeAnalysis
{
    public sealed class Diagnostic
    {
        public bool IsError { get; }
        public bool IsWarning { get; }
        public TextSpan Span { get; }
        public string Message { get; }

        public Diagnostic(bool isError, TextSpan span, string message)
        {
            IsError = isError;
            IsWarning = !isError;
            Span = span;
            Message = message;
        }
        
        public override string ToString() => Message;

        public static Diagnostic Error(TextSpan span, string message) => new Diagnostic(true, span, message);
        public static Diagnostic Warning(TextSpan span, string message) => new Diagnostic(false, span, message);
    }
}