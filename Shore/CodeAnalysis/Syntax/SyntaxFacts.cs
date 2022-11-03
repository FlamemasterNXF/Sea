namespace Shore.CodeAnalysis.Syntax
{
    internal static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.PlusToken => 5,
                TokType.DashToken => 5,
                TokType.BangToken => 5,
                
                _ => 0
            };
        }
        
        public static int GetBinaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.StarToken => 4,
                TokType.SlashToken => 4,
                
                TokType.PlusToken => 3,
                TokType.DashToken => 3,
                
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
    }
}