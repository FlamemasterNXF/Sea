using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram _program;
        private readonly Dictionary<VariableSymbol?, object?> _globals;
        private readonly Dictionary<FunctionSymbol?, BoundBlockStatement> _functions = new();
        private readonly Stack<Dictionary<VariableSymbol?, object?>> _locals = new();
        private Random _random;

        private object? _lastValue;

        public Evaluator(BoundProgram program, Dictionary<VariableSymbol?, object?> variables)
        {
            _program = program;
            _globals = variables;
            _locals.Push(new Dictionary<VariableSymbol?, object?>());

            var current = program;
            while (current != null)
            {
                foreach (var kv in current.Functions)
                {
                    var function = kv.Key;
                    var body = kv.Value;
                    _functions.Add(function, body);
                }

                current = current.Previous;
            }
        }

        public object? Evaluate()
        {
            var function = _program.MainFunction ?? _program.ScriptFunction;
            if (function is null) return null;

            var body = _functions[function];
            return EvaluateStatement(body);
        }

        private object? EvaluateStatement(BoundBlockStatement body)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (var i = 0; i < body.Statements.Length; i++)
            {
                if (body.Statements[i] is BoundLabelStatement l) labelToIndex.Add(l.BoundLabel, i + 1);
            }

            var index = 0;

            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];

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
                    case BoundNodeKind.ConditionalGotoStatement:
                        var cgs = (BoundConditionalGotoStatement)s;
                        var condition = (bool)EvaluateExpression(cgs.Condition)!;
                        if (condition == cgs.JumpIfTrue) index = labelToIndex[cgs.BoundLabel];
                        else index++;
                        break;
                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;
                    case BoundNodeKind.ReturnStatement:
                        var rs = (BoundReturnStatement)s;
                        _lastValue = rs.Expression == null ? null : EvaluateExpression(rs.Expression);
                        return _lastValue;
                    default:
                        throw new Exception($"Unexpected Node {s.Kind}");
                }
            }

            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _lastValue = value;
            Assign(node.Variable, value);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node) => _lastValue = EvaluateExpression(node.Expression);

        private object? EvaluateExpression(BoundExpression? node)
        {
            return node!.Kind switch
            {
                BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression)node),
                BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression)node),
                BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression)node),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression)node),
                BoundNodeKind.CallExpression => EvaluateCallExpression((BoundCallExpression)node),
                BoundNodeKind.ConversionExpression => EvaluateConversionExpression((BoundConversionExpression)node),
                _ => throw new Exception($"Unexpected Node {node.Kind}")
            };
        }

        private static object? EvaluateLiteralExpression(BoundLiteralExpression n) => n.Value;

        private object? EvaluateVariableExpression(BoundVariableExpression v)
        {
            if (v.Variable!.Kind == SymbolKind.GlobalVariable) return _globals[v.Variable];
            else
            {
                var locals = _locals.Peek();
                return locals[v.Variable];
            }
        }

        private object? EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            Assign(a.Variable, value);
            return value;
        }

        private object? EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            return u.Op!.Kind switch
            {
                BoundUnaryOperatorKind.Identity => operand,
                BoundUnaryOperatorKind.Negation => -(int)operand!,
                BoundUnaryOperatorKind.LogicalNegation => !(bool)operand!,
                BoundUnaryOperatorKind.OnesComplement => ~(int)operand!,
                _ => throw new Exception($"Unexpected unary operator {u.Op}")
            };
        }

        private object? EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (b.Type!.ParentType == TypeSymbol.Number) return (int)left! + (int)right!;
                    return (string)left! + (string)right!;
                case BoundBinaryOperatorKind.Subtraction: return (int)left! - (int)right!;
                case BoundBinaryOperatorKind.Multiplication: return (int)left! * (int)right!;
                case BoundBinaryOperatorKind.Division: return (int)left! / (int)right!;
                case BoundBinaryOperatorKind.Exponentiation: return (int)Math.Pow((int)left!, (int)right!);
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (b.Type!.ParentType == TypeSymbol.Number) return (int)left! & (int)right!;
                    return (bool)left! & (bool)right!;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (b.Type!.ParentType == TypeSymbol.Number) return (int)left! | (int)right!;
                    return (bool)left! | (bool)right!;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (b.Type!.ParentType == TypeSymbol.Number) return (int)left! ^ (int)right!;
                    return (bool)left! ^ (bool)right!;
                case BoundBinaryOperatorKind.BitwiseLeftShift: return (int)left! << (int)right!;
                case BoundBinaryOperatorKind.BitwiseRightShift: return (int)left! >> (int)right!;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left! && (bool)right!;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left! || (bool)right!;
                case BoundBinaryOperatorKind.LogicalEquals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.LogicalNotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.LessThan:
                    return (int)left! < (int)right!;
                case BoundBinaryOperatorKind.LessThanOrEqual:
                    return (int)left! <= (int)right!;
                case BoundBinaryOperatorKind.GreaterThan:
                    return (int)left! > (int)right!;
                case BoundBinaryOperatorKind.GreaterThanOrEqual:
                    return (int)left! >= (int)right!;
                default:
                    throw new Exception($"Unexpected Binary Operator {b.Op}");
            }
        }

        private object? EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
            {
                return Console.ReadLine();
            }

            if (node.Function == BuiltinFunctions.Print)
            {
                var value = EvaluateExpression(node.Arguments[0])!;
                Console.WriteLine(value);
                return null;
            }

            var locals = new Dictionary<VariableSymbol?, object?>();
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                var parameter = node.Function.Parameters[i];
                var value = EvaluateExpression(node.Arguments[i]);
                locals.Add(parameter, value);
            }

            _locals.Push(locals);

            var statement = _functions[node.Function];
            var result = EvaluateStatement(statement);

            _locals.Pop();

            return result;
        }

        private object? EvaluateConversionExpression(BoundConversionExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            if (node.Type == TypeSymbol.Any) return value;
            if (node.Type == TypeSymbol.Bool) return Convert.ToBoolean(value);
            if (node.Type?.ParentType == TypeSymbol.Number) return Convert.ToInt32(value);
            if (node.Type == TypeSymbol.String) return Convert.ToString(value);
            throw new Exception($"Unexpected type {node.Type}");
        }

        private void Assign(VariableSymbol? variable, object? value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable) _globals[variable] = value;
            else
            {
                var locals = _locals.Peek();
                locals[variable] = value;
            }
        }
    }
}