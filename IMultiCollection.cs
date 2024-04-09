namespace ReplicatorLib
{
    /// <summary>
    /// Interface for a mutable multi-dimensional collection.
    /// </summary>
    public interface IMultiCollection<T> : IReadOnlyMultiCollection<T>
    {
        /// <summary>
        /// Set the value at the given coordinates. Throws exceptions if the coordinates contained a different number of dimensions as the space, or if any of the coordinates were out of bounds.
        /// </summary>
        void SetValueUnchecked(MultiVector coordinates, T value);
        /// <summary>
        /// Set the value at the given coordinates. Returns false if the coordinates contained a different number of dimensions as the space, or if any of the coordinates were out of bounds.
        /// </summary>
        bool TrySetValue(MultiVector coordinates, T value);
    }
}
