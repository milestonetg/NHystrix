using System;

namespace NHystrix.Metric
{
    /// <summary>
    /// Interface implemented by all event streams
    /// </summary>
    /// <typeparam name="E"></typeparam>
    public interface IHystrixEventStream<E> where E : IHystrixEvent
    {
        /// <summary>
        /// Returns an <see cref="IObservable{T}"/> for subscribing to the event stream.
        /// </summary>
        /// <returns>IObservable&lt;E&gt;.</returns>
        IObservable<E> Observe();
    }
}
