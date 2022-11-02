using Shore.CodeAnalysis.Nodes;
using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis
{
    public sealed class Evaluator
    {
        private readonly ExpressionNode _root;
        public Evaluator(ExpressionNode root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionNode node)
        {
            if (node is LiteralExpressionNode n) return (int) n.LiteralToken.Value;

            if (node is UnaryExpressionNode u)
            {
                var operand = EvaluateExpression(u.Operand);

                return u.OperatorToken.Type switch
                {
                    TokType.PlusToken => operand,
                    TokType.DashToken => -operand,
                    _ => throw new Exception($"Unexpected Unary Operator '{u.OperatorToken.Type}'")
                };
            }
            
            if (node is BinaryExpressionNode b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                return b.OperatorToken.Type switch
                {
                    TokType.PlusToken => left + right,
                    TokType.DashToken => left - right,
                    TokType.StarToken => left * right,
                    TokType.SlashToken => left / right,
                    _ => throw new Exception($"Unexpected Binary Operator '{b.OperatorToken.Type}'")
                };
            }

            if (node is ParenthesisExpressionNode p) return EvaluateExpression(p.Expression);
            
            throw new Exception($"Unexpected Node '{node.Type}'");
        }
    }
}