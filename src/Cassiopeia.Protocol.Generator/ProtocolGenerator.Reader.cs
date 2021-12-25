using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    private static readonly SyntaxToken ProtocolReaderToken = SF.Identifier("reader");
    public static ExpressionSyntax TryReadInt16(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadInt16"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryReadInt32(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadInt32"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryReadInt32(SyntaxToken target)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadInt32"), OutArgument(IdentifierName(target)));
    }
    public static ExpressionSyntax TryReadInt64(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadInt64"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryReadInt64(SyntaxToken target)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadInt64"), OutArgument(IdentifierName(target)));
    }
    public static ExpressionSyntax TryReadString(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadString"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryReadByte(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadByte"), OutArgument(assignOrDecl));
    }
    public static ExpressionSyntax TryReadBoolean(ExpressionSyntax assignOrDecl)
    {
        return InvocationExpr(ProtocolReaderToken, SF.IdentifierName("TryReadBoolean"), OutArgument(assignOrDecl));
    }
}
