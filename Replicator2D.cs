using System;
using System.Collections.Generic;

namespace ReplicatorLib
{
    public class Replicator2D<T> : ReplicatorN<T, List<List<T>>>
    {
        private Replicator2D(MultiSpace space, TilingAnalysis<T> analysis, Random rng)
            : base(space, analysis, rng)
        { }
        public static Replicator2D<T> Create((int X, int Y) min, (int X, int Y) max, (bool X, bool Y) isPeriodic, T[,] input, Random? rng = null)
        {
            var space = new MultiSpace(new MultiVector(min.X, min.Y), new MultiVector(max.X, max.Y), new bool[] { isPeriodic.X, isPeriodic.Y });
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