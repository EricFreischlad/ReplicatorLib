using System;

namespace ReplicatorLib
{
    public sealed class TilingRule<T> : IEquatable<TilingRule<T>>
    {
        public readonly T OriginTile;
        public readonly T AdjacentTile;
        public readonly MultiVector Direction;

        public TilingRule(T originTile, T adjacentTile, MultiVector direction)
        {
            OriginTile = originTile ?? throw new ArgumentNullException(nameof(originTile));
            AdjacentTile = adjacentTile ?? throw new ArgumentNullException(nameof(adjacentTile));
            Direction = direction;
        }
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