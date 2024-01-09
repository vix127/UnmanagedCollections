using UnmanagedCollections.Generic;

var arr = new UnmanagedArray<byte>((long)uint.MaxValue + (long)1);

arr.Fill(7);

Console.WriteLine(arr[arr.Length - 1]);

Console.ReadKey();