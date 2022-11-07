using Shore.CodeAnalysis.Binding;

namespace Shore.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;
        private object _lastValue;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }

        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement((BoundBlockStatement)node);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)node);
                    break;
                case BoundNodeKind.IfStatement:
                    EvaluateIfStatement((BoundIfStatement)node);
                    break;
                case BoundNodeKind.WhileStatement:
                    EvaluateWhileStatement((BoundWhileStatement)node);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)node);
                    break;
                default:
                    throw new Exception($"Unexpected Node '{node.Kind}'");
            }
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.Statements) EvaluateStatement(statement);
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }

        private void EvaluateIfStatement(BoundIfStatement node)
        {
            var condition = (bool)EvaluateExpression(node.Condition);
            if (condition) EvaluateStatement(node.ThenStatement);
            else if (node.ElseStatement is not null) EvaluateStatement(node.ElseStatement);
        }

        private void EvaluateWhileStatement(BoundWhileStatement node)
        {
            while ((bool)EvaluateExpression(node.Condition)) EvaluateStatement(node.Body);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node) =>
            _lastValue = EvaluateExpression(node.Expression);

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node)
            {
                case BoundLiteralExpression n:
                    return n.Value;
                case BoundVariableExpression v:
                    return _variables[v.Variable];
                case BoundAssignmentExpression a:
                {
                    var value = EvaluateExpression(a.Expression);
                    _variables[a.Variable] = value;
                    return value;
                }
                case BoundUnaryExpression u:
                {
                    var operand = EvaluateExpression(u.Operand);

                    return u.Op.Kind switch
                    {
                        BoundUnaryOperatorKind.Identity => (int) operand,
                        BoundUnaryOperatorKind.Negation => -(int) operand,
                        BoundUnaryOperatorKind.LogicalNegation => !(bool) operand,
                        BoundUnaryOperatorKind.OnesComplement => ~(int) operand,
                        _ => throw new Exception($"Unexpected Unary Operator '{u.Op.Kind}'")
                    };
                }
                case BoundBinaryExpression b:
                {
                    var left = EvaluateExpression(b.Left);
                    var right = EvaluateExpression(b.Right);
                    
                    return b.Op.Kind switch
                    {
                        BoundBinaryOperatorKind.Addition => (int) left + (int) right,
                        BoundBinaryOperatorKind.Subtraction => (int) left - (int) right,
                        BoundBinaryOperatorKind.Multiplication => (int) left * (int) right,
                        BoundBinaryOperatorKind.Division => (int) left / (int) right,
                        BoundBinaryOperatorKind.BitwiseRightShift => (int) left >> (int) right,
                        BoundBinaryOperatorKind.BitwiseLeftShift => (int) left << (int) right,
                        BoundBinaryOperatorKind.BitwiseAnd when b.Type == typeof(int) => (int)left & (int)right,
                        BoundBinaryOperatorKind.BitwiseAnd when b.Type == typeof(bool) => (bool)left & (bool)right,
                        BoundBinaryOperatorKind.BitwiseOr when b.Type == typeof(int) => (int)left | (int)right,
                        BoundBinaryOperatorKind.BitwiseOr when b.Type == typeof(bool) => (bool)left | (bool)right,
                        BoundBinaryOperatorKind.BitwiseXor when b.Type == typeof(int) => (int)left ^ (int)right,
                        BoundBinaryOperatorKind.BitwiseXor when b.Type == typeof(bool) => (bool)left ^ (bool)right,
                        BoundBinaryOperatorKind.LogicalAnd => (bool) left && (bool) right,
                        BoundBinaryOperatorKind.LogicalOr => (bool) left || (bool) right,
                        BoundBinaryOperatorKind.LogicalEquals => Equals(left, right),
                        BoundBinaryOperatorKind.LogicalNotEquals => !Equals(left, right),
                        BoundBinaryOperatorKind.GreaterThan => (int)left > (int)right,
                        BoundBinaryOperatorKind.GreaterThanOrEqual => (int)left >= (int)right,
                        BoundBinaryOperatorKind.LessThan => (int)left < (int)right,
                        BoundBinaryOperatorKind.LessThanOrEqual => (int)left <= (int)right,
                        _ => throw new Exception($"Unexpected Binary Operator '{b.Op.Kind}'")
                    };
                }
                default:
                    throw new Exception($"Unexpected Node '{node.Type}'");
            }
        }
    }
}