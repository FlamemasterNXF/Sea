using System.Collections.Immutable;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly NodeTree _nodeTree;
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        private readonly ImmutableArray<Token> _tokens;
        private int _position;

        public Parser(NodeTree nodeTree)
        {
            var tokens = new List<Token>();

            var lexer = new Lexer(nodeTree);
            Token token;

            do {
                token = lexer.Lex();
                if (token.Type != TokType.WhitespaceToken && token.Type != TokType.SingleLineCommentToken &&
                    token.Type != TokType.MultiLineCommentToken && token.Type != TokType.UnknownToken) 
                    tokens.Add(token);
            } while (token.Type != TokType.EndOfFileToken);

            _nodeTree = nodeTree;
            _text = nodeTree.Text;
            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }
        
        public DiagnosticBag Diagnostics => _diagnostics;

        private Token PeekToken(int offset)
        {
            var index = _position + offset;
            return index >= _tokens.Length ? _tokens[^1] : _tokens[index];
        }
        
        private Token CurrentToken => PeekToken(0);

        private Token NextToken()
        {
            var current = CurrentToken;
            _position++;
            return current;
        }
        
        private Token MatchToken(TokType type)
        {
            if (CurrentToken.Type == type) return NextToken();
            
            _diagnostics.ReportUnexpectedToken(CurrentToken.Location, CurrentToken.Type, type);
            return new Token(_nodeTree, type, CurrentToken.Position, null, null);
        }
        
        private ExpressionNode ParseExpression(bool isFloat = false)
        {
            return ParseAssignmentExpression(isFloat);
        }
        
        private ExpressionNode ParseAssignmentExpression(bool forceFloat = false)
        {
            if (CurrentToken.Type == TokType.IdentifierToken && PeekToken(1).Type == TokType.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression(forceFloat);
                return new AssignmentExpressionNode(_nodeTree, identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression(forceFloat);
        }
        
        private ExpressionNode ParseBinaryExpression(bool forceFloat, int parentPrecedence = 0)
        {
            ExpressionNode left;
            var unaryOperatorPrecedence = CurrentToken.Type.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(forceFloat, unaryOperatorPrecedence);
                left = new UnaryExpressionNode(_nodeTree, operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression(forceFloat);
            }

            while (true) 
            {
                var precedence = CurrentToken.Type.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence) break;

                var operatorToken = NextToken();
                var right = ParseBinaryExpression(forceFloat, precedence);
                left = new BinaryExpressionNode(_nodeTree, left, operatorToken, right);
            }

            return left;
        }

        public CompilationUnitNode ParseCompilationUnit()
        {
            var members = ParseMembers();
            var eof = MatchToken(TokType.EndOfFileToken);
            return new CompilationUnitNode(_nodeTree, members, eof);
        }

        private ImmutableArray<MemberNode> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberNode>();

            while (CurrentToken.Type != TokType.EndOfFileToken)
            {
                var startToken = CurrentToken;

                var member = ParseMember();
                members.Add(member);
                
                if (CurrentToken == startToken) NextToken();
            }

            return members.ToImmutable();
        }

        private MemberNode ParseMember()
        {
            return CurrentToken.Type == TokType.FunctionKeyword ? ParseFunctionDeclaration() : ParseGlobalStatement();
        }

        private MemberNode ParseFunctionDeclaration()
        {
            var functionKeyword = MatchToken(TokType.FunctionKeyword);
            var type = MatchToken(CurrentToken.Type);
            var identifier = MatchToken(TokType.IdentifierToken);
            var openParenToken = MatchToken(TokType.OpenParenToken);
            var parameters = ParseParameterList();
            var closeParenToken= MatchToken(TokType.CloseParenToken);
            var body = ParseBlockStatement();
            return new FunctionDeclarationNode(_nodeTree, functionKeyword, type, identifier, openParenToken, parameters,
                closeParenToken, body);
        }

        private SeparatedNodeList<ParameterNode> ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();

            var parseNextParameter = true;
            while (parseNextParameter && CurrentToken.Type != TokType.CloseParenToken && CurrentToken.Type != TokType.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);

                if (CurrentToken.Type == TokType.CommaToken)
                {
                    var comma = MatchToken(TokType.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else parseNextParameter = false;
            }

            return new SeparatedNodeList<ParameterNode>(nodesAndSeparators.ToImmutable());
        }

        private ParameterNode ParseParameter()
        {
            var type = MatchToken(CurrentToken.Type);
            var identifier = MatchToken(TokType.IdentifierToken);
            return new ParameterNode(_nodeTree, type, identifier);
        }

        private MemberNode ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementNode(_nodeTree, statement);
        }

        private StatementNode? ParseStatement()
        {
            return CurrentToken.Type switch
            {
                TokType.OpenBraceToken => ParseBlockStatement(),
                TokType.ReadOnlyKeyword => ParseVariableDeclaration(),
                TokType.BoolKeyword or TokType.StringKeyword or TokType.Int64Keyword or TokType.Float64Keyword 
                    => ParseVariableDeclaration(),
                TokType.IfKeyword => ParseIfStatement(),
                TokType.WhileKeyword => ParseWhileStatement(),
                TokType.ForKeyword => ParseForStatement(),
                TokType.BreakKeyword => ParseBreakStatement(),
                TokType.ContinueKeyword => ParseContinueStatement(),
                TokType.ReturnKeyword => ParseReturnStatement(),
                _=> ParseExpressionStatement()
            };
        }

        private BlockStatementNode? ParseBlockStatement()
        {
            ImmutableArray<StatementNode>.Builder statements = ImmutableArray.CreateBuilder<StatementNode>();
            var openBraceToken = MatchToken(TokType.OpenBraceToken);

            while (CurrentToken.Type != TokType.EndOfFileToken && CurrentToken.Type != TokType.CloseBraceToken)
            {
                var startToken = CurrentToken;
                
                var statement = ParseStatement();
                statements.Add(statement);

                // If no Tokens are consumed this is used to avoid an infinite loop.
                // No Error reporting is needed because the failed expression already reports one.
                if (CurrentToken == startToken) NextToken();
            }

            var closeBraceToken = MatchToken(TokType.CloseBraceToken);

            return new BlockStatementNode(_nodeTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private StatementNode? ParseVariableDeclaration()
        {
            var keyword = MatchToken(CurrentToken.Type);
            var isFloat = keyword.Text == "float";
            var identifier = MatchToken(TokType.IdentifierToken);
            var equals = MatchToken(TokType.EqualsToken);
            var initializer = ParseExpression(isFloat);
            return new VariableDeclarationNode(_nodeTree, keyword, identifier, equals, initializer);
        }

        private StatementNode? ParseIfStatement()
        {
            var keyword = MatchToken(TokType.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseStatement();
            return new IfStatementNode(_nodeTree, keyword, condition, statement, elseClause);
        }

        private ElseNode ParseElseStatement()
        {
            if (CurrentToken.Type != TokType.ElseKeyword) return null;

            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseNode(_nodeTree, keyword, statement);
        }

        private StatementNode? ParseWhileStatement()
        {
            var keyword = MatchToken(TokType.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementNode(_nodeTree, keyword, condition, body);
        }

        private StatementNode? ParseForStatement()
        {
            var keyword = MatchToken(TokType.ForKeyword);
            var identifier = MatchToken(TokType.IdentifierToken);
            var equalsToken = MatchToken(TokType.EqualsToken);
            var lowerBound = ParseExpression();
            var untilKeyword = MatchToken(TokType.UntilKeyword);
            var upperBound = ParseExpression();
            var body = ParseStatement();
            return new ForStatementNode(_nodeTree, keyword, identifier, equalsToken, lowerBound, untilKeyword, upperBound, body);
        }

        private StatementNode ParseBreakStatement()
        {
            var keyword = MatchToken(TokType.BreakKeyword);
            return new BreakStatementNode(_nodeTree, keyword);
        }
        
        private StatementNode ParseContinueStatement()
        {
            var keyword = MatchToken(TokType.ContinueKeyword);
            return new ContinueStatementNode(_nodeTree, keyword);
        }

        private StatementNode ParseReturnStatement()
        {
            var keyword = MatchToken(TokType.ReturnKeyword);
            var keywordLine = _text.GetLineIndex(keyword.Span.Start);
            var currentLine = _text.GetLineIndex(CurrentToken.Span.Start);
            var isEof = CurrentToken.Type == TokType.EndOfFileToken;
            var sameLine = !isEof && keywordLine == currentLine;
            var expression = sameLine ? ParseExpression() : null;
            return new ReturnStatementNode(_nodeTree, keyword, expression);
        }

        private ExpressionStatementNode? ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementNode(_nodeTree, expression);
        }
        
        private ExpressionNode ParsePrimaryExpression(bool forceFloat)
        {
            return CurrentToken.Type switch
            {
                TokType.OpenParenToken => ParseParenthesisExpression(),
                TokType.TrueKeyword => ParseBooleanLiteral(),
                TokType.FalseKeyword => ParseBooleanLiteral(),
                TokType.NumberToken => ParseNumberLiteral(forceFloat),
                TokType.StringToken => ParseStringLiteral(),
                TokType.IdentifierToken => ParseNameOrCallExpression(),
                _ => ParseNameExpression()
            };
        }

        private ExpressionNode ParseParenthesisExpression()
        {
            var left = MatchToken(TokType.OpenParenToken);
            var expression = ParseExpression();
            var right = MatchToken(TokType.CloseParenToken);
            return new ParenthesisExpressionNode(_nodeTree, left, expression, right);
        }

        private ExpressionNode ParseBooleanLiteral()
        {
            var isTrue = CurrentToken.Type == TokType.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(TokType.TrueKeyword) : MatchToken(TokType.FalseKeyword);
            return new LiteralExpressionNode(_nodeTree, keywordToken, isTrue, false);
        }

        private ExpressionNode ParseNumberLiteral(bool forceFloat)
        {
            var numberToken = MatchToken(TokType.NumberToken);
            var isFloat = numberToken.Text.Contains('.') || forceFloat;
            return new LiteralExpressionNode(_nodeTree, numberToken, isFloat);
        }
        
        private ExpressionNode ParseStringLiteral()
        {
            var stringToken = MatchToken(TokType.StringToken);
            return new LiteralExpressionNode(_nodeTree, stringToken);
        }

        private ExpressionNode ParseNameOrCallExpression()
        {
            if (CurrentToken.Type == TokType.IdentifierToken && PeekToken(1).Type == TokType.OpenParenToken)
                return ParseCallExpression();

            return ParseNameExpression();
        }

        private ExpressionNode ParseCallExpression()
        {
            var identifier = MatchToken(TokType.IdentifierToken);
            var openParenToken = MatchToken(TokType.OpenParenToken);
            var arguments = ParseArguments();
            var closeParenToken = MatchToken(TokType.CloseParenToken);
            return new CallExpressionNode(_nodeTree, identifier, openParenToken, arguments, closeParenToken);
        }

        private SeparatedNodeList<ExpressionNode> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();

            var parseNextArgument = true;
            while (parseNextArgument && CurrentToken.Type != TokType.CloseParenToken && CurrentToken.Type != TokType.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);

                if (CurrentToken.Type == TokType.CommaToken)
                {
                    var comma = MatchToken(TokType.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
                else parseNextArgument = false;
            }

            return new SeparatedNodeList<ExpressionNode>(nodesAndSeparators.ToImmutable());
        }

        private ExpressionNode ParseNameExpression()
        {
            var identifierToken = MatchToken(TokType.IdentifierToken);
            return new NameExpressionNode(_nodeTree, identifierToken);
        }
    }
}