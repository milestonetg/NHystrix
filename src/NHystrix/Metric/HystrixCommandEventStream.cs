using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace NHystrix.Metric
{
    /// <summary>
    /// Per-Command stream of <see cref="HystrixCommandEvent"/>s.
    /// </summary>
    /// <seealso cref="NHystrix.Metric.IHystrixEventStream{E}" />
    public class HystrixCommandEventStream : IHystrixEventStream<HystrixCommandEvent>
    {
        private static readonly ConcurrentDictionary<HystrixCommandKey, HystrixCommandEventStream> streams = new ConcurrentDictionary<HystrixCommandKey, HystrixCommandEventStream>();

        private readonly ISubject<HystrixCommandEvent> stream;

        /// <summary>
        /// Gets a Singleton instance for the specified <paramref name="commandKey"/>.
        /// </summary>
        /// <remarks>
        /// This is thread-safe and ensures only 1 <see cref="HystrixCommandEventStream"/> per <paramref name="commandKey"/>.
        /// </remarks>
        /// <param name="commandKey">The command key.</param>
        /// <returns>HystrixCommandStartStream.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        /// If an instance for <paramref name="commandKey"/> could not be found and one could not be created.
        /// In theory, this should never get thrown.
        /// </exception>
        public static HystrixCommandEventStream GetInstance(HystrixCommandKey commandKey)
        {
            // The original java code synchronized on the type to ensure only 1 thread at a time
            // could ever try to create a new instance.
            // We'll rely on ConcurrentDictionary instead.
            HystrixCommandEventStream existingStream = null;

            // if there isn't an existing stream...
            if (!streams.TryGetValue(commandKey, out existingStream))
            {
                // ...create one...
                HystrixCommandEventStream newStream = new HystrixCommandEventStream(commandKey);

                // ...and try and add it.  If if fails the add, then another thread beat us to it.
                streams.TryAdd(commandKey, newStream);
            }

            // In case the add failed, just get what ever instance is now in the dictionary.
            // This will either be the one we just created, or the one created by the thread that beat us here.
            if (streams.TryGetValue(commandKey, out existingStream))
                return existingStream;

            // In theory, we should never reach here.
            // But just in case we do, throw an exception.
            throw new KeyNotFoundException($"Event stream with command key '{commandKey}' was not found in the interned streams dictionary, nor could we create and successfully add one.");
        }

        private HystrixCommandEventStream(HystrixCommandKey commandKey)
        {
            CommandKey = commandKey;
            stream = new Subject<HystrixCommandEvent>();
        }

        /// <summary>
        /// Gets the command key.
        /// </summary>
        /// <value>The command key.</value>
        public HystrixCommandKey CommandKey { get; private set; }

        /// <summary>
        /// Completes each interned stream and, clears the interned streams dictionary. 
        /// </summary>
        internal static void Reset()
        {
            foreach (var hs in streams.Values)
                hs.stream.OnCompleted();

            streams.Clear();
        }

        /// <summary>
        /// Writes the specified hystrix event.
        /// </summary>
        /// <param name="hystrixEvent">The hystrix event.</param>
        internal void Write(HystrixCommandEvent hystrixEvent)
        {
            stream.OnNext(hystrixEvent);
        }

        /// <summary>
        /// Returns an <see cref="IObservable{T}" /> for subscribing to the event stream.
        /// </summary>
        /// <returns>IObservable&lt;HystrixCommandExecutionStarted&gt;.</returns>
        public IObservable<HystrixCommandEvent> Observe()
        {
            return stream;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>The name of the scream and <see cref="CommandKey"/> it's for in the form of "HystrixCommandStartStream({CommandKey})".</returns>
        public override string ToString()
        {
            return $"HystrixCommandStartStream({CommandKey})";
        }
    }
}
