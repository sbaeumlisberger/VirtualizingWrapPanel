using System.Collections.Generic;

namespace WpfToolkit.Controls;

internal class Utils
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

}
