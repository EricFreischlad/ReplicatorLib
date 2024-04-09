using System;

namespace ReplicatorLib
{
    public abstract class ReplicatorN<T, TOutput>
    {
        protected MultiSpace Space { get; }
        protected TilingAnalysis<T> Analysis { get; }
        protected Random Rng { get; }
        protected WaveFunction<T> WaveFunction { get; }
        
        protected ReplicatorN(MultiSpace space, TilingAnalysis<T> analysis, Random rng)
        {
            Space = space;
            Analysis = analysis;
            Rng = rng;
            WaveFunction = new WaveFunction<T>(Space, Analysis);
        }
        public bool TryRun(out TOutput output)
        {
            if (WaveFunction.TryRun(Rng, out var multiOutput))
            {
                output = ConvertOutput(multiOutput!);
                return true;
            }
            output = default!;
            return false;
        }
        protected abstract TOutput ConvertOutput(MultiArray<T> outputArray);
    }
}