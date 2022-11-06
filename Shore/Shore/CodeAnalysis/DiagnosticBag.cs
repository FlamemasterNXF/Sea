using System.Collections;
using Shore.CodeAnalysis.Syntax;
using Shore.Text;

namespace Shore.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();
        
        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        public void AddRange(DiagnosticBag diagnostics) => _diagnostics.AddRange(diagnostics);

        private void ReportError(TextSpan span, string message)
        {
            var eMessage = message;
            var diagnostic = Diagnostic.Error(span, eMessage);
            _diagnostics.Add(diagnostic);
        }

        private void ReportWarning(TextSpan span, string message)
        {
            var diagnostic = Diagnostic.Warning(span, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            var message = $"The number {text} isn't a valid {type}.";
            ReportError(span, message);
        }

        public void ReportUnknownCharacter(TextSpan span, char character)
        {
            var message = $"Unknown Character: '{character}'.";
            ReportError(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, TokType type, TokType expectedType)
        {
            var message = $"Unexpected Token {type}, {expectedType} was expected.";
            ReportError(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, Type operandType)
        {
            var sType = operandType.ToString().Replace("System.", "");
            var message = $"Unary Operator '{operatorText}' is not defined for Type '{sType}'.";
            ReportError(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string? operatorText, Type leftType, Type rightType)
        {
            var sLeftType = leftType.ToString().Replace("System.", "");
            var sRightType = rightType.ToString().Replace("System.", "");
            var message = $"Binary Operator '{operatorText}' is not defined for Types '{sLeftType}' and '{sRightType}'.";
            ReportError(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is not defined.";
            ReportError(span, message);
        }

        public void ReportCannotConvert(TextSpan span, Type fromType, Type toType)
        {
            var sFromType = fromType.ToString().Replace("System.", "");
            var sToType = toType.ToString().Replace("System.", "");
            var message = $"Cannot Convert Type '{sFromType}' to Type '{sToType}'.";
            ReportError(span, message);
        }

        public void ReportVariableReDeclaration(TextSpan span, string name)
        {
            var message = $"Variable '{name}' has already been declared in this Scope.";
            ReportError(span, message);
        }

        public void ReportCannotAssign(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is read-only and cannot be re-assigned.";
            ReportError(span, message);
        }
    }
}