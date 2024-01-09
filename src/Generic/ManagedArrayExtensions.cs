namespace UnmanagedCollections.Generic;

public static class ManagedArrayExtensions
{
    public static unsafe T[] ToArray<T>(this UnmanagedArray<T> unmanagedArray) where T : unmanaged
    {
        if (unmanagedArray.Length > Array.MaxLength)
            throw new NotImplementedException();

        var managedArray = GC.AllocateArray<T>((int)unmanagedArray.Length);

        var managedArraySpan = managedArray.AsSpan();
        var unmanagedArraySpan = unmanagedArray.AsSpan();

        unmanagedArraySpan.CopyTo(managedArraySpan);

        return managedArray;
    }
}
