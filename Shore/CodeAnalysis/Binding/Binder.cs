using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private List<string> _diagnostics = new List<string>();

        public IEnumerable<string> Diagnostics => _diagnostics;

        public BoundExpression BindExpression(ExpressionNode node)
        {
            return node.Type switch
            {
                TokType.LiteralExpression => BindLiteralExpression((LiteralExpressionNode)node),
                TokType.UnaryExpression => BindUnaryExpression((UnaryExpressionNode)node),
                TokType.BinaryExpression => BindBinaryExpression((BinaryExpressionNode)node),
                _ => throw new Exception($"Unexpected Node {node.Type}")
            };
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionNode node)
        {
            var value = node.Value ?? 0;
            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionNode node)
        {
            var boundOperand = BindExpression(node.Operand);
            var boundOperatorKind = BindUnaryOperatorKind(node.OperatorToken.Type, boundOperand.Type);
            
            if (boundOperatorKind is null)
            {
                _diagnostics.Add($"Unary Operator '{node.OperatorToken.Text}' is not defined for Type {boundOperand.Type}.");
                return boundOperand;
            }
            
            return new BoundUnaryExpression(boundOperatorKind.Value, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionNode node)
        {
            var boundLeft = BindExpression(node.Left);
            var boundRight = BindExpression(node.Right);
            var boundOperatorKind = BindBinaryOperatorKind(node.OperatorToken.Type, boundLeft.Type, boundRight.Type);
            
            if (boundOperatorKind is null)
            {
                _diagnostics.Add($"Binary Operator '{node.OperatorToken.Text}' is not defined for Types {boundLeft.Type} and {boundRight.Type}.");
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperatorKind.Value, boundRight);
        }
        
        private BoundUnaryOperatorKind? BindUnaryOperatorKind(TokType kind, Type operandType)
        {
            if (operandType != typeof(int)) return null;
            
            return kind switch
            {
                TokType.PlusToken => BoundUnaryOperatorKind.Identity,
                TokType.DashToken => BoundUnaryOperatorKind.Negation,
                _ => throw new Exception($"Unexpected Unary Operator '{kind}'")
            };
        }
        
        private BoundBinaryOperatorKind? BindBinaryOperatorKind(TokType kind, Type leftType, Type rightType)
        {
            if (leftType != typeof(int) || rightType != typeof(int)) return null;
            
            return kind switch
            {
                TokType.PlusToken => BoundBinaryOperatorKind.Addition,
                TokType.DashToken => BoundBinaryOperatorKind.Subtraction,
                TokType.StarToken => BoundBinaryOperatorKind.Multiplication,
                TokType.SlashToken => BoundBinaryOperatorKind.Division,
                _ => throw new Exception($"Unexpected Binary Operator '{kind}'")
            };
        }
    }
}