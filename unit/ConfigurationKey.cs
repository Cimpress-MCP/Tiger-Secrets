using System;
using JetBrains.Annotations;

namespace Test
{
    /// <summary>Represents a single-level configuration key.</summary>
    public readonly struct ConfigurationKey
    {
        /// <summary>Initializes a new instance of the <see cref="ConfigurationKey"/> struct.</summary>
        /// <param name="configurationKey">The raw configuration key.</param>
        public ConfigurationKey([NotNull] string configurationKey)
        {
            Get = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
        }

        /// <summary>Gets the underlying value.</summary>
        public string Get { get; }

        /// <inheritdoc/>
        [NotNull, Pure]
        public override string ToString() => Get;

        /// <summary>Converts a configuation key to a string.</summary>
        /// <param name="ck">The configuration key to convert.</param>
        [NotNull]
        public static implicit operator string(in ConfigurationKey ck) => ck.ToString();
    }
}
