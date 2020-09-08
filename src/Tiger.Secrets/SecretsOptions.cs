// <copyright file="SecretsOptions.cs" company="Cimpress, Inc.">
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Tiger.Secrets
{
    /// <summary>
    /// Represents the declarative configuration options for AWS Secrets Manager configuration.
    /// </summary>
    public sealed class SecretsOptions
    {
        /// <summary>The default name of the configuration section.</summary>
        public const string Secrets = "Secrets";

        /// <summary>Gets the collection of unique identifiers of the secrets to retrieve.</summary>
        public List<string> Ids { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the amount of time after which configuration will be reloaded.
        /// </summary>
        [SuppressMessage("Roslynator.Style", "RCS1170", Justification = "Configuration can bind to private setters.")]
        public TimeSpan Expiration { get; private set; } = Timeout.InfiniteTimeSpan;
    }
}
