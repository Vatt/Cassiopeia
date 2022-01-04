using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cassiopeia.Buffers
{
    public ref struct BufferWriter<T> where T : IBufferWriter<byte>
    {

        private T _output;
        private Span<byte> _span;
#if DEBUG
        private Span<byte> _origin;
#endif
        private int _buffered;
        private int _written;
        public int Written => _written;
        public readonly ref struct Reserved
        {
            private readonly Span<byte> _reserved1;
            private readonly Span<byte> _reserved2;

            public Reserved(Span<byte> r1, Span<byte> r2)
            {
                _reserved1 = r1;
                _reserved2 = r2;
            }
            public Reserved(Span<byte> r1)
            {
                _reserved1 = r1;
                _reserved2 = null;
            }

            public void WriteByte(byte source)
            {
                Debug.Assert(_reserved2 == null);
                _reserved1[0] = source;
            }

            public void Write(int source)
            {
                if (_reserved2.IsEmpty)
                {
                    BinaryPrimitives.WriteInt32LittleEndian(_reserved1, source);
                }
                else
                {
                    WriteSlow(source);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void WriteSlow(int source)
            {
                Span<byte> span = stackalloc byte[4];
                BinaryPrimitives.WriteInt32LittleEndian(span, source);
                WriteMultiSpan(span);
            }

            public void Write(Span<byte> source)
            {
                if (_reserved2.IsEmpty)
                {
                    WriteSingleSpan(source);
                }
                else
                {
                    WriteMultiSpan(source);
                }
            }

            private void WriteSingleSpan(Span<byte> source)
            {
                Debug.Assert(_reserved1.Length == source.Length);
                source.CopyTo(_reserved1);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private void WriteMultiSpan(Span<byte> source)
            {
                Debug.Assert(source.Length == _reserved1.Length + _reserved2.Length);
                source.Slice(0, _reserved1.Length).CopyTo(_reserved1);
                source.Slice(_reserved1.Length, _reserved2.Length).CopyTo(_reserved2);
            }
        }

        public Reserved Reserve(int length)
        {
            if (_span.Length >= length)
            {
                var reserved = new Reserved(_span.Slice(0, length));
                Advance(length);
                return reserved;
            }
            else
            {
                return ReserveSlow(length);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Reserved ReserveSlow(int length)
        {
            if (length > 4096)
            {
                //TODO: do something
            }

            var secondLen = length - _span.Length;
            var first = _span.Slice(0, _span.Length);
            Advance(first.Length);
            var second = _span.Slice(0, secondLen);
            Advance(secondLen);
            return new Reserved(first, second);
        }

        public BufferWriter(T output)
        {
            _output = output;
            _span = _output.GetSpan();
#if DEBUG
            _origin = _span;
#endif
            _buffered = 0;
            _written = 0;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Commit()
        {
            var buffered = _buffered;
            if (buffered > 0)
            {
                _buffered = 0;
                _output.Advance(buffered);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetNextSpan(int sizeHint = 0)
        {
            Commit();
            _span = _output.GetSpan(sizeHint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            _buffered += count;
            _written += count;
            _span = _span.Slice(count);
            if (_span.IsEmpty)
            {
                GetNextSpan();
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBytes(ReadOnlySpan<byte> source)
        {
            WriteInt32(source.Length);
            if (source.TryCopyTo(_span))
            {
                Advance(source.Length);
                return;
            }

            SlowWriteBytes(source);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteBytes(ReadOnlySpan<byte> source)
        {
            var slice = source;
            while (slice.Length > 0)
            {
                var writable = Math.Min(slice.Length, _span.Length);
                slice.Slice(0, writable).CopyTo(_span);
                slice = slice.Slice(writable);
                Advance(writable);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            _span[0] = value;
            Advance(1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            if (BinaryPrimitives.TryWriteInt16LittleEndian(_span, value))
            {
                Advance(sizeof(short));
                return;
            }

            SlowWriteInt16(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void SlowWriteInt16(short value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(short)];
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(sizeof(short) - rem);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            if (BinaryPrimitives.TryWriteInt32LittleEndian(_span, value))
            {
                Advance(sizeof(int));
                return;
            }
            SlowWriteInt32(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteInt32(int value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(sizeof(int) - rem);
            
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            if (BinaryPrimitives.TryWriteInt64LittleEndian(_span, value))
            {
                Advance(sizeof(long));
                return;
            }

            SlowWriteInt64(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteInt64(long value)
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);

            var rem = _span.Length;
            buffer.Slice(0, rem).CopyTo(_span);
            Advance(rem);
            buffer.Slice(rem).CopyTo(_span);
            Advance(sizeof(long) - rem);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteString(ReadOnlySpan<char> value)
        {
            var count = Encoding.UTF8.GetByteCount(value);
            WriteInt32(count);
            if (count <= _span.Length)
            {
                var written = Encoding.UTF8.GetBytes(value, _span);
                Advance(written);
                return;
            }

            SlowWriteString(value);
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteString(ReadOnlySpan<char> value)
        {
            if (_span.Length < 4)
            {
                GetNextSpan(4096);
            }
            var encoder = Encoding.UTF8.GetEncoder();
            do
            {
                encoder.Convert(value, _span, true, out var charsUsedJustNow, out var bytesWrittenJustNow, out var completed);

                value = value.Slice(charsUsedJustNow);
                Advance(bytesWrittenJustNow);
                if (completed == false && _span.Length < 4)
                {
                    GetNextSpan(4096);
                }
            } while (!value.IsEmpty);
        }
        /*
        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SlowWriteString(ReadOnlySpan<char> value, int bytesCount)
        {
            if (bytesCount < 512)
            {
                Span<byte> data = stackalloc byte[bytesCount];
                Encoding.UTF8.GetBytes(value, data);
                while (data.Length > 0)
                {
                    var writable = Math.Min(data.Length, _span.Length);
                    data.Slice(0, writable).CopyTo(_span);
                    data = data.Slice(writable);
                    Advance(writable);
                }
            }
            else
            {
                var raw = ArrayPool<byte>.Shared.Rent(bytesCount);
                Span<byte> data = raw.AsSpan().Slice(0, bytesCount);
                Encoding.UTF8.GetBytes(value, data);
                SlowWriteBytes(data);
                ArrayPool<byte>.Shared.Return(raw);
            }
        }
        */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            WriteByte((byte)(value ? 1 : 0));
        }

    }
}