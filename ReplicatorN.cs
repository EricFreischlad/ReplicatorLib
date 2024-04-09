using System;

namespace ReplicatorLib
{
    /// <summary>
    /// Base class which wraps around a WaveFunction&lt;T&gt; to simplify interaction with the system.
    /// </summary>
    /// <typeparam name="T">any type used as the "tile"</typeparam>
    /// <typeparam name="TOutput">the type of result emitted by the TryRun function. concrete classes convert a multi-dimensional output to this type</typeparam>
    public abstract class ReplicatorN<T, TOutput>
    {
        protected MultiSpace OutputSpace { get; }
        protected TilingAnalysis<T> Analysis { get; }
        protected Random Rng { get; }
        protected WaveFunction<T> WaveFunction { get; }
        
        protected ReplicatorN(MultiSpace outputSpace, TilingAnalysis<T> analysis, Random rng)
        {
            OutputSpace = outputSpace;
            Analysis = analysis;
            Rng = rng;
            WaveFunction = new WaveFunction<T>(OutputSpace, Analysis);
        }
        /// <summary>
        /// Emits a new procedurally-generated result of the wave function. Returns true if the function was successful. Returns false if the function reached a state which was unresolvable.
        /// </summary>
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
        /// <summary>
        /// Convert a multi-dimensional output to the type defined in the concrete subclass.
        /// </summary>
        protected abstract TOutput ConvertOutput(MultiArray<T> outputArray);
    }
}