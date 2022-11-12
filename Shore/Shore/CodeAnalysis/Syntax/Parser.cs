using System.Collections.Immutable;
using Shore.CodeAnalysis.Syntax.Nodes;
using Shore.Text;

namespace Shore.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly SourceText _text;
        private readonly ImmutableArray<Token> _tokens;
        private int _position;

        public Parser(SourceText text)
        {
            var tokens = new List<Token>();

            var lexer = new Lexer(text);
            Token token;

            do {
                token = lexer.Lex();
                if (token.Type != TokType.WhitespaceToken && token.Type != TokType.UnknownToken) tokens.Add(token);
            } while (token.Type != TokType.EndOfFileToken);

            _text = text;
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
            
            _diagnostics.ReportUnexpectedToken(CurrentToken.Span, CurrentToken.Type, type);
            return new Token(type, CurrentToken.Position, null, null);
        }
        
        private ExpressionNode ParseExpression()
        {
            return ParseAssignmentExpression();
        }
        
        private ExpressionNode ParseAssignmentExpression()
        {
            if (CurrentToken.Type == TokType.IdentifierToken && PeekToken(1).Type == TokType.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionNode(identifierToken, operatorToken, right);
            }

            return ParseBinaryExpression();
        }
        
        private ExpressionNode ParseBinaryExpression(int parentPrecedence = 0)
        {
            ExpressionNode left;
            var unaryOperatorPrecedence = CurrentToken.Type.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseBinaryExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionNode(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            while (true) 
            {
                var precedence = CurrentToken.Type.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence) break;

                var operatorToken = NextToken();
                var right = ParseBinaryExpression(precedence);
                left = new BinaryExpressionNode(left, operatorToken, right);
            }

            return left;
        }

        public CompilationUnitNode ParseCompilationUnit()
        {
            var members = ParseMembers();
            var eof = MatchToken(TokType.EndOfFileToken);
            return new CompilationUnitNode(members, eof);
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
            return new FunctionDeclarationNode(functionKeyword, type, identifier, openParenToken, parameters,
                closeParenToken, body);
        }

        private SeparatedNodeList<ParameterNode> ParseParameterList()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();

            while (CurrentToken.Type != TokType.CloseParenToken && CurrentToken.Type != TokType.EndOfFileToken)
            {
                var parameter = ParseParameter();
                nodesAndSeparators.Add(parameter);

                if (CurrentToken.Type != TokType.CloseParenToken)
                {
                    var comma = MatchToken(TokType.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }

            return new SeparatedNodeList<ParameterNode>(nodesAndSeparators.ToImmutable());
        }

        private ParameterNode ParseParameter()
        {
            var type = MatchToken(CurrentToken.Type);
            var identifier = MatchToken(TokType.IdentifierToken);
            return new ParameterNode(type, identifier);
        }

        private MemberNode ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementNode(statement);
        }

        private StatementNode? ParseStatement()
        {
            return CurrentToken.Type switch
            {
                TokType.OpenBraceToken => ParseBlockStatement(),
                TokType.ReadOnlyKeyword => ParseVariableDeclaration(),
                TokType.BoolKeyword or TokType.StringKeyword or TokType.Int8Keyword or TokType.Int16Keyword or 
                    TokType.Int32Keyword or TokType.Int64Keyword => ParseVariableDeclaration(),
                TokType.IfKeyword => ParseIfStatement(),
                TokType.WhileKeyword => ParseWhileStatement(),
                TokType.ForKeyword => ParseForStatement(),
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

            return new BlockStatementNode(openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private StatementNode? ParseVariableDeclaration()
        {
            var keyword = MatchToken(CurrentToken.Type);
            var identifier = MatchToken(TokType.IdentifierToken);
            var equals = MatchToken(TokType.EqualsToken);
            var initializer = ParseExpression();
            return new VariableDeclarationNode(keyword, identifier, equals, initializer);
        }

        private StatementNode? ParseIfStatement()
        {
            var keyword = MatchToken(TokType.IfKeyword);
            var condition = ParseExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseStatement();
            return new IfStatementNode(keyword, condition, statement, elseClause);
        }

        private ElseNode ParseElseStatement()
        {
            if (CurrentToken.Type != TokType.ElseKeyword) return null;

            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseNode(keyword, statement);
        }

        private StatementNode? ParseWhileStatement()
        {
            var keyword = MatchToken(TokType.WhileKeyword);
            var condition = ParseExpression();
            var body = ParseStatement();
            return new WhileStatementNode(keyword, condition, body);
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
            return new ForStatementNode(keyword, identifier, equalsToken, lowerBound, untilKeyword, upperBound, body);
        }

        private ExpressionStatementNode? ParseExpressionStatement()
        {
            var expression = ParseExpression();
            return new ExpressionStatementNode(expression);
        }
        
        private ExpressionNode ParsePrimaryExpression()
        {
            return CurrentToken.Type switch
            {
                TokType.OpenParenToken => ParseParenthesisExpression(),
                TokType.TrueKeyword => ParseBooleanLiteral(),
                TokType.FalseKeyword => ParseBooleanLiteral(),
                TokType.NumberToken => ParseNumberLiteral(),
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
            return new ParenthesisExpressionNode(left, expression, right);
        }

        private ExpressionNode ParseBooleanLiteral()
        {
            var isTrue = CurrentToken.Type == TokType.TrueKeyword;
            var keywordToken = isTrue ? MatchToken(TokType.TrueKeyword) : MatchToken(TokType.FalseKeyword);
            return new LiteralExpressionNode(keywordToken, isTrue);
        }

        private ExpressionNode ParseNumberLiteral()
        {
            var numberToken = MatchToken(TokType.NumberToken);
            return new LiteralExpressionNode(numberToken);
        }
        
        private ExpressionNode ParseStringLiteral()
        {
            var stringToken = MatchToken(TokType.StringToken);
            return new LiteralExpressionNode(stringToken);
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
            return new CallExpressionNode(identifier, openParenToken, arguments, closeParenToken);
        }

        private SeparatedNodeList<ExpressionNode> ParseArguments()
        {
            var nodesAndSeparators = ImmutableArray.CreateBuilder<Node>();

            while (CurrentToken.Type != TokType.CloseParenToken && CurrentToken.Type != TokType.EndOfFileToken)
            {
                var expression = ParseExpression();
                nodesAndSeparators.Add(expression);

                if (CurrentToken.Type != TokType.CloseParenToken)
                {
                    var comma = MatchToken(TokType.CommaToken);
                    nodesAndSeparators.Add(comma);
                }
            }

            return new SeparatedNodeList<ExpressionNode>(nodesAndSeparators.ToImmutable());
        }

        private ExpressionNode ParseNameExpression()
        {
            var identifierToken = MatchToken(TokType.IdentifierToken);
            return new NameExpressionNode(identifierToken);
        }
    }
}