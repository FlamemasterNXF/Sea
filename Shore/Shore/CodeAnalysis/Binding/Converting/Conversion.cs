using Shore.CodeAnalysis.Symbols;

namespace Shore.CodeAnalysis.Binding.Converting
{
    internal sealed class Conversion
    {
        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;

        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public static readonly Conversion None = new Conversion(false, false, false);
        public static readonly Conversion Identity = new Conversion(true, true, true);
        public static readonly Conversion Implicit = new Conversion(true, false, true);
        public static readonly Conversion Explicit = new Conversion(true, false, false);

        public static Conversion Classify(TypeSymbol? from, TypeSymbol? to)
        {
            if (from == to) return Identity;

            if ((from == TypeSymbol.Bool || from?.ParentType == TypeSymbol.Number) && to == TypeSymbol.String) 
                return Explicit;

            if (from == TypeSymbol.String && (to == TypeSymbol.Bool || to?.ParentType == TypeSymbol.Number))
                return Explicit;

            return None;
        }
    }
}