using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReplicatorLib
{
    // A vector of arbitrary size. Used as a set of coordinates for an N-dimensional space.
    public class MultiVector : IReadOnlyList<int>, IEquatable<MultiVector>
    {
        public IReadOnlyList<int> Values { get; }
        public int DimensionCount => Values.Count;

        public MultiVector(IReadOnlyList<int> values)
        {
            Values = values.ToArray();
        }
        public MultiVector(params int[] values)
            :this((IReadOnlyList<int>)values)
        { }
        public static MultiVector Zero(int dimensionCount)
        {
            int[] vec = new int[dimensionCount];
            for (int i = 0; i < dimensionCount; i++)
            {
                vec[i] = 0;
            }
            return new MultiVector(vec);
        }
        public bool IsZero()
        {
            return Values.All(x => x == 0);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("(");
            sb.Append(Values.First());
            foreach (int value in Values.Skip(1))
            {
                sb.Append(", ");
                sb.Append(value);
            }
            sb.Append(")");
            return sb.ToString();
        }

        #region IReadOnlyList<int> Implementation
        public int this[int index] => Values[index];
        public int Count => Values.Count;
        public IEnumerator<int> GetEnumerator() => Values.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as MultiVector);
        }
        public bool Equals(MultiVector? other)
        {
            return other is { }
            && Values.SequenceEqual(other.Values);
        }
        public static bool operator ==(MultiVector a, MultiVector b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(MultiVector a, MultiVector b)
        {
            return !a.Equals(b);
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 19;
                foreach (var val in Values)
                {
                    hash = hash * 31 + val.GetHashCode(); // 31 can be replaced with another prime number
                }
                return hash;
            }
        }
        #endregion
        #region Math
        private static MultiVector UnaryMathOperation(MultiVector value, Func<int, int> operation)
        {
            return new MultiVector(value.Select(operation).ToArray());
        }
        public static MultiVector operator- (MultiVector value)
        {
            return UnaryMathOperation(value, x => -x);
        }
        private static MultiVector BinaryMathOperation(MultiVector l, MultiVector r, Func<int, int, int> operation)
        {
            if (l.DimensionCount != r.DimensionCount)
            {
                throw new ArgumentException("Dimension count mismatch", nameof(r));
            }

            return new MultiVector(l.Values.Zip(r.Values, operation).ToArray());
        }
        public static MultiVector operator +(MultiVector a, MultiVector b)
        {
            return BinaryMathOperation(a, b, (l, r) => l + r);
        }
        public static MultiVector operator -(MultiVector a, MultiVector b)
        {
            return BinaryMathOperation(a, b, (l, r) => l - r);
        }
        public static MultiVector operator *(MultiVector a, MultiVector b)
        {
            return BinaryMathOperation(a, b, (l, r) => l * r);
        }
        public static MultiVector operator /(MultiVector a, MultiVector b)
        {
            return BinaryMathOperation(a, b, (l, r) => l / r);
        }
        public static MultiVector operator %(MultiVector a, MultiVector b)
        {
            return BinaryMathOperation(a, b, (l, r) => l % r);
        }
        #endregion
    }
}
