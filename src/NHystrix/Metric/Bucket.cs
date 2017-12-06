using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NHystrix.Metric
{
    /// <summary>
    /// Class Bucket.
    /// </summary>
    internal class Bucket
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Bucket"/> class.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        internal Bucket(DateTime startTime)
        {
            Counters = new long[HystrixEventType.HystrixEventTypes.Count];
            WindowStartTime = startTime;
        }

        /// <summary>
        /// Gets the window start time.
        /// </summary>
        /// <value>The window start time.</value>
        public DateTime WindowStartTime { get; private set; }

        /// <summary>
        /// Gets the counters.
        /// </summary>
        /// <value>The counters.</value>
        internal long[] Counters { get; private set; }

        /// <summary>
        /// Adds the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="num">The number.</param>
        /// <returns>System.Int64.</returns>
        internal long Add(HystrixEventType eventType, int num)
        {
            return Interlocked.Add(ref Counters[eventType.Ordinal], num);
        }

        /// <summary>
        /// Adds to the counter for the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="num">The number.</param>
        /// <returns>System.Int64.</returns>
        internal long Add(HystrixEventType eventType, long num)
        {
            return Interlocked.Add(ref Counters[eventType.Ordinal], num);
        }

        /// <summary>
        /// Increments the counter for the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64.</returns>
        internal long Increment(HystrixEventType eventType)
        {
            return Interlocked.Increment(ref Counters[eventType.Ordinal]);
        }

        /// <summary>
        /// Gets counter value for the specified event type.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64.</returns>
        internal long Get(HystrixEventType eventType)
        {
            return Interlocked.Read(ref Counters[eventType.Ordinal]);
        }
    }
}
