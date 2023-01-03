using System.Text;
using Shore.CodeAnalysis.Binding;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundProgram _program;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private readonly Dictionary<VariableSymbol, object[]> _globalArrays;
        private readonly Dictionary<VariableSymbol, Dictionary<VariableSymbol, object>> _globalLists;
        private readonly Dictionary<VariableSymbol, Dictionary<object, object>> _globalDicts;
        private readonly Dictionary<FunctionSymbol, BoundBlockStatement> _functions = new();
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();
        private readonly Stack<Dictionary<VariableSymbol, object[]>> _localArrays = new();
        private readonly Stack<Dictionary<VariableSymbol, Dictionary<VariableSymbol, object>>> _localLists = new();
        private readonly Stack<Dictionary<VariableSymbol, Dictionary<object, object>>> _localDicts = new();

        private object? _lastValue;

        public Evaluator(BoundProgram program, Dictionary<VariableSymbol, object> variables,
            Dictionary<VariableSymbol, object[]> arrays,
            Dictionary<VariableSymbol, Dictionary<VariableSymbol, object>> lists,
            Dictionary<VariableSymbol, Dictionary<object, object>> dicts)
        {
            _program = program;
            _globals = variables;
            _globalArrays = arrays;
            _globalLists = lists;
            _globalDicts = dicts;
            _locals.Push(new Dictionary<VariableSymbol, object>());
            _localArrays.Push(new Dictionary<VariableSymbol, object[]>());
            _localLists.Push(new Dictionary<VariableSymbol, Dictionary<VariableSymbol, object>>());
            _localDicts.Push(new Dictionary<VariableSymbol, Dictionary<object, object>>());

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
                    case BoundNodeKind.ArrayDeclaration:
                        EvaluateArrayDeclaration((BoundArrayDeclaration)s);
                        index++;
                        break;
                    case BoundNodeKind.ListDeclaration:
                        EvaluateListDeclaration((BoundListDeclaration)s);
                        index++;
                        break;
                    case BoundNodeKind.DictDeclaration:
                        EvaluateDictDeclaration((BoundDictDeclaration)s);
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

        private void EvaluateArrayDeclaration(BoundArrayDeclaration node) =>
            AssignArray(node.Array, node.Members.Select(EvaluateExpression).ToArray());

        private void EvaluateListDeclaration(BoundListDeclaration node)
        {
            Dictionary<VariableSymbol, object> values = new();
            foreach (var pair in node.Members)
            {
                var evaluatedValue = EvaluateExpression(pair.Value);
                values.Add(pair.Key, evaluatedValue);
            }
            AssignList(node.Array, values);
        }
        
        private void EvaluateDictDeclaration(BoundDictDeclaration node)
        {
            Dictionary<object, object> values = new();
            foreach (var pair in node.Values)
            {
                var evaluatedKey = EvaluateExpression(pair.Key);
                var evaluatedValue = EvaluateExpression(pair.Value);
                values.Add(evaluatedKey, evaluatedValue);
            }
            AssignDict(node.Array, values);
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node) => _lastValue = EvaluateExpression(node.Expression);

        private object? EvaluateExpression(BoundExpression? node)
        {
            return node!.Kind switch
            {
                BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression)node),
                BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression)node),
                BoundNodeKind.ArrayExpression => EvaluateArrayExpression((BoundArrayExpression)node),
                BoundNodeKind.ListExpression => EvaluateListExpression((BoundListExpression)node),
                BoundNodeKind.DictExpression => EvaluateDictExpression((BoundDictExpression)node),
                BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression)node),
                BoundNodeKind.ListAssignmentExpression => EvaluateListAssignmentExpression((BoundListAssignmentExpression)node),
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression)node),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression)node),
                BoundNodeKind.CallExpression => EvaluateCallExpression((BoundCallExpression)node),
                BoundNodeKind.ConversionExpression => EvaluateConversionExpression((BoundConversionExpression)node),
                _ => throw new Exception($"Unexpected Node {node.Kind}")
            };
        }

        private object EvaluateLiteralExpression(BoundLiteralExpression n) => n.Value;

        private object EvaluateVariableExpression(BoundVariableExpression v, bool getLength = false)
        {
            if (v.Type.HeadType == TypeSymbol.Array)
            {
                var sb = new StringBuilder();
                if (v.Variable.Kind == SymbolKind.GlobalVariable)
                {
                    foreach(var value in _globalArrays[v.Variable]) sb.Append($"{value}, ");
                    return getLength? (long)_globalArrays[v.Variable].Length : $"[{sb.Remove(sb.Length-2, 2)}]";   
                }
                
                var localArrays = _localArrays.Peek();
                foreach (var value in localArrays.SelectMany(value => value.Value)) sb.Append($"{value}, ");
                return getLength ? (long)localArrays[v.Variable].Length : $"[{sb.Remove(sb.Length-2, 2)}]";   
            }
            
            if (v.Type.HeadType == TypeSymbol.List)
            {
                var sb = new StringBuilder();
                if (v.Variable.Kind == SymbolKind.GlobalVariable)
                {
                    foreach(var value in _globalLists[v.Variable]) sb.Append($"{value.Value}, ");
                    return getLength? (long)_globalLists[v.Variable].Count : $"[{sb.Remove(sb.Length-2, 2)}]";   
                }
                
                var localArrays = _localLists.Peek();
                foreach (var value in localArrays.SelectMany(value => value.Value)) sb.Append($"{value.Value}, ");
                return getLength ? (long)localArrays[v.Variable].Count : $"[{sb.Remove(sb.Length-2, 2)}]";   
            }
            
            if (v.Type.HeadType == TypeSymbol.Dictionary)
            {
                var sb = new StringBuilder();
                if (v.Variable.Kind == SymbolKind.GlobalVariable)
                {
                    foreach(var value in _globalDicts[v.Variable]) sb.Append($"{value.Key} : {value.Value}, ");
                    return getLength? (long)_globalDicts[v.Variable].Count : $"[{sb.Remove(sb.Length-2, 2)}]";   
                }
                
                var localArrays = _localDicts.Peek();
                foreach (var value in localArrays.SelectMany(value => value.Value)) sb.Append($"{value.Key} : {value.Value}, ");
                return getLength ? (long)localArrays[v.Variable].Count : $"[{sb.Remove(sb.Length-2, 2)}]";   
            }
            
            if (v.Variable!.Kind == SymbolKind.GlobalVariable) return _globals[v.Variable];
            
            var locals = _locals.Peek();
            return locals[v.Variable];
        }
        
        private object EvaluateArrayExpression(BoundArrayExpression a)
        {
            var accessor = EvaluateExpression(a.Accessor);
            if (a.Array.Kind == SymbolKind.GlobalVariable) return _globalArrays[a.Array][Convert.ToInt64(accessor)];
           
            var locals = _localArrays.Peek();
            return locals[a.Array][Convert.ToInt64(accessor)];
        }
        
        private object EvaluateListExpression(BoundListExpression a)
        {
            var accessor = EvaluateExpression(a.Accessor);
            
            if (a.Array.Kind == SymbolKind.GlobalVariable)
                return _globalLists[a.Array].ElementAt(Convert.ToInt32(accessor)).Value;
           
            var locals = _localLists.Peek();
            return locals[a.Array].ElementAt(Convert.ToInt32(accessor)).Value;
        }
        
        private object EvaluateDictExpression(BoundDictExpression a)
        {
            var accessor = EvaluateExpression(a.Accessor);
            
            if (a.Array.Kind == SymbolKind.GlobalVariable)
                return _globalDicts[a.Array][accessor];
           
            var locals = _localDicts.Peek();
            return locals[a.Array][accessor];
        }

        private object? EvaluateAssignmentExpression(BoundAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            Assign(a.Variable, value);
            return value;
        }
        
        private object? EvaluateListAssignmentExpression(BoundListAssignmentExpression a)
        {
            var value = EvaluateExpression(a.Expression);
            var accessor = EvaluateExpression(a.Accessor);
            AssignListValue(a.Variable, value, accessor);
            return value;
        }

        private object? EvaluateUnaryExpression(BoundUnaryExpression u)
        {
            var operand = EvaluateExpression(u.Operand);

            return u.Op.Kind switch
            {
                BoundUnaryOperatorKind.Identity => operand,
                BoundUnaryOperatorKind.Negation => u.Operand.Type == TypeSymbol.Int64 ? 
                    -Convert.ToInt64(operand) : -Convert.ToDouble(operand),
                BoundUnaryOperatorKind.LogicalNegation => !(bool)operand,
                BoundUnaryOperatorKind.OnesComplement => ~Convert.ToInt64(operand),
                _ => throw new Exception($"Unexpected unary operator {u.Op}")
            };
        }

        private object? EvaluateBinaryExpression(BoundBinaryExpression b)
        {
            var useFloat = b.Left.Type == TypeSymbol.Float64 || b.Right.Type == TypeSymbol.Float64;
                var left = EvaluateExpression(b.Left);
            var right = EvaluateExpression(b.Right);

            switch (b.Op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    if (useFloat) return Convert.ToDouble(left) + Convert.ToDouble(right);
                    if (b.Type == TypeSymbol.Int64) return Convert.ToInt64(left) + Convert.ToInt64(right);
                    return (string)left! + (string)right!;
                case BoundBinaryOperatorKind.Subtraction: 
                    if (useFloat) return Convert.ToDouble(left) - Convert.ToDouble(right);
                    return Convert.ToInt64(left) - Convert.ToInt64(right);
                case BoundBinaryOperatorKind.Multiplication: 
                    if (useFloat) return Convert.ToDouble(left) * Convert.ToDouble(right);
                    return Convert.ToInt64(left) * Convert.ToInt64(right);
                case BoundBinaryOperatorKind.Division:
                    if (Convert.ToDouble(left) == 0 && Convert.ToDouble(right) == 0) return "undefined";
                    if (useFloat) return Convert.ToDouble(left) / Convert.ToDouble(right);
                    if (b.Type == TypeSymbol.Int64) return Convert.ToInt64(left) / Convert.ToInt64(right);
                    return left.ToString()[Convert.ToInt32(right)].ToString();
                case BoundBinaryOperatorKind.Exponentiation: 
                    if (useFloat) return Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right));
                    return (long)Math.Pow(Convert.ToInt64(left), Convert.ToInt64(right));
                case BoundBinaryOperatorKind.BitwiseAnd:
                    if (b.Type == TypeSymbol.Float64) return Convert.ToInt64(left) & Convert.ToInt64(right);
                    return (bool)left! & (bool)right!;
                case BoundBinaryOperatorKind.BitwiseOr:
                    if (b.Type == TypeSymbol.Int64) return Convert.ToInt64(left) | Convert.ToInt64(right);
                    return (bool)left! | (bool)right!;
                case BoundBinaryOperatorKind.BitwiseXor:
                    if (b.Type == TypeSymbol.Int64) return Convert.ToInt64(left) ^ Convert.ToInt64(right);
                    return (bool)left! ^ (bool)right!;
                case BoundBinaryOperatorKind.BitwiseLeftShift: return Convert.ToInt64(left) << Convert.ToInt32(right);
                case BoundBinaryOperatorKind.BitwiseRightShift: return Convert.ToInt64(left) >> Convert.ToInt32(right);
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool)left! && (bool)right!;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool)left! || (bool)right!;
                case BoundBinaryOperatorKind.LogicalEquals:
                    return Equals(left, right);
                case BoundBinaryOperatorKind.LogicalNotEquals:
                    return !Equals(left, right);
                case BoundBinaryOperatorKind.LessThan:
                    if (useFloat) return Convert.ToDouble(left) < Convert.ToDouble(right);
                    return Convert.ToInt64(left) < Convert.ToInt64(right);
                case BoundBinaryOperatorKind.LessThanOrEqual:
                    if (useFloat) return Convert.ToDouble(left) <= Convert.ToDouble(right);
                    return Convert.ToInt64(left) <= Convert.ToInt64(right);
                case BoundBinaryOperatorKind.GreaterThan:
                    if (useFloat) return Convert.ToDouble(left) > Convert.ToDouble(right);
                    return Convert.ToInt64(left) > Convert.ToInt64(right);
                case BoundBinaryOperatorKind.GreaterThanOrEqual:
                    if (useFloat) return Convert.ToDouble(left) >= Convert.ToDouble(right);
                    return Convert.ToInt64(left) >= Convert.ToInt64(right);
                default:
                    throw new Exception($"Unexpected Binary Operator {b.Op}");
            }
        }

        private object? EvaluateCallExpression(BoundCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input) return Console.ReadLine();

            if (node.Function == BuiltinFunctions.UnixTimestamp)
            {
                DateTimeOffset offset = new DateTimeOffset(DateTime.UtcNow);
                return offset.ToUnixTimeMilliseconds().ToString();
            }

            if (node.Function == BuiltinFunctions.Print)
            {
                var value = EvaluateExpression(node.Arguments[0])!;
                Console.WriteLine(value);
                return null;
            }

            if (node.Function == BuiltinFunctions.Round)
            {
                var value = EvaluateExpression(node.Arguments[0]);
                var round = Math.Round((double)value);
                return Convert.ToInt64(round);
            }
            
            if (node.Function == BuiltinFunctions.Floor)
            {
                var value = EvaluateExpression(node.Arguments[0]);
                var round = Math.Floor((double)value);
                return Convert.ToInt64(round);
            }
            
            if (node.Function == BuiltinFunctions.Ceil)
            {
                var value = EvaluateExpression(node.Arguments[0]);
                var round = Math.Ceiling((double)value);
                return Convert.ToInt64(round);
            }

            if (node.Function == BuiltinFunctions.Length)
            {
                if (node.Arguments[0].Type == TypeSymbol.String)
                {
                    var value = EvaluateExpression(node.Arguments[0]);
                    return Convert.ToInt64(Convert.ToString(value).Length);
                }
                return EvaluateVariableExpression((BoundVariableExpression)node.Arguments[0], true);
            }
            
            if (node.Function == BuiltinFunctions.Sleep)
            {
                var value = EvaluateExpression(node.Arguments[0]);
                Thread.Sleep(Convert.ToInt32(value));
                return 0;
            }

            var locals = new Dictionary<VariableSymbol?, object?>();
            for (var i = 0; i < node.Arguments.Length; i++)
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
            if (node.Type == TypeSymbol.Float64) return Convert.ToDouble(value);
            if (node.Type == TypeSymbol.Int64) return Convert.ToInt64(value);
            if (node.Type == TypeSymbol.Float64List) return Convert.ToDouble(value);
            if (node.Type == TypeSymbol.Int64List) return Convert.ToInt64(value);
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

        private void AssignArray(VariableSymbol array, object[] values)
        {
            if (array.Kind == SymbolKind.GlobalVariable) _globalArrays[array] = values;
            else
            {
                var locals = _localArrays.Peek();
                locals[array] = values;
            }
        }
        
        private void AssignList(VariableSymbol array, Dictionary<VariableSymbol, object> values)
        {
            if (array.Kind == SymbolKind.GlobalVariable) _globalLists[array] = values;
            else
            {
                var locals = _localLists.Peek();
                locals[array] = values;
            }
        }
        
        private void AssignListValue(VariableSymbol array, object value, object accessor)
        {
            if (array.Kind == SymbolKind.GlobalVariable)
            {
                var oldEntry = _globalLists[array].ElementAt(Convert.ToInt32(accessor));
                _globalLists[array][oldEntry.Key] = value;
            }
            else
            {
                var locals = _localLists.Peek();
                var oldEntry = locals[array].ElementAt(Convert.ToInt32(accessor));
                locals[array][oldEntry.Key] = value;
            }
        }
        
        private void AssignDict(VariableSymbol array, Dictionary<object, object> values)
        {
            if (array.Kind == SymbolKind.GlobalVariable) _globalDicts[array] = values;
            else
            {
                var locals = _localDicts.Peek();
                locals[array] = values;
            }
        }
        
        /*private void AssignListValue(VariableSymbol array, object value, object accessor)
        {
            if (array.Kind == SymbolKind.GlobalVariable)
            {
                var oldEntry = _globalLists[array].ElementAt(Convert.ToInt32(accessor));
                _globalLists[array][oldEntry.Key] = value;
            }
            else
            {
                var locals = _localLists.Peek();
                var oldEntry = locals[array].ElementAt(Convert.ToInt32(accessor));
                locals[array][oldEntry.Key] = value;
            }
        }
        */
    }
}