using System.Collections.Immutable;
using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundCallExpression : BoundExpression
    {
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
        public override TypeSymbol Type => Function.Type;
        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

        public BoundCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
        {
            Function = function;
            Arguments = arguments;
        }
    }
}