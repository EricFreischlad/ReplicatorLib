using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    /// <summary>
    /// A multi-dimensional collection implemented as a flat list. Designed for spaces where each point is expected to have value.
    /// </summary>
    public class MultiArray<T> : IMultiCollection<T>
    {
        // The flat list actually containing the items.
        private readonly List<T> _flatList;

        // Pre-calculated combined ranges of each dimension and higher dimensions. Used for calculating point indexes.
        private readonly MultiVector _combinedRanges;

        /// <inheritdoc cref="IReadOnlyMultiCollection{T}.Space"/>
        public MultiSpace Space { get; }

        private MultiArray(MultiSpace space, List<T> flatList)
        {
            Space = space;
            _flatList = flatList;

            _combinedRanges = CalculateCombinedRanges();
        }
        /// <summary>
        /// Create a new MultiArray with all values initialized to the type default.
        /// </summary>
        public static MultiArray<T> Create(MultiSpace space)
        {
            var flatList = new List<T>(space.PointCount);
            for (int i = 0; i < space.PointCount; i++)
            {
                flatList.Add(default!);
            }
            return new MultiArray<T>(space, flatList);
        }
        /// <summary>
        /// Create a new MultiArray, supplying a function to call when initializing each point.
        /// </summary>
        public static MultiArray<T> Create(MultiSpace space, Func<T> fillFunc)
        {
            var flatList = new List<T>(space.PointCount);
            for (int i = 0; i < space.PointCount; i++)
            {
                flatList.Add(fillFunc.Invoke());
            }
            return new MultiArray<T>(space, flatList);
        }
        /// <summary>
        /// Create a new MultiArray, supplying a function to call when initializing each point. The coordinates are provided at each call.
        /// </summary>
        public static MultiArray<T> Create(MultiSpace space, Func<MultiVector, T> fillFunc)
        {
            var flatList = new List<T>(space.PointCount);
            int pointIndex = 0;
            foreach (var coords in space.EnumeratePoints())
            {
                flatList[pointIndex] = fillFunc.Invoke(coords);
                pointIndex++;
            }
            return new MultiArray<T>(space, flatList);
        }
        /// <summary>
        /// Create a new MultiArray, supplying a collection containing all initial values. The values should be in the same order as when enumerating the space.
        /// </summary>
        public static MultiArray<T> Create(MultiSpace space, ICollection<T> initialValues)
        {
            if (initialValues.Count != space.PointCount)
            {
                throw new ArgumentOutOfRangeException($"Unexpected number of values in collection. Expected: {space.PointCount}. Actual: {initialValues.Count}", nameof(initialValues));
            }
            return new MultiArray<T>(space, initialValues.ToList());
        }
        /// <summary>
        /// Create a new MultiArray from another multi-dimensional collection.
        /// </summary>
        public static MultiArray<T> Create(IReadOnlyMultiCollection<T> other)
        {
            return new MultiArray<T>(other.Space, other.Space.EnumeratePoints().Select(coord => other.GetValueUnchecked(coord)).ToList());
        }
        /// <inheritdoc cref="IMultiCollection{T}.SetValueUnchecked(MultiVector, T)"/>
        public void SetValueUnchecked(MultiVector coordinates, T value)
        {
            int pointIndex = GetPointIndexUnchecked(coordinates);
            _flatList[pointIndex] = value;
        }
        /// <inheritdoc cref="IMultiCollection{T}.TrySetValue(MultiVector, T)"/>
        public bool TrySetValue(MultiVector coordinates, T value)
        {
            if (coordinates.DimensionCount == Space.DimensionCount && Space.IsInBounds(coordinates))
            {
                SetValueUnchecked(coordinates, value);
                return true;
            }
            return false;
        }
        /// <inheritdoc cref="IReadOnlyMultiCollection{T}.GetValueUnchecked(MultiVector)"/>
        public T GetValueUnchecked(MultiVector coordinates)
        {
            return _flatList[GetPointIndexUnchecked(coordinates)];
        }
        /// <inheritdoc cref="IReadOnlyMultiCollection{T}.TryGetValue(MultiVector, out T)"/>
        public bool TryGetValue(MultiVector coordinates, out T value)
        {
            if (Space.IsInBounds(coordinates))
            {
                value = GetValueUnchecked(coordinates);
                return true;
            }

            value = default!;
            return false;
        }
        private int GetPointIndexUnchecked(MultiVector coordinates)
        {
            int pointIndex = 0;
            for (int dimIndex = 0; dimIndex < Space.DimensionCount; dimIndex++)
            {
                pointIndex += _combinedRanges[dimIndex] * coordinates[dimIndex];
            }
            return pointIndex;
        }
        private MultiVector GetCoordinatesUnchecked(int pointIndex)
        {
            int[] coordinates = new int[Space.DimensionCount];
            for (int dimIndex = 0; dimIndex < Space.DimensionCount; dimIndex++)
            {
                int range = _combinedRanges[dimIndex];
                coordinates[dimIndex] = pointIndex / range;
                pointIndex %= range;
            }
            return new MultiVector(coordinates);
        }
        private MultiVector CalculateCombinedRanges()
        {
            int[] combinedRanges = new int[Space.DimensionCount];
            int runningMultiplier = 1;
            for (int dimIndex = Space.DimensionCount - 1; dimIndex >= 0; dimIndex--)
            {
                combinedRanges[dimIndex] = runningMultiplier;
                runningMultiplier *= Space.Ranges[dimIndex];
            }
            return new MultiVector(combinedRanges);
        }
        public IEnumerable<(T Value, MultiVector Coordinates)> EnumeratePointsWithCoordinates()
        {
            for (int i = 0; i < _flatList.Count; i++)
            {
                yield return (_flatList[i], GetCoordinatesUnchecked(i));
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var coords in Space.EnumeratePoints())
            {
                yield return GetValueUnchecked(coords);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
