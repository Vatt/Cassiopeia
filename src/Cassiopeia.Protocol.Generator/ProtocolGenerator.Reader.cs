using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    private static readonly SyntaxToken ProtocolReaderToken = SF.Identifier("ProtocolReader");
    public static ExpressionSyntax TryGetInt16(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetInt16"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryGetInt32(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetInt32"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryGetInt32(SyntaxToken target)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetInt32"), OutArgument(IdentifierName(target)));
    }
    public static ExpressionSyntax TryGetInt64(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetInt64"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryGetInt64(SyntaxToken target)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetInt64"), OutArgument(IdentifierName(target)));
    }
    public static ExpressionSyntax TryGetString(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetString"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryGetByte(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetByte"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryGetBoolean(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryGetBoolean"), OutArgument(assignOrDecl));
    }
}
