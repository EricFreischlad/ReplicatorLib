using System;

namespace ReplicatorLib
{
    /// <summary>
    /// A rule definition for how tiles may be placed in relation to one another.
    /// </summary>
    public sealed class TilingRule<T> : IEquatable<TilingRule<T>>
    {
        /// <summary>
        /// The origin tile. The subject of the rule.
        /// </summary>
        public readonly T OriginTile;
        /// <summary>
        /// The tile that may be adjacent to the subject tile.
        /// </summary>
        public readonly T AdjacentTile;
        /// <summary>
        /// The direction in which the adjacent tile is allowed.
        /// </summary>
        public readonly MultiVector Direction;

        public TilingRule(T originTile, T adjacentTile, MultiVector direction)
        {
            OriginTile = originTile ?? throw new ArgumentNullException(nameof(originTile));
            AdjacentTile = adjacentTile ?? throw new ArgumentNullException(nameof(adjacentTile));
            Direction = direction;
        }
        /// <summary>
        /// Generate the inverse to this rule. Example: If this rule allows red to be left of green, then this method returns a new rule allowing green to be right of red.
        /// </summary>
        public TilingRule<T> GetInverseRule()
        {
            return new TilingRule<T>(AdjacentTile, OriginTile, -Direction);
        }
        public override string ToString()
        {
            return $"(From: \"{OriginTile}\", To: \"{AdjacentTile}\", Direction: {Direction})";
        }
        public bool Equals(TilingRule<T>? other)
        {
            return other is { }
                && other.OriginTile!.GetHashCode() == OriginTile!.GetHashCode()
                && other.AdjacentTile!.GetHashCode() == AdjacentTile!.GetHashCode()
                && other.Direction == Direction;
        }
        public override bool Equals(object? obj)
        {
            return Equals(obj as TilingRule<T>);
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(OriginTile, AdjacentTile, Direction);
        }
    }
}