using Cassiopeia.Protocol.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    public static SyntaxToken SerializerInterfaceToken = Identifier("IProtocolSerializer");
    public static readonly GenericNameSyntax ReadOnlySpanByteName = GenericName(Identifier("ReadOnlySpan"), BytePredefinedType());
    public static readonly GenericNameSyntax SpanByteName = GenericName(Identifier("Span"), BytePredefinedType());
    public static readonly ContinueStatementSyntax ContinueStatement = SF.ContinueStatement();
    public static readonly StatementSyntax ReturnTrueStatement = ReturnStatement(SF.LiteralExpression(SyntaxKind.TrueLiteralExpression));
    public static readonly StatementSyntax ReturnFalseStatement = ReturnStatement(SF.LiteralExpression(SyntaxKind.FalseLiteralExpression));
    public static readonly StatementSyntax ReturnNothingStatement = ReturnStatement();
    public static readonly SizeOfExpressionSyntax SizeOfInt32Expr = SizeOf(IntPredefinedType());
    public static readonly BreakStatementSyntax BreakStatement = SF.BreakStatement();
    public static readonly TypeSyntax VarType = SF.ParseTypeName("var");
    public static DeclarationExpressionSyntax VarVariableDeclarationExpr(SyntaxToken varId)
    {
        return SF.DeclarationExpression(VarType, SF.SingleVariableDesignation(varId));
    }
    public static PredefinedTypeSyntax IntPredefinedType()
    {
        return SF.PredefinedType(IntKeyword());
    }
    public static PredefinedTypeSyntax NativeIntPredefinedType()
    {
        return SF.PredefinedType(IntKeyword());
    }
    public static PredefinedTypeSyntax LongPredefinedType()
    {
        return SF.PredefinedType(LongKeyword());
    }
    public static PredefinedTypeSyntax BytePredefinedType()
    {
        return SF.PredefinedType(ByteKeyword());
    }
    public static PredefinedTypeSyntax ShortPredefinedType()
    {
        return SF.PredefinedType(ShortKeyword());
    }
    public static PredefinedTypeSyntax BoolPredefinedType()
    {
        return SF.PredefinedType(BoolKeyword());
    }
    public static PredefinedTypeSyntax VoidPredefinedType()
    {
        return SF.PredefinedType(VoidKeyword());
    }
    public static ArgumentSyntax InArgument(SyntaxToken expr, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, SF.Token(SyntaxKind.InKeyword), IdentifierName(expr));
    }
    public static ArgumentSyntax RefArgument(SyntaxToken token, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, SF.Token(SyntaxKind.RefKeyword), IdentifierName(token));
    }
    public static ArgumentSyntax RefArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, SF.Token(SyntaxKind.RefKeyword), expr);
    }
    public static ParameterSyntax OutParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
    {
        return SF.Parameter(
            attributeLists: default,
            modifiers: SyntaxTokenList(SF.Token(SyntaxKind.OutKeyword)),
            identifier: identifier,
            type: type,
            @default: @default);
    }
    public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
    {
        return SF.Parameter(
            attributeLists: default,
            modifiers: default,
            identifier: identifier,
            type: type,
            @default: @default);
    }
    public static ParameterSyntax InParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
    {
        return SF.Parameter(
            attributeLists: default,
            modifiers: SyntaxTokenList(SF.Token(SyntaxKind.InKeyword)),
            identifier: identifier,
            type: type,
            @default: @default);
    }
    public static ParameterSyntax RefParameter(TypeSyntax type, SyntaxToken identifier, EqualsValueClauseSyntax @default = default)
    {
        return SF.Parameter(
            attributeLists: default,
            modifiers: SyntaxTokenList(SF.Token(SyntaxKind.RefKeyword)),
            identifier: identifier,
            type: type,
            @default: @default);
    }
    public static ParameterListSyntax ParameterList(params ParameterSyntax[] parameters)
    {
        return SF.ParameterList().AddParameters(parameters);
    }
    public static TypeSyntax TypeFullName(ISymbol sym)
    {
        //TODO: FIX THIS SHIT
        ISymbol trueType = sym.Name.Equals("Nullable") ? ((INamedTypeSymbol)sym).TypeParameters[0] : sym;
        return SF.ParseTypeName(trueType.ToString());
    }
    public static SizeOfExpressionSyntax SizeOf(TypeSyntax type)
    {
        return SF.SizeOfExpression(type);
    }
    public static ReturnStatementSyntax ReturnStatement(ExpressionSyntax expr = null)
    {
        return SF.ReturnStatement(expr);
    }
    internal static BaseTypeSyntax SerializerBaseType(DeclContext ctx)
    {
        //return GenericName(SerializerInterfaceToken, SF.ParseTypeName(ctx.Declaration.ToString()));
        return SF.SimpleBaseType(GenericName(SerializerInterfaceToken, SF.ParseTypeName(ctx.Declaration.ToString())));
    }
    public static GenericNameSyntax GenericName(SyntaxToken name, params TypeSyntax[] types)
    {
        var typeList = SF.TypeArgumentList().AddArguments(types);
        return SF.GenericName(name, typeList);
    }
    public static SyntaxToken Identifier(string name)
    {
        return SF.Identifier(name);
    }
    public static StatementSyntax Statement(ExpressionSyntax expr)
    {
        return SF.ExpressionStatement(expr);
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, IdentifierNameSyntax member)
    {
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, SimpleNameSyntax member)
    {
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, GenericNameSyntax member)
    {
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member);
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, SyntaxToken member)
    {
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, IdentifierName(member));
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(SyntaxToken source, SyntaxToken member)
    {
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(source), IdentifierName(member));
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(SyntaxToken source, IdentifierNameSyntax member)
    {
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(source), member);
    }
    public static MemberAccessExpressionSyntax SimpleMemberAccess(ExpressionSyntax source, IdentifierNameSyntax member1, IdentifierNameSyntax member2)
    {
        var first = SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, source, member1);
        return SF.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, first, member2);
    }

    public static ExpressionStatementSyntax SimpleAssignExprStatement(ExpressionSyntax left, ExpressionSyntax right)
    {
        return SF.ExpressionStatement(SimpleAssignExpr(left, right));
    }
    public static ExpressionStatementSyntax SimpleAssignExprStatement(SyntaxToken left, SyntaxToken right)
    {
        return SF.ExpressionStatement(SimpleAssignExpr(IdentifierName(left), IdentifierName(right)));
    }
    public static ExpressionStatementSyntax SimpleAssignExprStatement(SyntaxToken left, ExpressionSyntax right)
    {
        return SF.ExpressionStatement(SimpleAssignExpr(IdentifierName(left), right));
    }
    public static ExpressionStatementSyntax SimpleAssignExprStatement(ExpressionSyntax left, SyntaxToken right)
    {
        return SF.ExpressionStatement(SimpleAssignExpr(left, right));
    }
    public static AssignmentExpressionSyntax SimpleAssignExpr(ExpressionSyntax left, ExpressionSyntax right)
    {
        return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right);
    }
    public static AssignmentExpressionSyntax SimpleAssignExpr(SyntaxToken left, ExpressionSyntax right)
    {
        return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(left), right);
    }
    public static AssignmentExpressionSyntax SimpleAssignExpr(ExpressionSyntax left, SyntaxToken right)
    {
        return SF.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, IdentifierName(right));
    }
    public static ArrayCreationExpressionSyntax SingleDimensionArrayCreation(TypeSyntax arrayType, int size, InitializerExpressionSyntax initializer = default)
    {
        var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));
        return SF.ArrayCreationExpression(SF.ArrayType(arrayType, rank), initializer);
    }

    public static StackAllocArrayCreationExpressionSyntax StackAllocByteArray(int size)
    {
        var rank = new SyntaxList<ArrayRankSpecifierSyntax>(SF.ArrayRankSpecifier().AddSizes(NumericLiteralExpr(size)));
        return SF.StackAllocArrayCreationExpression(SF.ArrayType(BytePredefinedType(), rank));
    }
    public static IdentifierNameSyntax IdentifierName(SyntaxToken token)
    {
        return SF.IdentifierName(token);
    }
    public static IdentifierNameSyntax IdentifierName(string name)
    {
        return SF.IdentifierName(name);
    }
    public static ExpressionSyntax InvocationExpr(SyntaxToken source, SyntaxToken member, params ArgumentSyntax[] args)
    {
        return InvocationExpr(IdentifierName(source), IdentifierName(member), args);
    }
    public static ExpressionSyntax InvocationExpr(SyntaxToken source, GenericNameSyntax member, params ArgumentSyntax[] args)
    {
        return SF.InvocationExpression(SimpleMemberAccess(IdentifierName(source), member), SF.ArgumentList().AddArguments(args));
    }
    public static ExpressionSyntax InvocationExpr(ExpressionSyntax source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
    {
        return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
    }
    public static ExpressionSyntax InvocationExpr(ExpressionSyntax source, SyntaxToken member, params ArgumentSyntax[] args)
    {
        return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
    }
    public static ExpressionSyntax InvocationExpr(SyntaxToken source, IdentifierNameSyntax member, params ArgumentSyntax[] args)
    {
        return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
    }
    public static InvocationExpressionSyntax InvocationExpr(ExpressionSyntax source, SimpleNameSyntax member, params ArgumentSyntax[] args)
    {
        return SF.InvocationExpression(SimpleMemberAccess(source, member), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
    }
    public static ExpressionSyntax InvocationExpr(IdentifierNameSyntax member, params ArgumentSyntax[] args)
    {
        return SF.InvocationExpression(member, args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args));
    }
    public static ArgumentSyntax OutArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, SF.Token(SyntaxKind.OutKeyword), expr);
    }
    public static ArgumentSyntax OutArgument(SyntaxToken token, NameColonSyntax colonName = default)
    {
        return OutArgument(IdentifierName(token), colonName);
    }
    public static ArgumentSyntax Argument(SyntaxToken token, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, default, IdentifierName(token));
    }
    public static ArgumentSyntax Argument(ExpressionSyntax expr, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, default, expr);
    }
    public static ArgumentSyntax InArgument(ExpressionSyntax expr, NameColonSyntax colonName = default)
    {
        return SF.Argument(colonName, SF.Token(SyntaxKind.InKeyword), expr);
    }
    public static SyntaxToken ByteKeyword()
    {
        return SF.Token(SyntaxKind.ByteKeyword);
    }
    public static SyntaxToken ShortKeyword()
    {
        return SF.Token(SyntaxKind.ShortKeyword);
    }
    public static SyntaxToken BoolKeyword()
    {
        return SF.Token(SyntaxKind.BoolKeyword);
    }
    public static SyntaxToken VoidKeyword()
    {
        return SF.Token(SyntaxKind.VoidKeyword);
    }
    public static SyntaxToken IntKeyword()
    {
        return SF.Token(SyntaxKind.IntKeyword);
    }
    public static SyntaxToken LongKeyword()
    {
        return SF.Token(SyntaxKind.LongKeyword);
    }
    public static SyntaxToken PublicKeyword()
    {
        return SF.Token(SyntaxKind.PublicKeyword);
    }
    public static SyntaxToken PrivateKeyword()
    {
        return SF.Token(SyntaxKind.PrivateKeyword);
    }
    public static SyntaxToken PartialKeyword()
    {
        return SF.Token(SyntaxKind.PartialKeyword);
    }
    public static SyntaxToken RecordKeyword()
    {
        return SF.Token(SyntaxKind.RecordKeyword);
    }
    public static SyntaxToken StaticKeyword()
    {
        return SF.Token(SyntaxKind.StaticKeyword);
    }
    public static SyntaxToken SealedKeyword()
    {
        return SF.Token(SyntaxKind.SealedKeyword);
    }
    public static SyntaxToken ReadOnlyKeyword()
    {
        return SF.Token(SyntaxKind.ReadOnlyKeyword);
    }
    public static SyntaxToken TokenName(ISymbol sym)
    {
        return Identifier(sym.Name);
    }

    public static SyntaxToken SemicolonToken()
    {
        return SF.Token(SyntaxKind.SemicolonToken);
    }
    public static SyntaxToken OpenBraceToken()
    {
        return SF.Token(SyntaxKind.OpenBraceToken);
    }
    public static SyntaxToken CloseBraceToken()
    {
        return SF.Token(SyntaxKind.CloseBraceToken);
    }
    public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>(IEnumerable<T> source) where T : SyntaxNode
    {
        return SF.SeparatedList(source);
    }
    public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>() where T : SyntaxNode
    {
        return SF.SeparatedList<T>();
    }
    public static SeparatedSyntaxList<SyntaxNode> SeparatedList<T>(T source) where T : SyntaxNode
    {
        return SF.SeparatedList(new[] { source });
    }

    public static SyntaxTokenList SyntaxTokenList(params SyntaxToken[] tokens)
    {
        return new(tokens);
    }
    public static LiteralExpressionSyntax DefaultLiteralExpr()
    {
        return SF.LiteralExpression(SyntaxKind.DefaultLiteralExpression);
    }
    public static LiteralExpressionSyntax NullLiteralExpr()
    {
        return SF.LiteralExpression(SyntaxKind.NullLiteralExpression);
    }
    public static LiteralExpressionSyntax NumericLiteralExpr(int value)
    {
        return SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(value));
    }
    public static LiteralExpressionSyntax NumericLiteralExpr(byte value)
    {
        return SF.LiteralExpression(SyntaxKind.NumericLiteralExpression, SF.Literal(value));
    }
    public static LiteralExpressionSyntax CharacterLiteralExpr(char value)
    {
        return SF.LiteralExpression(SyntaxKind.CharacterLiteralExpression, SF.Literal(value));
    }
    public static NameColonSyntax NameColon(SyntaxToken name)
    {
        return SF.NameColon(IdentifierName(name));
    }
    public static NameColonSyntax NameColon(IdentifierNameSyntax name)
    {
        return SF.NameColon(name);
    }
    public static NameColonSyntax NameColon(ISymbol symbol)
    {
        return SF.NameColon(IdentifierName(symbol.Name));
    }
    public static NameColonSyntax NameColon(string name)
    {
        return SF.NameColon(IdentifierName(name));
    }
    public static ObjectCreationExpressionSyntax ObjectCreation(TypeSyntax type, params ArgumentSyntax[] args)
    {
        return SF.ObjectCreationExpression(type, args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args), default);
    }
    public static ObjectCreationExpressionSyntax ObjectCreation(INamedTypeSymbol sym, params ArgumentSyntax[] args)
    {
        ITypeSymbol trueType = sym.Name.Equals("Nullable") ? sym.TypeParameters[0] : sym;
        return SF.ObjectCreationExpression(SF.ParseTypeName(trueType.ToString()), args.Length == 0 ? SF.ArgumentList() : SF.ArgumentList().AddArguments(args), default);
    }
    public static IfStatementSyntax IfNotReturn(ExpressionSyntax condition, StatementSyntax returnStatement)
    {
        return SF.IfStatement(SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition), SF.Block(returnStatement));
    }
    public static IfStatementSyntax IfNotReturnFalse(ExpressionSyntax condition)
    {
        return IfNotReturn(condition, ReturnFalseStatement);
    }
    public static IfStatementSyntax IfNotReturnFalseElse(ExpressionSyntax condition, ExpressionSyntax elseClause)
    {
        return SF.IfStatement(
            SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
            SF.Block(ReturnFalseStatement),
            SF.ElseClause(SF.Block(SF.ExpressionStatement(elseClause))));
    }
    public static IfStatementSyntax IfNotReturnFalseElse(ExpressionSyntax condition, BlockSyntax @else)
    {
        //return IfNotReturn(condition, SF.ReturnStatement(FalseLiteralExpr()));
        return SF.IfStatement(
            SF.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, condition),
            SF.Block(ReturnFalseStatement),
            SF.ElseClause(@else));
    }
}
