// <copyright file="AWSSecretsManagerConfigurationSource.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

using System;
using Amazon.SecretsManager;
using JetBrains.Annotations;
using static System.TimeSpan;
using static Tiger.Secrets.Properties.Resources;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>A source of AWS Secrets Manager configuration key/values for an application.</summary>
    public sealed class AWSSecretsManagerConfigurationSource
        : IConfigurationSource
    {
        /// <summary>Initializes a new instance of the <see cref="AWSSecretsManagerConfigurationSource"/> class.</summary>
        /// <param name="secretsManagerClient">The client to use to retrieve secret values.</param>
        /// <param name="secretId">The unique identifer of the secret to use for configuration.</param>
        /// <param name="expiration">The amount of time after which configuration will be reloaded.</param>
        /// <exception cref="ArgumentNullException"><paramref name="secretsManagerClient"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="secretId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="expiration"/> is negative.</exception>
        public AWSSecretsManagerConfigurationSource(
            [NotNull] IAmazonSecretsManager secretsManagerClient,
            [NotNull] string secretId,
            [NotNull] TimeSpan expiration)
        {
            if (expiration == Zero) { throw new ArgumentOutOfRangeException(nameof(expiration), ExpirationIsInvalid); }

            SecretsManagerClient = secretsManagerClient ?? throw new ArgumentNullException(nameof(secretsManagerClient));
            SecretId = secretId ?? throw new ArgumentNullException(nameof(secretId));
            Expiration = expiration;
        }

        /// <summary>Gets the client to use to retrieve secret values.</summary>
        public IAmazonSecretsManager SecretsManagerClient { get; }

        /// <summary>Gets the unique identifer of the secret to use for configuration.</summary>
        public string SecretId { get; }

        /// <summary>Gets the amount of time after which configuration will be reloaded.</summary>
        public TimeSpan Expiration { get; }

        /// <inheritdoc/>
        IConfigurationProvider IConfigurationSource.Build(IConfigurationBuilder builder) =>
            new AWSSecretsManagerConfigurationProvider(this);
    }
}
