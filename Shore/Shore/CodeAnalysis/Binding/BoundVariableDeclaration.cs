using Shore.CodeAnalysis.Symbols;
using Shore.CodeAnalysis.Syntax.Nodes;

namespace Shore.CodeAnalysis.Binding
{
    internal sealed class BoundVariableDeclaration : BoundStatement
    {
        public VariableSymbol? Variable { get; }
        public BoundExpression Initializer { get; }
        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;

        public BoundVariableDeclaration(VariableSymbol? variable, BoundExpression initializer)
        {
            Variable = variable;
            Initializer = initializer;
        }
    }
}