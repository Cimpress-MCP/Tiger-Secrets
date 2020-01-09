// <copyright file="SecretsManagerConfigurationBuilderExtensions.cs" company="Cimpress, Inc.">
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

using System.Linq;
using Amazon.SecretsManager;
using JetBrains.Annotations;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>Extends the functionality of <see cref="IConfigurationBuilder"/> for AWS Secrets Manager.</summary>
    public static class SecretsManagerConfigurationBuilderExtensions
    {
        const string SectionName = "Secrets";

        /// <summary>Adds AWS Secrets Manager as a configuration source.</summary>
        /// <param name="builder">The configuration builder to which to add.</param>
        /// <param name="sectionName">
        /// The name of the configuration section from which to configure the configuration source.
        /// If no value is provided, a default value of "Secrets" is used.
        /// </param>
        /// <returns>The modified configuration builder.</returns>
        [NotNull]
        public static IConfigurationBuilder AddSecretsManager(
            [NotNull] this IConfigurationBuilder builder,
            string sectionName = SectionName)
        {
            /* because(cosborn)
             * I hate doing this, but:
             * 1. We want to call Build() once and reuse the IAmazonSecretsManager instance if we can. (And we can.)
             * 2. We want the IAmazonSecretsManager instance to have a _hope_ of being configured.
             * There must be something better??? (spoiler: there's not)
             */
            var configuration = builder.AddEnvironmentVariables().Build();
            var secretsOpts = configuration.GetSection(sectionName).Get<SecretsOptions>();

            if (secretsOpts.Ids.Count == 0)
            {
                // note(cosborn) Don't pay the cost of creating a service client if we can avoid it.
                return builder;
            }

            var secretsManagerClient = configuration.GetAWSOptions().CreateServiceClient<IAmazonSecretsManager>();
            return secretsOpts.Ids.Aggregate(
                builder,
                (acc, curr) => acc.Add(new SecretsManagerConfigurationSource(secretsManagerClient, curr, secretsOpts.Expiration)));
        }
    }
}
