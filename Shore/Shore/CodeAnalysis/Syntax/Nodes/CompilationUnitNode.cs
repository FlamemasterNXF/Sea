using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public sealed class CompilationUnitNode : Node
    {
        public ImmutableArray<MemberNode> Members { get; }
        public Token EndOfFileToken { get; }
        public override TokType Type => TokType.CompilationUnit;

        public CompilationUnitNode(ImmutableArray<MemberNode> members, Token endOfFileToken)
        {
            Members = members;
            EndOfFileToken = endOfFileToken;
        }
    }
}