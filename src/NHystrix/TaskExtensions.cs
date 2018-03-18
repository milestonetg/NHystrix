using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NHystrix
{
    /// <summary>
    /// Extension methods for <see cref="Task"/>
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Execute the task with the specified timeout.
        /// </summary>
        /// <typeparam name="TResult">The type of the t result.</typeparam>
        /// <param name="task">The task.</param>
        /// <param name="timeoutInMilliseconds">The timeout in milliseconds.</param>
        /// <returns>Task&lt;TResult&gt;.</returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, int timeoutInMilliseconds)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeoutInMilliseconds)))
            {
                return await task.ConfigureAwait(false);
            }
            throw new TimeoutException();
        }
    }
}
