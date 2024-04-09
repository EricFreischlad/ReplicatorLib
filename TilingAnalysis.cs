using System.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ReplicatorLib
{
    public sealed class TilingAnalysis<T>
    {
        /// <summary>
        /// The space helping to quickly define all directions. Each coordinate is either -1, 0, or 1.
        /// </summary>
        public MultiSpace DirectionSpace { get; }
        
        // Whitelist of allowed adjacent tiles.
        private readonly List<TilingRule<T>> _rules = new List<TilingRule<T>>();
        public IReadOnlyList<TilingRule<T>> Rules => _rules;

        // TileID -> Weight
        private readonly Dictionary<T, TileWeight> _weights;
        public IReadOnlyDictionary<T, TileWeight> Weights => _weights;
        public IEnumerable<T> RepresentedTiles => Weights.Keys;

        public TileWeight TotalWeight { get; }
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

        private void AnalyzeTile(IReadOnlyMultiCollection<T> tiling, MultiVector coords, Dictionary<T, int> pureCountsPerTile)
        {
            T tile = tiling.GetValueUnchecked(coords);

            // Freq.
            if (!pureCountsPerTile.ContainsKey(tile))
            {
                pureCountsPerTile[tile] = 1;
            }
            else
            {
                pureCountsPerTile[tile]++;
            }

            // Rules.
            foreach (var direction in DirectionSpace.EnumeratePoints())
            {
                // Skip the identity direction (0, 0, 0, ...). That doesn't count as adjacency.
                if (direction.IsZero())
                {
                    continue;
                }
                
                var adjacentCoords = tiling.Space.SimplifyCoordinates(coords + direction);
                if (!tiling.TryGetValue(adjacentCoords, out T adjacentTile)
                    || tile is null
                    || adjacentTile is null)
                {
                    continue;
                }

                var learnedRule = new TilingRule<T>(tile, adjacentTile, direction);
                
                // If the rule has not been learned yet,
                if (!_rules.Contains(learnedRule))
                {
                    // Learn the rule.
                    _rules.Add(learnedRule);

                    // Add its inverse rule as well.
                    var inverse = learnedRule.GetInverseRule();
                    if (!_rules.Contains(inverse))
                    {
                        _rules.Add(inverse);
                    }
                }
            }
        }
    }
}