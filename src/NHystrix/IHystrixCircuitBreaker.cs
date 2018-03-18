using System;

namespace NHystrix
{
    /// <summary>
    /// Circuit breaker logic that is hooked into <see cref="HystrixCommand{TRequest, TResult}"/> execution and will stop allowing executions if failures have gone past the defined threshold.
    /// <para>
    /// The default (and only) implementation  will then allow a single retry after a defined sleepWindow until the execution
    /// succeeds at which point it will again close the circuit and allow executions again.
    /// </para>
    ///</summary>
    public interface IHystrixCircuitBreaker
    {
        /// <summary>
        /// Every <see cref="HystrixCommand{TRequest, TResult}"/> request asks this if it is allowed to proceed or not.  It is idempotent and does
        /// not modify any internal state, and takes into account the half-open logic which allows some requests through
        /// after the circuit has been opened
        ///</summary>
        ///<returns>boolean whether a request should be permitted</returns>
        bool ShouldAllowRequest { get; }

        /// <summary>
        /// Invoked at start of command execution to attempt an execution.  This is non-idempotent - it may modify internal
        /// state.
        /// </summary>
        bool ShouldAttemptExecution { get; }

        /// <summary>
        /// Whether the circuit is currently open (tripped).
        ///</summary>
        ///<returns>boolean state of circuit breaker</returns>
        bool IsOpen { get; }

        /// <summary>
        /// Trips the circuit breaker and begins the sleep window.
        /// </summary>
        void Trip();

        /// <summary>
        /// Invoked on successful executions from <see cref="HystrixCommand{TRequest, TResult}"/> as part of feedback mechanism when in a half-open state.
        /// </summary>
        void MarkSuccess();

        /// <summary>
        /// Invoked on unsuccessful executions from <see cref="HystrixCommand{TRequest, TResult}"/> as part of feedback mechanism when in a half-open state.
        ///</summary>
        void MarkNonSuccess();
    }
}
