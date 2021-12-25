using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    private static readonly SyntaxToken ProtocolWriterToken = SF.Identifier("writer");
    public static ExpressionSyntax WriteInt16(ExpressionSyntax name)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteInt16"), Argument(name));
    }
    public static ExpressionSyntax WriteInt32(ExpressionSyntax name)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteInt32"), Argument(name));
    }
    public static ExpressionSyntax WriteInt64(ExpressionSyntax name)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteInt64"), Argument(name));
    }
    public static ExpressionSyntax WriteString(ExpressionSyntax name)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteString"), Argument(name));
    }
    public static ExpressionSyntax WriteString(SyntaxToken name)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteString"), Argument(IdentifierName(name)));
    }
    public static ExpressionSyntax WriteByte(byte value)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteByte"), Argument(NumericLiteralExpr(value)));
    }
    public static ExpressionSyntax WriteBoolean(SyntaxToken value)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteBoolean"), Argument(value));
    }
    public static ExpressionSyntax WriteBoolean(ExpressionSyntax value)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteBoolean"), Argument(value));
    }
    public static ExpressionSyntax WriterReserve(int size)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("Reserve"), Argument(NumericLiteralExpr(size)));
    }
    public static ExpressionSyntax ReservedWrite(SyntaxToken reserved, SyntaxToken target)
    {
        return InvocationExpr(IdentifierName(reserved), IdentifierName("Write"), Argument(IdentifierName(target)));
    }
    public static ExpressionSyntax ReservedWriteByte(SyntaxToken reserved, SyntaxToken target)
    {
        return InvocationExpr(IdentifierName(reserved), IdentifierName("WriteByte"), Argument(IdentifierName(target)));
    }
    public static ExpressionSyntax WriteByte(ExpressionSyntax value)
    {
        return InvocationExpr(ProtocolWriterToken, IdentifierName("WriteByte"), Argument(value));
    }
    public static ExpressionStatementSyntax WriteByteStatement(byte value)
    {
        return SF.ExpressionStatement(InvocationExpr(ProtocolWriterToken, IdentifierName("WriteByte"), Argument(NumericLiteralExpr(value))));
    }
    public static ExpressionStatementSyntax WriteByteStatement(ExpressionSyntax value)
    {
        return SF.ExpressionStatement(InvocationExpr(ProtocolWriterToken, IdentifierName("WriteByte"), Argument(value)));
    }
}
