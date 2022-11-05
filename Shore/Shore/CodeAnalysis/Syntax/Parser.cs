using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private DiagnosticBag _diagnostics = new DiagnosticBag();
        private readonly Token[] _tokens;
        private int _position;

        public Parser(string text)
        {
            var tokens = new List<Token>();

            var lexer = new Lexer(text);
            Token token;

            do {
                token = lexer.Lex();
                if (token.Type != TokType.WhitespaceToken && token.Type != TokType.UnknownToken) tokens.Add(token);
            } while (token.Type != TokType.EndOfFileToken);

            _tokens = tokens.ToArray();
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

        public NodeTree Parse()
        {
            var expression = ParseExpression();
            var eof = MatchToken(TokType.EndOfFileToken);
            return new NodeTree(_diagnostics, expression, eof);
        }
        
        private ExpressionNode ParsePrimaryExpression()
        {
            return CurrentToken.Type switch
            {
                TokType.OpenParenToken => ParseParenthesisExpression(),
                TokType.TrueKeyword => ParseBooleanLiteral(),
                TokType.FalseKeyword => ParseBooleanLiteral(),
                TokType.NumberToken => ParseNumberLiteral(),
                TokType.IdentifierToken => ParseNameExpression(),
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

        private ExpressionNode ParseNameExpression()
        {
            var identifierToken = MatchToken(TokType.IdentifierToken);
            return new NameExpressionNode(identifierToken);
        }
    }
}