using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnmanagedCollections.Generic;

public static class UnmanagedArrayExtensions
{
    public static unsafe UnmanagedArray<T> ToUnmanagedArray<T>(this T[] managedArray) where T : unmanaged
    {
        ArgumentNullException.ThrowIfNull(managedArray);

        var unmanagedArray = UnsafeAlloc<T>(managedArray.Length);

        ref var managedArrayReference = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(managedArray));
        ref var unmanagedArrayReference = ref *(byte*)unmanagedArray.Pointer;
        var arrayLengthInBytes = (uint)ByteLength(unmanagedArray);

        Unsafe.CopyBlock(ref unmanagedArrayReference, ref managedArrayReference, arrayLengthInBytes);

        return unmanagedArray;
    }

    #region Utils

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe UnmanagedArray<T> UnsafeAlloc<T>(int length) where T : unmanaged
    {
        Debug.Assert(length >= 0);

        return new()
        {
            Pointer = (T*)NativeMemory.Alloc((nuint)length * (nuint)sizeof(T)),
            Length = length
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe UnmanagedArray<T> UnsafeAlloc<T>(long length) where T : unmanaged
    {
        Debug.Assert(length > 0);

        return new()
        {
            Pointer = (T*)NativeMemory.Alloc((nuint)length * (nuint)sizeof(T)),
            Length = length
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long ByteLength<T>(UnmanagedArray<T> unmanagedArray) where T : unmanaged
    {
        return unmanagedArray.Length * sizeof(T);
    }

    #endregion
}
