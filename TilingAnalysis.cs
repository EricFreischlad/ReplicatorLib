using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ReplicatorLib
{
    /// <summary>
    /// A collection of tiling rules, tile frequencies/weights, and adjacency direction vectors used to create an output which immitates an analyzed input.
    /// </summary>
    public sealed class TilingAnalysis<T>
    {
        /// <summary>
        /// The space helping to quickly define all directions. Each coordinate is either -1, 0, or 1. Be sure to skip the origin direction (coordinates all 0) when enumerating, unless you need it for some reason.
        /// </summary>
        public MultiSpace DirectionSpace { get; }

        private readonly List<TilingRule<T>> _rules = new List<TilingRule<T>>();
        /// <summary>
        /// All tiling rules. A.k.a. the complete whitelist of allowed adjacent tiles.
        /// </summary>
        public IReadOnlyList<TilingRule<T>> Rules => _rules;

        private readonly Dictionary<T, TileWeight> _weights;
        /// <summary>
        /// Dictionary of weights by tile. Determines the expected frequency of tiles in the output.
        /// </summary>
        public IReadOnlyDictionary<T, TileWeight> Weights => _weights;
        /// <summary>
        /// A list of all possible tiles in the output.
        /// </summary>
        public IEnumerable<T> RepresentedTiles => Weights.Keys;

        /// <summary>
        /// The total weight (and weight-log-weight) of all possible tiles in the output.
        /// </summary>
        public TileWeight TotalWeight { get; }
        /// <summary>
        /// The total Shannon entropy of a new node in the output.
        /// </summary>
        public double MaxEntropy { get; }

        public TilingAnalysis(IReadOnlyMultiCollection<T> tiling)
        {
            DirectionSpace = CalculateDirectionSpace(tiling.Space);
            
            Dictionary<T, int> pureCountsPerTile = new Dictionary<T, int>();
            foreach (MultiVector coord in tiling.Space.EnumeratePoints())
            {
                AnalyzeTile(tiling, coord, pureCountsPerTile);
            }

            // Don't want to waste time calculating weight-log-weights every time we add a new tile instance. Just do it when we've counted all.
            _weights = new Dictionary<T, TileWeight>(pureCountsPerTile.Select(kvp => new KeyValuePair<T, TileWeight>(kvp.Key, new TileWeight(kvp.Value))));
            TotalWeight = new TileWeight(pureCountsPerTile.Sum(kvp => kvp.Value));
            MaxEntropy = MathFunctions.GetEntropy(TotalWeight);
        }
        public TilingAnalysis(MultiSpace space, IEnumerable<TilingRule<T>> rules, IReadOnlyDictionary<T, int> tileCounts)
        {
            DirectionSpace = CalculateDirectionSpace(space);

            // Add only the unique tiling rules.
            _rules.AddRange(rules.Distinct());

            if (rules.Any(r => r.Direction.DimensionCount != space.DimensionCount))
            {
                throw new System.ArgumentException($"Dimension count mismatch for a provided rule.", nameof(rules));
            }

            _weights = new Dictionary<T, TileWeight>(tileCounts.Select(kvp => new KeyValuePair<T, TileWeight>(kvp.Key, new TileWeight(kvp.Value))));
            TotalWeight = new TileWeight(tileCounts.Sum(kvp => kvp.Value));
            MaxEntropy = MathFunctions.GetEntropy(TotalWeight);
        }
        private static MultiSpace CalculateDirectionSpace(MultiSpace space)
        {
            int[] directionVector = new int[space.DimensionCount];
            for (int i = 0; i < directionVector.Length; i++)
            {
                directionVector[i] = space.Ranges[i] == 0 ? 0 : 1;
            }
            var pos = new MultiVector(directionVector);
            return new MultiSpace(-pos, pos, space.PeriodicityByDimension);
        }

        // Look at a single tile of the input and use it to learn rules and tile frequencies for the output.
        private void AnalyzeTile(IReadOnlyMultiCollection<T> tiling, MultiVector coords, Dictionary<T, int> countPerTile)
        {
            // Start with the observed tile.
            T tile = tiling.GetValueUnchecked(coords);

            // 1.) Update the weight (frequency) for this tile.

            // If the tile has not been seen yet,
            if (!countPerTile.ContainsKey(tile))
            {
                // Add the tile to the dictionary and set its weight to 1.
                countPerTile[tile] = 1;
            }
            else
            {
                // Otherwise, add 1 to the weight.
                countPerTile[tile]++;
            }

            // 2.) Learn any new rules.

            // For each adjacent direction (depends on the number of dimensions in the space).
            foreach (var direction in DirectionSpace.EnumeratePoints())
            {
                // Skip the identity direction (0, 0, 0, ...). That doesn't count as adjacency for our purposes.
                if (direction.IsZero())
                {
                    continue;
                }
                
                // Get the adjacent coordinates (accounting for periodic dimensions).
                var adjacentCoords = tiling.Space.SimplifyCoordinates(coords + direction);
                
                // Get the tile at those coordinates, unless it's out of bounds.
                if (!tiling.TryGetValue(adjacentCoords, out T adjacentTile)
                    || tile is null
                    || adjacentTile is null)
                {
                    continue;
                }

                // Determine the tiling rule that would be learned by the relationship between these two tiles.
                var learnedRule = new TilingRule<T>(tile, adjacentTile, direction);
                
                // If the rule has not been learned yet,
                if (!_rules.Contains(learnedRule))
                {
                    // Learn the rule.
                    _rules.Add(learnedRule);

                    // Add its inverse rule as well. (If red is allowed to be right of green, then green is allowed to be left of red)
                    _rules.Add(learnedRule.GetInverseRule());
                }
            }
        }
    }
}