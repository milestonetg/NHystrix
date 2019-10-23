using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NHystrix
{
    /// <summary>
    /// Used by the HystrixCommand for queuing up asynchronous calls.
    /// Dequeue is throttled by the <see cref="HystrixCommandProperties.MaxConcurrentRequests"/> property value.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    internal class HystrixWorker<TRequest, TResult>
    {
        // boxes up the work
        class WorkItem
        {
            public HystrixCommand<TRequest, TResult> Command { get; set; }

            public TRequest Request { get; set; }

            public Action<Task<TResult>> Callback { get; set; }

        }

        readonly static ConcurrentDictionary<HystrixCommandKey, HystrixWorker<TRequest, TResult>> intern =
                        new ConcurrentDictionary<HystrixCommandKey, HystrixWorker<TRequest, TResult>>();

        ConcurrentQueue<WorkItem> queue;
        HystrixCommandProperties properties;
        SemaphoreSlim bulkhead;
        Task processTask;
        CancellationTokenSource tokenSource;
        object processLock = new object();

        private HystrixWorker(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            CommandKey = commandKey;
            this.properties = properties;

            bulkhead = new SemaphoreSlim(properties.MaxConcurrentRequests);
            queue = new ConcurrentQueue<WorkItem>();
            tokenSource = new CancellationTokenSource();
            processTask = Task.Factory.StartNew(Process, tokenSource.Token);
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
        /// <returns>HystrixWorker.</returns>
        public static HystrixWorker<TRequest, TResult> GetInstance(HystrixCommandKey commandKey, HystrixCommandProperties properties)
        {
            HystrixWorker<TRequest, TResult> cb = null;
            if (intern.TryGetValue(commandKey, out cb))
            {
                //the key existed, so we'll return the existing instance
                return cb;
            }
            else
            {
                //we need to add a new one...
                cb = new HystrixWorker<TRequest, TResult>(commandKey, properties);

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
        /// Enqueues the specified command for work on a background thread.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="callback">The callback action</param>
        public void Enqueue(HystrixCommand<TRequest, TResult> command, TRequest request, Action<Task<TResult>> callback = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            queue.Enqueue(new WorkItem { Command = command, Callback = callback, Request = request });

            lock(processLock)
            {
                Monitor.PulseAll(processLock);
            }
        }

        private void Process()
        {
            while(!tokenSource.IsCancellationRequested)
            {
                if (queue.TryDequeue(out WorkItem workItem) && workItem != null)
                {
                    bulkhead.Wait();

                    workItem.Command.ExecuteAsync(workItem.Request)
                        .ContinueWith(t => {
                            try
                            {
                                workItem.Callback?.Invoke(t);
                            }
                            catch
                            {
                                //safety net in case there is no try/catch in the callback.
                                //we don't want to blow up the worker
                            }
                            finally
                            {
                                bulkhead.Release();
                            }
                        });
                }
                else
                {
                    lock (processLock)
                    {
                        Monitor.Wait(processLock);
                    }
                }
            }
        }
    }
}
