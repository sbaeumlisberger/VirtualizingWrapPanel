﻿#if NET462
// Copied from https://raw.githubusercontent.com/dotnet/runtime/refs/tags/v8.0.0/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/ReferenceEqualityComparer.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    /// An <see cref="IEqualityComparer{Object}"/> that uses reference equality (<see cref="object.ReferenceEquals(object?, object?)"/>)
    /// instead of value equality (<see cref="object.Equals(object?)"/>) when comparing two object instances.
    /// </summary>
    /// <remarks>
    /// The <see cref="ReferenceEqualityComparer"/> type cannot be instantiated. Instead, use the <see cref="Instance"/> property
    /// to access the singleton instance of this type.
    /// </remarks>
    internal sealed class ReferenceEqualityComparer : IEqualityComparer<object?>, IEqualityComparer
    {
        private ReferenceEqualityComparer() { }

        /// <summary>
        /// Gets the singleton <see cref="ReferenceEqualityComparer"/> instance.
        /// </summary>
        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        /// <summary>
        /// Determines whether two object references refer to the same object instance.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if both <paramref name="x"/> and <paramref name="y"/> refer to the same object instance
        /// or if both are <see langword="null"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This API is a wrapper around <see cref="object.ReferenceEquals(object?, object?)"/>.
        /// It is not necessarily equivalent to calling <see cref="object.Equals(object?, object?)"/>.
        /// </remarks>
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        /// <summary>
        /// Returns a hash code for the specified object. The returned hash code is based on the object
        /// identity, not on the contents of the object.
        /// </summary>
        /// <param name="obj">The object for which to retrieve the hash code.</param>
        /// <returns>A hash code for the identity of <paramref name="obj"/>.</returns>
        /// <remarks>
        /// This API is a wrapper around <see cref="RuntimeHelpers.GetHashCode(object)"/>.
        /// It is not necessarily equivalent to calling <see cref="object.GetHashCode()"/>.
        /// </remarks>
        public int GetHashCode(object? obj)
        {
            // Depending on target framework, RuntimeHelpers.GetHashCode might not be annotated
            // with the proper nullability attribute. We'll suppress any warning that might
            // result.
            return RuntimeHelpers.GetHashCode(obj!);
        }
    }
}
#endif
