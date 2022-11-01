using Shore.CodeAnalysis.Syntax;

namespace Shore.CodeAnalysis.Nodes
{
    public abstract class Node
    {
        public abstract TokType Type { get; }

        public abstract IEnumerable<Node> GetChildren();
    }
}