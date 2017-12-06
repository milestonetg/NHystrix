using System;

namespace NHystrix
{
    /// <summary>
    /// A HystrixCommandKey is used to identify a specific command within a group.
    /// This key is used as the caching key for metrics, etc.
    /// </summary>
    public struct HystrixCommandKey
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Group.Name}/{Name}";
        }
    }
}
