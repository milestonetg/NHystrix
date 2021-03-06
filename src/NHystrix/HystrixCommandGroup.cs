﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NHystrix
{
    /// <summary>
    /// A HystrixCommandGroup is used to group commands together.  For example, by service or resource.
    /// </summary>
    public struct HystrixCommandGroup : IEquatable<HystrixCommandGroup>
    {
        ConcurrentDictionary<string, HystrixCommandKey> keys;

        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandGroup"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        public HystrixCommandGroup(string name)
        {
            Name = name;
            keys = new ConcurrentDictionary<string, HystrixCommandKey>();
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
        public IReadOnlyCollection<HystrixCommandKey> CommandKeys { get => new ReadOnlyCollection<HystrixCommandKey>(keys.Values.ToList()); }

        /// <summary>
        /// Creates a new <see cref="HystrixCommandKey" /> with the specified name and
        /// adds it to this group.
        /// </summary>
        /// <param name="commandKeyName">Name of the command key.</param>
        /// <returns>The new <see cref="HystrixCommandKey"/>.</returns>
        public HystrixCommandKey AddCommandKey(string commandKeyName)
        {
            return AddCommandKey(new HystrixCommandKey(commandKeyName, this));
        }

        /// <summary>
        /// Adds the command key to this group.
        /// </summary>
        /// <param name="commandKey">The command key.</param>
        /// <exception cref="System.InvalidOperationException">Key already belongs to a different group.</exception>
        internal HystrixCommandKey AddCommandKey(HystrixCommandKey commandKey)
        {
            if (!commandKey.Group.Equals(this))
                throw new InvalidOperationException("Key already belongs to a different group.");

            return keys.GetOrAdd(commandKey.Name, commandKey);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is HystrixCommandGroup && Equals((HystrixCommandGroup)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(HystrixCommandGroup other)
        {
            return Name == other.Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
