using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        UnaryExpression,
        LiteralExpression,
        BinaryExpression
    }
    
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }
    }

    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }

    internal sealed class BoundLiteralExpression : BoundExpression
    {
        public object Value { get; }

        public BoundLiteralExpression(object value)
        {
            Value = value;
        }

        public override Type Type => Value.GetType();

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    }
    
    internal enum BoundUnaryOperatorKind
    {
        Identity, 
        Negation
    }
    
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryOperatorKind OperatorKind { get; }
        public BoundExpression Operand { get; }

        public BoundUnaryExpression(BoundUnaryOperatorKind operatorKind, BoundExpression operand)
        {
            OperatorKind = operatorKind;
            Operand = operand;
        }

        public override Type Type => Operand.Type;
        
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    }
    
    internal enum BoundBinaryOperatorKind
    {
        Addition, 
        Subtraction,
        Multiplication,
        Division
    }

    internal sealed class BoundBinaryExpression : BoundExpression
    {
        public BoundExpression Left { get; }
        public BoundBinaryOperatorKind OperatorKind { get; }
        public BoundExpression Right { get; }

        public BoundBinaryExpression(BoundExpression left, BoundBinaryOperatorKind operatorKind, BoundExpression right)
        {
            Left = left;
            OperatorKind = operatorKind;
            Right = right;
        }

        public override Type Type => Left.Type;

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    }

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
            var value = node.LiteralToken.Value as int? ?? 0;
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
                _diagnostics.Add($"Unary Operator '{node.OperatorToken.Text}' is not defined for Types {boundLeft.Type} and {boundRight.Type}.");
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
            if (leftType != typeof(int) || leftType != rightType) return null;
            
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