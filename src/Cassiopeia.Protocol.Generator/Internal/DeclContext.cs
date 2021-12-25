using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

#nullable enable

namespace Cassiopeia.Protocol.Generator.Internal
{
    internal class DeclContext
    {
        public INamedTypeSymbol Declaration { get; }
        public SyntaxNode DeclarationNode { get; }
        public List<MemberContext> Members { get; }
        public ImmutableArray<IParameterSymbol>? ConstructorParams { get; }
        public IReadOnlyDictionary<ISymbol, string>? ConstructorParamsBinds { get; }
        public bool HavePrimaryConstructor { get; }
        public DeclContext(SyntaxNode node, INamedTypeSymbol symbol)
        {
            Declaration = symbol;
            DeclarationNode = node;
            Members = new List<MemberContext>();
            if (ProtocolGenerator.TryFindPrimaryConstructor(Declaration, node, out var constructor))
            {
                if (constructor!.Parameters.Length != 0)
                {
                    ConstructorParams = constructor.Parameters;
                    HavePrimaryConstructor = true;
                }
            }
            if (ConstructorParams.HasValue)
            {
                ConstructorParamsBinds = MatchContructorArguments();
            }
            CreateContructorOnlyMembers();
        }
        private Dictionary<ISymbol, string> MatchContructorArguments()
        {
#pragma warning disable RS1024 // Compare symbols correctly
            var binds = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly
            foreach (var param in ConstructorParams!)
            {
                var member = FindMemberByName(Declaration, param.Name);
                if (member is null)
                {
                    //ProtocolGenerator.ReportMatchConstructorParametersError(Declaration);
                }
                else
                {
                    binds.Add(member, param.Name);
                }

            }

            return binds;
        }
        public void CreateContructorOnlyMembers()
        {
            if (ConstructorParams.HasValue == false)
            {
                //TODO: report diagnostic error
                return;
            }
            foreach (var param in ConstructorParams)
            {
                if (MembersContains(param, out var namedType))
                {
                    Members.Add(new MemberContext(this, namedType));
                }

            }
        }
        public bool MembersContains(IParameterSymbol parameter, out ISymbol symbol)
        {
            symbol = Declaration.GetMembers()
                .Where(sym => IsBadMemberSym(sym) == false)
                .FirstOrDefault(sym => CheckGetAccessibility(sym) &&
                                       ExtractTypeFromSymbol(sym, out var type) &&
                                       NameEquals(sym.Name, parameter.Name) &&
                                       type!.Equals(parameter.Type, SymbolEqualityComparer.Default));
            if (symbol is not null)
            {
                return true;
            }
            return false;
        }
        private static bool ExtractTypeFromSymbol(ISymbol sym, out ITypeSymbol? type)
        {
            switch (sym)
            {
                case IFieldSymbol field:
                    type = field.Type;
                    return true;
                case IPropertySymbol prop:
                    type = prop.Type;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }
        private static ISymbol? FindMemberByName(INamedTypeSymbol sym, string name)
        {
            foreach (var member in sym.GetMembers())
            {
                if (IsBadMemberSym(member))
                {
                    continue;
                }
                if (CheckAccessibility(member) && NameEquals(member.Name, name))
                {
                    return member;
                }
                if (CheckGetAccessibility(member) && NameEquals(member.Name, name))
                {
                    return member;
                }
            }

            return null;
        }
        private static bool NameEquals(string symName, string paramName)
        {
            var fmtParamName = paramName.ToUpper().Replace("_", string.Empty);
            return symName.ToUpper().Equals(fmtParamName);
        }
        private static bool IsBadMemberSym(ISymbol member)
        {
            if (member.IsAbstract || /*ProtocolGenerator.IsIgnore(member) ||*/
               (member.Kind != SymbolKind.Property && member.Kind != SymbolKind.Field) ||
                member.DeclaredAccessibility != Accessibility.Public)
            {
                return true;
            }

            return false;
        }
        private static bool CheckAccessibility(ISymbol member)
        {
            if ((member is IPropertySymbol { SetMethod: { }, GetMethod: { }, IsReadOnly: false }) ||
                (member is IFieldSymbol { IsReadOnly: false, IsConst: false }))
            {
                return true;
            }

            return false;
        }

        private static bool CheckGetAccessibility(ISymbol member)
        {
            if (member is IPropertySymbol prop && (prop.IsReadOnly || prop.GetMethod != null))
            {
                return true;
            }
            if (member is IFieldSymbol { IsReadOnly: true, IsConst: false })
            {
                return true;
            }

            return false;
        }
    }
}
