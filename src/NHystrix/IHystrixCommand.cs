using System.Threading.Tasks;

namespace NHystrix
{
    /// <summary>
    /// Interface implemented by <see cref="HystrixCommand{TResult}"/>s.
    /// </summary>
    /// <typeparam name="TResult">The type of the t result.</typeparam>
    public interface IHystrixCommand<TResult>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <returns>TResult.</returns>
        TResult Execute();

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>Task&lt;TResult&gt;.</returns>
        Task<TResult> ExecuteAsync();
    }
}
