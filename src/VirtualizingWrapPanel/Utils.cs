using System.Collections.Generic;

namespace WpfToolkit.Controls;

internal class Utils
{

    public static HashSet<T> HashSetOfRange<T>(IList<T> collection, int startIndex, int endIndex)
    {
        var hashSet = new HashSet<T>();
        if (startIndex >= 0)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                hashSet.Add(collection[i]);
            }
        }
        return hashSet;
    }

}
