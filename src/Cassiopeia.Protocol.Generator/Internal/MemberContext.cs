using Microsoft.CodeAnalysis;

namespace Cassiopeia.Protocol.Generator.Internal
{
    internal class MemberContext
    {
        internal DeclContext Root { get; }
        internal ISymbol NameSym { get; }
        internal ITypeSymbol TypeSym { get; }
        internal SyntaxToken AssignedVariableToken { get; }
        public MemberContext(DeclContext root, ISymbol symbol)
        {
            Root = root;
            NameSym = symbol;
            switch (symbol)
            {
                case IFieldSymbol field:
                    TypeSym = field.Type;
                    break;
                case IPropertySymbol prop:
                    TypeSym = prop.Type;
                    break;
                default: break;
            }
            var trueType = ProtocolGenerator.ExtractTypeFromNullableIfNeed(TypeSym);
            if (trueType.IsReferenceType)
            {
                if (trueType.Equals(ProtocolGenerator.ArrayByteTypeSym, SymbolEqualityComparer.Default))
                {
                    AssignedVariableToken = ProtocolGenerator.Identifier($"ByteArray{NameSym.Name}");
                }
                else
                {
                    AssignedVariableToken = ProtocolGenerator.Identifier($"{trueType.Name}{NameSym.Name}");
                }

            }
            else
            {
                AssignedVariableToken = ProtocolGenerator.Identifier($"{TypeSym.Name}{NameSym.Name}");
            }
        }
    }
}
