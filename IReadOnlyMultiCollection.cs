using System.Collections.Generic;

namespace ReplicatorLib
{
    /// <summary>
    /// Interface for an immutable multi-dimensional collection.
    /// </summary>
    public interface IReadOnlyMultiCollection<T> : IEnumerable<T>
    {
        /// <summary>
        /// The space definition for the collection (the min and max values in each dimension).
        /// </summary>
        MultiSpace Space { get; }
        /// <summary>
        /// Get the value at the given coordinates. Returns false if the coordinates contained a different number of dimensions as the space, or if any of the coordinates were out of bounds.
        /// </summary>
        bool TryGetValue(MultiVector coordinates, out T value);
        /// <summary>
        /// Get the value at the given coordinates. Throws exceptions if the coordinates contained a different number of dimensions as the space, or if any of the coordinates were out of bounds.
        /// </summary>
        T GetValueUnchecked(MultiVector coordinates);
    }
}