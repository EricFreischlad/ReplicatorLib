using System;

namespace ReplicatorLib
{
    /// <summary>
    /// The weight (likelihood) of a tile being selected as the collapsed/observed tile in the wave function. Also contains the precalculated weight-log-weight used in the Shannon entropy function.
    /// </summary>
    public readonly struct TileWeight
    {
        /// <summary>
        /// The weight (likelihood) of the tile. Equivalent to the number of copies of the result in a pool of possible results.
        /// </summary>
        public readonly double Weight;
        /// <summary>
        /// The weight of the tile times the natural logorithm of that weight. Used in calculating Shannon entropy.
        /// </summary>
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