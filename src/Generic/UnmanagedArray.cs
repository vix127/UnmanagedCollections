using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace UnmanagedCollections.Generic;

[DebuggerDisplay("[{Length}]")]
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct UnmanagedArray<T> : IEnumerable<T>, IDisposable where T : unmanaged
{
    public readonly T* Pointer { get; internal init; } = null;
    public readonly long Length { get; init; } = 0;

    #region Ctor

    public UnmanagedArray(T* pointer, long length)
    {
        if (length is < 0)
            throw new ArgumentException("Length cannot be less than zero", nameof(length));

        if (pointer is null)
            throw new ArgumentNullException(nameof(pointer), "Pointer cannot be null");

        Pointer = pointer;
        Length = length;
    }

    public UnmanagedArray(ref T source, long length)
    {
        if (Unsafe.IsNullRef(ref source))
            throw new ArgumentNullException(nameof(source), "Source cannot be null");

        if (length is < 0)
            throw new ArgumentException("Length cannot be less than zero", nameof(length));

        Pointer = (T*)Unsafe.AsPointer(ref source);
        Length = length;
    }

    public UnmanagedArray(long length, bool zeroed = false)
    {
        if (length < 0)
            throw new ArgumentException("Length cannot be less than zero", nameof(length));

        Length = length;

        if (zeroed)
            Pointer = (T*)NativeMemory.AllocZeroed((nuint)length * (nuint)sizeof(T));
        else
            Pointer = (T*)NativeMemory.Alloc((nuint)length * (nuint)sizeof(T));
    }

    #endregion

    #region Indexers

    public T this[long index]
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            return Pointer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            Pointer[index] = value;
        }
    }

    public T this[int index]
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            return Pointer[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();

            Pointer[index] = value;
        }
    }

    public T this[Index index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get
        {
            var offset = index.GetOffset((int)Length);
            return this[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            var offset = index.GetOffset((int)Length);
            this[offset] = value;
        }
    }

    public Span<T> this[Range index]
    {
        get
        {
            if (index.Start.Value < index.End.Value)
            {
                long starIndex = index.Start.Value;
                int length = index.End.Value - index.Start.Value;

                var span = AsSpan(starIndex, length);
                return span;
            }
            else if (index.Start.Value > index.End.Value)
            {
                long starIndex = index.End.Value;
                int length = index.Start.Value - index.End.Value;

                var span = AsSpan(starIndex, length);
                return span;
            }
            else return [];
        }

        // TODO: set (put readonly on get)
    }

    #endregion

    #region Methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly UnmanagedArray<TTo> As<TTo>() where TTo : unmanaged
    {
        return new()
        {
            Pointer = (TTo*)this.Pointer,
            Length = this.Length * sizeof(T) / sizeof(TTo)
        };
    }

    public void Fill(in T value)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void Dispose()
    {
        NativeMemory.Free(Pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan()
    {
        return new(Pointer, Length > int.MaxValue ? int.MaxValue : (int)Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan(long elementOffset, int length)
    {
        if (elementOffset >= Length)
            throw new ArgumentException("Start index must be less than the length of the array", nameof(elementOffset));

        if (length >= Length)
            throw new ArgumentException("Length must be less than the length of the array", nameof(length));

        void* source = PointerOperations.AddElementOffset(this.Pointer, elementOffset);

        return new Span<T>(source, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly void CopyTo(Span<T> span)
    {
        if (span.Length < Length)
            throw new ArgumentException("", nameof(span));

        this.AsSpan().CopyTo(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool TryCopyTo(Span<T> span)
    {
        if (span.Length < Length)
            return false;

        AsSpan().CopyTo(span);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> Slice(long start, int length)
    {
        return AsSpan(start, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    readonly IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator()
    {
        return new(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"UnmanagedArray[{Length}]";
    }

    #endregion

    [DebuggerDisplay("Index = {_index} Current = {Current}")]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public struct Enumerator(in UnmanagedArray<T> unmanagedArray) : IEnumerator<T>
    {
        private readonly UnmanagedArray<T> _unmanagedArray = unmanagedArray;
        private long _index = -1;

        public readonly T Current => _unmanagedArray.Pointer[_index];
        readonly object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            long index = _index + 1;
            if (index < _unmanagedArray.Length)
            {
                _index = index;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _index = -1;
        }

        public readonly void Dispose()
        {
        }
    }
}