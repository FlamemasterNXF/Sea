using Shore.Text;

namespace Shore.CodeAnalysis
{
    public sealed class Diagnostic
    {
        public bool IsError { get; }
        public bool IsWarning { get; }
        public TextLocation Location { get; }
        public string Message { get; }

        public Diagnostic(bool isError, TextLocation location, string message)
        {
            IsError = isError;
            IsWarning = !isError;
            Location = location;
            Message = message;
        }
        
        public override string ToString() => Message;

        public static Diagnostic Error(TextLocation location, string message) => new Diagnostic(true, location, message);
        public static Diagnostic Warning(TextLocation location, string message) => new Diagnostic(false, location, message);
    }
}