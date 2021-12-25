using Cassiopeia.Protocol.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cassiopeia.Protocol.Generator
{
    [Generator(LanguageNames.CSharp)]
    public partial class ProtocolGenerator : IIncrementalGenerator
    {
        private static Compilation Compilation;
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();

            var declarations = context.SyntaxProvider.CreateSyntaxProvider(Predicate, Transform).Where(static decl => decl != null);
            IncrementalValueProvider<(Compilation, ImmutableArray<DeclContext>)> compilationAndDeclarations = context.CompilationProvider.Combine(declarations.Collect());
            context.RegisterSourceOutput(compilationAndDeclarations, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }
        private bool Predicate(SyntaxNode node, CancellationToken token)
        {
            switch (node)
            {
                case ClassDeclarationSyntax classDecl when classDecl.AttributeLists.Count > 0: return true;
                case StructDeclarationSyntax structDecl when structDecl.AttributeLists.Count > 0: return true;
                case RecordDeclarationSyntax recordDecl when recordDecl.AttributeLists.Count > 0: return true;
                default: return false;
            }
        }
        private DeclContext Transform(GeneratorSyntaxContext context, CancellationToken token)
        {
            Compilation = context.SemanticModel.Compilation;
            var protocolAttr = ProtocolAttr;
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol;
            foreach (var attr in symbol?.GetAttributes())
            {
                if (attr.AttributeClass.Equals(protocolAttr, SymbolEqualityComparer.Default))
                {
                    return new DeclContext(context.Node, symbol);
                }
            }
            return null;
        }
        private static void Execute(Compilation compilation, ImmutableArray<DeclContext> declarations, SourceProductionContext context)
        {
            Compilation = compilation;
            var systemDirective = SF.UsingDirective(SF.ParseName("System"));
            var systemCollectionsGenericDirective = SF.UsingDirective(SF.ParseName("System.Collections.Generic"));
            var systemBuffersBinaryDirective = SF.UsingDirective(SF.ParseName("System.Buffers.Binary"));
            var coreDirective = SF.UsingDirective(SF.ParseName("Cassiopeia.Protocol.Serialization"));

            for (int index = 0; index < declarations.Length; index++)
            {
                var decl = declarations[index];
                context.AddSource($"{decl.Declaration.Name}.g.cs",
                    SF.CompilationUnit()
                    .AddUsings(
                        systemDirective,
                        systemCollectionsGenericDirective,
                        systemBuffersBinaryDirective,
                        coreDirective)
                    .AddMembers(Generate(decl))
                    .NormalizeWhitespace()
                    .ToString());
            }
        }
        private static MemberDeclarationSyntax Generate(DeclContext ctx)
        {
            MemberDeclarationSyntax declaration = default;
            SyntaxTokenList modifiers;
            if (ctx.Declaration.TypeKind == TypeKind.Struct && ctx.Declaration.IsReadOnly)
            {
                modifiers = new(PublicKeyword(), ReadOnlyKeyword(), PartialKeyword());
            }
            else
            {
                modifiers = new(PublicKeyword(), PartialKeyword());
            }
            switch (ctx.DeclarationNode)
            {
                case ClassDeclarationSyntax:
                    declaration = SF.ClassDeclaration(ctx.Declaration.Name)
                        .WithModifiers(modifiers)
                        .AddMembers(GenerateStaticIDs(ctx))
                        .AddMembers(TryParseMethod(ctx))
                        .AddMembers(WriteMethod(ctx))
                        .AddBaseListTypes(SerializerBaseType(ctx));
                    break;
                case StructDeclarationSyntax:
                    declaration = SF.StructDeclaration(ctx.Declaration.Name)
                        .WithModifiers(modifiers)
                        .AddMembers(GenerateStaticIDs(ctx))
                        .AddMembers(TryParseMethod(ctx))
                        .AddMembers(WriteMethod(ctx))
                        .AddBaseListTypes(SerializerBaseType(ctx));
                    break;
                case RecordDeclarationSyntax decl:
                    var members = new SyntaxList<MemberDeclarationSyntax>()
                        .AddRange(GenerateStaticIDs(ctx))
                        .AddRange(new List<MemberDeclarationSyntax>
                        {
                            TryParseMethod(ctx), WriteMethod(ctx)
                        });
                    declaration = SF.RecordDeclaration(
                        kind: decl.Kind(),
                        attributeLists: default,
                        modifiers: modifiers,
                        keyword: decl.Keyword,
                        classOrStructKeyword: decl.Kind() == SyntaxKind.RecordStructDeclaration ? SF.ParseToken("struct") : SF.ParseToken("class"),
                        identifier: Identifier(ctx.Declaration.Name),
                        typeParameterList: default,
                        parameterList: default,
                        baseList: SF.BaseList(SeparatedList(SerializerBaseType(ctx))),
                        constraintClauses: default,
                        openBraceToken: OpenBraceToken(),
                        members: members,
                        closeBraceToken: CloseBraceToken(), default);


                    //declaration = SF.RecordDeclaration(
                    //    decl.Kind() == SyntaxKind.RecordStructDeclaration ? SyntaxKind.RecordStructDeclaration : SyntaxKind.RecordDeclaration,
                    //    default,
                    //    modifiers,
                    //    decl.Keyword,
                    //    Identifier(ctx.Declaration.Name),
                    //    default, default, SF.BaseList(SeparatedList(SerializerBaseType(ctx))), default,
                    //    OpenBraceToken(), members, CloseBraceToken(), default);
                    //SF.RecordDeclaration()
                    break;
                    //case RecordStructDeclaration recordStruct:
                    //    break;
            }
            return SF.NamespaceDeclaration(SF.ParseName(ctx.Declaration.ContainingNamespace.ToString())).AddMembers(declaration);
        }

        private static MemberDeclarationSyntax[] GenerateStaticIDs(DeclContext ctx)
        {
            var ids = GetProtocolAttrConstructorParams(ctx.Declaration);
            
            return new[]
            {
                SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword(), StaticKeyword()),
                        type: ShortPredefinedType(),
                        explicitInterfaceSpecifier: default,
                        identifier: Identifier("GroupId"),
                        accessorList: default,
                        expressionBody: SF.ArrowExpressionClause(NumericLiteralExpr(ids.GroupId)),
                        initializer: default,
                        semicolonToken: SemicolonToken()),
                SF.PropertyDeclaration(
                        attributeLists: default,
                        modifiers: new(PublicKeyword(), StaticKeyword()),
                        type: ShortPredefinedType(),
                        explicitInterfaceSpecifier: default,
                        identifier: Identifier("Id"),
                        accessorList: default,
                        expressionBody: SF.ArrowExpressionClause(NumericLiteralExpr(ids.Id)),
                        initializer: default,
                        semicolonToken: SemicolonToken())
            };
        }
        private static (byte GroupId, byte Id) GetProtocolAttrConstructorParams(ISymbol decl)
        {
            var protocolAttr = ProtocolAttr;
            foreach (var attr in decl.GetAttributes())
            {
                if (attr.AttributeClass.Equals(protocolAttr, SymbolEqualityComparer.Default))
                {
                    return new((byte)attr.ConstructorArguments[0].Value, (byte)attr.ConstructorArguments[1].Value);
                }
            }
            return new(255, 255);
        }
    }
}