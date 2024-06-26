﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    /// <summary>
    /// A multi-dimensional space defined by minimum and maximum extents, such as a line (1-dim), plane (2-dim), or solid (3-dim), etc...
    /// </summary>
    public class MultiSpace
    {
        // Pre-calculated combined ranges of each dimension and higher dimensions. Used for calculating point indexes.
        private readonly MultiVector _combinedRanges;

        /// <summary>
        /// The minimum extents of the space.
        /// </summary>
        public MultiVector MinValues { get; }
        /// <summary>
        /// The maximum extents of the space.
        /// </summary>
        public MultiVector MaxValues { get; }
        /// <summary>
        /// The number of dimensions in the space.
        /// </summary>
        public int DimensionCount { get; }

        /// <summary>
        /// The total range in each dimension. That is, the length, width, height, etc...
        /// </summary>
        public MultiVector Ranges { get; }
        
        /// <summary>
        /// The total number of unit points in the space. A.k.a. the length, the area, the volume, etc... The product of the ranges of each dimension.
        /// </summary>
        public int PointCount { get; }

        /// <summary>
        /// Holds a value telling whether or not the dimension at that index is periodic (loops from the end back to the beginning). If a dimension is periodic, no value is considered out of bounds.
        /// </summary>
        public IReadOnlyList<bool> PeriodicityByDimension { get; }

        public MultiSpace(MultiVector minValues, MultiVector maxValues, IReadOnlyList<bool> periodicityByDimension)
        {
            if (minValues.DimensionCount != maxValues.DimensionCount)
            {
                throw new ArgumentException($"Dimension count mismatch between min and max values (minValues.DimensionCount = {minValues.DimensionCount}, maxValues.DimensionCount = {maxValues.DimensionCount}).", nameof(minValues));
            }
            for (int dimIndex = 0; dimIndex < minValues.DimensionCount; dimIndex++)
            {
                if (minValues[dimIndex] > maxValues[dimIndex])
                {
                    throw new ArgumentOutOfRangeException($"The minimum value for the dimension with index {dimIndex} was greater than the maximum value (min = {minValues[dimIndex]}, max = {maxValues[dimIndex]}).", nameof(minValues));
                }
            }
            if (periodicityByDimension.Count != minValues.DimensionCount)
            {
                throw new ArgumentException($"Dimension count mismatch in periodicity values (minValues.DimensionCount = {minValues.DimensionCount}, periodicityByDimension.Count = {periodicityByDimension.Count}).", nameof(periodicityByDimension));
            }

            MinValues = minValues;
            MaxValues = maxValues;
            DimensionCount = minValues.DimensionCount;
            PeriodicityByDimension = periodicityByDimension.ToArray();

            Ranges = CalculateRanges();
            _combinedRanges = CalculateCombinedRanges();
            PointCount = CountPoints();
        }
        // For if you like it the other way.
        public MultiSpace(IReadOnlyList<(int min, int max, bool isPeriodic)> dimensionDefinitions)
            :this(
                 minValues: new MultiVector(dimensionDefinitions.Select(dd => dd.min).ToArray()),
                 maxValues: new MultiVector(dimensionDefinitions.Select(dd => dd.max).ToArray()),
                 periodicityByDimension: dimensionDefinitions.Select(dd => dd.isPeriodic).ToArray())
        { }
        public MultiSpace(params (int min, int max, bool isPeriodic)[] dimensionDefinitions)
            :this((IReadOnlyList<(int, int, bool)>)dimensionDefinitions)
        { }
        /// <summary>
        /// Returns false if the number of dimensions in the coordinates did not match the space or if one of the coordinates was out of bounds for the space. Otherwise returns true.
        /// </summary>
        public bool IsInBounds(MultiVector coordinates)
        {
            if (coordinates.DimensionCount != DimensionCount)
            {
                return false;
            }

            for (int dimIndex = 0; dimIndex < DimensionCount; dimIndex++)
            {
                int thisCoord = coordinates[dimIndex];

                // If the dimension is not periodic and the coordinate is smaller than the minimum value or larger than the maximum value,
                if (!PeriodicityByDimension[dimIndex] && (thisCoord < MinValues[dimIndex] || thisCoord > MaxValues[dimIndex]))
                {
                    // These coordinates are out of bounds.
                    return false;
                }
                // All coordinate values are in bounds for periodic dimensions.
            }
            return true;
        }
        /// <summary>
        /// Returns the same coordinates but with any coordinate values of periodic dimensions wrapped to their simplest representation.
        /// </summary>
        public MultiVector SimplifyCoordinates(MultiVector coordinates)
        {
            int[] simplifiedCoordinates = new int[DimensionCount];
            for (int dimIndex = 0; dimIndex < DimensionCount; dimIndex++)
            {
                if (PeriodicityByDimension[dimIndex])
                {
                    simplifiedCoordinates[dimIndex] = (coordinates[dimIndex] % Ranges[dimIndex]) - MinValues[dimIndex];
                }
                else
                {
                    simplifiedCoordinates[dimIndex] = coordinates[dimIndex];
                }
            }
            return new MultiVector(simplifiedCoordinates);
        }
        public int GetPointIndexUnchecked(MultiVector coordinates)
        {
            int pointIndex = 0;
            for (int dimIndex = 0; dimIndex < DimensionCount; dimIndex++)
            {
                pointIndex += _combinedRanges[dimIndex] * coordinates[dimIndex];
            }
            return pointIndex;
        }
        public MultiVector GetCoordinatesUnchecked(int pointIndex)
        {
            int[] coordinates = new int[DimensionCount];
            for (int dimIndex = 0; dimIndex < DimensionCount; dimIndex++)
            {
                int range = _combinedRanges[dimIndex];
                coordinates[dimIndex] = pointIndex / range;
                pointIndex %= range;
            }
            return new MultiVector(coordinates);
        }
        /// <summary>
        /// Enumerate the points of the space, in order of highest dimension (last coordinate) to lowest (first coordinate).
        /// </summary>
        public IEnumerable<MultiVector> EnumeratePoints()
        {
            int[] workingCoords = new int[DimensionCount];
            foreach (var point in EnumeratePoints_Internal(workingCoords, 0))
            {
                yield return point;
            }
        }
        // Recursive.
        private IEnumerable<MultiVector> EnumeratePoints_Internal(int[] workingCoords, int dimIndex)
        {
            // If the dimension index exceeds the dimension count, then we've reached the bottom of the nested loop structure.
            if (dimIndex >= DimensionCount)
            {
                // Do the thing.
                yield return new MultiVector(workingCoords);
            }
            else
            {
                for (int i = MinValues[dimIndex]; i <= MaxValues[dimIndex]; i++)
                {
                    workingCoords[dimIndex] = i;
                    foreach (var point in EnumeratePoints_Internal(workingCoords, dimIndex + 1))
                    {
                        yield return point;
                    }
                }
            }
        }
        private MultiVector CalculateRanges()
        {
            int[] ranges = new int[DimensionCount];
            for (int dimIndex = 0; dimIndex < DimensionCount; dimIndex++)
            {
                ranges[dimIndex] = Math.Abs(MaxValues[dimIndex] - MinValues[dimIndex] + 1);
            }
            return new MultiVector(ranges);
        }
        private MultiVector CalculateCombinedRanges()
        {
            int[] combinedRanges = new int[DimensionCount];
            int runningMultiplier = 1;
            for (int dimIndex = DimensionCount - 1; dimIndex >= 0; dimIndex--)
            {
                combinedRanges[dimIndex] = runningMultiplier;
                runningMultiplier *= Ranges[dimIndex];
            }
            return new MultiVector(combinedRanges);
        }
        private int CountPoints()
        {
            int volume = 1;
            foreach (int range in Ranges)
            {
                volume *= range;
            }
            return volume;
        }
    }
}
