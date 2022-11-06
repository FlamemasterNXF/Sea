namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class CompilationUnitNode : Node
    {
        public StatementNode Statement { get; }
        public Token EndOfFileToken { get; }
        public override TokType Type => TokType.CompilationUnit;

        public CompilationUnitNode(StatementNode statement, Token endOfFileToken)
        {
            Statement = statement;
            EndOfFileToken = endOfFileToken;
        }
    }
}