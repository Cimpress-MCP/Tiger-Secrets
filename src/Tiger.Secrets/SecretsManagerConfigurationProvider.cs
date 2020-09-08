// <copyright file="SecretsManagerConfigurationProvider.cs" company="Cimpress, Inc.">
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
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using static System.Globalization.CultureInfo;
using static System.TimeSpan;
using static Microsoft.Extensions.Configuration.ConfigurationPath;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>Provides AWS Secrets Manager configuration key/values for an application.</summary>
    public sealed class SecretsManagerConfigurationProvider
        : ConfigurationProvider, IDisposable
    {
        /// <summary>Another delimiter "__" used to separate individual keys in a path.</summary>
        public const string AlternativeKeyDelimiter = "__";

        readonly IAmazonSecretsManager _client;
        readonly string _secretId;
        readonly ManualResetEvent _reloadTaskEvent = new ManualResetEvent(initialState: true);

        /// <summary>Initializes a new instance of the <see cref="SecretsManagerConfigurationProvider"/> class.</summary>
        /// <param name="configurationSource">The source of AWS Secrets Manager configuration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="configurationSource"/> is <see langword="null"/>.</exception>
        public SecretsManagerConfigurationProvider(SecretsManagerConfigurationSource configurationSource)
        {
            if (configurationSource is null) { throw new ArgumentNullException(nameof(configurationSource)); }

            _client = configurationSource.SecretsManagerClient;
            _secretId = configurationSource.SecretId;

            // note(cosborn) Remember that negative means infinite and zero is invalid.
            if (configurationSource.Expiration > Zero)
            {
                _ = ChangeToken.OnChange(
                    () => new CancellationChangeToken(new CancellationTokenSource(configurationSource.Expiration).Token),
                    async () =>
                    {
                        _ = _reloadTaskEvent.Reset();
                        try
                        {
                            await LoadCoreAsync().ConfigureAwait(false);
                        }
                        finally
                        {
                            _ = _reloadTaskEvent.Set();
                        }
                    });
            }
        }

        /// <summary>
        /// Blocks the configuration provider until reload of configuration is complete
        /// if a reload has already begun.
        /// </summary>
        /// <remarks><para>
        /// This method is not for general use; it is exposed so that a Lambda Function can wait
        /// for the reload to complete before completing the event causing the Lambda compute
        /// environment to be frozen.
        /// </para></remarks>
        /// <param name="timeout">
        /// A <see cref="TimeSpan"/> representing the maximum time to wait for reload to complete.
        /// </param>
        public void WaitForReloadToComplete(TimeSpan timeout) => _reloadTaskEvent.WaitOne(timeout);

        /// <inheritdoc/>
        // because(cosborn) Configuration is purely a sync API, and we want good exceptions. Gross, gross, gross.
        public override void Load() => LoadCoreAsync().GetAwaiter().GetResult();

        async Task LoadCoreAsync()
        {
            var req = new GetSecretValueRequest
            {
                SecretId = _secretId
            };
            var resp = await _client.GetSecretValueAsync(req).ConfigureAwait(false);
            if (resp?.SecretString is null) { return; }

            Data = NormalizeData(JObject.Parse(resp.SecretString));
            OnReload();
        }

        static IDictionary<string, string> NormalizeData(JObject rawData)
        {
            /* note(cosborn)
             * "Normalize" the data? But it's already in proper JSON form. Why?
             *
             * The configuration providers that read JSON from files allow
             * configuration to be specified in two ways. Normalization means that:
             *
             * { "Compound": { "Key": "value" } }
             *
             * ...and:
             *
             * { "Compound:Key": "value" }
             *
             * ...are equivalent, and the latter is canonical. I do the same
             * operations here, as it's convenient to specify configuration as
             * key/value pairs in Secrets Manager. (It's still read back as
             * a JSON object, natch.) If raw JSON is entered directly, however,
             * we'll still do the right thing.
             */

            var data = new Dictionary<string, string>();
            VisitObject(rawData, ImmutableArray<string>.Empty, data);
            return data;

            static void VisitObject(JObject @object, ImmutableArray<string> context, IDictionary<string, string> data)
            {
                foreach (var property in @object.Properties())
                {
                    VisitToken(property.Value, context.Add(property.Name), data);
                }
            }

            static void VisitArray(JArray array, ImmutableArray<string> context, IDictionary<string, string> data)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    /* note(cosborn)
                     * Remember, configuration considers arrays to be objects with "numeric" indices.
                     * That's why they merge how they do in AppSettings.
                     */
                    VisitToken(array[i], context.Add(i.ToString(InvariantCulture)), data);
                }
            }

            static void VisitToken(JToken token, ImmutableArray<string> context, IDictionary<string, string> data)
            {
                switch (token.Type)
                {
                    case JTokenType.Object:
                        VisitObject((JObject)token, context, data);
                        break;
                    case JTokenType.Array:
                        VisitArray((JArray)token, context, data);
                        break;
                    default:
                        // note(cosborn) ¯\_(ツ)_/¯
                        VisitValue((JValue)token, context, data);
                        break;
                }
            }

            static void VisitValue(JValue value, ImmutableArray<string> context, IDictionary<string, string> data)
            {
                var key = NormalizeKey(Combine(context));

                // note(cosborn) If you create JSON with duplicate keys, you get what you get.
                data[key] = value.ToString(InvariantCulture);

                // note(cosborn) Colons put into Secrets Manager keys can't always be extracted.
                static string NormalizeKey(string k) => k.Replace(AlternativeKeyDelimiter, KeyDelimiter, StringComparison.Ordinal);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _client.Dispose();
            _reloadTaskEvent.Dispose();
        }
    }
}
