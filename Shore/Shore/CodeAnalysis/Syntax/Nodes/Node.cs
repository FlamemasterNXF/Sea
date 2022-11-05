namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public abstract class Node
    {
        public abstract TokType Type { get; }

        public abstract IEnumerable<Node> GetChildren();
    }
}