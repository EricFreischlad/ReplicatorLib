using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    public sealed class WaveFunction<T>
    {
        public MultiSpace Space { get; }
        public TilingAnalysis<T> TilingAnalysis { get; }

        public WaveFunction(MultiSpace space, TilingAnalysis<T> tilingAnalysis)
        {
            Space = space ?? throw new ArgumentNullException(nameof(space));

            if (tilingAnalysis.DirectionSpace.DimensionCount != space.DimensionCount)
            {
                throw new ArgumentException($"Dimension count mismatch (expected = {space.DimensionCount}, actual = {tilingAnalysis.DirectionSpace.DimensionCount}).", nameof(tilingAnalysis));
            }
            TilingAnalysis = tilingAnalysis;
        }

        public bool TryRun(Random rng, out MultiArray<T>? output)
        {
            // Set up prefab node.
            var prefabNode = new WaveNode<T>(TilingAnalysis);

            // Set up nodes by copying the prefab (a bit less calculation).
            var nodes = MultiArray<WaveNode<T>>.Create(Space, () => new WaveNode<T>(prefabNode));

            // Set up propagation stack (<coordinates, tile>). This stack contains tiles which are no longer enabled at a particular location.
            var propagationStack = new Stack<(MultiVector, T)>();

            // Forever.
            while (true)
            {
                // Get next unobserved node by lowest entropy (and select randomly from amongst ties).
                var (lowestEntropyNode, lowestEntropyNodeCoords) = nodes.EnumeratePointsWithCoordinates()
                    .Where(node => node.Value.PossibleTiles.Count > 1)
                    .OrderBy(node => node.Value.CurrentTotalEntropy)
                    .ThenBy(node => rng.NextDouble())
                    .FirstOrDefault()!;

                if (lowestEntropyNode is null)
                {
                    // All nodes are collapsed. Function complete.
                    output = MultiArray<T>.Create(Space, nodes.Select(node => node.PossibleTiles.First().Key).ToArray());
                    return true;
                }

                // Select a random state to collapse into by weight.
                T selectedTile = MathFunctions.SelectRandomFromWeightedList(lowestEntropyNode.PossibleTiles.Select(kvp => (kvp.Key, TilingAnalysis.Weights[kvp.Key].Weight)).ToArray(), rng);

                // Observe/Collapse. Ban everything that's not the selected tile.
                foreach (var possibileTile in lowestEntropyNode.PossibleTiles.Keys)
                {
                    // Skip selected tile.
                    if (possibileTile!.GetHashCode() == selectedTile!.GetHashCode())
                    {
                        continue;
                    }

                    // Ban the tile as a possibility for this node.
                    lowestEntropyNode.RemovePossibleTile(possibileTile, out bool _);
                    
                    // Push this new impossibility to the propagation stack.
                    propagationStack.Push((lowestEntropyNodeCoords, possibileTile));
                }

                // Propagate changes.
                while (propagationStack.Count > 0)
                {
                    (MultiVector propagatingNodeCoords, T bannedTile) = propagationStack.Pop();

                    // For each adjacent node, deduct enablement by this tile as per adjacency rules.
                    foreach (var direction in TilingAnalysis.DirectionSpace.EnumeratePoints())
                    {
                        // Get the coordinates of the adjacent node by moving in the direction.
                        MultiVector adjacentCoords = propagatingNodeCoords + direction;
                        
                        // Simplify the coordinates for the cases where one or more of the values wrapped around the end of a periodic dimension.
                        adjacentCoords = Space.SimplifyCoordinates(adjacentCoords);

                        // If the would-be node in this direction is out of bounds,
                        if (!nodes.TryGetValue(adjacentCoords, out var adjacentNode))
                        {
                            // Skip it.
                            continue;
                        }

                        // For each possible tile the adjacent node could be...
                        foreach (var (possibleTile, possibleTileEnablement) in adjacentNode.PossibleTiles)
                        {
                            // If the possibility is enabled by the tile that is now being banned,
                            if (TilingAnalysis.Rules.Contains(new TilingRule<T>(bannedTile, possibleTile, direction)))
                            {
                                // Remove that enablement.
                                possibleTileEnablement.RemoveEnablementFromDirection(-direction, 1, out bool isStillPossible);

                                // If the tile is no longer a possibility for the adjacent node (due to losing all enablers),
                                if (!isStillPossible)
                                {
                                    // Ban the tile as a possibility for this node.
                                    adjacentNode.RemovePossibleTile(possibleTile, out bool isUnresolvable);
                                    
                                    // If the adjacent node no longer has any possible states, then this function can't complete. Bail out.
                                    if (isUnresolvable)
                                    {
                                        output = default;
                                        return false;
                                    }

                                    // If there are still options for the adjacent node, push the banned tile to the propagation stack.
                                    propagationStack.Push((adjacentCoords, possibleTile));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}