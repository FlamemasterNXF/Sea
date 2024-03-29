﻿using Shore.CodeAnalysis.Symbols;

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

        public static readonly Conversion None = new(false, false, false);
        public static readonly Conversion Identity = new(true, true, true);
        public static readonly Conversion Implicit = new(true, false, true);
        public static readonly Conversion Explicit = new(true, false, false);

        public static Conversion Classify(TypeSymbol? from, TypeSymbol? to)
        {
            if (from == to) return Identity;

            if ((from == TypeSymbol.Bool || from?.ParentType == TypeSymbol.Number) && to == TypeSymbol.String) 
                return Explicit;

            if (from == TypeSymbol.String && (to == TypeSymbol.Bool || to.ParentType == TypeSymbol.Number)) 
                return Explicit;
            
            if (from != TypeSymbol.Void && to == TypeSymbol.Any) return Implicit;
            
            //if (from == TypeSymbol.Float32 && to == TypeSymbol.Int32) return Implicit;
            if (from == TypeSymbol.Float64 && to == TypeSymbol.Int64) return Implicit;
            
            if (from == TypeSymbol.BoolArr && to == TypeSymbol.Bool) return Implicit;
            if (from == TypeSymbol.StringArr && to == TypeSymbol.String) return Implicit;
            
            if (from == TypeSymbol.Int64Arr && to == TypeSymbol.Int64) return Implicit;
            if (from == TypeSymbol.Float64Arr && to == TypeSymbol.Float64) return Implicit;
            
            if (from == TypeSymbol.Int64List && to == TypeSymbol.Int64) return Implicit;
            if (from == TypeSymbol.Float64List && to == TypeSymbol.Float64) return Implicit;
            if (from == TypeSymbol.Int64 && to == TypeSymbol.Int64List) return Implicit;
            if (from == TypeSymbol.Float64 && to == TypeSymbol.Float64List) return Implicit;
            if (from == TypeSymbol.Int64Arr && to == TypeSymbol.Int64List) return Implicit;
            if (from == TypeSymbol.Float64Arr && to == TypeSymbol.Float64List) return Implicit;
            
            if (from == TypeSymbol.Any && to != TypeSymbol.Void) return Explicit;

            return None;
        }
    }
}