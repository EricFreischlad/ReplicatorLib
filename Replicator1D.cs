using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    /// <summary>
    /// Convenience class for 1-dimensional wave functions. A wrapper around a WaveFunction&lt;T&gt; to simplify interaction with the system.
    /// </summary>
    public class Replicator1D<T> : ReplicatorN<T, List<T>>
    {
        private Replicator1D(MultiSpace space, TilingAnalysis<T> analysis, Random rng)
            :base(space, analysis, rng)
        { }
        /// <summary>
        /// Create a new 1-D replicator for procedurally generating locally-similar content to an input.
        /// </summary>
        /// <param name="minOutputIndex">the smallest index of the output line/list</param>
        /// <param name="maxOutputIndex">the largest index of the output line/list</param>
        /// <param name="isPeriodic">if true, the output will be seamlessly tileable (the last value will be a legal neighbor to the first value)</param>
        /// <param name="input">a list of values for the algorithm to analyze and imitate</param>
        /// <param name="rng">optional random number generator, if more control over output values is necessary</param>
        /// <returns></returns>
        public static Replicator1D<T> Create(int minOutputIndex, int maxOutputIndex, bool isPeriodic, IReadOnlyList<T> input, Random? rng = null)
        {
            var space = new MultiSpace((minOutputIndex, maxOutputIndex, isPeriodic));
            var inputArray = MultiArray<T>.Create(new MultiSpace((0, input.Count - 1, false)), (ICollection<T>)input);
            var analysis = new TilingAnalysis<T>(inputArray);
            return new Replicator1D<T>(space, analysis, rng ?? new Random());
        }
        protected override List<T> ConvertOutput(MultiArray<WaveNode<T>> output)
        {
            return output.Select(node => node.PossibleTiles.First().Key).ToList();
        }
    }
}