using System.Collections.Immutable;
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

            BoundScope? parent = null;

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables) scope.TryDeclare(v);

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
            var initializer = BindExpression(node.Initializer);
            var variable = BindVariable(node.Identifier, isReadOnly, initializer.Type);

            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindIfStatement(IfStatementNode node)
        {
            var condition = BindExpression(node.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatement(node.ThenStatement);
            var elseStatement = node.ElseNode == null ? null : BindStatement(node.ElseNode.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatementNode node)
        {
            var condition = BindExpression(node.Condition, TypeSymbol.Bool);
            var body = BindStatement(node.Body);
            return new BoundWhileStatement(condition, body);
        }

        private BoundStatement BindForStatement(ForStatementNode node)
        {
            var lowerBound = BindExpression(node.LowerBound, TypeSymbol.Int32);
            var upperBound = BindExpression(node.UpperBound, TypeSymbol.Int32);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(node.Identifier, true, TypeSymbol.Int32);
            var body = BindStatement(node.Body);

            _scope = _scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementNode node)
        {
            var expression = BindExpression(node.Expression);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionNode node, TypeSymbol targetType)
        {
            var result = BindExpression(node);
            if (targetType != TypeSymbol.Null && result.Type != TypeSymbol.Null && result.Type != targetType)
                _diagnostics.ReportCannotConvert(node.Span, result.Type, targetType);
            return result;
        }

        private BoundExpression BindExpression(ExpressionNode node)
        {
            return node.Type switch
            {
                TokType.LiteralExpression => BindLiteralExpression((LiteralExpressionNode)node),
                TokType.UnaryExpression => BindUnaryExpression((UnaryExpressionNode)node),
                TokType.BinaryExpression => BindBinaryExpression((BinaryExpressionNode)node),
                TokType.ParenthesisExpression => BindExpression(((ParenthesisExpressionNode)node).Expression),
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

            if (!_scope.TryLookup(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(node.IdentifierToken.Span, name);
                return new BoundNullExpression();
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionNode node)
        {
            var name = node.IdentifierToken.Text;
            var boundExpression = BindExpression(node.Expression);

            if (!_scope.TryLookup(name, out var variable))
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
            var boundOperand = BindExpression(node.Operand);
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
            var boundLeft = BindExpression(node.Left);
            var boundRight = BindExpression(node.Right);

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

        private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = new VariableSymbol(name, isReadOnly, type);

            if (declare && !_scope.TryDeclare(variable))
                _diagnostics.ReportVariableReDeclaration(identifier.Span, name);

            return variable;
        }
    }
}