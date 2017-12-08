using System;
using System.Collections.Generic;

namespace NHystrix
{
    /// <summary>
    /// A HystrixCommandKey is used to identify a specific command within a group.
    /// This key is used as the caching key for metrics, etc.
    /// </summary>
    public struct HystrixCommandKey : IEquatable<HystrixCommandKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HystrixCommandKey"/> struct.
        /// </summary>
        /// <param name="name">The name of this command.</param>
        /// <param name="group">The <see cref="HystrixCommandGroup"/> to which this command belongs.</param>
        /// <exception cref="System.ArgumentException">Invalid argument. name cannot be null or empty. - name</exception>
        public HystrixCommandKey(string name, HystrixCommandGroup group)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Invalid argument. {nameof(name)} cannot be null or empty.", nameof(name));

            Name = name;
            Group = group;

            Group.AddCommandKey(this);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <value>The group.</value>
        public HystrixCommandGroup Group { get; private set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return obj is HystrixCommandKey && Equals((HystrixCommandKey)obj);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(HystrixCommandKey other)
        {
            return Name == other.Name && Group.Name == other.Group.Name;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return $"{Group.Name}/{Name}".GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Group.Name}/{Name}";
        }
    }
}
