using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    public static class MathFunctions
    {
        public static double GetEntropy(double totalTileWeight) => GetEntropy(new TileWeight(totalTileWeight));
        public static double GetEntropy(TileWeight totalTileWeights)
        {
            // Shannon entropy = Log(sumOfWeights) - sumOfWeightLogWeights / sumOfWeights;
            return Math.Log(totalTileWeights.Weight) - (totalTileWeights.WeightLogWeight / totalTileWeights.Weight);
        }

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
