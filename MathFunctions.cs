using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    public static class MathFunctions
    {
        /// <summary>
        /// Get the Shannon entropy from the total tile weight.
        /// </summary>
        public static double GetEntropy(double totalTileWeight) => GetEntropy(new TileWeight(totalTileWeight));
        /// <summary>
        /// Get the Shannon entropy from the total tile weight (with precalculated weight-log-weight).
        /// </summary>
        /// <param name="totalTileWeights"></param>
        /// <returns></returns>
        public static double GetEntropy(TileWeight totalTileWeights)
        {
            // Shannon entropy = Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;
            return Math.Log(totalTileWeights.Weight) - (totalTileWeights.WeightLogWeight / totalTileWeights.Weight);
        }

        /// <summary>
        /// Select a random value from a collection of possible values with individual weights (likelihoods). The gacha algorithm.
        /// </summary>
        public static T SelectRandomFromWeightedList<T>(IReadOnlyList<(T value, double weight)> weightedList, Random rng)
        {
            double totalWeight = weightedList.Sum(i => i.weight);

            double randomSelection = totalWeight * rng.NextDouble();
            foreach (var (value, weight) in weightedList)
            {
                if (randomSelection < weight)
                {
                    return value;
                }

                randomSelection -= weight;
            }

            throw new InvalidOperationException("Unable to select random value from weighted list.");
        }
    }
}
