using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NHystrix
{
    /// <summary>
    /// Encapsulates the bulkhead semaphore for a HystrixCommand with the given HystrixCommandKey.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal class HystrixBulkhead : IDisposable
    {

        readonly static ConcurrentDictionary<HystrixCommandKey, HystrixBulkhead> intern =
                                new ConcurrentDictionary<HystrixCommandKey, HystrixBulkhead>();

        HystrixCommandProperties properties;

        SemaphoreSlim bulkhead;

        private HystrixBulkhead(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            CommandKey = commandKey;
            this.properties = properties;
            
            bulkhead = new SemaphoreSlim(properties.MaxConcurrentRequests);
        }

        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>The command key.</value>
        public HystrixCommandKey CommandKey { get; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <param name="properties">The properties.</param>
        /// <returns>HystrixBulkhead.</returns>
        public static HystrixBulkhead GetInstance(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            HystrixBulkhead cb = null;
            if (intern.TryGetValue(commandKey, out cb))
            {
                //the key existed, so we'll return the existing instance
                return cb;
            }
            else
            {
                //we need to add a new one...
                cb = new HystrixBulkhead(commandKey, properties);

                //try to add the key
                if (intern.TryAdd(commandKey, cb))
                {
                    return cb;
                }
                else
                {
                    //another thread beat us to it, so we'll get that instance instead.
                    intern.TryGetValue(commandKey, out cb);
                    return cb;
                }
            }
        }

        /// <summary>
        /// Tries to obtain a semaphore.
        /// </summary>
        /// <returns><c>true</c> if successful or bulkheading is disabled, <c>false</c> otherwise.</returns>
        public bool Try()
        {
            if (properties.BulkheadingEnabled)
                return bulkhead.Wait(properties.BulkheadSemaphoreAcquireTimeoutInMilliseconds);

            return true;
        }

        /// <summary>
        /// Releases the semaphore.
        /// </summary>
        /// <returns>The number of semaphores released. -1 if bulkheading is disabled.</returns>
        public int Release()
        {
            if (properties.BulkheadingEnabled)
                return bulkhead.Release();

            return -1;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    intern.TryRemove(CommandKey, out HystrixBulkhead value);
                    value = null;

                    bulkhead.Dispose();
                }

                bulkhead = null;

                disposedValue = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HystrixBulkhead"/> class.
        /// </summary>
        ~HystrixBulkhead()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
