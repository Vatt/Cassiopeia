using Microsoft.CodeAnalysis;

namespace Cassiopeia.Protocol.Generator;

public partial class ProtocolGenerator
{
    public static INamedTypeSymbol System_Buffers_IBufferWriter => Compilation.GetTypeByMetadataName("System.Buffers.IBufferWriter`1")!;
    public static INamedTypeSymbol System_Byte => Compilation.GetSpecialType(SpecialType.System_Byte);
    public static INamedTypeSymbol System_Int32 => Compilation.GetSpecialType(SpecialType.System_Int32);
    public static INamedTypeSymbol System_String => Compilation.GetSpecialType(SpecialType.System_String);
    public static INamedTypeSymbol System_Memory => Compilation.GetTypeByMetadataName("System.Memory`1")!;
    public static ITypeSymbol ArrayByteTypeSym => Compilation.CreateArrayTypeSymbol(System_Byte);
    public static ITypeSymbol MemoryByteTypeSym => System_Memory.Construct(System_Byte);
    public static ITypeSymbol IBufferWriterOfByteTypeSym => System_Buffers_IBufferWriter.Construct(System_Byte);
}
