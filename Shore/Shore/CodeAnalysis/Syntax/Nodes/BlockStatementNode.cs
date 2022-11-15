using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class BlockStatementNode : StatementNode
    {
        public Token OpenBraceToken { get; }
        public ImmutableArray<StatementNode?> Statements { get; }
        public Token CloseBraceToken { get; }
        public override TokType Type => TokType.BlockStatement;

        public BlockStatementNode(NodeTree nodeTree, Token openBraceToken, ImmutableArray<StatementNode?> statements,
            Token closeBraceToken)
            : base(nodeTree)

        {
            OpenBraceToken = openBraceToken;
            Statements = statements;
            CloseBraceToken = closeBraceToken;
        }
    }
}