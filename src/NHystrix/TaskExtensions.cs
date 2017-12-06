using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NHystrix
{
    public static class TaskExtensions
    {
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
