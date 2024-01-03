using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfToolkit.Controls;

internal static class EnumerableUtil
{

    public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T? element)
    {
        if (element is not null) 
        {
            return enumerable.Except(Enumerable.Repeat(element, 1));
        }
        return enumerable;
    }

}
