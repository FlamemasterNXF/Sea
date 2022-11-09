namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ParameterNode : Node
    {
        public Token PType { get; }
        public Token Identifier { get; }
        public override TokType Type => TokType.Parameter;

        public ParameterNode(Token type, Token identifier)
        {
            PType = type;
            Identifier = identifier;
        }
    }
}