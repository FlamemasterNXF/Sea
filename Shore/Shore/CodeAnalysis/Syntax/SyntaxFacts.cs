namespace Shore.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.PlusToken => 6,
                TokType.DashToken => 6,
                TokType.BangToken => 6,
                
                _ => 0
            };
        }
        
        public static int GetBinaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.StarToken => 5,
                TokType.SlashToken => 5,
                
                TokType.PlusToken => 4,
                TokType.DashToken => 4,
                
                TokType.DoubleEqualsToken => 3,
                TokType.BangEqualsToken => 3,
                
                TokType.DoubleAmpersandToken => 2,
                TokType.DoublePipeToken => 1,
                
                _ => 0
            };
        }

        public static TokType GetKeywordType(this string text)
        {
            return text switch
            {
                "true" => TokType.TrueKeyword,
                "false" => TokType.FalseKeyword,
                _ => TokType.IdentifierToken
            };
        }

        public static string? GetText(TokType type)
        {
            return type switch
            {
                TokType.PlusToken => "+",
                TokType.DashToken => "-",
                TokType.StarToken => "*",
                TokType.SlashToken => "/",
                TokType.BangToken => "!",
                TokType.EqualsToken => "=",
                TokType.DoubleAmpersandToken => "&&",
                TokType.DoublePipeToken => "||",
                TokType.DoubleEqualsToken => "==",
                TokType.OpenParenToken => "(",
                TokType.CloseParenToken => ")",
                TokType.TrueKeyword => "true",
                TokType.FalseKeyword => "false",
                _ => null
            };
        }
    }
}