using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NHystrix.Metric
{
    /// <summary>
    /// Class CumulativeSum.
    /// </summary>
    internal class CumulativeSum
    {
        long[] sums;

        /// <summary>
        /// Initializes a new instance of the <see cref="CumulativeSum"/> class.
        /// </summary>
        internal CumulativeSum()
        {
            sums = new long[HystrixEventType.HystrixEventTypes.Count];
        }

        /// <summary>
        /// Adds values in the specified bucket to their cumulative sums.
        /// </summary>
        /// <param name="lastBucket">The last bucket.</param>
        internal void AddBucket(Bucket lastBucket)
        {
            foreach (HystrixEventType type in HystrixEventType.HystrixEventTypes)
            {
                //if (type.isCounter())
                {
                    Interlocked.Add(ref sums[type.Ordinal], lastBucket.Counters[type.Ordinal]);
                }
                //if (type.isMaxUpdater())
                //{
                //    getMaxUpdater(type).update(lastBucket.getMaxUpdater(type).max());
                //}
            }
        }

        /// <summary>
        /// Gets cumulative sum for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.Int64.</returns>
        internal long Get(HystrixEventType type)
        {
            //if (type.IsCounter)
            {
                return Interlocked.Read(ref sums[type.Ordinal]);
            }

            //if (type.IsMaxUpdater)
            //{
            //    return updaterForCounterType[type.ordinal()].max();
            //}
            //throw new IllegalStateException("Unknown type of event: " + type.name());
        }
    }
}
