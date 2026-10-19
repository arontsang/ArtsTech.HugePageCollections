// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace ArtsTech.HugePageCollections.Collections
{
    /// <summary>
    /// Equality comparer for hashsets of hashsets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class HugePagesSetEqualityComparer<T> : IEqualityComparer<HugePagesSet<T>>
    {
        private readonly IEqualityComparer<T> _comparer;

        public HugePagesSetEqualityComparer()
        {
            _comparer = EqualityComparer<T>.Default;
        }

        // using m_comparer to keep equals properties intact; don't want to choose one of the comparers
        public bool Equals(HugePagesSet<T>? x, HugePagesSet<T>? y)
        {
            return HugePagesSet<T>.HugePagesSetEquals(x, y, _comparer);
        }

        public int GetHashCode(HugePagesSet<T> obj)
        {
            int hashCode = 0;
            if (obj != null)
            {
                foreach (T t in obj)
                {
                    hashCode ^= (_comparer.GetHashCode(t) & 0x7FFFFFFF);
                }
            } // else returns hashcode of 0 for null hashsets
            return hashCode;
        }

        // Equals method for the comparer itself. 
        public override bool Equals(object? obj)
        {
            if (obj is HugePagesSetEqualityComparer<T> comparer)
            {
                return (_comparer == comparer._comparer);
            }
            else if (obj is IEqualityComparer<T> ieq)
            {
                return _comparer == ieq;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _comparer.GetHashCode();
        }
    }
}