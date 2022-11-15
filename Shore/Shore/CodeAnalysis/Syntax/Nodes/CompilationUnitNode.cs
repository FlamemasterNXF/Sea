using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class CompilationUnitNode : Node
    {
        public ImmutableArray<MemberNode> Members { get; }
        public Token EndOfFileToken { get; }
        public override TokType Type => TokType.CompilationUnit;

        public CompilationUnitNode(NodeTree nodeTree, ImmutableArray<MemberNode> members, Token endOfFileToken)
            : base(nodeTree)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }
    }
}