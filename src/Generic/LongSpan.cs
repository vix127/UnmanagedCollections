using System.Runtime.CompilerServices;

namespace UnmanagedCollections.Generic;

public readonly ref struct LongSpan<T> where T : unmanaged
{
    private readonly ref T _reference;
    private readonly long _length;

    internal readonly ref T Reference => ref _reference;
    public readonly long Length => _length;
    public readonly bool IsEmpty => _length == 0;


    public LongSpan(ref T reference, long length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _reference = ref reference;
        _length = length;
    }

    public ref T this[long index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if ((ulong)index >= (ulong)_length)
                throw new IndexOutOfRangeException();

            return ref Unsafe.Add(ref _reference, (nuint)index);
        }
    }

    public void Fill(T value)
    {
        MemoryOperations.Fill(ref _reference, (nuint)_length, value);
    }

    public void Clear()
    {
        MemoryOperations.Fill(ref _reference, (nuint)_length, default);
    }

    public Enumerator GetEnumerator()
    {
        return new(this);
    }

    public ref struct Enumerator
    {
        /// <summary>The span being enumerated.</summary>
        private readonly LongSpan<T> _span;
        /// <summary>The next index to yield.</summary>
        private int _index;

        /// <summary>Initialize the enumerator.</summary>
        /// <param name="span">The span to enumerate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(LongSpan<T> span)
        {
            _span = span;
            _index = -1;
        }

        /// <summary>Advances the enumerator to the next element of the span.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            int index = _index + 1;
            if (index < _span.Length)
            {
                _index = index;
                return true;
            }

            return false;
        }

        /// <summary>Gets the element at the current position of the enumerator.</summary>
        public readonly ref T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _span[_index];
        }
    }
}
