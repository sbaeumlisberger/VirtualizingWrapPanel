using System;
using System.Collections.Generic;

namespace WpfToolkit.Controls;

#if NETFRAMEWORK

internal static class PolyfillExtensions
{
    public static void Deconstruct<TKey, TValue>(
        this KeyValuePair<TKey, TValue> target,
        out TKey key,
        out TValue value)
    {
        key = target.Key;
        value = target.Value;
    }

    extension(Math)
    {
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }
}

#endif