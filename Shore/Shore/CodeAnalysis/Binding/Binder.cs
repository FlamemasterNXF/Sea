using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding.Converting;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private BoundScope _scope;
        private DiagnosticBag _diagnostics = new DiagnosticBag();

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }

        private static BoundScope? CreateParentScope(BoundGlobalScope? previous)
        {
            var stack = new Stack<BoundGlobalScope>();
            while (previous != null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables) scope.TryDeclareVariable(v);

                parent = scope;
            }

            return parent;
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitNode node)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope);
            var expression = binder.BindStatement(node.Statement);
            var variables = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous is not null) diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);
            foreach (var function in BuiltinFunctions.GetAll()) result.TryDeclareFunction(function);
            return result;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private BoundStatement BindStatement(StatementNode node)
        {
            return node.Type switch
            {
                TokType.BlockStatement => BindBlockStatement((BlockStatementNode)node),
                TokType.VariableDeclarationStatement => BindVariableDeclaration((VariableDeclarationNode)node),
                TokType.IfStatement => BindIfStatement((IfStatementNode)node),
                TokType.WhileStatement => BindWhileStatement((WhileStatementNode)node),
                TokType.ForStatement => BindForStatement((ForStatementNode)node),
                TokType.ExpressionStatement => BindExpressionStatement((ExpressionStatementNode)node),
                _ => throw new Exception($"Unexpected Node {node.Type}")
            };
        }

        private BoundStatement BindBlockStatement(BlockStatementNode node)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementNode in node.Statements)
            {
                var statement = BindStatement(statementNode);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationNode node)
        {
            var isReadOnly = node.Keyword.Type == TokType.ReadOnlyKeyword;
            var initializer = BindExpressionDistributor(node.Initializer);
            var variable = BindVariable(node.Identifier, isReadOnly, initializer.Type);

            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindIfStatement(IfStatementNode node)
        {
            var condition = BindExpressionDistributor(node.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(node.ThenStatement);
            var elseStatement = node.ElseNode == null ? null : BindStatement(node.ElseNode.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatementNode node)
        {
            var condition = BindExpressionDistributor(node.Condition, TypeSymbol.Bool);
            var body = BindStatement(node.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindForStatement(ForStatementNode node)
        {
            var lowerBound = BindExpressionDistributor(node.LowerBound, TypeSymbol.Int32);
            var upperBound = BindExpressionDistributor(node.UpperBound, TypeSymbol.Int32);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(node.Identifier, true, TypeSymbol.Int32);
            var body = BindStatement(node.Body);

            _scope = _scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementNode node)
        {
            var expression = BindExpression(node.Expression, true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionNode node, bool canBeVoid = false)
        {
            var result = BindExpressionDistributor(node);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(node.Span);
                return new BoundNullExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionDistributor(ExpressionNode node, TypeSymbol targetType)
        {
            var result = BindExpressionDistributor(node);
            if (targetType != TypeSymbol.Null && result.Type != TypeSymbol.Null && result.Type != targetType)
                _diagnostics.ReportCannotConvert(node.Span, result.Type, targetType);
            return result;
        }

        private BoundExpression BindExpressionDistributor(ExpressionNode node)
        {
            return node.Type switch
            {
                TokType.LiteralExpression => BindLiteralExpression((LiteralExpressionNode)node),
                TokType.UnaryExpression => BindUnaryExpression((UnaryExpressionNode)node),
                TokType.BinaryExpression => BindBinaryExpression((BinaryExpressionNode)node),
                TokType.CallExpression => BindCallExpression((CallExpressionNode)node),
                TokType.ParenthesisExpression => BindExpressionDistributor(((ParenthesisExpressionNode)node).Expression),
                TokType.NameExpression => BindNameExpression((NameExpressionNode)node),
                TokType.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionNode)node),
                _ => throw new Exception($"Unexpected Node {node.Type}")
            };
        }

        private BoundExpression BindNameExpression(NameExpressionNode node)
        {
            var name = node.IdentifierToken.Text;

            if (string.IsNullOrEmpty(name))
            {
                // This ensures that 'Token Fabrication' does not cause an Error.
                return new BoundNullExpression();
            }

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(node.IdentifierToken.Span, name);
                return new BoundNullExpression();
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionNode node)
        {
            var name = node.IdentifierToken.Text;
            var boundExpression = BindExpressionDistributor(node.Expression);

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(node.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsReadOnly) _diagnostics.ReportCannotAssign(node.EqualsToken.Span, name);

            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvert(node.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

            return new BoundAssignmentExpression(variable, boundExpression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionNode node)
        {
            var value = node.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionNode node)
        {
            var boundOperand = BindExpressionDistributor(node.Operand);
            var boundOperator = BoundUnaryOperator.Bind(node.OperatorToken.Type, boundOperand.Type);

            if (boundOperand.Type == TypeSymbol.Null) return new BoundNullExpression();

            if (boundOperator is null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(node.OperatorToken.Span, node.OperatorToken.Text,
                    boundOperand.Type);
                return new BoundNullExpression();
            }

            return new BoundUnaryExpression(boundOperator, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionNode node)
        {
            var boundLeft = BindExpressionDistributor(node.Left);
            var boundRight = BindExpressionDistributor(node.Right);

            if (boundLeft.Type == TypeSymbol.Null || boundRight.Type == TypeSymbol.Null)
                return new BoundNullExpression();

            var boundOperator = BoundBinaryOperator.Bind(node.OperatorToken.Type, boundLeft.Type, boundRight.Type);

            if (boundOperator is null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(node.OperatorToken.Span, node.OperatorToken.Text,
                    boundLeft.Type, boundRight.Type);
                return new BoundNullExpression();
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindCallExpression(CallExpressionNode node)
        {
            if (node.Arguments.Count == 1 && LookupType(node.Identifier.Text) is TypeSymbol type)
                return BindConversion(type, node.Arguments[0]);
            var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var argument in node.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            var functions = BuiltinFunctions.GetAll();

            var function = functions.SingleOrDefault(f => f.Name == node.Identifier.Text);
            if (function is null)
            {
                _diagnostics.ReportUndefinedFunction(node.Identifier.Span, node.Identifier.Text);
                return new BoundNullExpression();
            }

            if (node.Arguments.Count != function.Parameters.Length)
            {
                _diagnostics.ReportWrongArgumentCount(node.Span, function.Name, function.Parameters.Length,
                    node.Arguments.Count);
                return new BoundNullExpression();
            }

            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];

                if (argument.Type != parameter.Type)
                {
                    _diagnostics.ReportWrongArgumentType(node.Span, parameter.Name, parameter.Type, argument.Type);
                    return new BoundNullExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(TypeSymbol type, ExpressionNode node)
        {
            var expression = BindExpression(node);
            var conversion = Conversion.Classify(expression.Type, type);
            if (!conversion.Exists)
            {
                _diagnostics.ReportCannotConvert(node.Span, expression.Type, type);
                return new BoundNullExpression();
            }

            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = new VariableSymbol(name, isReadOnly, type);

            if (declare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportVariableReDeclaration(identifier.Span, name);

            return variable;
        }

        private TypeSymbol? LookupType(string name)
        {
            return name switch
            {
                "bool" => TypeSymbol.Bool,
                "string" => TypeSymbol.String,
                "int8" => TypeSymbol.Int8,
                "int16" => TypeSymbol.Int16,
                "int32" => TypeSymbol.Int32,
                "int64" => TypeSymbol.Int64,
                _ => null
            };
        }
    }
}