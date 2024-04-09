using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ReplicatorLib
{
    /// <summary>
    /// A single node in a wave function. Manages all possibilities for a given point in the function's ouput space.
    /// </summary>
    public sealed class WaveNode<T>
    {
        // A quick handle to the tiling analysis for the parent wave function.
        private readonly TilingAnalysis<T> _analysis;

        private readonly Dictionary<T, TileEnablement> _possibleTiles;
        /// <summary>
        /// A dictionary of all possible tiles and their enablement from all directions.
        /// </summary>
        public IReadOnlyDictionary<T, TileEnablement> PossibleTiles => _possibleTiles;

        /// <summary>
        /// The current sum total of the weights of all possible tiles for this node.
        /// </summary>
        public TileWeight CurrentTotalWeight { get; private set; }
        /// <summary>
        /// The current total Shannon entropy for this node.
        /// </summary>
        public double CurrentTotalEntropy { get; private set; }

        public WaveNode(TilingAnalysis<T> tilingAnalysis)
        {
            _analysis = tilingAnalysis;
            _possibleTiles = InitializePossibleTiles(tilingAnalysis);

            CurrentTotalWeight = tilingAnalysis.TotalWeight;
            CurrentTotalEntropy = tilingAnalysis.MaxEntropy;
        }
        public WaveNode(WaveNode<T> other)
        {
            _analysis = other._analysis;
            _possibleTiles = new Dictionary<T, TileEnablement>(other._possibleTiles.Select((kvp => new KeyValuePair<T, TileEnablement>(kvp.Key, new TileEnablement(kvp.Value)))));

            CurrentTotalWeight = other.CurrentTotalWeight;
            CurrentTotalEntropy = other.CurrentTotalEntropy;
        }
        private static Dictionary<T, TileEnablement> InitializePossibleTiles(TilingAnalysis<T> tilingAnalysis)
        {
            var dict = new Dictionary<T, TileEnablement>(tilingAnalysis.RepresentedTiles.Select(tile => new KeyValuePair<T, TileEnablement>(tile, new TileEnablement())));

            // "Enablement count" is equal to the number of rules which allow this tile to be adjacent to another one.
            // We use the opposite direction because if a rule says "red can have blue to the east", we're declaring that "blue is enabled while west of red".

            foreach (var direction in tilingAnalysis.DirectionSpace.EnumeratePoints())
            {
                foreach (var (tile, enablement) in dict)
                {
                    enablement.SetEnablementCount(direction,
                        tilingAnalysis.Rules.Count(rule =>
                            rule.AdjacentTile!.GetHashCode() == tile!.GetHashCode()
                            && rule.Direction == -direction));
                }
            }

            return dict;
        }

        /// <summary>
        /// Remove a possibile tile from this node. Emits true if there are no longer any possible tiles at this location and the wave function cannot resolve.
        /// </summary>
        /// <exception cref="System.ArgumentException"></exception>
        public void RemovePossibleTile(T tile, out bool isUnresolveable)
        {
            if (_possibleTiles.TryGetValue(tile, out var _))
            {
                _possibleTiles.Remove(tile);

                isUnresolveable = _possibleTiles.Count == 0;

                // Don't need to update weights if this object is going to be thrown out in a second.
                if (isUnresolveable)
                {
                    return;
                }

                var weightsOfRemovedTile = _analysis.Weights[tile];

                // Update pre-calculated math.
                CurrentTotalWeight = new TileWeight(CurrentTotalWeight.Weight - weightsOfRemovedTile.Weight, CurrentTotalWeight.WeightLogWeight - weightsOfRemovedTile.WeightLogWeight);
                CurrentTotalEntropy = MathFunctions.GetEntropy(CurrentTotalWeight);
            }
            else
            {
                throw new System.ArgumentException($"Unrecognized tile \"{tile}\".", nameof(tile));
            }
        }
    }
}