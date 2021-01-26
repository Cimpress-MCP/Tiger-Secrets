// <copyright file="ConfigurationExtensions.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;

// note(cosborn) Hidden in plain sight to avoid casual misuse of this extension method.
namespace Tiger.Secrets.Lambda
{
    /// <summary>Extensions to the functionality of the <see cref="IConfiguration"/> interface.</summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Blocks execution of a Lambda Function until any instances of <see cref="SecretsManagerConfigurationProvider"/>
        /// added to the configuration have completed a reload of configuration from Secrets Manager if such a reload
        /// is in progress when this method is invoked.
        /// </summary>
        /// <param name="configuration">The configuration containing the providers for which to wait.</param>
        /// <param name="timeout">The amout of time after which to give up on waiting for a reload to complete.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configuration"/> is <see langword="null"/>.</exception>
        [SuppressMessage("Microsoft.Style", "IDE0083", Justification = "Compiler verison is still in preview.")]
        public static void WaitForSecretsManagerReloadToComplete(this IConfiguration configuration, TimeSpan timeout)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (configuration is not IConfigurationRoot configurationRoot)
            {
                return;
            }

            foreach (var provider in configurationRoot.Providers.OfType<SecretsManagerConfigurationProvider>())
            {
                provider.WaitForReloadToComplete(timeout);
            }
        }
    }
}
