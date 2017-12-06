using System;
using System.Collections.Generic;

namespace NHystrix
{
    /// <summary>
    /// Class HystrixEventType. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="System.IEquatable{T}" />
    public sealed class HystrixEventType : IEquatable<HystrixEventType>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public static readonly HystrixEventType EMIT;
        public static readonly HystrixEventType SUCCESS;
        public static readonly HystrixEventType FAILURE;
        public static readonly HystrixEventType TIMEOUT;
        public static readonly HystrixEventType BAD_REQUEST;

        /// <summary>
        /// The circuit breaker is open, so the command was short circuited.
        /// </summary>
        public static readonly HystrixEventType SHORT_CIRCUITED;

        /// <summary>
        /// The maximum number of bulkheading semaphores is reached and a semaphore could not be acquired.
        /// </summary>
        public static readonly HystrixEventType SEMAPHORE_REJECTED;

        public static readonly HystrixEventType THREAD_POOL_REJECTED;
        public static readonly HystrixEventType FALLBACK_EMIT;
        public static readonly HystrixEventType FALLBACK_SUCCESS;
        public static readonly HystrixEventType FALLBACK_FAILURE;
        public static readonly HystrixEventType FALLBACK_REJECTION;
        public static readonly HystrixEventType FALLBACK_DISABLED;
        public static readonly HystrixEventType FALLBACK_MISSING;
        public static readonly HystrixEventType EXCEPTION_THROWN;
        public static readonly HystrixEventType RESPONSE_FROM_CACHE;
        public static readonly HystrixEventType CANCELLED;
        public static readonly HystrixEventType COLLAPSED;
        public static readonly HystrixEventType COMMAND_MAX_ACTIVE;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        internal static readonly IReadOnlyList<HystrixEventType> HystrixEventTypes;
        
        static HystrixEventType()
        {
            EMIT = new HystrixEventType(0, 0, false);
            SUCCESS = new HystrixEventType(1, 1, true);
            FAILURE = new HystrixEventType(2, 2, false);
            TIMEOUT = new HystrixEventType(3, 3, false);
            BAD_REQUEST = new HystrixEventType(4, 4, true);
            SHORT_CIRCUITED = new HystrixEventType(5, 5, false);
            THREAD_POOL_REJECTED = new HystrixEventType(6, 6, false);
            SEMAPHORE_REJECTED = new HystrixEventType(7, 7, false);
            FALLBACK_EMIT = new HystrixEventType(8, 8, false);
            FALLBACK_SUCCESS = new HystrixEventType(9, 9, true);
            FALLBACK_FAILURE = new HystrixEventType(10, 10, true);
            FALLBACK_REJECTION = new HystrixEventType(11, 11, true);
            FALLBACK_DISABLED = new HystrixEventType(12, 12, true);
            FALLBACK_MISSING = new HystrixEventType(13, 13, true);
            EXCEPTION_THROWN = new HystrixEventType(14, 14, false);
            RESPONSE_FROM_CACHE = new HystrixEventType(15, 15, true);
            CANCELLED = new HystrixEventType(16, 16, true);
            COLLAPSED = new HystrixEventType(17, 17, false);
            COMMAND_MAX_ACTIVE = new HystrixEventType(18, 18, false);

            HystrixEventTypes = new List<HystrixEventType>(new[] 
            {
                EMIT, SUCCESS, FAILURE, TIMEOUT, BAD_REQUEST, SHORT_CIRCUITED, THREAD_POOL_REJECTED, SEMAPHORE_REJECTED,
                FALLBACK_DISABLED, FALLBACK_EMIT, FALLBACK_FAILURE, FALLBACK_MISSING, FALLBACK_REJECTION, FALLBACK_SUCCESS,
                EXCEPTION_THROWN, RESPONSE_FROM_CACHE, CANCELLED, COLLAPSED, COMMAND_MAX_ACTIVE
            });
        }

        HystrixEventType(int ordinal, short value, bool isTerminal)
        {
            IsTerminal = isTerminal;
            Value = value;
            Ordinal = ordinal;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public short Value { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is terminal.
        /// </summary>
        /// <value><c>true</c> if this instance is terminal; otherwise, <c>false</c>.</value>
        public bool IsTerminal { get; private set; }

        /// <summary>
        /// Gets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        internal int Ordinal { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is HystrixEventType && Equals((HystrixEventType)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(HystrixEventType other)
        {
            return Value == other.Value;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            var hashCode = -783812246;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Value.GetHashCode();
            return hashCode;
        }
    }
}
