using System.Collections.Immutable;
using Shore.CodeAnalysis.Binding.ControlFlow;
using Shore.CodeAnalysis.Binding.Converting;
using Shore.CodeAnalysis.Lowering;
using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly bool _isScript;
        private readonly FunctionSymbol? _function;
        private BoundScope? _scope;
        private readonly DiagnosticBag _diagnostics = new();
        private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopStack = new();
        private int _labelCounter;

        private Binder(bool isScript, BoundScope? parent, FunctionSymbol? function)
        {
            _isScript = isScript;
            _function = function;
            _scope = new BoundScope(parent);

            if (function is not null)
            {
                foreach (var parameter in function.Parameters) _scope.TryDeclareVariable(parameter);
            }
        }

        private DiagnosticBag Diagnostics => _diagnostics;
        
        private static BoundScope CreateParentScope(BoundGlobalScope? previous)
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
                foreach (var f in previous.Functions) scope.TryDeclareFunction(f);

                foreach (var v in previous.Variables) scope.TryDeclareVariable(v);

                parent = scope;
            }

            return parent;
        }

        public static BoundGlobalScope BindGlobalScope(bool isScript, BoundGlobalScope? previous,
            ImmutableArray<NodeTree> nodeTrees)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(isScript, parentScope, null);

            var functionDeclarations = nodeTrees.SelectMany(nt => nt.Root.Members).OfType<FunctionDeclarationNode>();

            foreach (var function in functionDeclarations) binder.BindFunctionDeclaration(function);

            var globalStatements = nodeTrees.SelectMany(nt => nt.Root.Members).OfType<GlobalStatementNode>();

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            var globalStatementNodes = globalStatements as GlobalStatementNode[] ?? globalStatements.ToArray();
            foreach (var globalStatement in globalStatementNodes)
            {
                var statement = binder.BindGlobalStatement(globalStatement.Statement);
                statements.Add(statement);
            }

            var firstGlobalStatementPerNodeTree = nodeTrees
                .Select(nt => nt.Root.Members.OfType<GlobalStatementNode>().FirstOrDefault()).Where(g => g != null)
                .ToArray();

            if (firstGlobalStatementPerNodeTree.Length > 1)
                foreach (var globalStatement in firstGlobalStatementPerNodeTree)
                    binder.Diagnostics.ReportOnlyOneFileCanHaveGlobalStatements(globalStatement!.Location);

            var functions = binder._scope!.GetDeclaredFunctions();

            FunctionSymbol? mainFunction;
            FunctionSymbol? scriptFunction;

            if (isScript)
            {
                mainFunction = null;
                scriptFunction = globalStatementNodes.Any()
                    ? new FunctionSymbol("$eval", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Any)
                    : null;
            }
            else
            {
                mainFunction = functions.FirstOrDefault(f => f?.Name == "main");
                scriptFunction = null;

                if (mainFunction is not null)
                {
                    if (mainFunction.Type != TypeSymbol.Void || mainFunction.Parameters.Any())
                        binder.Diagnostics.ReportMainMustHaveCorrectSignature(mainFunction.Declaration!.Identifier
                            .Location);
                }

                if (globalStatementNodes.Any())
                {
                    if (mainFunction is not null)
                    {
                        binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(mainFunction.Declaration!.Identifier
                            .Location);

                        foreach (var globalStatement in firstGlobalStatementPerNodeTree)
                            if (mainFunction != null)
                                binder.Diagnostics.ReportCannotMixMainAndGlobalStatements(globalStatement!.Location);
                    }
                    else
                    {
                        mainFunction = new FunctionSymbol("main", ImmutableArray<ParameterSymbol>.Empty,
                            TypeSymbol.Void);
                    }
                }
            }

            var diagnostics = binder.Diagnostics.ToImmutableArray();
            var variables = binder._scope.GetDeclaredVariables();
            
            if (previous is not null) diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, mainFunction, scriptFunction, functions, variables,
                statements.ToImmutable());
        }

        public static BoundProgram BindProgram(bool isScript, BoundProgram? previous, BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);

            ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach (var function in globalScope.Functions)
            {
                var binder = new Binder(isScript, parentScope, function);
                var body = binder.BindStatementDistributor(function?.Declaration?.Body);
                var loweredBody = Lowerer.Lower(body);
                
                if (function?.Type != TypeSymbol.Void && !ControlFlowGraph.AllPathsReturn(loweredBody))
                    binder._diagnostics.ReportAllPathsMustReturn(function!.Declaration!.Identifier.Location);
                
                functionBodies.Add(function!, loweredBody);
                diagnostics.AddRange(binder.Diagnostics);
            }
            
            if (globalScope.MainFunction != null && globalScope.Statements.Any())
            {
                var body = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));
                functionBodies.Add(globalScope.MainFunction, body);
            }
            else if (globalScope.ScriptFunction != null)
            {
                var statements = globalScope.Statements;
                if (statements is [BoundExpressionStatement es] &&
                    es.Expression.Type != TypeSymbol.Void)
                {
                    statements = statements.SetItem(0, new BoundReturnStatement(es.Expression));
                }
                else if (statements.Any() && statements.Last().Kind != BoundNodeKind.ReturnStatement)
                {
                    var nullValue = new BoundLiteralExpression("");
                    statements = statements.Add(new BoundReturnStatement(nullValue));
                }

                var body = Lowerer.Lower(new BoundBlockStatement(statements));
                functionBodies.Add(globalScope.ScriptFunction, body);
            }

            return new BoundProgram(previous, diagnostics.ToImmutable(), globalScope.MainFunction, 
                globalScope.ScriptFunction, functionBodies.ToImmutable());
        }
        
        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);
            foreach (var function in BuiltinFunctions.GetAll()) result.TryDeclareFunction(function);
            return result;
        }

        private void BindFunctionDeclaration(FunctionDeclarationNode node)
        {
            ImmutableArray<ParameterSymbol>.Builder parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();

            var seenParameterNames = new HashSet<string?>();

            foreach (var parameterNode in node.Parameters)
            {
                var parameterName = parameterNode.Identifier.Text;
                var parameterType = LookupType(parameterNode.PType.Text);
                if (!seenParameterNames.Add(parameterName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(parameterNode.Location, parameterName);
                }
                else
                {
                    var parameter = new ParameterSymbol(parameterName!, parameterType!);
                    parameters.Add(parameter);
                }
            }

            var type = LookupType(node.FType.Text) ?? TypeSymbol.Void;
            
            var function = new FunctionSymbol(node.Identifier.Text!, parameters.ToImmutable(), type, node);
            if (!_scope!.TryDeclareFunction(function))
                _diagnostics.ReportSymbolAlreadyDeclared(node.Identifier.Location, function.Name);
        }

        private BoundStatement BindNullStatement()
        {
            return new BoundExpressionStatement(new BoundNullExpression());
        }

        private BoundStatement BindGlobalStatement(StatementNode node) => BindStatement(node, true);

        private BoundStatement BindStatement(StatementNode node, bool isGlobal = false)
        {
            var result = BindStatementDistributor(node);

            if (!_isScript || !isGlobal)
            {
                if (result is BoundExpressionStatement e)
                {
                    var isAllowedExpression = e.Expression.Kind is BoundNodeKind.NullExpression
                        or BoundNodeKind.AssignmentExpression or BoundNodeKind.CallExpression;
                    
                    if (!isAllowedExpression) _diagnostics.ReportInvalidExpressionStatement(node.Location);
                }
            }

            return result;
        }

        private BoundStatement BindStatementDistributor(StatementNode? node)
        {
            return node?.Type switch
            {
                TokType.BlockStatement => BindBlockStatement((BlockStatementNode)node),
                TokType.VariableDeclarationStatement => BindVariableDeclaration((VariableDeclarationNode)node),
                TokType.ArrayDeclarationStatement => BindArrayDeclaration((ArrayDeclarationNode)node),
                TokType.ListDeclarationStatement => BindListDeclaration((ListDeclarationNode)node),
                TokType.IfStatement => BindIfStatement((IfStatementNode)node),
                TokType.WhileStatement => BindWhileStatement((WhileStatementNode)node),
                TokType.ForStatement => BindForStatement((ForStatementNode)node),
                TokType.BreakStatement => BindBreakStatement((BreakStatementNode)node),
                TokType.ContinueStatement => BindContinueStatement((ContinueStatementNode)node),
                TokType.ReturnStatement => BindReturnStatement((ReturnStatementNode)node),
                TokType.ExpressionStatement => BindExpressionStatement((ExpressionStatementNode)node),
                _ => throw new Exception($"Unexpected Node {node?.Type}")
            };
        }

        private BoundStatement BindBlockStatement(BlockStatementNode node)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementNode in node.Statements)
            {
                var statement = BindStatementDistributor(statementNode);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationNode node)
        {
            var isReadOnly = node.VType.Type == TokType.ReadOnlyKeyword;
            var initializer = BindExpressionDistributor(node.Initializer);
            var type = LookupType(node.VType.Text) ?? initializer.Type;
            var variable = BindVariable(node.Identifier, isReadOnly, type);

            var convertedInitializer = BindConversion(node.Initializer.Location, initializer, type);
            return new BoundVariableDeclaration(variable, convertedInitializer);
        }
        
        private BoundStatement BindArrayDeclaration(ArrayDeclarationNode node)
        {
            var type = LookupType(node.AType.Text);

            ImmutableArray<BoundExpression>.Builder boundMembers = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var member in node.Members)
            {
                var boundMember = BindExpression(member);

                if (boundMember.Type != TypeSymbol.GetAcceptedType(type))
                    _diagnostics.ReportCannotConvertImplicitly(node.Identifier.Location, boundMember.Type, type);
                
                boundMembers.Add(boundMember);
            }

            var array = BindArray(node, type);
            
            //var convertedInitializer = BindConversion(node.Initializer.Location, initializer, type);
            return new BoundArrayDeclaration(array, boundMembers.ToImmutable());
        }
        
        private BoundStatement BindListDeclaration(ListDeclarationNode node)
        {
            var type = LookupType(node.AType.Text);

            var boundMembers = new List<BoundExpression>();

            foreach (var member in node.Members)
            {
                var boundMember = BindExpression(member);

                if (boundMember.Type != TypeSymbol.GetAcceptedType(type))
                    _diagnostics.ReportCannotConvertImplicitly(node.Identifier.Location, boundMember.Type, type);
                
                boundMembers.Add(boundMember);
            }

            var array = BindList(node, type);

            var variables = ImmutableArray.CreateBuilder<VariableSymbol>();
            for (var i = 0; i < boundMembers.Count; i++)
            {
                if (_function != null) variables.Add(new LocalVariableSymbol($"%HIDDEN_{node.Identifier.Text}_{i}_{_function.Name}", false, TypeSymbol.GetAcceptedType(type)));
                variables.Add(new GlobalVariableSymbol($"%HIDDEN_{node.Identifier.Text}_{i}", false, TypeSymbol.GetAcceptedType(type)));
            }

            variables.ToImmutable();
            
            var dict = new Dictionary<VariableSymbol, BoundExpression>();
            for (var i = 0; i < variables.Count; i++)
            {
                dict.Add(variables[i], boundMembers[i]);
            }

            return new BoundListDeclaration(array, dict);
        }

        private BoundStatement BindIfStatement(IfStatementNode node)
        {
            var condition = BindExpressionDistributor(node.Condition, TypeSymbol.Bool);
            var thenStatement = BindStatementDistributor(node.ThenStatement);
            var elseStatement = node.ElseNode == null ? null : BindStatementDistributor(node.ElseNode.ElseStatement);
            return new BoundIfStatement(condition, thenStatement, elseStatement);
        }

        private BoundStatement BindWhileStatement(WhileStatementNode node)
        {
            var condition = BindExpressionDistributor(node.Condition, TypeSymbol.Bool);
            var body = BindLoopBody(node.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, body, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatement(ForStatementNode node)
        {
            var lowerBound = BindExpressionDistributor(node.LowerBound, TypeSymbol.Int64);
            var upperBound = BindExpressionDistributor(node.UpperBound, TypeSymbol.Int64);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(node.Identifier, true, TypeSymbol.Int64);
            var body = BindLoopBody(node.Body, out var breakLabel, out var continueLabel);

            _scope = _scope.Parent;
            return new BoundForStatement(variable, lowerBound, upperBound, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(StatementNode body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _labelCounter++;
            breakLabel = new BoundLabel($"break{_labelCounter}");
            continueLabel = new BoundLabel($"continue{_labelCounter}");
            
            _loopStack.Push((breakLabel, continueLabel));
            var boundBody = BindStatementDistributor(body);
            _loopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindBreakStatement(BreakStatementNode node)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(node.Keyword.Location, node.Keyword.Text);
                return BindNullStatement();
            }

            var breakLabel = _loopStack.Peek().BreakLabel;
            return new BoundGotoStatement(breakLabel);
        }
        
        private BoundStatement BindContinueStatement(ContinueStatementNode node)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportInvalidBreakOrContinue(node.Keyword.Location, node.Keyword.Text);
                return BindNullStatement();
            }

            var continueLabel = _loopStack.Peek().ContinueLabel;
            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindReturnStatement(ReturnStatementNode node)
        {
            var expression = node.Expression == null ? null : BindExpression(node.Expression);

            if (_function == null)
            {
                if (_isScript) expression ??= new BoundLiteralExpression("");
                else if (expression != null)
                    _diagnostics.ReportInvalidReturnWithValueInGlobalStatements(node.Expression!.Location);
            }
            else
            {
                if (_function.Type == TypeSymbol.Void && expression != null) 
                    _diagnostics.ReportInvalidReturnExpression(node.Expression!.Location, _function.Name);
                else
                {
                    if (expression == null) _diagnostics.ReportMissingReturnExpression(node.Keyword.Location, _function.Type);
                    else expression = BindConversion(node.Expression!.Location, expression, _function.Type);
                }
            }

            return new BoundReturnStatement(expression);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementNode node)
        {
            var expression = BindExpression(node.Expression, true);
            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionNode node, TypeSymbol? targetType)
        {
            return BindConversion(node, targetType);
        }

        private BoundExpression BindExpression(ExpressionNode node, bool canBeVoid = false)
        {
            var result = BindExpressionDistributor(node);
            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionMustHaveValue(node.Location);
                return new BoundNullExpression();
            }

            return result;
        }

        private BoundExpression BindExpressionDistributor(ExpressionNode node, TypeSymbol? targetType)
        {
            var result = BindExpressionDistributor(node);
            if (targetType != TypeSymbol.Null && result.Type != TypeSymbol.Null && result.Type != targetType)
                _diagnostics.ReportCannotConvert(node.Location, result.Type, targetType);
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
                TokType.ArrayAccessExpression => BindArrayAccessExpression((ArrayAccessExpressionNode)node),
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

            if (!_scope!.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(node.IdentifierToken.Location, name);
                return new BoundNullExpression();
            }


            return new BoundVariableExpression(variable);
        }
        
        private BoundExpression BindArrayAccessExpression(ArrayAccessExpressionNode node)
        {
            var name = node.Identifier.Text;
            var accessor = BindExpression(node.Accessor);

            if (string.IsNullOrEmpty(name)) return new BoundNullExpression();

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(node.Identifier.Location, name);
                return new BoundNullExpression();
            }

            if (variable.IsList) return new BoundListExpression(variable, accessor);

            return new BoundArrayExpression(variable, accessor);
        }
        

        private BoundExpression BindAssignmentExpression(AssignmentExpressionNode node)
        {
            var name = node.IdentifierToken.Text;
            var boundExpression = BindExpressionDistributor(node.Expression);

            Console.WriteLine(name);

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(node.IdentifierToken.Location, name);
                return boundExpression;
            }

            if (variable!.IsReadOnly) _diagnostics.ReportCannotAssign(node.EqualsToken.Location, name);

            var convertedExpression = BindConversion(node.Expression.Location, boundExpression, variable.Type);
            return new BoundAssignmentExpression(variable, convertedExpression);
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionNode node)
        {
            var value = node.Value ?? (long)0;

            return new BoundLiteralExpression(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionNode node)
        {
            var boundOperand = BindExpressionDistributor(node.Operand);
            var boundOperator = BoundUnaryOperator.Bind(node.OperatorToken.Type, boundOperand.Type);

            if (boundOperand.Type == TypeSymbol.Null) return new BoundNullExpression();

            if (boundOperator is null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(node.OperatorToken.Location, node.OperatorToken.Text,
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
                _diagnostics.ReportUndefinedBinaryOperator(node.OperatorToken.Location, node.OperatorToken.Text!,
                    boundLeft.Type, boundRight.Type);
                return new BoundNullExpression();
            }

            return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
        }

        private BoundExpression BindCallExpression(CallExpressionNode node)
        {
            if (node.Arguments.Count == 1 && LookupType(node.Identifier.Text) is TypeSymbol type)
                return BindConversion(node.Arguments[0], type, allowExplicit: true);

            ImmutableArray<BoundExpression>.Builder boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

            foreach (var argument in node.Arguments)
            {
                var boundArgument = BindExpression(argument);
                boundArguments.Add(boundArgument);
            }

            if (!_scope!.TryLookupFunction(node.Identifier.Text, out var function))
            {
                _diagnostics.ReportUndefinedFunction(node.Identifier.Location, node.Identifier.Text);
                return new BoundNullExpression();
            }

            if (node.Arguments.Count != function!.Parameters.Length)
            {
                _diagnostics.ReportWrongArgumentCount(node.Location, function.Name, function.Parameters.Length, node.Arguments.Count);
                return new BoundNullExpression();
            }

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var argument = boundArguments[i];
                var parameter = function.Parameters[i];
                
                if ((argument.Type != parameter.Type) && (argument.Type.ParentType != parameter.Type) && (argument.Type.HeadType != parameter.Type) && (parameter.Type != TypeSymbol.Any))
                {
                    _diagnostics.ReportWrongArgumentType(node.Arguments[i].Location, parameter?.Name, parameter?.Type, argument.Type);
                    return new BoundNullExpression();
                }
            }

            return new BoundCallExpression(function, boundArguments.ToImmutable());
        }

        private BoundExpression BindConversion(ExpressionNode node, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(node);
            return BindConversion(node.Location, expression, type, allowExplicit);

        }

        private BoundExpression BindConversion(TextLocation diagnosticLocation, BoundExpression expression, TypeSymbol type, 
            bool allowExplicit = false)
        {
            var conversion = Conversion.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Null && type != TypeSymbol.Null)
                    _diagnostics.ReportCannotConvert(diagnosticLocation, expression.Type, type);
                return new BoundNullExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
                _diagnostics.ReportCannotConvertImplicitly(diagnosticLocation, expression.Type, type);

            if (conversion.IsIdentity) return expression;
            return new BoundConversionExpression(type, expression);
        }

        private VariableSymbol BindVariable(Token identifier, bool isReadOnly, TypeSymbol type)
        {
            var name = identifier.Text ?? "?";
            var declare = !identifier.IsMissing;
            var variable = _function == null
                ? (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type)
                : new LocalVariableSymbol(name, isReadOnly, type);
            
            if (declare && !_scope!.TryDeclareVariable(variable))
                _diagnostics.ReportVariableReDeclaration(identifier.Location, name);

            return variable;
        }

        private VariableSymbol BindArray(ArrayDeclarationNode node, TypeSymbol type)
        {
            var name = node.Identifier.Text ?? "?";
            var array = _function == null ? 
                (VariableSymbol)new GlobalVariableSymbol(name, true, type)
                : new LocalVariableSymbol(name, true, type);

            if (node.Members.Count == 0) _diagnostics.ReportEmptyArray(node.Identifier.Location, name);
            
            if (!_scope!.TryDeclareVariable(array))
                _diagnostics.ReportVariableReDeclaration(node.Identifier.Location, name);

            return array;
        }
        
        private VariableSymbol BindList(ListDeclarationNode node, TypeSymbol type)
        {
            var name = node.Identifier.Text ?? "?";
            var array = _function == null ? 
                (VariableSymbol)new GlobalVariableSymbol(name, true, type, true)
                : new LocalVariableSymbol(name, true, type, true);

            if (node.Members.Count == 0) _diagnostics.ReportEmptyArray(node.Identifier.Location, name);
            
            if (!_scope.TryDeclareVariable(array))
                _diagnostics.ReportVariableReDeclaration(node.Identifier.Location, name);
            
            return array;
        }


        private TypeSymbol? LookupType(string? name)
        {
            return name switch
            {
                "bool" => TypeSymbol.Bool,
                "string" => TypeSymbol.String,
                "int64" or "int" => TypeSymbol.Int64,
                "float64" or "float" => TypeSymbol.Float64,
                "bool[]" => TypeSymbol.BoolArr,
                "string[]" => TypeSymbol.StringArr,
                "int[]" => TypeSymbol.Int64Arr,
                "float[]" => TypeSymbol.Float64Arr,
                "bool<>" => TypeSymbol.BoolList,
                "string<>" => TypeSymbol.StringList,
                "int<>" => TypeSymbol.Int64List,
                "float<>" => TypeSymbol.Float64List,
                _ => null
            };
        }
    }
}