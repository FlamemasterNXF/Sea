using System.Collections;
using Shore.CodeAnalysis.Symbols;
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
            var diagnostic = Diagnostic.Error(span, message);
            _diagnostics.Add(diagnostic);
        }

        private void ReportWarning(TextSpan span, string message)
        {
            var diagnostic = Diagnostic.Warning(span, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol? type)
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

        public void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, TypeSymbol? operandType)
        {
            var sType = operandType.ToString().Replace("System.", "");
            var message = $"Unary Operator '{operatorText}' is not defined for Type '{sType}'.";
            ReportError(span, message);
        }

        public void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Binary Operator '{operatorText}' is not defined for Types '{leftType}' and '{rightType}'.";
            ReportError(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string? name)
        {
            var message = $"Variable '{name}' is not defined.";
            ReportError(span, message);
        }

        public void ReportCannotConvert(TextSpan span, TypeSymbol? fromType, TypeSymbol? toType)
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

        public void ReportCannotAssign(TextSpan span, string? name)
        {
            var message = $"Variable '{name}' is read-only and cannot be re-assigned.";
            ReportError(span, message);
        }

        public void ReportUnterminatedString(TextSpan span)
        {
            var message = "Unterminated String.";
            ReportError(span, message);
        }
        
        public void ReportUndefinedFunction(TextSpan span, string? name)
        {
            var message = $"Function '{name}' doesn't exist.";
            ReportError(span, message);
        }

        public void ReportWrongArgumentCount(TextSpan span, string? name, int expectedCount, int actualCount)
        {
            var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            ReportError(span, message);
        }

        public void ReportWrongArgumentType(TextSpan span, string? name, TypeSymbol? expectedType, TypeSymbol? actualType)
        {
            var message = $"Parameter '{name}' requires a value of Type '{expectedType}' but was given a value of Type '{actualType}'.";
            ReportError(span, message);
        }

        public void ReportExpressionMustHaveValue(TextSpan span)
        {
            var message = "Expression must have a value.";
            ReportError(span, message);
        }

        public void ReportInvalidType(TextSpan span, string type)
        {
            var message = $"Type '{type}' is invalid.";
            ReportError(span, message);
        }

        public void ReportCannotConvertImplicitly(TextSpan span, TypeSymbol? fromType, TypeSymbol? toType)
        {
            var message =
                $"Cannot convert Type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?).";
            ReportError(span, message);
        }

        public void ReportFunctionsAreUnsupported(TextSpan span)
        {
            var message = "Functions with return values are unsupported.";
            ReportError(span, message);
        }

        public void ReportSymbolAlreadyDeclared(TextSpan span, string? functionName)
        {
            var message = $"A Function with the name '{functionName}' already exists.";
            ReportError(span, message);
        }

        public void ReportParameterAlreadyDeclared(TextSpan span, string? parameterName)
        {
            var message = $"A parameter with the name '{parameterName}' already exists.";
            ReportError(span, message);
        }

        public void ReportInvalidBreakOrContinue(TextSpan span, string? text)
        {
            var message = $"'{text}' can only be used inside of Loops.";
            ReportError(span, message);
        }
    }
}