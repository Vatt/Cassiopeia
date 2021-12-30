using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cassiopeia.Buffers;

public ref struct BufferReader
{
    private SequenceReader<byte> _input;
    public SequencePosition Position => _input.Position;
    public BufferReader(ReadOnlySequence<byte> sequence)
    {
        _input = new SequenceReader<byte>(sequence);
    }
    public BufferReader(Memory<byte> memory) 
        : this(new ReadOnlySequence<byte>(memory))
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadByte(out byte value)
    {
        if (_input.TryRead(out value))
        {
            return true;
        }
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        var value = _input.UnreadSpan[0];
        _input.Advance(1);
        return value;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadInt16(out short value)
    {
        return _input.TryReadLittleEndian(out value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        var value = BinaryPrimitives.ReadInt16LittleEndian(_input.UnreadSpan);
        _input.Advance(sizeof(short));
        return value;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadInt32(out int value)
    {
        return _input.TryReadLittleEndian(out value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        var value =  BinaryPrimitives.ReadInt32LittleEndian(_input.UnreadSpan);
        _input.Advance(sizeof(int));
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadInt64(out long value)
    {
        return _input.TryReadLittleEndian(out value);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(_input.UnreadSpan);
        _input.Advance(sizeof(long));
        return value;
    }

    public bool TryReadString([MaybeNullWhen(false)] out string value)
    {
        if (TryReadInt32(out int length))
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

            return SlowTryReadString(stringLength, out value);
        }
        value = default;
        return false;
    }
    public string ReadString()
    {
        var length = ReadInt32();
        if (_input.Remaining < length)
        {
            ThrowArgumentOutOfRangeException(nameof(length)); //TODO:
        }
        var stringLength = length;
        if (_input.UnreadSpan.Length >= stringLength)
        {
            var data = _input.UnreadSpan.Slice(0, stringLength);
            var value = Encoding.UTF8.GetString(data);
            _input.Advance(length);
            return value;
        }

        return SlowReadString(stringLength);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool SlowTryReadString(int stringLength, [MaybeNullWhen(false)] out string value)
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
    [MethodImpl(MethodImplOptions.NoInlining)]
    private string SlowReadString(int stringLength)
    {
        if (_input.Remaining >= stringLength)
        {
            var data = _input.UnreadSequence.Slice(0, stringLength);
            _input.Advance(stringLength + 1);
            return Encoding.UTF8.GetString(data);
        }
        return ThrowArgumentOutOfRangeException<string>(nameof(stringLength));
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadBoolean(out bool value)
    {
        if (TryReadByte(out var boolean))
        {
            value = boolean == 1;
            return true;

        }

        value = default;
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBoolean()
    {
        return ReadByte() == 1;
    }
    public bool TryReadBytes(out Memory<byte> value)
    {
        value = null;
        if (!TryReadInt32(out var len))
        {
            return false;
        }
        if (len > _input.Remaining)
        {
            return false;
        }
        value = new byte[len];
        _input.TryCopyTo(value.Span);
        _input.Advance(len);
        return true;
    }
    public bool TryReadBytesTo(Memory<byte> value)
    {
        if (!TryReadInt32(out var len))
        {
            return false;
        }
        if (len > _input.Remaining)
        {
            return false;
        }
        _input.TryCopyTo(value.Span);
        _input.Advance(len);
        return true;
    }
    public Memory<byte> ReadBytes()
    {
        var len = ReadInt32();
        if (len > _input.Remaining)
        {
            ThrowArgumentOutOfRangeException(nameof(len)); 
        }
        Memory<byte> value = new byte[len];
        _input.TryCopyTo(value.Span);
        _input.Advance(len);
        return value;
    }
    public void ReadBytesTo(in Memory<byte> value)
    {
        var len = ReadInt32();
        if (len < value.Length)
        {
            ThrowArgumentOutOfRangeException(nameof(value));
        }
        if (len > _input.Remaining)
        {
            ThrowArgumentOutOfRangeException(nameof(len));
        }
        _input.TryCopyTo(value.Span);
        _input.Advance(len);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private T ThrowArgumentOutOfRangeException<T>(string msg)
    {
        throw new ArgumentOutOfRangeException(msg);
    }
    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private void ThrowArgumentOutOfRangeException(string msg)
    {
        throw new ArgumentOutOfRangeException(msg);
    }
}

