using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    public static INamedTypeSymbol ProtocolConstructorAttr => Compilation.GetTypeByMetadataName("Cassiopeia.Protocol.Attributes.ProtocolConstructorAttribute")!;
    public static INamedTypeSymbol ProtocolAttr => Compilation.GetTypeByMetadataName("Cassiopeia.Protocol.Attributes.CassiopeiaProtocolAttribute")!;
    public static ITypeSymbol ExtractTypeFromNullableIfNeed(ITypeSymbol original)
    {
        if (original is INamedTypeSymbol namedType)
        {
            if (original.NullableAnnotation == NullableAnnotation.NotAnnotated || original.NullableAnnotation == NullableAnnotation.None)
            {
                return original;
            }
            if (namedType.IsReferenceType)
            {
                if (namedType.IsGenericType)
                {
                    var constucted = namedType.OriginalDefinition.Construct(namedType.TypeArguments.ToArray());
                    return constucted;
                }

                return namedType.OriginalDefinition;
            }

            return namedType.TypeArguments[0];
        }
        if (original is IArrayTypeSymbol arraySym && arraySym.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return Compilation.CreateArrayTypeSymbol(arraySym.ElementType, arraySym.Rank);
        }
        return original;
    }
    public static bool TryFindPrimaryConstructor(INamedTypeSymbol symbol, SyntaxNode node, out IMethodSymbol constructor)
    {
        constructor = default;
        if (symbol.Constructors.Length == 0)
        {
            return false;
        }


        if (node is RecordDeclarationSyntax recordDecl)
        {
            if (recordDecl.ParameterList != null)
            {
                constructor = symbol.Constructors[0];
                return true;
            }
        }

        if (symbol.Constructors.Length == 1 && symbol.TypeKind == TypeKind.Class && node is not RecordDeclarationSyntax)
        {
            constructor = symbol.Constructors[0];
            return true;
        }

        if (symbol.Constructors.Length == 2 && symbol.TypeKind == TypeKind.Struct && symbol.IsReadOnly)
        {
            //constructor = symbol.Constructors.Where(sym => sym.Parameters.Length != 0).FirstOrDefault();
            constructor = symbol.Constructors[0];
            return true;
        }
        var constructorAttr = ProtocolConstructorAttr;
        foreach (var item in symbol.Constructors)
        {
            foreach (var attr in item.GetAttributes())
            {
                if (attr.AttributeClass == null)
                {
                    continue;
                }
                if (attr.AttributeClass.Equals(constructorAttr, SymbolEqualityComparer.Default))
                {
                    constructor = item;
                    return true;
                }
            }
        }
        return false;
    }
}
