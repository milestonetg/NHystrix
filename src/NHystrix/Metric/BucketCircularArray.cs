using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NHystrix.Metric
{
    /// <summary>
    /// Class BucketCircularArray.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{T}" />
    internal class BucketCircularArray : IEnumerable<Bucket>
    {
        LinkedList<Bucket> buckets;

        internal BucketCircularArray(int numberOfBuckets)
        {
            Size = numberOfBuckets;

            buckets = new LinkedList<Bucket>();
        }

        /// <summary>
        /// Gets the number of buckets.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; private set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Bucket> GetEnumerator()
        {
            return buckets.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return buckets.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified bucket.
        /// </summary>
        /// <param name="bucket">The bucket.</param>
        internal void Add(Bucket bucket)
        {
            lock (buckets)
            {
                if (!buckets.Any(b => b.WindowStartTime == bucket.WindowStartTime))
                {
                    buckets.AddLast(bucket);
                    if (buckets.Count > Size)
                        buckets.RemoveFirst();
                }
            }
        }

        /// <summary>
        /// Gets the last bucket (tail).
        /// </summary>
        /// <returns>Bucket.</returns>
        internal Bucket GetLast()
        {
            lock (buckets)
            {
                if (buckets.Count == 0)
                    return null;

                return buckets.Last.Value;
            }
        }

        /// <summary>
        /// Clears this array.
        /// </summary>
        internal void Clear()
        {
            lock(buckets)
                buckets.Clear();
        }

        /// <summary>
        /// Converts this instance to an array of <see cref="Bucket"/>s.
        /// </summary>
        /// <returns>Bucket[]</returns>
        internal Bucket[] ToArray()
        {
            return buckets.ToArray();
        }
    }
}
