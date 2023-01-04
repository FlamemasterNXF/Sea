namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class ExtendStatementNode : MemberNode
    {
        public Token Keyword { get; }
        public FunctionDeclarationNode Function { get; }

        public override TokType Type => TokType.ExtendFunctionDeclaration;

        public ExtendStatementNode(NodeTree nodeTree, Token keyword, FunctionDeclarationNode function)
            : base(nodeTree)
        {
            Keyword = keyword;
            Function = function;
        }
    }
}