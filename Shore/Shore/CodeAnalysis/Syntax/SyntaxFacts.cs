namespace Shore.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        public static int GetUnaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.PlusToken => 8,
                TokType.DashToken => 8,
                TokType.BangToken => 8,
                TokType.TildeToken => 8,
                
                _ => 0
            };
        }
        
        public static int GetBinaryOperatorPrecedence(this TokType type)
        {
            return type switch
            {
                TokType.DoubleStarToken => 7,
                
                TokType.StarToken => 6,
                TokType.SlashToken => 6,
                
                TokType.PlusToken => 5,
                TokType.DashToken => 5,
                
                TokType.DoubleEqualsToken => 4,
                TokType.BangEqualsToken => 4,
                TokType.GreaterThanToken => 4,
                TokType.GreaterThanOrEqualToken => 4,
                TokType.LessThanToken => 4,
                TokType.LessThanOrEqualToken => 4,
                
                TokType.LeftShiftToken => 3,
                TokType.RightShiftToken => 3,
                
                TokType.AmpersandToken => 2,
                TokType.DoubleAmpersandToken => 2,
                TokType.PipeToken => 1,
                TokType.DoublePipeToken => 1,
                TokType.CaratToken => 1,
                
                _ => 0
            };
        }

        public static bool IsComment(this TokType type) => type is TokType.SingleLineCommentToken or TokType.MultiLineCommentToken;

        public static TokType GetKeywordType(this string text)
        {
            return text switch
            {
                "true" => TokType.TrueKeyword,
                "false" => TokType.FalseKeyword,
                "bool" => TokType.BoolKeyword,
                "string" => TokType.StringKeyword,
                //"byte" => TokType.Int8Keyword,
                //"short" => TokType.Int16Keyword,
                //"int" => TokType.Int32Keyword,
                "int" => TokType.Int64Keyword,
                //"float" => TokType.Float32Keyword,
                "float" => TokType.Float64Keyword,
                "bool[]" => TokType.BoolArrayKeyword,
                "string[]" => TokType.StringArrayKeyword,
                "int[]" => TokType.IntArrayKeyword,
                "float[]" => TokType.FloatArrayKeyword,
                "bool<>" => TokType.BoolListKeyword,
                "string<>" => TokType.StringListKeyword,
                "int<>" => TokType.IntListKeyword,
                "float<>" => TokType.FloatListKeyword,
                "readonly" => TokType.ReadOnlyKeyword,
                "if" => TokType.IfKeyword,
                "else" => TokType.ElseKeyword,
                "while" => TokType.WhileKeyword,
                "for" => TokType.ForKeyword,
                "until" => TokType.UntilKeyword,
                "function" => TokType.FunctionKeyword,
                "void" => TokType.VoidKeyword,
                "break" => TokType.BreakKeyword,
                "continue" => TokType.ContinueKeyword,
                "return" => TokType.ReturnKeyword,
                _ => TokType.IdentifierToken
            };
        }

        public static IEnumerable<TokType> GetUnaryOperatorTypes()
        {
            var types = (TokType[])Enum.GetValues(typeof(TokType));
            foreach (var type in types)
                if (GetUnaryOperatorPrecedence(type) > 0)
                    yield return type;
        }
        
        public static IEnumerable<TokType> GetBinaryOperatorTypes()
        {
            var types = (TokType[])Enum.GetValues(typeof(TokType));
            foreach (var type in types)
                if (GetBinaryOperatorPrecedence(type) > 0)
                    yield return type;
        }

        public static string? GetText(TokType type)
        {
            return type switch
            {
                TokType.PlusToken => "+",
                TokType.DashToken => "-",
                TokType.StarToken => "*",
                TokType.SlashToken => "/",
                TokType.DoubleStarToken => "**",
                TokType.BangToken => "!",
                TokType.EqualsToken => "=",
                TokType.TildeToken => "~",
                TokType.CaratToken => "^",
                TokType.GreaterThanToken => ">",
                TokType.RightArrowToken => ">",
                TokType.GreaterThanOrEqualToken => ">=",
                TokType.RightShiftToken => ">>",
                TokType.LessThanToken => "<",
                TokType.LeftArrowToken => "<",
                TokType.LessThanOrEqualToken => "<=",
                TokType.LeftShiftToken => "<<",
                TokType.AmpersandToken => "&",
                TokType.DoubleAmpersandToken => "&&",
                TokType.PipeToken => "|",
                TokType.DoublePipeToken => "||",
                TokType.DoubleEqualsToken => "==",
                TokType.BangEqualsToken => "!=",
                TokType.OpenParenToken => "(",
                TokType.CloseParenToken => ")",
                TokType.OpenBraceToken => "{",
                TokType.CloseBraceToken => "}",
                TokType.CommaToken => ",",
                TokType.FalseKeyword => "false",
                TokType.TrueKeyword => "true",
                TokType.BoolKeyword => "bool",
                TokType.StringKeyword => "string",
                TokType.Int64Keyword => "int",
                TokType.Float64Keyword => "float",
                TokType.BoolArrayKeyword => "bool[]",
                TokType.StringArrayKeyword => "string[]",
                TokType.IntArrayKeyword => "int[]",
                TokType.FloatArrayKeyword => "float[]",
                TokType.BoolListKeyword => "bool<>",
                TokType.StringListKeyword => "string<>",
                TokType.IntListKeyword => "int<>",
                TokType.FloatListKeyword => "float<>",
                TokType.ReadOnlyKeyword => "readonly",
                TokType.IfKeyword => "if",
                TokType.ElseKeyword => "else",
                TokType.WhileKeyword => "while",
                TokType.ForKeyword => "for",
                TokType.UntilKeyword => "until",
                TokType.FunctionKeyword => "function",
                TokType.VoidKeyword => "void",
                TokType.BreakKeyword => "break",
                TokType.ContinueKeyword => "continue",
                TokType.ReturnKeyword => "return",
                _ => null
            };
        }
    }
}