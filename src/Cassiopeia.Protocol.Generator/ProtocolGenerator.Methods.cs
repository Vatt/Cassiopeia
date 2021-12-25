using Cassiopeia.Protocol.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    private static SyntaxToken TryParseToken = Identifier("TryParse");
    private static SyntaxToken WriteToken = Identifier("Write");
    private static readonly TypeSyntax ProtocolReaderType = SF.ParseTypeName("Cassiopeia.Protocol.Serialization.ProtocolReader");
    private static readonly TypeSyntax ProtocoWriterType = SF.ParseTypeName("Cassiopeia.Protocol.Serialization.ProtocolWriter");
    private static SyntaxToken TryParseOutVarToken => Identifier("message");
    private static SyntaxToken WriterInputVarToken => Identifier("message");
    private static MemberDeclarationSyntax TryParseMethod(DeclContext ctx)
    {
        return SF.MethodDeclaration(
                    attributeLists: default,
                    modifiers: new(PublicKeyword(), StaticKeyword()),
                    explicitInterfaceSpecifier: default,//SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(decl))),
                    returnType: BoolPredefinedType(),
                    identifier: TryParseToken,
                    parameterList: ParameterList(RefParameter(ProtocolReaderType, ProtocolReaderToken),
                                                 OutParameter(TypeFullName(ctx.Declaration), TryParseOutVarToken)),
                    body: default,
                    constraintClauses: default,
                    expressionBody: default,
                    typeParameterList: default,
                    semicolonToken: default)
                .WithBody(SF.Block(TryParseStatements(ctx)));
    }
    private static MemberDeclarationSyntax WriteMethod(DeclContext ctx)
    {
        return SF.MethodDeclaration(
        attributeLists: default,
        modifiers: new(PublicKeyword(), StaticKeyword()),
        explicitInterfaceSpecifier: default,// SF.ExplicitInterfaceSpecifier(GenericName(SerializerInterfaceToken, TypeFullName(ctx.Declaration))),
        returnType: VoidPredefinedType(),
        identifier: WriteToken,
        parameterList: ParameterList(
            RefParameter(ProtocoWriterType, ProtocolWriterToken),
            InParameter(TypeFullName(ctx.Declaration), TryParseOutVarToken)),
        body: default,
        constraintClauses: default,
        expressionBody: default,
        typeParameterList: default,
        semicolonToken: default)
    .WithBody(SF.Block(WriteStatements(ctx)));
    }
    private static StatementSyntax[] WriteStatements(DeclContext ctx)
    {
        List<StatementSyntax> statements = new();
        foreach (var member in ctx.Members)
        {
            if (TryGetProtocolWriterMethod(member, MessageMemberAccess(member.NameSym), out var expr))
            {
                statements.Add(Statement(expr));
            }
        }
        return statements.ToArray();

        ExpressionSyntax MessageMemberAccess(ISymbol symbol)
        {
            return SimpleMemberAccess(WriterInputVarToken, Identifier(symbol.Name));
        }
    }
    private static StatementSyntax[] TryParseStatements(DeclContext ctx)
    {
        List<StatementSyntax> statements = new();
        statements.Add(SimpleAssignExprStatement(TryParseOutVarToken, DefaultLiteralExpr()));
        foreach (var member in ctx.Members)
        {
            if (TryGetProtocolReaderMethod(member, VarVariableDeclarationExpr(member.AssignedVariableToken), out var expr))
            {
                statements.Add(IfNotReturnFalse(expr));
            }
        }
        statements.AddRange(CreateMessage(ctx));
        statements.Add(ReturnTrueStatement);
        return statements.ToArray();
    }
    private static StatementSyntax[] CreateMessage(DeclContext ctx)
    {
        var result = new List<ExpressionStatementSyntax>();
        if (ctx.HavePrimaryConstructor)
        {
            List<ArgumentSyntax> args = new();
            var assignments = new List<ExpressionStatementSyntax>();
            foreach (var member in ctx.Members)
            {
                if (ctx.ConstructorParamsBinds.TryGetValue(member.NameSym, out var parameter))
                {
                    args.Add(Argument(member.AssignedVariableToken, NameColon(parameter)));
                }
                else
                {
                    assignments.Add(
                        SimpleAssignExprStatement(
                            SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                            IdentifierName(member.AssignedVariableToken)));
                }
            }

            var creation = SimpleAssignExprStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration, args.ToArray()));
            result.Add(creation);
            result.AddRange(assignments);
        }
        else
        {
            result.Add(SimpleAssignExprStatement(TryParseOutVarToken, ObjectCreation(ctx.Declaration)));
            foreach (var member in ctx.Members)
            {
                result.Add(
                    SimpleAssignExprStatement(
                        SimpleMemberAccess(TryParseOutVarToken, IdentifierName(member.NameSym.Name)),
                        IdentifierName(member.AssignedVariableToken)));
            }
        }
        return result.ToArray();
    }
    private static bool TryGetProtocolReaderMethod(MemberContext ctx, ExpressionSyntax variable, out ExpressionSyntax expr)
    {
        expr = default;
        switch (ctx.TypeSym.SpecialType)
        {
            case SpecialType.System_Int16:
                expr = TryGetInt16(variable);
                return true;
            case SpecialType.System_Int32:
                expr = TryGetInt32(variable);
                return true;
            case SpecialType.System_Int64:
                expr = TryGetInt64(variable);
                return true;
            case SpecialType.System_Byte:
                expr = TryGetByte(variable);
                return true;
            case SpecialType.System_String:
                expr = TryGetString(variable);
                return true;
            case SpecialType.System_Boolean:
                expr = TryGetBoolean(variable);
                return true;
            default: return false;
        }
    }
    private static bool TryGetProtocolWriterMethod(MemberContext ctx, ExpressionSyntax variable, out ExpressionSyntax expr)
    {
        expr = default;
        switch (ctx.TypeSym.SpecialType)
        {
            case SpecialType.System_Int16:
                expr = WriteInt16(variable);
                return true;
            case SpecialType.System_Int32:
                expr = WriteInt32(variable);
                return true;
            case SpecialType.System_Int64:
                expr = WriteInt64(variable);
                return true;
            case SpecialType.System_Byte:
                expr = WriteByte(variable);
                return true;
            case SpecialType.System_String:
                expr = WriteString(variable);
                return true;
            case SpecialType.System_Boolean:
                expr = WriteBoolean(variable);
                return true;
            default: return false;
        }
    }
}
