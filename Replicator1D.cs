using System;
using System.Collections.Generic;
using System.Linq;

namespace ReplicatorLib
{
    public class Replicator1D<T> : ReplicatorN<T, List<T>>
    {
        private Replicator1D(MultiSpace space, TilingAnalysis<T> analysis, Random rng)
            :base(space, analysis, rng)
        { }
        public static Replicator1D<T> Create(int min, int max, bool isPeriodic, IReadOnlyList<T> input, Random? rng = null)
        {
            var space = new MultiSpace((min, max, isPeriodic));
            var inputArray = MultiArray<T>.Create(new MultiSpace((0, input.Count - 1, false)), (ICollection<T>)input);
            var analysis = new TilingAnalysis<T>(inputArray);
            return new Replicator1D<T>(space, analysis, rng ?? new Random());
        }
        protected override List<T> ConvertOutput(MultiArray<T> outputArray)
        {
            return outputArray.ToList();
        }
    }
}