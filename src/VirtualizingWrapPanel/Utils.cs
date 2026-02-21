using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace WpfToolkit.Controls;

internal static class Utils
{
    /// <summary>
    /// Creates a HashSet from a range of elements in a collection. 
    /// Start and end index are inclusive and can be out of bounds. 
    /// </summary>
    /// <param name="startIndex">Index of the first item to take (inclusive).</param>
    /// <param name="endIndex">Index of the last item to take (inclusive).</param>
    public static HashSet<T> HashSetOfRange<T>(IList<T> collection, int startIndex, int endIndex)
    {
        var hashSet = new HashSet<T>();
        if (startIndex >= 0)
        {
            int count = collection.Count;
            for (int i = startIndex; i <= endIndex && i < count; i++)
            {
                hashSet.Add(collection[i]);
            }
        }
        return hashSet;
    }

    public static List<T> NewUninitializedList<T>(int count)
    {
#if NET8_0_OR_GREATER
        var list = new List<T>(count);
        CollectionsMarshal.SetCount(list, count);
        return list;
#else
        var list = new List<T>(new T[count]);
        return list;
#endif
    }
}
