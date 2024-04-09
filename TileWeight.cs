using System;

namespace ReplicatorLib
{
    public readonly struct TileWeight
    {
        public readonly double Weight;
        public readonly double WeightLogWeight;

        public TileWeight(double weight)
        {
            Weight = weight;
            WeightLogWeight = weight * Math.Log(weight);
        }
        public TileWeight(double weight, double precalculatedWeightLogWeight)
        {
            Weight = weight;
            WeightLogWeight = precalculatedWeightLogWeight;
        }
    }
}