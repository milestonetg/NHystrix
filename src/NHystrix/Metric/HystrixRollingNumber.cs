using System;
using System.Linq;

namespace NHystrix.Metric
{
    /// <summary>
    /// A number which can be used to track counters (increment) or set values over time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is "rolling" in the sense that a 'timeInMilliseconds' is given that you want to track (such as 10 seconds) and then that is broken into buckets (defaults to 10) so that the 10 second window
    /// doesn't empty out and restart every 10 seconds, but instead every 1 second you have a new bucket added and one dropped so that 9 of the buckets remain and only the newest starts from scratch.
    /// </para>
    /// <para>
    /// This is done so that the statistics are gathered over a rolling 10 second window with data being added/dropped in 1 second intervals(or whatever granularity is defined by the arguments) rather
    /// than each 10 second window starting at 0 again.
    /// </para>
    /// <para>
    /// Performance-wise this class is optimized for writes, not reads. This is done because it expects far higher write volume (thousands/second) than reads (a few per second).
    /// </para>
    /// <para>
    /// For example, on each read to getSum/getCount it will iterate buckets to sum the data so that on writes we don't need to maintain the overall sum and pay the synchronization cost at each write to
    /// ensure the sum is up-to-date when the read can easily iterate each bucket to get the sum when it needs it.
    /// </para>
    /// <para>
    /// See UnitTest for usage and expected behavior examples.
    /// </para>
    /// 
    /// # Thread Safety
    /// 
    /// This class is thread-safe.
    /// </remarks>
    public class HystrixRollingNumber
    {
        int timeInMilliseconds;
        int numberOfBuckets;
        int bucketSizeInMilliseconds;

        CumulativeSum cumulativeSum;
        BucketCircularArray buckets;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixRollingNumber"/> class.
        /// </summary>
        /// <param name="timeInMilliseconds">Length of time in milliseconds to report metrics over.</param>
        /// <param name="numberOfBuckets">The number of buckets.</param>
        public HystrixRollingNumber(int timeInMilliseconds, int numberOfBuckets)
        {
            this.timeInMilliseconds = timeInMilliseconds;
            this.numberOfBuckets = numberOfBuckets;

            if (timeInMilliseconds % numberOfBuckets != 0)
                throw new ArgumentException("The timeInMilliseconds must divide equally into numberOfBuckets. For example 1000/10 is ok, 1000/11 is not.");

            bucketSizeInMilliseconds = timeInMilliseconds / numberOfBuckets;

            buckets = new BucketCircularArray(numberOfBuckets);
            cumulativeSum = new CumulativeSum();
        }

        /// <summary>
        /// Increments the counter for the specified <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        public void Increment(HystrixEventType eventType)
        {
            GetCurrentBucket().Increment(eventType);
        }

        /// <summary>
        /// Adds to the counter for the specified <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="value">The value.</param>
        public void Add(HystrixEventType eventType, long value)
        {
            GetCurrentBucket().Add(eventType, value);
        }

        /// <summary>
        /// Update a value and retain the max value for the specified <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void UpdateRollingMax(HystrixEventType eventType, long value)
        {
            //getCurrentBucket().getMaxUpdater(type).update(value);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Force a reset of all rolling counters (clear all buckets) so that statistics start being gathered from scratch.
        /// <para>
        /// This does NOT reset the CumulativeSum values.
        /// </para>
        /// </summary>
        public void Reset()
        {
            // if we are resetting, that means the lastBucket won't have a chance to be captured in CumulativeSum, so let's do it here
            Bucket lastBucket = buckets.GetLast();
            if (lastBucket != null)
            {
                cumulativeSum.AddBucket(lastBucket);
            }

            // clear buckets so we start over again
            buckets.Clear();
        }

        /// <summary>
        /// Get the cumulative sum of all buckets ever since the JVM started without rolling for the given <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64.</returns>
        public long GetCumulativeSum(HystrixEventType eventType)
        {
            // this isn't 100% atomic since multiple threads can be affecting latestBucket & cumulativeSum independently
            // but that's okay since the count is always a moving target and we're accepting a "point in time" best attempt
            // we are however putting 'getValueOfLatestBucket' first since it can have side-affects on cumulativeSum whereas the inverse is not true
            return GetValueOfLatestBucket(eventType) + cumulativeSum.Get(eventType);
        }

        /// <summary>
        /// Get the sum of all buckets in the rolling counter for the given <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64.</returns>
        public long GetRollingSum(HystrixEventType eventType)
        {
            Bucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
                return 0;

            long sum = 0;
            foreach (Bucket b in buckets)
            {
                sum += b.Counters[eventType.Ordinal];
            }
            return sum;
        }

        /// <summary>
        /// Get the value of the latest (current) bucket in the rolling counter for the given <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64.</returns>
        public long GetValueOfLatestBucket(HystrixEventType eventType)
        {
            Bucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
                return 0;
            // we have bucket data so we'll return the lastBucket
            return lastBucket.Get(eventType);
        }

        /// <summary>
        /// Get an array of values for all buckets in the rolling counter for the given <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64[].</returns>
        public long[] GetValues(HystrixEventType eventType)
        {
            Bucket lastBucket = GetCurrentBucket();
            if (lastBucket == null)
                return new long[0];

            // get buckets as an array (which is a copy of the current state at this point in time)
            Bucket[] bucketArray = buckets.ToArray();

            // we have bucket data so we'll return an array of values for all buckets
            long[] values = new long[bucketArray.Length];
            int i = 0;
            foreach (Bucket bucket in bucketArray)
            {
                values[i++] = bucket.Get(eventType);
            }
            return values;
        }

        /// <summary>
        /// Get the max value of values in all buckets for the given <see cref="HystrixEventType"/>.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <returns>System.Int64.</returns>
        public long GetRollingMaxValue(HystrixEventType eventType)
        {
            long[] values = GetValues(eventType);
            if (values.Length == 0)
            {
                return 0;
            }
            else
            {
                return values.Max();
            }
        }

        Bucket GetCurrentBucket()
        {
            DateTime currentTime = DateTime.UtcNow;

            //Get the current bucket.
            Bucket lastBucket = buckets.GetLast();

            //If the current time falls withing the bucket's window, return it.
            if (lastBucket != null && currentTime < lastBucket.WindowStartTime.AddMilliseconds(bucketSizeInMilliseconds))
                return lastBucket;


            //We didn't find a bucket. If there aren't any buckets at all, create one and return it.
            if (buckets.GetLast() == null)
            {
                Bucket bucket = new Bucket(currentTime);
                buckets.Add(bucket);
                return bucket;
            }

            //We have a bucket, but it's old.
            lastBucket = buckets.GetLast();

            //Catch up
            for (int i = 0; i < numberOfBuckets; i++)
            {
                if (currentTime < lastBucket.WindowStartTime.AddMilliseconds(bucketSizeInMilliseconds))
                {
                    return lastBucket;
                }
                else if ((currentTime - (lastBucket.WindowStartTime.AddMilliseconds(bucketSizeInMilliseconds))).TotalMilliseconds > timeInMilliseconds)
                {
                    // the time passed is greater than the entire rolling counter so we want to clear it all and start from scratch
                    Reset();
                    // recursively call getCurrentBucket which will create a new bucket and return it
                    return GetCurrentBucket();
                }
                else
                {
                    // we're past the window so we need to create a new bucket
                    // create a new bucket and add it as the new 'last'
                    Bucket bucket = new Bucket(currentTime);
                    buckets.Add(bucket);

                    // add the lastBucket values to the cumulativeSum
                    cumulativeSum.AddBucket(lastBucket);
                }
            }

            return buckets.GetLast();
        }
    }
}
