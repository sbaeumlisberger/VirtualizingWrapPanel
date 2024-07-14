using System.Collections.Generic;

namespace WpfToolkit.Controls;

internal static class PolyfillExtensions
{

#if NETFRAMEWORK

    public static void Deconstruct<TKey, TValue>(
        this KeyValuePair<TKey, TValue> target,
        out TKey key,
        out TValue value)
    {
        key = target.Key;
        value = target.Value;
    }

#endif

}
