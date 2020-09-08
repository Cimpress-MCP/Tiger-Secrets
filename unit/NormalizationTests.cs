// <copyright file="NormalizationTests.cs" company="Cimpress, Inc.">
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
using System.Linq;
using System.Threading;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Test;
using Moq;
using Newtonsoft.Json;
using Xunit;
using static Microsoft.Extensions.Configuration.SecretsManagerConfigurationProvider;
using static Microsoft.Extensions.Configuration.ConfigurationPath;

namespace Test
{
    /// <summary>Tests of configuration data normalization.</summary>
    [Properties(Arbitrary = new[] { typeof(Generators) }, QuietOnSuccess = true)]
    public static class NormalizationTests
    {
        [Property(DisplayName = "A plain value is unchanged by normalization.")]
        public static void PlainValue_Unchanged(ConfigurationKey key, NonEmptyString value, NonEmptyString secretId)
        {
            var datum = new Dictionary<string, object>
            {
                [key.Get] = value.Get
            };
            var response = new GetSecretValueResponse
            {
                SecretString = JsonConvert.SerializeObject(datum)
            };
            var client = new Mock<IAmazonSecretsManager>();
            _ = client
                .Setup(m => m.GetSecretValueAsync(It.IsNotNull<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var configurationSource = new SecretsManagerConfigurationSource(client.Object, secretId.Get, Timeout.InfiniteTimeSpan);
            using var sut = new SecretsManagerConfigurationProvider(configurationSource);
            sut.Load();

            client.Verify(m => m.GetSecretValueAsync(It.Is<GetSecretValueRequest>(r => r.SecretId == secretId.Get), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(value.Get, sut.Get(key.Get));
        }

        [Property(DisplayName = "A compound value is unchanged by normalization.")]
        public static void CompoundValue_Normalized(ConfigurationKey[] key, NonEmptyString value, NonEmptyString secretId)
        {
            var compoundKey = Combine(key.Select(k => k.Get));
            var datum = new Dictionary<string, object>
            {
                [compoundKey] = value.Get
            };
            var response = new GetSecretValueResponse
            {
                SecretString = JsonConvert.SerializeObject(datum)
            };
            var client = new Mock<IAmazonSecretsManager>();
            _ = client
                .Setup(m => m.GetSecretValueAsync(It.IsNotNull<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var configurationSource = new SecretsManagerConfigurationSource(client.Object, secretId.Get, Timeout.InfiniteTimeSpan);
            using var sut = new SecretsManagerConfigurationProvider(configurationSource);
            sut.Load();

            client.Verify(m => m.GetSecretValueAsync(It.Is<GetSecretValueRequest>(r => r.SecretId == secretId.Get), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(value.Get, sut.Get(compoundKey));
        }

        [Property(DisplayName = "An alternatively compound value is normalized.")]
        public static void AternativelyCompoundValue_Normalized(ConfigurationKey[] key, NonEmptyString value, NonEmptyString secretId)
        {
            var alternativelyCompoundKey = string.Join(AlternativeKeyDelimiter, key);
            var datum = new Dictionary<string, object>
            {
                [alternativelyCompoundKey] = value.Get
            };
            var response = new GetSecretValueResponse
            {
                SecretString = JsonConvert.SerializeObject(datum)
            };
            var client = new Mock<IAmazonSecretsManager>();
            _ = client
                .Setup(m => m.GetSecretValueAsync(It.IsNotNull<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var configurationSource = new SecretsManagerConfigurationSource(client.Object, secretId.Get, Timeout.InfiniteTimeSpan);
            using var sut = new SecretsManagerConfigurationProvider(configurationSource);
            sut.Load();

            var compoundKey = Combine(key.Select(k => k.Get));
            client.Verify(m => m.GetSecretValueAsync(It.Is<GetSecretValueRequest>(r => r.SecretId == secretId.Get), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(value.Get, sut.Get(compoundKey));
        }

        [Property(DisplayName = "A deep value is normalized.")]
        public static void DeepValue_Normalized(NonEmptyArray<ConfigurationKey> key, NonEmptyString value, NonEmptyString secretId)
        {
            var datum = GenerateDatum(key.Get, value.Get);
            var response = new GetSecretValueResponse
            {
                SecretString = JsonConvert.SerializeObject(datum)
            };
            var client = new Mock<IAmazonSecretsManager>();
            _ = client
                .Setup(m => m.GetSecretValueAsync(It.IsNotNull<GetSecretValueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var configurationSource = new SecretsManagerConfigurationSource(client.Object, secretId.Get, Timeout.InfiniteTimeSpan);
            using var sut = new SecretsManagerConfigurationProvider(configurationSource);
            sut.Load();

            var compoundKey = Combine(key.Get.Select(k => k.Get));
            client.Verify(m => m.GetSecretValueAsync(It.Is<GetSecretValueRequest>(r => r.SecretId == secretId.Get), It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(value.Get, sut.Get(compoundKey));

            static ImmutableDictionary<string, object> GenerateDatum(in ReadOnlySpan<ConfigurationKey> k, string v)
            {
                var (head, tail) = k;

                var pair = tail.Length == 0
                    ? KeyValuePair.Create<string, object>(head, v)
                    : KeyValuePair.Create<string, object>(head, GenerateDatum(tail, v));

                return ImmutableDictionary.CreateRange(new[] { pair });
            }
        }
    }
}
