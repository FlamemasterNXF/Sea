using System.Collections;
using System.Collections.Immutable;

namespace Shore.CodeAnalysis.Syntax.Nodes
{
    public abstract class SeparatedNodeList
    {
        public abstract ImmutableArray<Node> GetWithSeparators();
    }
    
    public sealed class SeparatedNodeList<T> : SeparatedNodeList, IEnumerable<T>
        where T : Node
    {
        private readonly ImmutableArray<Node> _nodesAndSeparators;

        public SeparatedNodeList(ImmutableArray<Node> nodesAndSeparators)
        {
            _nodesAndSeparators = nodesAndSeparators;
        }

        public int Count => (_nodesAndSeparators.Length + 1) / 2;

        public T this[int index] => (T)_nodesAndSeparators[index * 2];

        public Token? GetSeparator(int index)
        {
            if (index == Count - 1) return null;
            return (Token)_nodesAndSeparators[index * 2 + 1];
        }

        public override ImmutableArray<Node> GetWithSeparators() => _nodesAndSeparators;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++) yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}