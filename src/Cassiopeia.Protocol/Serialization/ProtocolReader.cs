using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cassiopeia.Protocol.Serialization;

public ref struct ProtocolReader
{
    private SequenceReader<byte> _input;
    public SequencePosition Position => _input.Position;
    public ProtocolReader(ReadOnlySequence<byte> sequence)
    {
        _input = new SequenceReader<byte>(sequence);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetByte(out byte value)
    {
        if (_input.TryRead(out value))
        {
            return true;
        }
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetInt16(out short value)
    {
        return _input.TryReadLittleEndian(out value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetInt32(out int value)
    {
        return _input.TryReadLittleEndian(out value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetInt64(out int value)
    {
        return _input.TryReadLittleEndian(out value);
    }

    public bool TryGetString([MaybeNullWhen(false)] out string value)
    {
        if (TryGetInt32(out int length))
        {
            value = default;
            if (_input.Remaining < length)
            {
                return false;
            }
            var stringLength = length;
            if (_input.UnreadSpan.Length >= stringLength)
            {
                var data = _input.UnreadSpan.Slice(0, stringLength);
                value = Encoding.UTF8.GetString(data);
                _input.Advance(length);
                return true;
            }

            return SlowTryGetString(stringLength, out value);
        }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool SlowTryGetString(int stringLength, [MaybeNullWhen(false)] out string value)
    {
        if (_input.Remaining >= stringLength)
        {
            var data = _input.UnreadSequence.Slice(0, stringLength);
            _input.Advance(stringLength + 1);
            value = Encoding.UTF8.GetString(data);
            return true;
        }

        value = default;
        return false;
    }
    public bool TryGetBoolean(out bool value)
    {
        if (TryGetByte(out var boolean))
        {
            value = boolean == 1;
            return true;

        }

        value = default;
        return false;
    }
    public bool TryReadBytes(out Memory<byte> value)
    {
        value = null;
        if (!TryGetInt32(out var len))
        {
            return false;
        }
        if (len > _input.Remaining)
        {
            return false;
        }
        value = new byte[len];
        _input.TryCopyTo(value.Span);
        return true;
    }
}

