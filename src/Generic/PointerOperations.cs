using System.Runtime.CompilerServices;

namespace UnmanagedCollections.Generic;

internal class PointerOperations
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal unsafe static T* AddElementOffset<T>(T* source, long elementOffset) where T : unmanaged
    {
        return source + elementOffset;
    }
}