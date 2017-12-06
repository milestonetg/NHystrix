﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NHystrix
{
    /// <summary>
    /// A HystrixCommandGroup is used to group commands together.  For example, by service or resource.
    /// </summary>
    public struct HystrixCommandGroup 
    {
        ConcurrentBag<HystrixCommandKey> keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandGroup"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        public HystrixCommandGroup(string name)
        {
            Name = name;
            keys = new ConcurrentBag<HystrixCommandKey>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets the command keys.
        /// </summary>
        /// <value>The command keys.</value>
        public IReadOnlyCollection<HystrixCommandKey> CommandKeys { get => keys; }

        /// <summary>
        /// Creates a new <see cref="HystrixCommandKey" /> with the specified name and
        /// adds it to this group.
        /// </summary>
        /// <param name="commandKeyName">Name of the command key.</param>
        /// <returns>The new <see cref="HystrixCommandKey"/>.</returns>
        public HystrixCommandKey AddCommandKey(string commandKeyName)
        {
            return new HystrixCommandKey(commandKeyName, this);
        }

        /// <summary>
        /// Adds the command key to this group.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <exception cref="System.InvalidOperationException">Key already belongs to a different group.</exception>
        internal void AddCommandKey(HystrixCommandKey commandKey)
        {
            if (!commandKey.Group.Equals(this))
                throw new InvalidOperationException("Key already belongs to a different group.");

            if (!keys.TryPeek(out HystrixCommandKey existingKey))
                keys.Add(commandKey);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}