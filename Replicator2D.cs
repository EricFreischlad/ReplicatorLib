using System;
using System.Collections.Generic;

namespace ReplicatorLib
{
    /// <summary>
    /// Convenience class for 2-dimensional wave functions. A wrapper around a WaveFunction&lt;T&gt; to simplify interaction with the system.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Replicator2D<T> : ReplicatorN<T, List<List<T>>>
    {
        private Replicator2D(MultiSpace space, TilingAnalysis<T> analysis, Random rng)
            : base(space, analysis, rng)
        { }
        /// <summary>
        /// Create a new 2-D replicator for procedurally generating locally-similar content to an input.
        /// </summary>
        /// <param name="minOutputIndexes">the corner of the output grid with the smallest values</param>
        /// <param name="maxOutputIndexes">the corner of the output grid with the largest values</param>
        /// <param name="isPeriodic">two values indicating whether the X or Y dimensions will be seamlessly tileable (the values on that edge will be legal neighbors to those on the opposite edge).</param>
        /// <param name="input">a grid of values for the algorithm to analyze and imitate</param>
        /// <param name="rng">optional random number generator, if more control over output values is necessary</param>
        public static Replicator2D<T> Create((int X, int Y) minOutputIndexes, (int X, int Y) maxOutputIndexes, (bool X, bool Y) isPeriodic, T[,] input, Random? rng = null)
        {
            var space = new MultiSpace(new MultiVector(minOutputIndexes.X, minOutputIndexes.Y), new MultiVector(maxOutputIndexes.X, maxOutputIndexes.Y), new bool[] { isPeriodic.X, isPeriodic.Y });
            var inputArray = MultiArray<T>.Create(new MultiSpace((0, input.GetLength(0) - 1, false), (0, input.GetLength(1) - 1, false)), coords => input[coords[0], coords[1]]);
            var analysis = new TilingAnalysis<T>(inputArray);
            return new Replicator2D<T>(space, analysis, rng ?? new Random());
        }
        protected override List<List<T>> ConvertOutput(MultiArray<T> outputArray)
        {
            var superList = new List<List<T>>(outputArray.Space.Ranges[0]);
            for (int x = 0; x < outputArray.Space.Ranges[0]; x++)
            {
                var subList = new List<T>(outputArray.Space.Ranges[1]);
                for (int y = 0; y < outputArray.Space.Ranges[1]; y++)
                {
                    subList.Add(outputArray.GetValueUnchecked(new MultiVector(x, y)));
                }
                superList.Add(subList);
            }
            return superList;
        }
    }
}