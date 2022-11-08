using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;
        private object _lastValue;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (var i = 0; i < _root.Statements.Length; i++)
            {
                if (_root.Statements[i] is BoundLabelStatement l)
                    labelToIndex.Add(l.BoundLabel, i + 1);
            }

            var index = 0;

            while (index < _root.Statements.Length)
            {
                var s = _root.Statements[index];

                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                        index++;
                        break;
                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement((BoundExpressionStatement)s);
                        index++;
                        break;
                    case BoundNodeKind.GotoStatement:
                        var gs = (BoundGotoStatement)s;
                        index = labelToIndex[gs.BoundLabel];
                        break;
                    case BoundNodeKind.ConiditonalGotoStatement:
                        var cgs = (BoundConditionalGotoStatement)s;
                        var condition = (bool)EvaluateExpression(cgs.Condition);
                        if (condition == cgs.JumpIfTrue) index = labelToIndex[cgs.BoundLabel];
                        else index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    default:
                        throw new Exception($"Unexpected node {s.Kind}");
                }
            }
            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
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
                        BoundBinaryOperatorKind.Addition when TypeSymbol.CheckType(b.Type, TypeSymbol.Number) => (int) left + (int) right,
                        BoundBinaryOperatorKind.Addition when TypeSymbol.CheckType(b.Type, TypeSymbol.String) => (string) left + (string) right,
                        BoundBinaryOperatorKind.Subtraction => (int) left - (int) right,
                        BoundBinaryOperatorKind.Multiplication => (int) left * (int) right,
                        BoundBinaryOperatorKind.Division => (int) left / (int) right,
                        BoundBinaryOperatorKind.BitwiseRightShift => (int) left >> (int) right,
                        BoundBinaryOperatorKind.BitwiseLeftShift => (int) left << (int) right,
                        BoundBinaryOperatorKind.BitwiseAnd when TypeSymbol.CheckType(b.Type, TypeSymbol.Number) => (int) left & (int) right,
                        BoundBinaryOperatorKind.BitwiseAnd when TypeSymbol.CheckType(b.Type, TypeSymbol.Bool) => (bool) left & (bool) right,
                        BoundBinaryOperatorKind.BitwiseOr when TypeSymbol.CheckType(b.Type, TypeSymbol.Number) => (int) left | (int) right,
                        BoundBinaryOperatorKind.BitwiseOr when TypeSymbol.CheckType(b.Type, TypeSymbol.Bool)=> (bool) left | (bool) right,
                        BoundBinaryOperatorKind.BitwiseXor when TypeSymbol.CheckType(b.Type, TypeSymbol.Number) => (int) left ^ (int) right,
                        BoundBinaryOperatorKind.BitwiseXor when TypeSymbol.CheckType(b.Type, TypeSymbol.Bool) => (bool) left ^ (bool) right,
                        BoundBinaryOperatorKind.LogicalAnd => (bool) left && (bool) right,
                        BoundBinaryOperatorKind.LogicalOr => (bool) left || (bool) right,
                        BoundBinaryOperatorKind.LogicalEquals => Equals(left, right),
                        BoundBinaryOperatorKind.LogicalNotEquals => !Equals(left, right),
                        BoundBinaryOperatorKind.GreaterThan => (int) left > (int) right,
                        BoundBinaryOperatorKind.GreaterThanOrEqual => (int) left >= (int) right,
                        BoundBinaryOperatorKind.LessThan => (int) left < (int) right,
                        BoundBinaryOperatorKind.LessThanOrEqual => (int) left <= (int) right,
                        _ => throw new Exception($"Unexpected Binary Operator '{b.Op.Kind}'")
                    };
                }
                case BoundCallExpression c: 
                    if (c.Function == BuiltinFunctions.Input) return Console.ReadLine();
                    
                    if (c.Function == BuiltinFunctions.Print)
                    {
                        var message = (string)EvaluateExpression(c.Arguments[0]);
                        Console.WriteLine(message);
                        return null;
                    }
                    throw new Exception($"Unexpected function {c.Function}");
                
                default:
                    throw new Exception($"Unexpected Node '{node.Type}'");
            }
        }
    }
}