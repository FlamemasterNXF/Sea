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

        private void ReportError(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Error(location, message);
            _diagnostics.Add(diagnostic);
        }

        private void ReportWarning(TextLocation location, string message)
        {
            var diagnostic = Diagnostic.Warning(location, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextLocation location, string text)
        {
            var message = $"The number {text} isn't a valid Number.";
            ReportError(location, message);
        }

        public void ReportUnknownCharacter(TextLocation location, char character)
        {
            var message = $"Unknown Character: '{character}'.";
            ReportError(location, message);
        }

        public void ReportUnexpectedToken(TextLocation location, TokType type, TokType expectedType)
        {
            var message = $"Unexpected Token {type}, {expectedType} was expected.";
            ReportError(location, message);
        }

        public void ReportUndefinedUnaryOperator(TextLocation location, string? operatorText, TypeSymbol? operandType)
        {
            var sType = operandType.ToString().Replace("System.", "");
            var message = $"Unary Operator '{operatorText}' is not defined for Type '{sType}'.";
            ReportError(location, message);
        }

        public void ReportUndefinedBinaryOperator(TextLocation location, string operatorText, TypeSymbol leftType, TypeSymbol rightType)
        {
            var message = $"Binary Operator '{operatorText}' is not defined for Types '{leftType}' and '{rightType}'.";
            ReportError(location, message);
        }

        public void ReportUndefinedName(TextLocation location, string? name)
        {
            var message = $"Variable '{name}' is not defined.";
            ReportError(location, message);
        }

        public void ReportCannotConvert(TextLocation location, TypeSymbol? fromType, TypeSymbol? toType)
        {
            var sFromType = fromType.ToString().Replace("System.", "");
            var sToType = toType.ToString().Replace("System.", "");
            var message = $"Cannot Convert Type '{sFromType}' to Type '{sToType}'.";
            ReportError(location, message);
        }

        public void ReportVariableReDeclaration(TextLocation location, string name)
        {
            var message = $"Variable '{name}' has already been declared in this Scope.";
            ReportError(location, message);
        }

        public void ReportCannotAssign(TextLocation location, string? name)
        {
            var message = $"Variable '{name}' is read-only and cannot be re-assigned.";
            ReportError(location, message);
        }

        public void ReportUnterminatedString(TextLocation location)
        {
            var message = "Unterminated String.";
            ReportError(location, message);
        }
        
        public void ReportUndefinedFunction(TextLocation location, string? name)
        {
            var message = $"Function '{name}' doesn't exist.";
            ReportError(location, message);
        }

        public void ReportWrongArgumentCount(TextLocation location, string? name, int expectedCount, int actualCount)
        {
            var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
            ReportError(location, message);
        }

        public void ReportWrongArgumentType(TextLocation location, string? name, TypeSymbol? expectedType, TypeSymbol? actualType)
        {
            var message = $"Parameter '{name}' requires a value of Type '{expectedType}' but was given a value of Type '{actualType}'.";
            ReportError(location, message);
        }

        public void ReportExpressionMustHaveValue(TextLocation location)
        {
            var message = "Expression must have a value.";
            ReportError(location, message);
        }

        public void ReportInvalidType(TextLocation location, string type)
        {
            var message = $"Type '{type}' is invalid.";
            ReportError(location, message);
        }

        public void ReportCannotConvertImplicitly(TextLocation location, TypeSymbol? fromType, TypeSymbol? toType)
        {
            var message =
                $"Cannot convert Type '{fromType}' to '{toType}'. An explicit conversion exists (are you missing a cast?).";
            ReportError(location, message);
        }

        public void ReportSymbolAlreadyDeclared(TextLocation location, string? functionName)
        {
            var message = $"A Function with the name '{functionName}' already exists.";
            ReportError(location, message);
        }

        public void ReportParameterAlreadyDeclared(TextLocation location, string? parameterName)
        {
            var message = $"A parameter with the name '{parameterName}' already exists.";
            ReportError(location, message);
        }

        public void ReportInvalidBreakOrContinue(TextLocation location, string? text)
        {
            var message = $"'{text}' can only be used inside of Loops.";
            ReportError(location, message);
        }

        public void ReportInvalidReturn(TextLocation location)
        {
            var message = "The 'return' Keyword can only be used inside of Functions.";
            ReportError(location, message);
        }

        public void ReportInvalidReturnExpression(TextLocation location, string name)
        {
            var message = $"Since the Function '{name}' is of Type 'void' 'return' cannot be followed by an expression.";
            ReportError(location, message);
        }

        public void ReportMissingReturnExpression(TextLocation location, TypeSymbol type)
        {
            var message = $"Expected an expression of Type '{type}'.";
            ReportError(location, message);
        }

        public void ReportAllPathsMustReturn(TextLocation location)
        {
            var message = "Not all code paths return a value.";
            ReportError(location, message);
        }
        
        public void ReportInvalidExpressionStatement(TextLocation location)
        {
            var message = $"Only Assignment and Call Expressions can be used as a Statement.";
            ReportError(location, message);
        }

        public void ReportInvalidReturnWithValueInGlobalStatements(TextLocation location)
        {
            var message = "The 'return' Keyword cannot be followed by an expression in Global Statements.";
            ReportError(location, message);
        }
        
        public void ReportOnlyOneFileCanHaveGlobalStatements(TextLocation location)
        {
            var message = $"At most one File can have Global Statements.";
            ReportError(location, message);
        }

        public void ReportMainMustHaveCorrectSignature(TextLocation location)
        {
            var message = $"'Main' must not take Arguments and not Return anything.";
            ReportError(location, message);
        }

        public void ReportCannotMixMainAndGlobalStatements(TextLocation location)
        {
            var message = $"Cannot declare 'Main' Function when Global Statements are used.";
            ReportError(location, message);
        }

        public void ReportUnterminatedMultiLineComment(TextLocation location)
        {
            var message = $"Unterminated Multi-Line Comment.";
            ReportError(location, message);
        }

        public void ReportEmptyArray(TextLocation location, string name)
        {
            var message = $"Array '{name}' is empty.";
            ReportWarning(location, message);
        }
    }
}