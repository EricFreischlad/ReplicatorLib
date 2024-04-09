using System;
using System.Collections;
using System.Collections.Generic;

namespace ReplicatorLib
{
    /// <summary>
    /// A multi-dimensional collection implemented as a dictionary of non-default values stored by coordinates. Designed for spaces where few points are expected to have non-default value.
    /// </summary>
    public class MultiDict<T> : IMultiCollection<T>
        where T : class
    {
        // The dict actually containing the items.
        private readonly Dictionary<MultiVector, T> _internalDict;
        
        /// <inheritdoc cref="IReadOnlyMultiCollection{T}.Space"/>
        public MultiSpace Space { get; }

        public MultiDict(MultiSpace space)
        {
            Space = space;
            _internalDict = new Dictionary<MultiVector, T>();
        }
        /// <inheritdoc cref="IReadOnlyMultiCollection{T}.TryGetValue(MultiVector, out T)"/>
        public bool TryGetValue(MultiVector coordinates, out T value)
        {
            if (Space.IsInBounds(coordinates))
            {
                // Assume all values not yet in the dict are at default value.
                _internalDict.TryGetValue(coordinates, out value);
                return true;
            }

            value = default!;
            return false;
        }
        /// <inheritdoc cref="IReadOnlyMultiCollection{T}.GetValueUnchecked(MultiVector)"/>
        public T GetValueUnchecked(MultiVector coordinates)
        {
            if (Space.IsInBounds(coordinates))
            {
                // Assume all values not yet in the dict are at default value.
                return _internalDict.TryGetValue(coordinates, out var value) ? value : default!;
            }
            throw new ArgumentOutOfRangeException(nameof(coordinates));
        }
        /// <inheritdoc cref="IMultiCollection{T}.SetValueUnchecked(MultiVector, T)"/>
        public void SetValueUnchecked(MultiVector coordinates, T value)
        {
            if (coordinates.DimensionCount != Space.DimensionCount)
            {
                throw new ArgumentOutOfRangeException($"The number of dimensions in the provided coordinates did not match the space.", nameof(coordinates));
            }
            _internalDict[coordinates] = value;
        }
        /// <inheritdoc cref="IMultiCollection{T}.TrySetValue(MultiVector, T)"/>
        public bool TrySetValue(MultiVector coordinates, T value)
        {
            if (Space.IsInBounds(coordinates))
            {
                SetValueUnchecked(coordinates, value);
                return true;
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var coords in Space.EnumeratePoints())
            {
                // NOTE: TryGetValue will emit default(T) if not found, which is also what we want to return.
                _internalDict.TryGetValue(coords, out var value);
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
