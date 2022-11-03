namespace Shore.CodeAnalysis.Syntax
{
    internal static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.PlusToken => 3,
                TokType.DashToken => 3,
                _ => 0
            };
        }
        
        public static int GetBinaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.StarToken => 2,
                TokType.SlashToken => 2,
                TokType.PlusToken => 1,
                TokType.DashToken => 1,
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