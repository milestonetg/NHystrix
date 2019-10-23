using System;
using System.Threading.Tasks;

namespace NHystrix
{
    /// <summary>
    /// Interface implemented by <see cref="HystrixCommand{TRequest, TResult}" />s.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IHystrixCommand<TRequest, TResult>
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>TResult.</returns>
        TResult Execute(TRequest request);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Task&lt;TResult&gt;.</returns>
        Task<TResult> ExecuteAsync(TRequest request);
    }
}
